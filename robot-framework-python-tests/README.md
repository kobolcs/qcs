# Robot Framework + Python: A Hybrid Approach

This project demonstrates how to build a powerful, maintainable, and modern test automation framework by combining the declarative, keyword-driven syntax of Robot Framework with the flexibility and power of Python. It specifically showcases how to overcome common criticisms of Robot Framework by moving complex logic into Python helper libraries and API clients.

The goal is to let business analysts define tests in plain language while engineers maintain reusable Python utilities behind the scenes.
---

### When to Use Robot Framework with Python

This hybrid approach is particularly effective in environments with mixed-skill teams or when you need to balance readability with powerful backend integration.

* **High-Level Readability**: The `.robot` files provide a clear, high-level view of the test flow, making them accessible to manual QAs, BAs, and product owners.
* **Flexible Syntax**: Supports both traditional **keyword-driven** tests and **BDD-style** (Given/When/Then) tests, catering to different team preferences and improving collaboration.
* **Python for Power**: By writing custom keywords in Python, you can handle complex API interactions, database connections, intricate logic, and integrate with any Python library, removing all limitations of traditional Robot Framework setups.
* **Separation of Concerns**: This model creates a clean separation between the "what" (the test case steps in `.robot` files) and the "how" (the implementation details in `.py` files).
* **Extensible Ecosystem**: Leverages Robot Framework's rich ecosystem of existing libraries (SeleniumLibrary, Browser, etc.) while allowing for limitless custom extension through Python.

### Strategic Advantage
- Keeps test steps business readable while Python handles the heavy lifting.
- Modular keywords and helpers grow with your application over time.
- For design patterns see [Architectural Principles](../ARCHITECTURAL_PRINCIPLES.md).

### Similar Tooling in Other Languages

### Testing Paradigms: Keyword-Driven vs. Layered BDD

Pure keyword-driven tests often appear "robotic" due to their rigid, step-by-step nature, characteristic of tools like RPA. In contrast, a common and flexible approach is to combine a BDD or keyword-driven syntax with the full power of a general-purpose programming language.

### Pure Keyword-Driven Tools (like Robot Framework)
These tools enable test creation primarily by assembling predefined keywords or visual components, minimizing or removing direct code writing for test logic:

* HP UFT (Unified Functional Testing) / Micro Focus UFT One
* TestComplete (SmartBear) visual
* Leapwork 
* WorkFusion (RPA)
* UiPath (RPA)
* Automation Anywhere (RPA)
* Testsigma (scriptless modes)

### Behavior-driven development/keyword-driven
The pure keyword driven tests are Robotic are mainly RPA or visual tools, but the pattern of a BDD/keyword-driven layer on top of a general-purpose programming language is common:
* **C#**: `SpecFlow` or `BDDfy` on top of C#.
* **Java**: `Cucumber` or `JBehave` on top of Java.
* **JavaScript/TypeScript**: `Cucumber.js` or `Gauge` on top of Node.js.

### Installation and Running

**Prerequisites:**
* Python (version 3.10 or later)
* (Optional) Docker

Install Python only if you plan to run the suite locally without Docker.

#### 1. Local Machine (Windows/macOS/Linux)

1.  **Navigate to the project directory**:
    ```bash
    cd robot-framework-python-tests
    ```
2.  **Install dependencies**:
    ```bash
    pip install -r requirements.txt
    pip install -r requirements-dev.txt
    ```
3.  **Custom CSV Keyword**: The suite uses a small helper library
    `helpers/csv_keywords.py` that provides the `Read CSV File To List`
    keyword. No additional packages are required.
4.  **Run the tests using `pabot`** (for parallel execution):
    ```bash
    pabot --processes 4 --outputdir reports tests/api_tests
    ```
    The unit tests default to mocked HTTP responses. Set `USE_LIVE_SPACEX=1` to
    hit the real SpaceX API when running `pytest`.
5.  **View the generated report**: Open `robot-framework-python-tests/reports/log.html` in your browser.

#### 2. Docker

A `Dockerfile` is already included for containerized execution.

1. **Build the Docker image**:
   ```bash
   docker build -t robot-tests .
   ```
2. **Run the tests using Docker**:
    ```bash
    docker run --rm robot-tests
    ```

## Secret Management

Configure secrets like API tokens using environment variables or a vault such as
**Azure Key Vault** or **HashiCorp Vault**. GitHub Secrets supply these values
in CI. For a full example, see the "Configure API Credentials" section in
[csharp-specflow-api-tests/README.md](../csharp-specflow-api-tests/README.md).

---

## Client Scenarios

- This hybrid approach cut script maintenance by **30%** for a telecom client, enabling thousands of tests without hiring extra QA engineers—about **€60k annually** saved.
- Non-technical stakeholders wrote high-level `.robot` scenarios, freeing engineers to focus on Python libraries and reducing defect triage time by 20%.

### Addressing Common Criticisms

For a detailed breakdown of how this framework's architecture mitigates common complaints about Robot Framework, please see the **[Robot Framework Alleviation Guide](./robot_framework_alleviation.md)**.
