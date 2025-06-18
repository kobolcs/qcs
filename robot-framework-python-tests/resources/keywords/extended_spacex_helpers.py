from robot.api import logger
from resources.api_clients.spacex_client import SpaceXClient, SpaceXClientError

class ExtendedSpaceX:
    def __init__(self):
        self.client = SpaceXClient()

    def get_combined_launch_and_rocket(self, launch_json: dict) -> dict:
        """
        Orchestrates calls to get launch and rocket data and combines them.
        This keyword's responsibility is now business logic, not HTTP calls.
        """
        rocket_id = launch_json.get('rocket')
        logger.info(f"Using client to fetch rocket info for ID: {rocket_id}")

        try:
            rocket_json = self.client.get_rocket(rocket_id)

            combined = {
                'mission': launch_json.get('name'),
                'rocket_name': rocket_json.get('name'),
                'launch_date': launch_json.get('date_utc')
            }
            logger.info(f"Successfully combined data: {combined}")
            return combined
        except SpaceXClientError as e:
            logger.error(f"Failed to get rocket data: {e}")
            raise  # Re-raise the exception to fail the test
