from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

app = FastAPI()

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

@app.get("/")
def root():
    return {"message": "Server is working"}

@app.get("/test")
def test():
    return {"msg": "Server is working"}

@app.post("/register")
def register(user: UserCreate):
    print(f"Register attempt: {user.username}")
    return {"msg": f"User {user.username} created successfully"}

@app.post("/register_raw")
async def register_raw(request: dict):
    print(f"Raw request: {request}")
    return {"msg": f"User {request.get('username')} created"}
