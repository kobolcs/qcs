# Robot Framework + Python: A Hybrid Approach

This project demonstrates how to build a powerful, maintainable, and modern test automation framework by combining the declarative, keyword-driven syntax of Robot Framework with the flexibility and power of Python. It specifically showcases how to overcome common criticisms of Robot Framework by moving complex logic into Python helper libraries and API clients.

---

### When to Use Robot Framework with Python

This hybrid approach is particularly effective in environments with mixed-skill teams or when you need to balance readability with powerful backend integration.

* **High-Level Readability**: The `.robot` files provide a clear, high-level view of the test flow, making them accessible to manual QAs, BAs, and product owners.
* **Flexible Syntax**: Supports both traditional **keyword-driven** tests and **BDD-style** (Given/When/Then) tests, catering to different team preferences and improving collaboration.
* **Python for Power**: By writing custom keywords in Python, you can handle complex API interactions, database connections, intricate logic, and integrate with any Python library, removing all limitations of traditional Robot Framework setups.
* **Separation of Concerns**: This model creates a clean separation between the "what" (the test case steps in `.robot` files) and the "how" (the implementation details in `.py` files).
* **Extensible Ecosystem**: Leverages Robot Framework's rich ecosystem of existing libraries (SeleniumLibrary, Browser, etc.) while allowing for limitless custom extension through Python.

### Similar Tooling in Other Languages

This pattern of a BDD/keyword-driven layer on top of a general-purpose programming language is common.
* **C#**: `SpecFlow` or `BDDfy` on top of C#.
* **Java**: `Cucumber` or `JBehave` on top of Java.
* **JavaScript/TypeScript**: `Cucumber.js` or `Gauge` on top of Node.js.

### Installation and Running

**Prerequisites:**
* Python (version 3.10 or later)
* (Optional) Docker

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

---
### Addressing Common Criticisms

For a detailed breakdown of how this framework's architecture mitigates common complaints about Robot Framework, please see the **[Robot Framework Alleviation Guide](./robot_framework_alleviation.md)**.
