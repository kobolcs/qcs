import csv
from typing import List, Dict

class CSVKeywords:
    """Keywords for handling CSV files."""

    def read_csv_file_to_list(self, csv_path: str) -> List[Dict[str, str]]:
        """Return the contents of a CSV file as a list of dictionaries."""
        with open(csv_path, newline='') as csvfile:
            reader = csv.DictReader(csvfile)
            return [row for row in reader]
