from fastapi import FastAPI

app = FastAPI()

@app.get("/")
def root():
    return {"message": "Server is running"}

@app.get("/test")
def test():
    return {"status": "ok"}

@app.post("/register")
def register(username: str, password: str):
    return {"msg": f"User {username} created"}
