# The QA Consultant Suite

The QA Consultant Suite demonstrates how disciplined automation drives measurable value for stakeholders. By applying scalable patterns across multiple technologies, the examples reduce manual regression effort by **50% or more** and surface defects before they become costly production issues. Parallel execution shortens feedback loops from hours to minutes, delivering faster releases with less risk. For a high-level snapshot of every project and its CI status, see the **[Portfolio Dashboard](dashboard.md)**.

## How I Can Help Your Business

The suite demonstrates a consulting approach that ties technical excellence directly to business outcomes. Whether you need faster feedback cycles, reduced cloud spend, or improved customer satisfaction, these patterns scale to meet those goals.

- **Unified Quality Strategy**: By leveraging the right tool for each layer of the testing pyramid, clients typically reduce regression costs by 50% or more.
- **Actionable Metrics**: Every project generates machine-readable reports so leadership can see return on investment after each run.
- **Proactive Risk Reduction**: Integrating security, performance, and contract checks prevents issues that can result in thousands of euros in lost revenue or engineer rework.

## Guiding Principles

This repository is built on a few core principles:

-   **Pragmatism Over Dogma**: Choosing the right tool for the job, whether it's a mainstream framework like Playwright or a nuanced tool like Robot Framework, and demonstrating how to overcome its perceived limitations.
-   **Architecture & Maintainability**: Writing tests is easy; writing a scalable, maintainable, and robust test architecture is hard. These projects emphasize clean code, dependency injection, and clear separation of concerns.
-   **Full-Spectrum Quality**: Quality is not just about testing. It encompasses CI/CD integration, security scanning (SAST), performance analysis, accessibility (a11y), and clear, actionable reporting.
-   **Automation at All Levels**: Demonstrating proficiency across the entire testing pyramid, from unit tests of helper logic (Pytest, JUnit) to complex end-to-end tests for event-driven systems.

---

## Technology & Skills Showcase

The following table serves as a guide to the skills and technologies demonstrated in each project within this suite.

| Project Directory | Primary Technologies | Key Concepts Demonstrated | Live Report |
| :--- | :--- | :--- | :--- |
| `csharp-specflow-api-tests` | C#, SpecFlow, .NET, NUnit | BDD, API Testing, Dependency Injection, Structured Logging, CI/CD with Living Documentation. | `LivingDoc.html` artifact |
| `java-event-driven-tests` | Java, Spring Boot, Testcontainers | Testing for Microservices, Event-Driven Architecture (Kafka), Asynchronous Flows, Containerized Test Dependencies (Redis). | Surefire HTML reports |
| `robot-framework-python-tests` | Robot Framework, Python, Pytest | Hybrid frameworks, mitigated RF anti-patterns, custom keywords, POM, parallel execution. | `allure-report` artifact |
| `playwright-ts-api-test` | TypeScript, Playwright, Node.js | Modern web testing, API interception (`page.route`), hybrid UI/API tests, visual regression, a11y checks. | `playwright-report` HTML |
| `go-api-tests` | Go (Golang) | High-performance API testing, concurrency, custom reporting, resiliency patterns. | `weather_test_report.json` |
| `pact-contract-testing` | Node.js, Pact, Jest, Express | Consumer-driven contract testing with CI/CD, API mocking, provider state management. | Pact logs & HTML |
| `k6-performance-tests` | k6, JavaScript | Performance testing as code with SLOs and thresholds, CI/CD integration. | `summary.html` |
| `elixir-api-tests` | Elixir, ExUnit, StreamData | Property-based testing, concurrency on the BEAM VM, custom HTML reporting. | `elixir-reports` HTML |
| `prototypes/ai-test-observability` | Python, Scikit-learn, Streamlit | *(Experimental)* AI/ML for flaky test detection and failure clustering. | Streamlit dashboard |
| `prototypes/blockchain-smart-contracts-tests` | Solidity, Hardhat, Ethers.js | *(Experimental)* Web3 quality, smart contract auditing, gas usage assertions. | Gas/coverage reports |

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

Docker is **the recommended way** to run this repository. A `Dockerfile` and `docker-compose.yml` are provided so you can execute the entire suite in a consistent environment without installing every tool locally. Build the image and execute all tests with:

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
For an overview of design patterns used across all projects, see [ARCHITECTURAL_PRINCIPLES.md](ARCHITECTURAL_PRINCIPLES.md).

## Engage with Me

If you're looking to accelerate your own testing strategy or have questions about the approaches shown here, let's talk. Connect with me on [LinkedIn](https://www.linkedin.com/) or visit [my website](https://example.com) to start the conversation.
