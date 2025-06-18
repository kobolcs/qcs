# C# SpecFlow API Test Suite

This project demonstrates a professional, BDD-style API testing framework using C#, SpecFlow, and NUnit. It is designed to showcase best practices in .NET test automation, including dependency injection, structured logging, and CI/CD integration using the modern .NET 9 framework.

This suite turns business requirements into executable specifications, ensuring everyone shares the same understanding of the API contract.
---

### When to Use C# and SpecFlow

This technology stack is a premier choice for teams working within the .NET ecosystem, particularly in enterprise environments where collaboration between technical and non-technical stakeholders is key.

* **Behavior-Driven Development (BDD)**: SpecFlow allows tests to be written in the human-readable Gherkin language. This enables business analysts, product owners, and manual testers to read and even contribute to the automated test specifications.
* **Enterprise Integration**: As a core part of the .NET ecosystem, it integrates seamlessly with Visual Studio, Azure DevOps, and other Microsoft technologies.
* **Strong Typing and Tooling**: C# provides the benefits of a statically-typed language, catching errors at compile-time and offering excellent IDE support for refactoring and building robust, maintainable test frameworks.

### Similar Tooling in Other Languages

The BDD-style of testing shown here is a popular, cross-language pattern.
* **Java**: `Java + Cucumber` is the direct equivalent. `Serenity BDD` is a higher-level framework that often uses Cucumber for its core functionality.
* **Python**: `Python + Behave` or `Python + pytest-bdd`.
* **JavaScript/TypeScript**: `JavaScript/TypeScript + Cucumber.js`.
### Strategic Advantage
- Bridges communication gaps between developers and business teams.
- Fits seamlessly into existing .NET pipelines for rapid feedback.
- See [Architectural Principles](../ARCHITECTURAL_PRINCIPLES.md) for cross-project design guidelines.


---

## Getting Started: Installation and Running

### Prerequisites

* **.NET 9.0 SDK** (Download from [microsoft.com/net/download](https://dotnet.microsoft.com/download))
* **(Optional) Docker** for running tests in a containerized environment.

### Method 1: Running on Your Local Machine

Follow these steps to configure and run the tests directly on your machine.

**1. Clone the Repository & Restore Dependencies**

```bash
# Navigate to your development folder
git clone <your-repository-url>
cd qa-consultant-suite/csharp-specflow-api-tests

# Restore all NuGet packages
dotnet restore
```

**2. Configure API Credentials (CRITICAL STEP)**

In any professional project, it is crucial to **never** hardcode sensitive information like passwords, API keys, or connection strings directly in the source code (e.g., in `.cs` or `appsettings.json` files).

**Why?**
* **Security:** Committing secrets to a Git repository—even a private one—exposes them to everyone with access to the codebase.
* **Flexibility:** It makes it difficult to use different credentials for different environments (Development vs. Production).
* **Risk of Leaks:** It is a common mistake to accidentally push a real production key to a public repository.

To solve this for local development, this project uses the **.NET Secret Manager**. This tool stores sensitive data in a separate JSON file on your local machine, completely outside of the project directory, ensuring it is never accidentally committed to Git. The application's configuration will automatically read from these User Secrets and merge them with the non-sensitive settings from `appsettings.json`.

For client or production scenarios, consider integrating with a secure vault such as **Azure Key Vault** or **HashiCorp Vault**. These services provide centralized secret management and auditing capabilities that go beyond the basic User Secrets mechanism.

* **Initialize the secret store** for this project (a one-time command):
    ```bash
    dotnet user-secrets init
    ```
* **Set the required API password**.
    ```bash
    dotnet user-secrets set "ApiSettings:Password" "password123"
    ```
> **Security Note:** For this specific public-facing demo API, the required password is `password123`, as specified in its official documentation.
>
> It is critical to understand that this is **not** standard practice. In a real-world application, you must **never** expose passwords in documentation or source code. Always use strong, unique passwords managed securely through a secrets manager or vault.

**3. Run the Tests**

Now that the project is configured, you can run the tests.

```bash
dotnet test
```

A log file will be created in `csharp-specflow-api-tests/logs/` and a `LivingDoc.html` report will be generated in `csharp-specflow-api-tests/SpecFlowApiTests/Reports/`.

### Method 2: Running with Docker

The included `Dockerfile` allows you to build and run the tests inside a self-contained environment.

**NOTE:** The Docker build uses the public password for this demo service. In a real-world CI/CD pipeline, secrets should be passed in securely at runtime.

1.  **Build the Docker image**:
    ```bash
    docker build -t csharp-specflow-tests .
    ```
2.  **Run the tests inside the container**:
    ```bash
    docker run --rm csharp-specflow-tests
    ```
