from fastapi import FastAPI, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer, OAuth2PasswordRequestForm
from sqlalchemy.orm import Session
from pydantic import BaseModel
from typing import List, Optional
from datetime import datetime
import models
import database
import auth_simple as auth
from fastapi.middleware.cors import CORSMiddleware


# Создание таблиц
models.Base.metadata.create_all(bind=database.engine)

app = FastAPI(title="ФинУчет API")

# Разрешаем CORS для WPF приложения
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="token")

# --- Схемы ---
class UserCreate(BaseModel):
    username: str
    password: str
    security_question: Optional[str] = "Какую кличку у вас было в детстве?"
    security_answer: Optional[str] = None

class TransactionCreate(BaseModel):
    amount: float
    description: str
    type: str
    category_id: int
    date: Optional[datetime] = None

class TransactionResponse(TransactionCreate):
    id: int
    
    class Config:
        from_attributes = True

# --- Зависимости ---
def get_current_user(token: str = Depends(oauth2_scheme), db: Session = Depends(database.get_db)):
    credentials_exception = HTTPException(status_code=401, detail="Invalid credentials")
    try:
        payload = auth.jwt.decode(token, auth.SECRET_KEY, algorithms=[auth.ALGORITHM])
        username: str = payload.get("sub")
        if username is None:
            raise credentials_exception
    except auth.JWTError:
        raise credentials_exception
    user = db.query(models.User).filter(models.User.username == username).first()
    if user is None:
        raise credentials_exception
    return user

# --- Эндпоинты ---

@app.get("/security-question")
def get_security_question(username: str, db: Session = Depends(database.get_db)):
    user = db.query(models.User).filter(models.User.username == username).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    return {"question": user.security_question}

@app.post("/register")
def register(user: UserCreate, db: Session = Depends(database.get_db)):
    db_user = db.query(models.User).filter(models.User.username == user.username).first()
    if db_user:
        raise HTTPException(status_code=400, detail="Username already registered")
    hashed_pw = auth.get_password_hash(user.password)
    
    # Хэшируем ответ на вопрос
    hashed_answer = auth.get_password_hash(user.security_answer) if user.security_answer else ""
    
    db_user = models.User(
        username=user.username, 
        hashed_password=hashed_pw,
        security_question=user.security_question,
        security_answer=hashed_answer
    )
    db.add(db_user)
    db.commit()
    db.refresh(db_user)
    return {"msg": "User created"}


class ResetPasswordRequest(BaseModel):
    username: str
    security_answer: str
    new_password: str

@app.post("/reset-password")
def reset_password(request: ResetPasswordRequest, db: Session = Depends(database.get_db)):
    user = db.query(models.User).filter(models.User.username == request.username).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    
    # Проверяем ответ на вопрос
    if not auth.verify_password(request.security_answer, user.security_answer):
        raise HTTPException(status_code=401, detail="Incorrect security answer")
    
    # Устанавливаем новый пароль
    user.hashed_password = auth.get_password_hash(request.new_password)
    db.commit()
    
    return {"msg": "Password reset successfully"}

@app.post("/token")
def login(form_data: OAuth2PasswordRequestForm = Depends(), db: Session = Depends(database.get_db)):
    user = db.query(models.User).filter(models.User.username == form_data.username).first()
    if not user or not auth.verify_password(form_data.password, user.hashed_password):
        raise HTTPException(status_code=401, detail="Incorrect username or password")
    access_token = auth.create_access_token(data={"sub": user.username})
    return {"access_token": access_token, "token_type": "bearer"}

