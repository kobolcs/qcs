import csv
from typing import List, Dict

class CSVKeywords:
    """Utility keywords for working with CSV files."""

    def read_csv_file_to_list(self, path: str) -> List[Dict[str, str]]:
        """Read a CSV file and return a list of dictionaries."""
        data: List[Dict[str, str]] = []
        with open(path, mode='r', encoding='utf-8') as csvfile:
            reader = csv.DictReader(csvfile)
            for row in reader:
                data.append(dict(row))
        return data
