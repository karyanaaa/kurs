import requests

print("Testing /test endpoint:")
try:
    response = requests.get("http://127.0.0.1:8080/test")
    print(f"Status: {response.status_code}")
    print(f"Response: {response.text}")
except Exception as e:
    print(f"Error: {e}")

print("\n" + "="*50 + "\n")

print("Testing /register endpoint:")
try:
    response = requests.post(
        "http://127.0.0.1:8080/register",
        json={"username": "hello", "password": "12345"}
    )
    print(f"Status: {response.status_code}")
    print(f"Response: {response.text}")
except Exception as e:
    print(f"Error: {e}")
