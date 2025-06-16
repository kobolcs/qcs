# Consumer-Driven Contract Testing with Pact

This project demonstrates consumer-driven contract testing using **Pact**. It ensures that a service consumer (the `consumer-frontend`) and a service provider (the `provider-api`) can evolve independently without breaking their integration contract.

---

### When to Use Pact for Contract Testing

Pact is essential for teams building systems with a microservices architecture. It solves the problem of ensuring that services can communicate with each other reliably without the slowness and brittleness of full end-to-end integration tests.

* **Decoupled Development**: It gives consumer and provider teams the confidence to develop and deploy their services independently, knowing that their integration contract is being honored.
* **Fast, Reliable Feedback**: Consumer tests run against a local mock, making them extremely fast and reliable. Provider verification tests are highly specific and pinpoint exact contract violations.
* **Preventing Integration Failures**: By catching breaking changes in the CI/CD pipeline before they are deployed, Pact prevents common and costly integration failures in production environments.

### Similar Tooling in Other Languages

* **Pact is Polyglot**: The primary strength of Pact is that it is the industry standard and has client libraries for nearly every major language (Java, .NET, JS/TS, Go, Python, Ruby, etc.). The "similar pairing" is simply `Pact + [Your Language]`.
* **Schema-Based Testing**: The main alternative to consumer-driven contract testing is schema-based testing (e.g., using Avro, Protobuf, or OpenAPI/Swagger schemas). This is a different philosophy where a central schema defines the contract, rather than the consumer's expectations.

### Installation and Running

**Prerequisites:**
* Node.js (version 18.x or later)
* (Optional) Docker with Docker Compose

#### 1. Local Machine (Windows/macOS/Linux)

1.  **Navigate to the consumer directory and run its tests**. This step generates the pact file (the contract).
    ```bash
    cd pact-contract-testing/consumer-frontend
    npm install
    npm test
    ```
2.  **Navigate to the provider directory and run its tests**. This step verifies the generated pact against the real provider API.
    ```bash
    cd ../provider-api
    npm install
    npm test
    ```

#### 2. Docker

The entire workflow can be simulated via the CI pipeline configuration. The `pact-ci.yml` workflow automates the steps of running the consumer tests first, followed by the provider verification.

### 🤝 Integration with Go API Tests

The repository root includes a `test-go-pact` Makefile target (and accompanying
`scripts/run_go_to_pact.js`) that runs the Go API tests and then triggers these
Pact tests. Using Node.js ensures the same workflow works on Windows as well as
macOS/Linux systems. This example illustrates how contract verification can be
gated by prior functional testing.
