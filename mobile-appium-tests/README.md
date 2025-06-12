# Mobile Testing with Appium & Python

This project demonstrates professional mobile test automation using **Appium** with the **Appium-Python-Client**. It showcases how to structure a mobile testing framework for running tests against native Android applications on emulators.

---

### When to Use Appium

Appium is the de-facto open-source standard for mobile test automation. It is the right choice when you need to:

* **Test Native or Hybrid Apps**: Appium can automate native (Android/iOS), hybrid, and mobile web applications.
* **Use a Familiar Language**: It provides client libraries for virtually all major programming languages (Python, Java, C#, JavaScript, etc.), allowing teams to write tests in a language they already know.
* **Run on Real Devices and Emulators**: Tests written with Appium can be executed on both real mobile devices and emulators/simulators.
* **Leverage Existing Standards**: Appium uses the W3C WebDriver protocol, making it familiar to anyone with Selenium experience.

### Similar Tooling

* **Native Frameworks**: `Espresso` (for Android) and `XCUITest` (for iOS) are the official testing frameworks from Google and Apple. They are often faster but are platform-specific and require writing tests in Java/Kotlin or Swift.
* **Modern Alternatives**: `Maestro` and `Detox` are newer frameworks that aim to simplify the setup and flakiness often associated with Appium, but Appium remains the most versatile and widely-used tool.

### Installation and Running (Local)

**Prerequisites:**

1.  **Python 3.10+**
2.  **Node.js and npm** (to install Appium Server)
3.  **Java Development Kit (JDK)** (version 11 or higher)
4.  **Android Studio** installed with the Android SDK and an Android Virtual Device (Emulator) created.
5.  **Set `ANDROID_HOME` environment variable** pointing to your Android SDK location.

**Steps:**

1.  **Install Appium Server**:
    ```bash
    npm install -g appium
    ```
2.  **Install Appium Drivers**:
    ```bash
    appium driver install uiautomator2
    ```
3.  **Start the Appium Server** in a separate terminal:
    ```bash
    appium
    ```
4.  **Navigate to the project directory**:
    ```bash
    cd mobile-appium-tests
    ```
5.  **Install Python dependencies**:
    ```bash
    pip install -r requirements.txt
    ```
6.  **Run the tests** (ensure your Android emulator is running):
    ```bash
    pytest
    ```

---

### Docker

A `Dockerfile` is included to run the tests in a fully isolated environment.

1. **Build the Docker image** from the root of the repository:
   ```bash
   docker build -t appium-tests -f mobile-appium-tests/Dockerfile .
   ```
2. **Execute the test suite** (the emulator and Appium server start automatically):
   ```bash
   docker run --rm appium-tests
   ```

---

### CI Workflow

The GitHub Actions workflow in `.github/workflows/mobile-ci.yml` fully automates this process. It uses the `reactivecircus/android-emulator-runner` action to start an emulator, install the app, and run the tests.
