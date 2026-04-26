import requests

endpoints = ["/", "/health", "/test", "/register"]

for endpoint in endpoints:
    url = f"http://127.0.0.1:8080{endpoint}"
    print(f"\nTesting {url}:")
    
    if endpoint == "/register":
        try:
            response = requests.post(url, json={"username": "hello", "password": "12345"})
            print(f"Status: {response.status_code}")
            print(f"Response: {response.text}")
        except Exception as e:
            print(f"Error: {e}")
    else:
        try:
            response = requests.get(url)
            print(f"Status: {response.status_code}")
            print(f"Response: {response.text}")
        except Exception as e:
            print(f"Error: {e}")
