# Performance Testing with k6

This project demonstrates how to perform automated performance testing as part of a CI/CD pipeline using [k6](https://k6.io/), a modern, developer-friendly load testing tool from Grafana Labs.

Regular load testing protects your brand by ensuring critical endpoints remain responsive as traffic grows.
---

### When to Use k6

k6 is a powerful tool for performance testing that is designed with developers and automation in mind. It excels in a "performance-as-code" approach.

* **CI/CD Integration**: k6 is ideal for running performance smoke tests and load tests directly within your CI/CD pipeline. It can enforce performance budgets using "Thresholds," failing the build if the system's performance degrades.
* **Developer-Friendly Scripting**: Tests are written in JavaScript, a language familiar to many developers, making it easy to write, version-control, and maintain test scripts.
* **Goal-Oriented Testing**: It is excellent for load, stress, and spike testing where you need to simulate realistic traffic patterns using its `stages` configuration.

### Strategic Advantage
- Lightweight scripting keeps performance checks close to the developers.
- Thresholds turn performance metrics into go/no-go business decisions.
- Additional context in [Architectural Principles](../ARCHITECTURAL_PRINCIPLES.md).

### Limitations
While k6 excels at codifying performance expectations, it cannot guarantee real-user conditions. Metrics should be tied to SLOs to avoid chasing unrealistic numbers, and very high loads may require distributed runners.

### Similar Tooling in Other Languages
* **JMeter**: A long-standing, powerful, UI-driven tool from Apache. It is written in Java and is excellent for complex protocols but can be more difficult to integrate into a "performance-as-code" workflow.
* **Gatling**: A strong, code-based competitor written in Scala. It is known for its high performance and detailed HTML reports.
* **Locust**: A popular performance testing tool where test scenarios are written in Python.

### Installation and Running

**Prerequisites:**
* [Install k6](https://k6.io/docs/getting-started/installation/)
* (Optional) Docker

k6 only needs to be installed locally if you choose not to use Docker. The
container image contains the k6 runtime.

#### 1. Local Machine (Windows/macOS/Linux)

1.  **Navigate to the project directory**:
    ```bash
    cd k6-performance-tests
    ```
2.  **Run the test from the command line**:
    ```bash
    k6 run smoke-test.js
    ```

#### 2. Docker

1.  **Run the official k6 Docker image**, mounting the local script directory into the container:
    ```bash
    # Make sure you are in the root of the monorepo
    docker run --rm -i grafana/k6 run - <k6-performance-tests/smoke-test.js
    ```
### Viewing the Report

This test script automatically generates `summary.html` when the run completes. Open that file in your browser to view the performance results. When executed via the GitHub Actions workflow (`.github/workflows/k6-ci.yml`), this HTML report is uploaded as a workflow artifact for easy access.
You can retrieve it from the "Artifacts" section of a workflow run by downloading the `k6-summary` artifact.

## Secret Management

Any API tokens or credentials should be provided via environment variables or a
vault such as **Azure Key Vault** or **HashiCorp Vault**. GitHub Secrets feed
these values into the CI pipeline. For a complete example, refer to the
"Configure API Credentials" section of
[csharp-specflow-api-tests/README.md](../csharp-specflow-api-tests/README.md).

## Client Scenarios

- Integrating k6 thresholds into CI alerted a retail client to a memory leak before launch, avoiding an estimated **€10k per month** in lost sales due to slow pages.
- Regular load tests provide quantifiable service level metrics so leadership can plan infrastructure costs with confidence.
