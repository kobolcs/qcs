import Config

config :elixir_api_tests,
  api_url: System.get_env("API_URL") || "https://restcountries.com/v3.1/region/asia",
  max_retries: 3,
  delay_ms: 1000,
  response_time_threshold_ms: 2000,
  http_timeout: 10_000,
  log_dir: "logs",
  snapshot_dir: "snapshots",
  report_output: "reports"