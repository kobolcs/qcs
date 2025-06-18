import requests
from requests.exceptions import HTTPError

class SpaceXClientError(Exception):
    pass

class SpaceXClient:
    """A centralized client for all interactions with the v4 SpaceX API."""

    def __init__(self, base_url: str = "https://api.spacexdata.com/v4"):
        self.base_url = base_url
        # Use a session object for connection pooling and default headers, which is more efficient.
        self.session = requests.Session()
        self.session.headers.update({"Content-Type": "application/json"})

    def get_launch(self, launch_id: str) -> dict:
        """Fetches data for a single launch by its ID."""
        return self._get(f"/launches/{launch_id}")

    def get_rocket(self, rocket_id: str) -> dict:
        """Fetches data for a single rocket by its ID."""
        return self._get(f"/rockets/{rocket_id}")

    def _get(self, endpoint: str, timeout: int = 10) -> dict:
        """Generic GET request helper with error handling."""
        url = f"{self.base_url}{endpoint}"
        try:
            response = self.session.get(url, timeout=timeout)
            # This will automatically raise an exception for 4xx/5xx responses
            response.raise_for_status()
            return response.json()
        except HTTPError as http_err:
            raise SpaceXClientError(f"HTTP error for {url}: {http_err}") from http_err
        except requests.RequestException as req_err:
            raise SpaceXClientError(f"Request failed for {url}: {req_err}") from req_err
