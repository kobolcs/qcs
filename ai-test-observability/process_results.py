import json
import csv
import argparse
import os


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
