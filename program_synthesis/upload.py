import requests

url = "https://api.reia-rehab.com/upload/file/?upload_id=sample_upload"

payload = {
    "title": "Sample Title",
    "body": "This is a sample"
}

headers = {
    "Content-Type": "application/json"
}

response = requests.post(url, json=payload, headers=headers)

print("Status Code:", response.status_code)
print("Response:", response.text)