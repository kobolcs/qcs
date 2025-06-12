import requests

def test_latest_launch_response_schema():
    url = "https://api.spacexdata.com/v4/launches/latest"
    r = requests.get(url)
    assert r.status_code == 200
    data = r.json()
    assert "name" in data
    assert "date_utc" in data
    assert "rocket" in data
    assert data["links"]["patch"]["small"].startswith("https://")

def test_rocket_details():
    latest = requests.get("https://api.spacexdata.com/v4/launches/latest").json()
    rocket_id = latest["rocket"]
    rocket = requests.get(f"https://api.spacexdata.com/v4/rockets/{rocket_id}").json()
    assert "name" in rocket and rocket["name"] != ""
    assert "active" in rocket