@app.get("/transactions", response_model=List[TransactionResponse])
def get_transactions(db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    return db.query(models.Transaction).filter(models.Transaction.user_id == current_user.id).all()

@app.post("/transactions")
def create_transaction(tr: TransactionCreate, db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    # Получаем данные из запроса
    data = tr.model_dump()
    # Устанавливаем user_id
    data['user_id'] = current_user.id
    # Устанавливаем дату, если не передана
    if 'date' not in data or data['date'] is None:
        data['date'] = datetime.utcnow()
    
    db_tr = models.Transaction(**data)
    db.add(db_tr)
    db.commit()
    db.refresh(db_tr)
    return db_tr

@app.delete("/transactions/{tr_id}")
def delete_transaction(tr_id: int, db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    tr = db.query(models.Transaction).filter(models.Transaction.id == tr_id, models.Transaction.user_id == current_user.id).first()
    if not tr:
        raise HTTPException(status_code=404, detail="Not found")
    db.delete(tr)
    db.commit()
    return {"msg": "Deleted"}

@app.get("/categories")
def get_categories(db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    return db.query(models.Category).filter(models.Category.user_id == current_user.id).all()
# --- Модель для категории ---
class CategoryCreate(BaseModel):
    name: str
    type: str  # 'income' or 'expense'

class CategoryResponse(CategoryCreate):
    id: int
    user_id: int
    
    class Config:
        from_attributes = True

# --- Эндпоинты для категорий ---

# Получить все категории пользователя
@app.get("/categories", response_model=List[CategoryResponse])
def get_categories(db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    return db.query(models.Category).filter(models.Category.user_id == current_user.id).all()

# Создать категорию
@app.post("/categories", response_model=CategoryResponse)
def create_category(category: CategoryCreate, db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    db_category = models.Category(
        name=category.name,
        type=category.type,
        user_id=current_user.id
    )
    db.add(db_category)
    db.commit()
    db.refresh(db_category)
    return db_category

# Обновить категорию
@app.put("/categories/{category_id}", response_model=CategoryResponse)
def update_category(category_id: int, category: CategoryCreate, db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    db_category = db.query(models.Category).filter(
        models.Category.id == category_id,
        models.Category.user_id == current_user.id
    ).first()
    if not db_category:
        raise HTTPException(status_code=404, detail="Category not found")
    
    db_category.name = category.name
    db_category.type = category.type
    db.commit()
    db.refresh(db_category)
    return db_category

@app.put("/transactions/{transaction_id}")
def update_transaction(transaction_id: int, tr: TransactionCreate, db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    db_transaction = db.query(models.Transaction).filter(
        models.Transaction.id == transaction_id,
        models.Transaction.user_id == current_user.id
    ).first()
    if not db_transaction:
        raise HTTPException(status_code=404, detail="Transaction not found")
    
    db_transaction.amount = tr.amount
    db_transaction.description = tr.description
    db_transaction.type = tr.type
    db_transaction.category_id = tr.category_id
    db_transaction.date = tr.date or datetime.utcnow()
    
    db.commit()
    db.refresh(db_transaction)
    return db_transaction

# Удалить категорию
@app.delete("/categories/{category_id}")
def delete_category(category_id: int, db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    db_category = db.query(models.Category).filter(
        models.Category.id == category_id,
        models.Category.user_id == current_user.id
    ).first()
    if not db_category:
        raise HTTPException(status_code=404, detail="Category not found")
    
    # Проверяем, есть ли транзакции с этой категорией
    transactions = db.query(models.Transaction).filter(models.Transaction.category_id == category_id).first()
    if transactions:
        raise HTTPException(status_code=400, detail="Cannot delete category with existing transactions")
    
    db.delete(db_category)
    db.commit()
    return {"msg": "Category deleted"}

class InvestmentCreate(BaseModel):
    name: str
    type: str
    amount: float
    purchase_price: float
    current_price: float
    quantity: float
    purchase_date: Optional[datetime] = None
    currency: Optional[str] = "RUB"
    notes: Optional[str] = ""

class InvestmentResponse(InvestmentCreate):
    id: int
    user_id: int
    
    class Config:
        from_attributes = True

# --- Эндпоинты для инвестиций ---

@app.get("/investments", response_model=List[InvestmentResponse])
def get_investments(db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    return db.query(models.Investment).filter(models.Investment.user_id == current_user.id).all()

@app.post("/investments", response_model=InvestmentResponse)
def create_investment(inv: InvestmentCreate, db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    data = inv.model_dump()
    data['user_id'] = current_user.id
    if 'purchase_date' not in data or data['purchase_date'] is None:
        data['purchase_date'] = datetime.utcnow()
    
    db_inv = models.Investment(**data)
    db.add(db_inv)
    db.commit()
    db.refresh(db_inv)
    return db_inv

@app.put("/investments/{inv_id}", response_model=InvestmentResponse)
def update_investment(inv_id: int, inv: InvestmentCreate, db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    db_inv = db.query(models.Investment).filter(
        models.Investment.id == inv_id,
        models.Investment.user_id == current_user.id
    ).first()
    if not db_inv:
        raise HTTPException(status_code=404, detail="Investment not found")
    
    for key, value in inv.model_dump().items():
        setattr(db_inv, key, value)
    
    db.commit()
    db.refresh(db_inv)
    return db_inv

@app.delete("/investments/{inv_id}")
def delete_investment(inv_id: int, db: Session = Depends(database.get_db), current_user: models.User = Depends(get_current_user)):
    db_inv = db.query(models.Investment).filter(
        models.Investment.id == inv_id,
        models.Investment.user_id == current_user.id
    ).first()
    if not db_inv:
        raise HTTPException(status_code=404, detail="Investment not found")
    
    db.delete(db_inv)
    db.commit()
    return {"msg": "Deleted"}