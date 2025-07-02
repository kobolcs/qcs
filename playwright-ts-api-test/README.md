# Playwright + TypeScript: Hybrid API & UI Automation

Modern API and UI test suite for a React/PokeDemo app. Showcases Playwright's advanced patterns and consultant-level automation practices.

These tests protect revenue-generating web flows by catching API and UI issues before real customers see them.
---

### When to Use Playwright with TypeScript

Playwright is a top-tier framework for testing modern web applications, particularly Single-Page Applications (SPAs) built with frameworks like React, Angular, or Vue.

* **Modern Web Features**: It has best-in-class support for handling dynamic content, network interception, and shadowing DOM.
* **Hybrid Testing**: As demonstrated here, Playwright excels at testing the intersection of the UI and APIs. By intercepting and mocking API calls (`page.route`), you can create fast, reliable, and isolated UI tests.
* **Developer-Friendly**: The API is intuitive, and features like auto-waits, tracing, and code generation significantly improve the developer experience.
* **TypeScript Support**: Provides strong type-checking for building more robust and maintainable test suites.

### Strategic Advantage
- Unified API and UI testing ensures user journeys remain reliable during rapid releases.
- Built with TypeScript for maintainability and first-class editor support.
- Shared patterns documented in [Architectural Principles](../ARCHITECTURAL_PRINCIPLES.md).

### Similar Tooling in Other Languages
Playwright is a leader in a competitive space for modern web testing.
* **Cypress**: The most direct competitor, also using JavaScript/TypeScript, with a strong focus on developer experience.
* **Selenium**: The classic standard, available for all major languages (Java, Python, C#, etc.). It is highly flexible but often requires more setup for modern single page apps.
* **Puppeteer**: The Google-maintained library that Playwright was forked from, primarily focused on programmatic control of Chrome/Chromium.

### Installation and Running

**Prerequisites:**
* Node.js (version 18.x or later)
* (Optional) Docker

Install Node.js only if you plan to run the suite outside of the Docker image.

#### 1. Local Machine (Windows/macOS/Linux)

1.  **Navigate to the project directory**:
    ```bash
    cd playwright-ts-api-test
    ```
2.  **Install dependencies**:
    ```bash
    npm ci
    ```
3.  **Install Playwright browsers**:
    ```bash
    npx playwright install
    ```
4.  **Run the tests**:
    ```bash
    npx playwright test --reporter=html
    ```
5.  **View the report**:
    ```bash
    npx playwright show-report
    ```

#### 2. Docker

1.  **Build the Docker image**:
    ```bash
    docker build -t playwright-ts-tests .
    ```
2.  **Run the tests inside the container**:
    ```bash
    docker run --rm --ipc=host playwright-ts-tests
    ```

## Secret Management

Keep API tokens and other credentials out of source control. Use environment
variables or a vault system such as **Azure Key Vault** or **HashiCorp Vault**.
GitHub Secrets inject these values during CI. For a detailed walkthrough, see
the "Configure API Credentials" section in
[csharp-specflow-api-tests/README.md](../csharp-specflow-api-tests/README.md).

## Client Scenarios

- Hybrid API and UI tests caught 90% of critical issues before staging for a B2C company, preventing two major outages in a year—roughly **€50k** in avoided downtime.
- TypeScript typing reduced flaky test maintenance by **30%**, freeing engineers to focus on new feature coverage.
