from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker

# Строка подключения к SQLite
SQLALCHEMY_DATABASE_URL = "sqlite:///./finuchet.db"

# Создаём движок БД
engine = create_engine(
    SQLALCHEMY_DATABASE_URL, 
    connect_args={"check_same_thread": False}
)

# Создаём сессию
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# Создаём Base
Base = declarative_base()

def get_db():
    """
    Зависимость FastAPI для получения сессии БД.
    Автоматически закрывает соединение после запроса.
    """
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()