# Elixir API Tests: Property-Based and Async Validation

Modern Elixir suite for validating API data with property-based and pattern-matching tests, async-safe.

---
### When to Use Elixir for Testing

Elixir, running on the Erlang VM (BEAM), is exceptionally suited for testing systems that are highly concurrent, distributed, and require high fault tolerance.

* **Concurrency and Load Testing**: Elixir's lightweight processes make it trivial to simulate thousands of concurrent users or connections to an API, making it a powerful tool for load testing from within the language itself.
* **Testing Asynchronous Systems**: The actor model and message-passing architecture make it natural to test complex, asynchronous workflows without the callback complexity found in other languages.
* **Property-Based Testing**: As shown with `StreamData`, Elixir has strong support for property-based testing, which is excellent for finding edge cases in data processing and validation logic by generating a wide range of inputs.
* **Pattern Matching**: Elixir's pattern matching allows for incredibly concise and expressive assertions, especially when validating complex JSON response structures.

### Similar Tooling in Other Languages

The concepts demonstrated here can be found in other ecosystems:
* **Property-Based Testing**: `Hypothesis` in Python, `FsCheck` in C#/.NET, and `QuickCheck` in Haskell pioneered this testing style.
* **Concurrency Model**: The Actor Model used by Elixir is similar to that found in `Akka` for the JVM (Java/Scala).

### Installation and Running

**Prerequisites:**
* [Install Elixir](https://elixir-lang.org/install.html) (version 1.15+ with OTP 26+)
* (Optional) Docker

#### 1. Local Machine (Windows/macOS/Linux)

1.  **Navigate to the project directory**:
    ```bash
    cd elixir-api-tests
    ```
2.  **Install dependencies**:
    * **Windows (Command Prompt/PowerShell)**:
        ```cmd
        mix deps.get
        ```
    * **macOS/Linux**:
        ```bash
        mix deps.get
        ```
3.  **Run tests**:
    ```bash
    mix test
    ```
    A `test_report.html` file will be generated in the `reports` directory. By
    default the tests mock the Rest Countries API. Set
    `USE_LIVE_REST_COUNTRIES=1` to exercise the real service.

#### 2. Docker

1.  **Build the Docker image**:
    ```bash
    docker build -t elixir-api-tests .
    ```
2.  **Run the tests inside the container**:
    ```bash
    docker run --rm elixir-api-tests
    ```
