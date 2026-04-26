import requests

try:
    response = requests.post(
        "http://127.0.0.1:8080/register",
        json={"username": "hello", "password": "12345"}
    )
    print(f"Status code: {response.status_code}")
    print(f"Response: {response.text}")
except Exception as e:
    print(f"Error: {e}")
