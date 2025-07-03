import csv
import logging
from typing import List, Dict


class CSVKeywords:  # follows proper CamelCase
    """Keywords for handling CSV files."""
    ROBOT_LIBRARY_SCOPE = "GLOBAL"

    def read_csv_file_to_list(self, csv_path: str) -> List[Dict[str, str]]:
        """Return the contents of a CSV file as a list of dictionaries."""
        try:
            with open(csv_path, newline='') as csvfile:
                reader = csv.DictReader(csvfile)
                return [row for row in reader]
        except FileNotFoundError:
            logging.error(f"CSV file not found: {csv_path}")
            return []
        except Exception as e:
            logging.error(f"Error reading CSV file {csv_path}: {e}")
            return []

