import pytest
import csv

from unittest.mock import patch, MagicMock
from resources.keywords.extended_spacex_helpers import ExtendedSpaceX
from resources.api_clients.spacex_client import SpaceXClientError

def read_csv_file_to_list(path: str) -> list:
    """Reads a CSV file and returns its content as a list of dictionaries."""
    data = []
    with open(path, mode='r', encoding='utf-8') as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            data.append(row)
    return data


class TestExtendedSpaceX:
    # Patch the SpaceXClient where it's used
    @patch("resources.keywords.extended_spacex_helpers.SpaceXClient")
    def test_get_combined_launch_and_rocket_happy_path(self, MockSpaceXClient):
        """Should merge launch and rocket info correctly when API returns 200."""
        # Arrange: Set up the mock client instance
        mock_client_instance = MockSpaceXClient.return_value
        mock_client_instance.get_rocket.return_value = {"name": "Falcon 9"}

        launch_json = {
            "rocket": "123abc",
            "name": "FalconSat",
            "date_utc": "2022-01-01T12:00:00.000Z"
        }

        # Act
        result = ExtendedSpaceX().get_combined_launch_and_rocket(launch_json)

        # Assert
        assert result == {
            "mission": "FalconSat",
            "rocket_name": "Falcon 9",
            "launch_date": "2022-01-01T12:00:00.000Z"
        }
        mock_client_instance.get_rocket.assert_called_once_with("123abc")

    @patch("resources.keywords.extended_spacex_helpers.SpaceXClient")
    def test_get_combined_launch_and_rocket_error(self, MockSpaceXClient):
        """Should raise SpaceXClientError when the client fails."""
        # Arrange: Configure the mock client to raise an error
        mock_client_instance = MockSpaceXClient.return_value
        mock_client_instance.get_rocket.side_effect = SpaceXClientError("API call failed")

        launch_json = {"rocket": "fail-rocket"}

        # Act & Assert: Verify that the specific exception is raised
        with pytest.raises(SpaceXClientError, match="API call failed"):
            ExtendedSpaceX().get_combined_launch_and_rocket(launch_json)

