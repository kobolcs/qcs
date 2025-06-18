# Go API Tests: Parallel, Table-Driven API Validation

**Modern, idiomatic Go API testing with full parallelization, retries, and reporting.** This project showcases advanced patterns used in senior-level test automation.

These tests provide fast confidence that your public-facing APIs return accurate, timely data even under heavy traffic.
---

### 🗝️ Key Features & Concepts Demonstrated

- **Table-Driven & Parallel Tests**: Uses `t.Run` and `t.Parallel` to test multiple cities concurrently for maximum efficiency.
- **Resiliency Patterns**: Includes a client with retry/backoff logic to handle transient API flakiness.
- **Performance Assertions**: Validates that API response times are within an acceptable threshold.
- **Data Validation**: Performs deep validation of the data returned from the API.
- **Custom JSON Reporting**: Generates a machine-readable `weather_test_report.json` for CI/CD consumption.
- **Graceful Execution**: Skips tests automatically if the required `OWM_API_KEY` secret is not set, improving the developer experience.
- **Docker & CI Integration**: Runs seamlessly both locally and within a containerized CI environment.

---
### Strategic Advantage
- Written in Go for minimal runtime overhead and easy deployment in containerized pipelines.
- Built-in concurrency demonstrates patterns that scale as the test suite grows.
- See [Architectural Principles](../ARCHITECTURAL_PRINCIPLES.md) for shared design approaches.


### 🚀 How to Run Locally

1.  **Set your API key** from a service like [OpenWeatherMap](https://openweathermap.org/api):
    ```bash
    export OWM_API_KEY="your-api-key-here"
    ```
    To run tests without an API key, set `USE_LIVE_OWM=0` (default) and a mock
    server will be used.
2.  **Run tests**:
    ```bash
    go test -v ./...
    ```
3.  **View the report**:
    ```bash
    cat weather_test_report.json
    ```

---

### 🐳 How to Run in Docker

1.  **Build the Docker image** from the root of the monorepo:
    ```bash
    docker build -t go-api-tests -f go-api-tests/Dockerfile .
    ```
2.  **Run the tests inside the container, passing the API key as an environment variable**:
    ```bash
    docker run --rm -e OWM_API_KEY="your-api-key-here" go-api-tests
    ```

---

### ⚙️ CI Workflow

- See `.github/workflows/go-ci.yml` for the complete GitHub Actions workflow.
- The CI pipeline runs tests, checks formatting, and performs a CodeQL security scan.

### 🤝 Integration with Pact Contract Tests

The root `scripts/run_go_to_pact.js` script demonstrates using the generated
`weather_test_report.json` to trigger the Pact consumer and provider tests.
Because it's written in Node.js, the flow works on Windows as well as macOS/Linux.
This mirrors a pipeline step where functional API checks gate contract verification.

---

### 📄 License

This project is licensed under the [MIT License](../LICENSE).
