package main

import (
    "context"
    "encoding/json"
    "fmt"
    "net/http"
    "os"
    "time"
)

const (
	maxRetries = 3
	retryDelay = 500 * time.Millisecond
)

// GetWithRetry performs a GET request with a simple backoff retry mechanism.
func GetWithRetry(ctx context.Context, url string) (*http.Response, error) {
    var resp *http.Response
    var err error

    for i := 0; i < maxRetries; i++ {
        req, reqErr := http.NewRequestWithContext(ctx, "GET", url, nil)
        if reqErr != nil {
            return nil, fmt.Errorf("failed to create request: %w", reqErr)
        }

        resp, err = http.DefaultClient.Do(req)
        if err == nil && resp.StatusCode == http.StatusOK {
            return resp, nil // Success
        }

        // If there's an error or a non-200 status, wait before retrying.
        time.Sleep(retryDelay * time.Duration(i+1))
    }

    // Return the last response and error after all attempts.
    if resp != nil {
         return resp, fmt.Errorf("request failed after %d attempts with status %s", maxRetries, resp.Status)
    }
    return nil, fmt.Errorf("request failed after %d attempts: %w", maxRetries, err)
}

func getBaseURL() string {
    if v := os.Getenv("OWM_BASE_URL"); v != "" {
        return v
    }
    return "https://api.openweathermap.org"
}

func FetchWeatherData(ctx context.Context, apiKey, city string) (*WeatherResponse, time.Duration, error) {
        start := time.Now()
        url := fmt.Sprintf("%s/data/2.5/weather?q=%s&appid=%s&units=metric", getBaseURL(), city, apiKey)

	resp, err := GetWithRetry(ctx, url)
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