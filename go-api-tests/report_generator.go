package main

import (
	"encoding/json"
	"os"
	"time"
)

// WriteReport generates a JSON report file from the test results.
func WriteReport(results []TestResult, filename string) error {
	report := Report{
		Timestamp: time.Now().Format(time.RFC3339),
		Results:   results,
	}

	file, err := os.Create(filename)
	if err != nil {
		return err
	}
	defer file.Close()

	encoder := json.NewEncoder(file)
	encoder.SetIndent("", "  ") // Pretty-print the JSON

	return encoder.Encode(report)
}
