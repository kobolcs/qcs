import json
import csv
import argparse
import os
from typing import Dict

import demo_pandas as pd


def parse_go_api_report(path: str):
    """Parse Go API test report JSON into dashboard row format."""
    with open(path, 'r', encoding='utf-8') as f:
        report = json.load(f)

    rows = []
    for result in report.get("results", []):
        name = result.get("City", "")
        passed = result.get("Passed", False)
        status = "passed" if passed else "failed"
        duration_ms = result.get("DurationMS", 0)
        time_sec = float(duration_ms) / 1000.0
        failure_message = "" if passed else result.get("Message", "")
        rows.append({
            "name": name,
            "status": status,
            "time": f"{time_sec:.2f}",
            "is_flaky": False,
            "failure_message": failure_message,
        })
    return rows


def load_data(path: str) -> Dict:
    """Load a JSON file containing k6 results.

    Parameters
    ----------
    path: str
        Path to the JSON results file.

    Returns
    -------
    dict
        Parsed JSON data.

    Raises
    ------
    FileNotFoundError
        If the file does not exist.
    ValueError
        If the file contents are not valid JSON.
    """

    try:
        with open(path, "r", encoding="utf-8") as f:
            return json.load(f)
    except FileNotFoundError:
        # Let the caller handle missing files
        raise
    except json.JSONDecodeError as exc:
        raise ValueError(f"Invalid JSON in {path}") from exc


def process_k6_results(data: Dict) -> pd.DataFrame:
    """Convert k6 summary JSON into a DataFrame of key metrics."""

    metrics = data.get("metrics", {})
    duration_values = metrics.get("http_req_duration", {}).get("values", {})
    failed_values = metrics.get("http_req_failed", {}).get("values", {})

    avg_duration = duration_values.get("avg", 0)
    p95_duration = duration_values.get("p(95)", 0)
    fail_rate = failed_values.get("rate", 0)

    return pd.DataFrame([
        {
            "avg_duration": avg_duration,
            "p95_duration": p95_duration,
            "fail_rate": fail_rate,
        }
    ])


def main():
    parser = argparse.ArgumentParser(description="Convert raw test results to CSV for the dashboard")
    parser.add_argument("--input-file", required=True, help="Path to the JSON report (e.g., weather_test_report.json)")
    parser.add_argument("--output-file", default="processed_results.csv", help="CSV file to write (default: processed_results.csv)")
    args = parser.parse_args()

    if not os.path.exists(args.input_file):
        parser.error(f"Input file not found: {args.input_file}")

    rows = parse_go_api_report(args.input_file)

    with open(args.output_file, "w", newline="", encoding="utf-8") as csvfile:
        writer = csv.DictWriter(csvfile, fieldnames=["name", "status", "time", "is_flaky", "failure_message"])
        writer.writeheader()
        for row in rows:
            writer.writerow(row)

    print(f"Wrote {len(rows)} rows to {args.output_file}")


if __name__ == "__main__":
    main()
