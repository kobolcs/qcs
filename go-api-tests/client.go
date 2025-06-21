package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"net/url"
	"os"
	"strconv"
	"time"
)

var (
	maxRetries = getEnvInt("GO_API_MAX_RETRIES", 3)
	retryDelay = getEnvDuration("GO_API_RETRY_DELAY_MS", 500) * time.Millisecond
)

func getEnvInt(key string, defaultVal int) int {
	if val, ok := os.LookupEnv(key); ok {
		if i, err := strconv.Atoi(val); err == nil {
			return i
		}
	}
	return defaultVal
}

func getEnvDuration(key string, defaultVal int) time.Duration {
	if val, ok := os.LookupEnv(key); ok {
		if i, err := strconv.Atoi(val); err == nil {
			return time.Duration(i)
		}
	}
	return time.Duration(defaultVal)
}

// HTTPClient defines the minimal interface GetWithRetry needs from an HTTP client.
type HTTPClient interface {
	Do(req *http.Request) (*http.Response, error)
}

// GetWithRetry performs a GET request with a simple backoff retry mechanism.
// Logs each attempt with method, URL, and error details.
func GetWithRetry(ctx context.Context, client HTTPClient, url string) (*http.Response, error) {
	var resp *http.Response
	var err error

	for i := 0; i < maxRetries; i++ {
		req, reqErr := http.NewRequestWithContext(ctx, "GET", url, nil)
		if reqErr != nil {
			log.Printf("[ERROR] Failed to create request for %s: %v", url, reqErr)
			return nil, fmt.Errorf("failed to create request: %w", reqErr)
		}

		resp, err = client.Do(req)
		if err == nil && resp.StatusCode == http.StatusOK {
			log.Printf("[INFO] GET %s succeeded on attempt %d", url, i+1)
			return resp, nil // Success
		}

		log.Printf("[WARN] GET %s attempt %d/%d failed: error=%v, status=%v", url, i+1, maxRetries, err, respStatus(resp))

		// Close body before retrying to avoid leaking resources
		if resp != nil && i < maxRetries-1 {
			resp.Body.Close()
		}

		// If there's an error or a non-200 status, wait before retrying.
		time.Sleep(retryDelay * time.Duration(i+1))
	}

	// Return the last response and error after all attempts.
	if resp != nil && err != nil {
		log.Printf("[ERROR] GET %s failed after %d attempts with status %s: %v", url, maxRetries, resp.Status, err)
		return resp, fmt.Errorf("GET %s failed after %d attempts with status %s: %w", url, maxRetries, resp.Status, err)
	}
	if resp != nil {
		log.Printf("[ERROR] GET %s failed after %d attempts with status %s", url, maxRetries, resp.Status)
		return resp, fmt.Errorf("GET %s failed after %d attempts with status %s", url, maxRetries, resp.Status)
	}
	if err != nil {
		log.Printf("[ERROR] GET %s failed after %d attempts: %v", url, maxRetries, err)
		return nil, fmt.Errorf("GET %s failed after %d attempts: %w", url, maxRetries, err)
	}
	log.Printf("[ERROR] GET %s failed after %d attempts", url, maxRetries)
	return nil, fmt.Errorf("GET %s failed after %d attempts", url, maxRetries)
}

func respStatus(resp *http.Response) string {
	if resp == nil {
		return "nil"
	}
	return resp.Status
}

func getBaseURL() string {
	if v := os.Getenv("OWM_BASE_URL"); v != "" {
		return v
	}
	return "https://api.openweathermap.org"
}

// FetchWeatherData retrieves weather data for a city from the API.
// Returns the parsed response, duration, and error if any.
func FetchWeatherData(ctx context.Context, apiKey, city string) (*WeatherResponse, time.Duration, error) {
	start := time.Now()
	url := fmt.Sprintf("%s/data/2.5/weather?q=%s&appid=%s&units=metric", getBaseURL(), url.QueryEscape(city), apiKey)

	resp, err := GetWithRetry(ctx, http.DefaultClient, url)
	duration := time.Since(start)
	if err != nil {
		return nil, duration, err
	}
	defer resp.Body.Close()

	var data WeatherResponse
	if err := json.NewDecoder(resp.Body).Decode(&data); err != nil {
		return nil, duration, fmt.Errorf("failed to decode response for %s: %w", city, err)
	}

	return &data, duration, nil
}
