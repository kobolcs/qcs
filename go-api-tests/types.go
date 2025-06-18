package main

// Config struct correctly matches the structure of config.json
type Config struct {
	APIKeyEnv string   `json:"api_key_env"` // Corrected from ApiKeyEnv to APIKeyEnv
	Cities    []string `json:"cities"`
}

// TestResult holds the structured outcome of a single city's test
type TestResult struct {
	City       string  `json:"city"`
	Passed     bool    `json:"passed"`
	Message    string  `json:"message"`
	DurationMS int64   `json:"duration_ms"`
	Temp       float64 `json:"temperature,omitempty"`
}

// WeatherResponse is the structure for the OpenWeatherMap API response
type WeatherResponse struct {
	Name string `json:"name"`
	Main struct {
		Temp float64 `json:"temp"`
	} `json:"main"`
}

// Report is the structure for the final JSON report
type Report struct {
	Timestamp string       `json:"timestamp"`
	Results   []TestResult `json:"results"`
}
