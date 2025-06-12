package main

import (
	"context"
	"encoding/json"
	"fmt"
	"os"
	"sync"
	"testing"
	"time"
)

// loadConfig loads the configuration from config.json
func loadConfig() (*Config, error) {
	f, err := os.Open("config.json")
	if err != nil {
		return nil, err
	}
	defer f.Close()
	var cfg Config
	err = json.NewDecoder(f).Decode(&cfg)
	return &cfg, err
}

// getAPIKey safely retrieves the API key from the environment variable.
func getAPIKey(envName string) string {
	return os.Getenv(envName)
}

func TestWeatherAPI(t *testing.T) {
	cfg, err := loadConfig()
	if err != nil {
		t.Fatalf("Failed to load config: %v", err)
	}
	apiKey := getAPIKey(cfg.APIKeyEnv)
	if apiKey == "" {
		t.Skipf("API key environment variable '%s' not set, skipping integration test", cfg.APIKeyEnv)
	}

	// Use a slice protected by a mutex for concurrent test appends.
	var results []TestResult
	var mu sync.Mutex

	t.Run("Cities", func(t *testing.T) {
		for _, city := range cfg.Cities {
			city := city // capture range variable
			t.Run(city, func(t *testing.T) {
				t.Parallel()

				weatherData, duration, err := FetchWeatherData(context.Background(), apiKey, city)

				res := TestResult{
					City:       city,
					DurationMS: duration.Milliseconds(),
				}

				if err != nil {
					res.Passed = false
					res.Message = fmt.Sprintf("API call failed: %v", err)
					t.Error(res.Message)
				} else if weatherData.Name == "" || weatherData.Main.Temp < -100 || weatherData.Main.Temp > 60 {
					res.Passed = false
					res.Message = "Validation failed: Unlikely or missing weather data"
					t.Error(res.Message)
				} else if duration > 2000*time.Millisecond {
					res.Passed = false
					res.Message = fmt.Sprintf("Performance validation failed: Response too slow at %d ms", duration.Milliseconds())
					t.Error(res.Message)
				} else {
					res.Passed = true
					res.Message = fmt.Sprintf("OK: %.1f°C in %s", weatherData.Main.Temp, weatherData.Name)
					res.Temp = weatherData.Main.Temp
				}

				mu.Lock()
				results = append(results, res)
				mu.Unlock()
			})
		}
	})

	// Use t.Cleanup to ensure the report is written after all parallel tests complete.
	t.Cleanup(func() {
		if err := WriteReport(results, "weather_test_report.json"); err != nil {
			t.Logf("Could not write report: %v", err)
		}
	})
}