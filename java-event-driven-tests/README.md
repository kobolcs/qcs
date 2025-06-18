# Java Event-Driven Tests: Kafka, Redis, Testcontainers

Integration suite for event-driven systems with full containerized infrastructure. Demonstrates advanced Java testing practices.

By recreating realistic infrastructure on demand, this project proves your services integrate correctly before they hit staging.
---

### When to Use Java and Testcontainers

This stack is ideal for testing complex, enterprise-grade backend systems, especially those built on a microservices or event-driven architecture.

* **Robust Integration Testing**: Modern applications rely on external dependencies like databases, message brokers, and caches. Testcontainers allows you to spin up these dependencies (e.g., Kafka, Redis) in Docker containers for each test run. This provides a high-fidelity testing environment without the cost and instability of a shared, persistent test environment.
* **Event-Driven Architectures**: As shown in this project, this approach is perfect for verifying asynchronous workflows where services communicate via message queues like Kafka. You can test that a message produced by one service is correctly consumed and processed by another.
* **Enterprise Ecosystem**: Java, combined with frameworks like Spring Boot, remains a dominant force in large-scale enterprise applications, making this a critical skill for testing in that domain.

### Strategic Advantage
- Validates message flows end-to-end without a costly staging environment.
- Containers spin up on demand, keeping pipelines both fast and isolated.
- See [Architectural Principles](../ARCHITECTURAL_PRINCIPLES.md) for the patterns reused across the suite.

### Similar Tooling in Other Languages
The **Testcontainers** pattern of managing ephemeral test dependencies in code is a cross-language standard.
* **.NET (C#)**: `Testcontainers for .NET` provides the exact same functionality.
* **Go**: `Testcontainers for Go` allows for the same style of testing.
* **Python**: `testcontainers-python` is the Python equivalent.
* **Node.js, Rust, and more**: Nearly every major language has a Testcontainers library, making this a universally valuable skill.

### Installation and Running

**Prerequisites:**
* Java Development Kit (JDK) 17 or later
* Apache Maven
* Docker Desktop (for Testcontainers)

#### 1. Local Machine (Windows/macOS/Linux)

This project uses Testcontainers, which will automatically start and manage the required Docker containers (Kafka, Redis) for you.

1.  **Ensure Docker Desktop is running.**
2.  **Navigate to the project directory**:
    ```bash
    cd java-event-driven-tests
    ```
3.  **Build the project and run the tests using Maven**:
    ```bash
    mvn clean install -DsuiteXmlFile=testng-integration-suite.xml
    ```
    Test reports are generated in the `target/surefire-reports` directory.

#### 2. Docker (via Docker Compose for dependencies)

The `docker-compose.yml` file is provided to manually run the dependencies if you wish to connect a running application to them, but the tests themselves are designed to manage their own containers.

