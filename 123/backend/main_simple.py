from fastapi import FastAPI, Depends, HTTPException
from sqlalchemy.orm import Session
from pydantic import BaseModel
import models
import database
import auth
from fastapi.middleware.cors import CORSMiddleware

# Создание таблиц
models.Base.metadata.create_all(bind=database.engine)

app = FastAPI(title="ФинУчет API")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class UserCreate(BaseModel):
    username: str
    password: str

@app.post("/register")
def register(user: UserCreate, db: Session = Depends(database.get_db)):
    try:
        # Проверяем, существует ли пользователь
        db_user = db.query(models.User).filter(models.User.username == user.username).first()
        if db_user:
            raise HTTPException(status_code=400, detail="Username already registered")
        
        # Создаем нового пользователя
        hashed_pw = auth.get_password_hash(user.password)
        db_user = models.User(username=user.username, hashed_password=hashed_pw)
        db.add(db_user)
        db.commit()
        db.refresh(db_user)
        
        return {"msg": "User created"}
    except Exception as e:
        print(f"Error in register: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/test")
def test():
    return {"msg": "Server is working"}