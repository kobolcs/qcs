# The QA Consultant Suite

Welcome to my QA Consultant Suite, a curated collection of projects demonstrating a multi-disciplinary approach to modern software quality and test automation. Each project is a self-contained showcase of specific technologies, methodologies, and best practices, designed to tackle real-world quality challenges.

## Guiding Principles

This repository is built on a few core principles:

-   **Pragmatism Over Dogma**: Choosing the right tool for the job, whether it's a mainstream framework like Playwright or a nuanced tool like Robot Framework, and demonstrating how to overcome its perceived limitations.
-   **Architecture & Maintainability**: Writing tests is easy; writing a scalable, maintainable, and robust test architecture is hard. These projects emphasize clean code, dependency injection, and clear separation of concerns.
-   **Full-Spectrum Quality**: Quality is not just about testing. It encompasses CI/CD integration, security scanning (SAST), performance analysis, accessibility (a11y), and clear, actionable reporting.
-   **Automation at All Levels**: Demonstrating proficiency across the entire testing pyramid, from unit tests of helper logic (Pytest, JUnit) to complex end-to-end tests for event-driven systems.

---

## Technology & Skills Showcase

The following table serves as a guide to the skills and technologies demonstrated in each project within this suite.

| Project Directory                                           | Primary Technologies                    | Key Concepts Demonstrated                                                                                                                   |
| :---------------------------------------------------------- | :-------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------ |
| `csharp-specflow-api-tests`                | C#, SpecFlow, .NET, NUnit               | BDD, API Testing, Dependency Injection, Structured Logging, CI/CD with Living Documentation.                                    |
| `java-event-driven-tests`                      | Java, Spring Boot, Testcontainers       | Testing for Microservices, Event-Driven Architecture (Kafka), Asynchronous Flows, Containerized Test Dependencies (Redis).        |
| `robot-framework-python-tests`               | Robot Framework, Python, Pytest         | Hybrid Test Frameworks, Mitigating Common RF Anti-Patterns, Custom Keyword Libraries, Page Object Model (POM), Parallel Execution (Pabot). |
| `playwright-ts-api-test`                | TypeScript, Playwright, Node.js | Modern Web Testing, API Interception (`page.route`), Hybrid UI/API tests, Visual Regression, Accessibility (a11y) Testing.  |
| `go-api-tests`                                   | Go (Golang)                             | High-Performance API Testing, Concurrency/Parallelism, Custom Reporting, Resiliency Patterns (Retries, Timeouts), Configuration Management. |
| `pact-contract-testing`                    | Node.js, Pact, Jest, Express       | Consumer-Driven Contract Testing, CI/CD integration for contract verification, API mocking, and provider state management.          |
| `k6-performance-tests`                          | k6, JavaScript                          | Performance Testing as Code, Service Level Objectives (SLOs), Thresholds, CI/CD-integrated performance checks.                   |
| `elixir-api-tests`                                 | Elixir, ExUnit, StreamData | Property-Based Testing, Concurrency with the BEAM VM, Pattern Matching for concise assertions, Custom HTML Reporting. |
| `prototypes/ai-test-observability`                             | Python, Scikit-learn, Streamlit         | *(Experimental)* Applying AI/ML to QA, Flaky Test Detection (Classification), Failure Clustering (NLP), Building Data-Driven Dashboards.       |
| `prototypes/blockchain-smart-contracts-tests` | Solidity, Hardhat, Ethers.js            | *(Experimental)* Web3 Quality, Smart Contract Auditing, Security Vulnerability Testing (Reentrancy & Overflow), Gas Usage Assertions with Hardhat Gas Reporter, Testing on a Local Blockchain. |

## Makefile Usage

The repository includes a top-level `Makefile` to simplify running tests and
linting checks across the various sub-projects. Each target focuses on a single
technology stack, while aggregate targets help verify everything at once.

- Run all test suites (C#, Java, Robot Framework, Playwright, Go, Elixir, Pact, k6, and mobile Appium) with:

```bash
make test-all
```

- Run only the mobile Appium suite with:

```bash
make test-mobile
```

- Run all available linters with:

```bash
make lint-all
```

Refer to `make help` for a full list of supported targets.

## Docker Usage

A `Dockerfile` and `docker-compose.yml` are provided to run the entire suite in a consistent environment. Build the image and execute all tests with:

```bash
docker-compose up --build
```

This will run `make install-all` followed by `make test-all` inside the container.

## Cross-Project Integration Example

An example workflow ties the Go API tests to the Pact contract suite. Running
the `test-go-pact` Makefile target executes the Go tests first and, if they
produce the `weather_test_report.json` artifact, automatically kicks off the
consumer and provider Pact tests. A Node.js helper script is used so the flow
works the same on Windows and Unix systems. This demonstrates how functional
checks can gate contract verification in a real CI/CD pipeline.

## Code Style

Refer to [CODE_STYLE.md](CODE_STYLE.md) for the coding conventions used across
this repository. Following consistent style guides helps maintain readability
as projects span multiple languages.
