import os
import requests
import responses

USE_LIVE = os.getenv("USE_LIVE_SPACEX", "0") == "1"

@responses.activate
def test_latest_launch_response_schema():
    url = "https://api.spacexdata.com/v4/launches/latest"
    if not USE_LIVE:
        responses.add(
            responses.GET,
            url,
            json={
                "name": "MockMission",
                "date_utc": "2022-01-01T00:00:00.000Z",
                "rocket": "rocket123",
                "links": {"patch": {"small": "https://img.test/mock.png"}},
            },
            status=200,
        )
    r = requests.get(url)
    assert r.status_code == 200
    data = r.json()
    assert "name" in data
    assert "date_utc" in data
    assert "rocket" in data
    assert data["links"]["patch"]["small"].startswith("https://")

@responses.activate
def test_rocket_details():
    launch_url = "https://api.spacexdata.com/v4/launches/latest"
    rocket_url = "https://api.spacexdata.com/v4/rockets/rocket123"
    if not USE_LIVE:
        responses.add(responses.GET, launch_url, json={"rocket": "rocket123"}, status=200)
        responses.add(
            responses.GET,
            rocket_url,
            json={"name": "Falcon 9", "active": True},
            status=200,
        )
    latest = requests.get(launch_url).json()
    rocket_id = latest["rocket"]
    rocket = requests.get(f"https://api.spacexdata.com/v4/rockets/{rocket_id}").json()
    assert "name" in rocket and rocket["name"] != ""
    assert "active" in rocket
