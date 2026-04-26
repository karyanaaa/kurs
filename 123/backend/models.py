from sqlalchemy import Column, Integer, String, Float, DateTime, ForeignKey, Boolean
from sqlalchemy.orm import relationship
from database import Base
from datetime import datetime

class User(Base):
    __tablename__ = "users"
    id = Column(Integer, primary_key=True, index=True)
    username = Column(String, unique=True, index=True)  
    hashed_password = Column(String)
    email = Column(String, nullable=True)
    security_question = Column(String, nullable=False, default="Какую кличку у вас было в детстве?")
    security_answer = Column(String, nullable=False, default="")
    transactions = relationship("Transaction", back_populates="owner")
    categories = relationship("Category", back_populates="owner")
    investments = relationship("Investment", back_populates="owner")

class Category(Base):
    __tablename__ = "categories"
    id = Column(Integer, primary_key=True, index=True)
    name = Column(String, index=True)
    type = Column(String)
    user_id = Column(Integer, ForeignKey("users.id"))
    owner = relationship("User", back_populates="categories")
    transactions = relationship("Transaction", back_populates="category")

class Transaction(Base):
    __tablename__ = "transactions"
    id = Column(Integer, primary_key=True, index=True)
    amount = Column(Float)
    date = Column(DateTime, default=datetime.utcnow)
    description = Column(String)
    type = Column(String)
    user_id = Column(Integer, ForeignKey("users.id"))
    category_id = Column(Integer, ForeignKey("categories.id"))
    owner = relationship("User", back_populates="transactions")
    category = relationship("Category", back_populates="transactions")

class Investment(Base):
    __tablename__ = "investments"
    id = Column(Integer, primary_key=True, index=True)
    name = Column(String, nullable=False)
    type = Column(String, nullable=False)
    amount = Column(Float, default=0)
    purchase_price = Column(Float, default=0)
    current_price = Column(Float, default=0)
    quantity = Column(Float, default=0)
    purchase_date = Column(DateTime, default=datetime.utcnow)
    currency = Column(String, default="RUB")
    notes = Column(String, default="")
    user_id = Column(Integer, ForeignKey("users.id"))
    
    owner = relationship("User", back_populates="investments")