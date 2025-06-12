# Go API Tests: Parallel, Table-Driven API Validation

**Modern, idiomatic Go API testing with full parallelization, retries, and reporting.** This project showcases advanced patterns used in senior-level test automation.

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

### 🚀 How to Run Locally

1.  **Set your API key** from a service like [OpenWeatherMap](https://openweathermap.org/api):
    ```bash
    export OWM_API_KEY="your-api-key-here"
    ```
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

---

### 📄 License

This project is licensed under the [MIT License](../LICENSE).
