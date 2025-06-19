package main

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"net/http/httptest"
	"os"
	"sync"
	"testing"
	"time"
)

// loadConfig reads config.json so all tests share one source of settings
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

// getAPIKey pulls the secret from the environment to keep it out of source control
func getAPIKey(envName string) string {
	return os.Getenv(envName)
}

func TestWeatherAPI(t *testing.T) {
	cfg, err := loadConfig()
	if err != nil {
		t.Fatalf("Failed to load config: %v", err)
	}
	useMock := os.Getenv("USE_LIVE_OWM") != "1"

	apiKey := getAPIKey(cfg.APIKeyEnv)
	if apiKey == "" && !useMock {
		t.Skipf("API key environment variable '%s' not set, skipping integration test", cfg.APIKeyEnv)
	}

	var server *httptest.Server
	if useMock {
		server = httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			city := r.URL.Query().Get("q")
			w.Header().Set("Content-Type", "application/json")
			fmt.Fprintf(w, `{"name":"%s","main":{"temp":15}}`, city)
		}))
		os.Setenv("OWM_BASE_URL", server.URL)
		t.Cleanup(func() {
			server.Close()
			os.Unsetenv("OWM_BASE_URL")
		})
	}

	// NOTE: results slice is guarded with a mutex for safe concurrent writes
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

	// NOTE: t.Cleanup runs after the parallel subtests finish so we write the report once
	t.Cleanup(func() {
		if err := WriteReport(results, "weather_test_report.json"); err != nil {
			t.Logf("Could not write report: %v", err)
		}
	})
}
