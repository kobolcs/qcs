# Makefile for the QA Consultant Suite
# This Makefile provides a unified interface to install, test, and lint all projects.
# It uses virtual environments for Python projects and includes targets for
# code coverage, security scanning, and comprehensive reporting.

SHELL := /bin/bash

# Define paths for Python virtual environments
ROBOT_VENV := robot-framework-python-tests/venv
AI_VENV := ai-test-observability/venv
MOBILE_VENV := mobile-appium-tests/venv

# ==============================================================================
# Help Target
# ==============================================================================
.PHONY: help
help:
	@echo "Usage: make [target]"
	@echo ""
	@echo "---------------------------------------------------------------------"
	@echo " Primary Setup Command"
	@echo "---------------------------------------------------------------------"
	@echo "  install-env           Checks prerequisites and installs all dependencies for all projects."
	@echo ""
	@echo "---------------------------------------------------------------------"
	@echo " High-Level Targets"
	@echo "---------------------------------------------------------------------"
	@echo "  test-all              Run all functional/integration tests."
	@echo "  test-unit-all         Run all available unit tests."
	@echo "  test-coverage-all     Run all code coverage analysis."
	@echo "  test-security-all     Run all security vulnerability scans."
	@echo "  lint-all              Run all linters."
	@echo "  clean                 Remove all generated artifacts and environments."
	@echo "  install-all           Alias for install-env."
	@echo ""
	@echo "---------------------------------------------------------------------"
	@echo " For more specific targets, see the Makefile."
	@echo "---------------------------------------------------------------------"


# ==============================================================================
# Environment Setup & Installation
# ==============================================================================

# The main entry point for setting up the environment
.PHONY: install-env
install-env: install-all

# install-all now serves as the core logic, depended on by install-env
.PHONY: install-all
install-all: check-env install-csharp install-java install-robot install-playwright install-go install-pact install-elixir install-ai install-blockchain install-mobile
	@echo "\n✅ All environment installations are complete."

# The check-env target remains our gatekeeper for system-level tools
.PHONY: check-env
check-env:
	@echo "--- Checking for required system-level tools ---"
	@for tool in go node npm python3 pip3 dotnet mvn k6 mix appium docker; do \
		if ! command -v $$tool &> /dev/null; then \
			echo "❌ ERROR: '$$tool' is not installed. Please install it to continue."; \
			if [ "$$tool" = "go" ]; then echo "   Follow instructions at https://golang.org/doc/install"; fi; \
			if [ "$$tool" = "node" ]; then echo "   Follow instructions at https://nodejs.org/"; fi; \
			if [ "$$tool" = "npm" ]; then echo "   Comes with Node.js. See https://nodejs.org/"; fi; \
			if [ "$$tool" = "python3" ]; then echo "   Follow instructions at https://www.python.org/downloads/"; fi; \
			if [ "$$tool" = "pip3" ]; then echo "   Comes with Python. See https://pip.pypa.io/en/stable/installation/"; fi; \
			if [ "$$tool" = "dotnet" ]; then echo "   Follow instructions at https://dotnet.microsoft.com/download"; fi; \
			if [ "$$tool" = "mvn" ]; then echo "   Follow instructions at https://maven.apache.org/install.html"; fi; \
			if [ "$$tool" = "k6" ]; then echo "   Follow instructions at https://k6.io/docs/getting-started/installation/"; fi; \
			if [ "$$tool" = "mix" ]; then echo "   Comes with Elixir. See https://elixir-lang.org/install.html"; fi; \
			if [ "$$tool" = "appium" ]; then echo "   Install with 'npm install -g appium'. See https://appium.io/"; fi; \
			if [ "$$tool" = "docker" ]; then echo "   Follow instructions at https://www.docker.com/get-started"; fi; \
			exit 1; \
		else \
			echo "✅ '$$tool' is installed."; \
		fi \
	done
	@echo "--- Environment check passed successfully! ---"

# --- Individual Installation Targets ---
# (These are unchanged)
.PHONY: install-csharp
install-csharp:
	@echo "\n--- Installing C# dependencies ---"
	cd csharp-specflow-api-tests && dotnet restore
	cd csharp-specflow-api-tests && dotnet user-secrets init > /dev/null 2>&1 && dotnet user-secrets set "ApiSettings:Password" "password123"
	@if ! dotnet tool list --global | grep -q specflow.plus.livingdoc.cli; then \
	echo "Installing SpecFlow+ LivingDoc CLI (global tool)..."; \
	dotnet tool install --global SpecFlow.Plus.LivingDoc.CLI; \
	else \
	echo "SpecFlow+ LivingDoc CLI already installed. Updating to the latest version..."; \
	dotnet tool update --global SpecFlow.Plus.LivingDoc.CLI; \
	fi

.PHONY: install-java
install-java:
	@echo "\n--- Installing Java dependencies (Maven) ---"
	cd java-event-driven-tests && mvn clean install -DskipTests

.PHONY: install-robot
install-robot:
	@echo "\n--- Setting up Python virtual environment for Robot Framework ---"
	@if [ ! -d "$(ROBOT_VENV)" ]; then \
		echo "Creating virtual environment at $(ROBOT_VENV)..."; \
		python3 -m venv $(ROBOT_VENV); \
	fi
	@echo "Installing/updating dependencies into $(ROBOT_VENV)..."
	$(ROBOT_VENV)/bin/pip install --upgrade pip
	$(ROBOT_VENV)/bin/pip install -r robot-framework-python-tests/requirements.txt
	$(ROBOT_VENV)/bin/pip install -r robot-framework-python-tests/requirements-dev.txt

.PHONY: install-playwright
install-playwright:
	@echo "\n--- Installing Playwright dependencies (npm) ---"
	cd playwright_ts_api_test && npm ci
	@echo "--- Installing Playwright browsers (this might take a moment) ---"
	cd playwright_ts_api_test && npx playwright install

.PHONY: install-go
install-go:
	@echo "\n--- Downloading Go dependencies ---"
	cd go-api-tests && go mod tidy

.PHONY: install-pact
install-pact:
	@echo "\n--- Installing Pact consumer & provider dependencies (npm) ---"
	cd pact-contract-testing/consumer-frontend && npm install
	cd pact-contract-testing/provider-api && npm install

.PHONY: install-k6
install-k6:
	@echo "\n--- Checking for k6 installation ---"
	@if ! command -v k6 &> /dev/null; then \
		echo "❌ k6 is not installed. The 'test-k6' target will fail."; \
		echo "   Please follow instructions at https://k6.io/docs/getting-started/installation/"; \
		exit 1; \
	else \
		echo "✅ k6 is installed."; \
	fi

.PHONY: install-elixir
install-elixir:
	@echo "\n--- Installing Elixir dependencies (mix) ---"
	cd elixir-api-tests && mix local.hex --force && mix deps.get

.PHONY: install-ai
install-ai:
	@echo "\n--- Setting up Python virtual environment for AI Test Observability ---"
	@if [ ! -d "$(AI_VENV)" ]; then \
		echo "Creating virtual environment at $(AI_VENV)..."; \
		python3 -m venv $(AI_VENV); \
	fi
	@echo "Installing/updating dependencies into $(AI_VENV)..."
	$(AI_VENV)/bin/pip install --upgrade pip
	$(AI_VENV)/bin/pip install -r ai-test-observability/requirements.txt

.PHONY: install-blockchain
install-blockchain:
	@echo "\n--- Installing Blockchain project dependencies (npm) ---"
	cd blockchain-smart-contracts-tests && npm install

.PHONY: install-mobile
install-mobile:
	@echo "\n--- Setting up Python virtual environment for Mobile Appium tests ---"
	@if [ ! -d "$(MOBILE_VENV)" ]; then \
		echo "Creating virtual environment at $(MOBILE_VENV)..."; \
		python3 -m venv $(MOBILE_VENV); \
	fi
	@echo "Installing/updating Python dependencies into $(MOBILE_VENV)..."
	$(MOBILE_VENV)/bin/pip install --upgrade pip
	$(MOBILE_VENV)/bin/pip install -r mobile-appium-tests/requirements.txt
	@if ! npm list -g | grep -q appium; then \
	echo "Installing Appium Server globally (this might take a moment)..."; \
	npm install -g appium; \
	else \
	echo "Appium is already installed globally. Updating to the latest version..."; \
	npm install -g appium; \
	fi
	@echo "--- Installing Appium UIAutomator2 driver ---"
	appium driver install uiautomator2

# ==============================================================================
# Comprehensive Test & Analysis Targets
# ==============================================================================
# (These targets are unchanged)

.PHONY: test-all
test-all:
	$(MAKE) test-csharp
	$(MAKE) test-java
	$(MAKE) test-robot
	$(MAKE) test-playwright
	$(MAKE) test-go
	$(MAKE) test-pact
	$(MAKE) test-k6
	$(MAKE) test-elixir
	$(MAKE) test-mobile

.PHONY: test-unit-all
test-unit-all:
	@echo "\n--- Running all unit tests ---"
	$(MAKE) test-robot-unit
	$(MAKE) test-ai-unit

.PHONY: test-coverage-all
test-coverage-all:
	@echo "\n--- Generating all code coverage reports ---"
	$(MAKE) test-csharp-coverage
	$(MAKE) test-java-coverage
	$(MAKE) test-robot-coverage
	$(MAKE) test-mobile-coverage
	$(MAKE) test-ai-coverage
	@echo "\n✅ All coverage reports generated."

.PHONY: test-security-all
test-security-all:
	@echo "\n--- Running all security scans ---"
	$(MAKE) test-java-security
	$(MAKE) test-playwright-security
	$(MAKE) test-pact-security
	$(MAKE) test-blockchain-security
	@echo "\n✅ All security scans complete."

.PHONY: lint-all
lint-all:
	$(MAKE) lint-robot
	$(MAKE) lint-playwright
	$(MAKE) lint-go

# ==============================================================================
# Individual Test & Analysis Targets
# ==============================================================================
# (These targets are unchanged)

# --- C# ---
.PHONY: test-csharp
test-csharp:
	@echo "\n--- Running C# SpecFlow API Tests ---"
	cd csharp-specflow-api-tests && dotnet test

.PHONY: test-csharp-coverage
test-csharp-coverage: install-csharp
	@echo "\n--- Running C# Tests with Code Coverage ---"
	cd csharp-specflow-api-tests && dotnet test --collect:"XPlat Code Coverage"

# --- Java ---
.PHONY: test-java
test-java:
	@echo "\n--- Running Java Event-Driven Tests ---"
	cd java-event-driven-tests && mvn clean test -DsuiteXmlFile=testng-integration-suite.xml

.PHONY: test-java-coverage
test-java-coverage: install-java
	@echo "\n--- Running Java Tests with Code Coverage (JaCoCo) ---"
	cd java-event-driven-tests && mvn clean verify
	@echo "JaCoCo report available at java-event-driven-tests/target/site/jacoco/index.html"

.PHONY: test-java-security
test-java-security: install-java
	@echo "\n--- Running Java Dependency Security Scan (OWASP) ---"
	cd java-event-driven-tests && mvn org.owasp:dependency-check-maven:check
	@echo "OWASP report available at java-event-driven-tests/target/dependency-check-report.html"

# --- Robot Framework / Python ---
.PHONY: test-robot
test-robot:
	@echo "\n--- Running Robot Framework API Tests (using venv) ---"
	@if [ ! -d "$(ROBOT_VENV)" ]; then echo "Robot Framework venv not found. Please run 'make install-robot' first."; exit 1; fi
	$(ROBOT_VENV)/bin/pabot --processes 4 --outputdir robot-framework-python-tests/reports/api_pabot robot-framework-python-tests/tests/api_tests

.PHONY: test-robot-unit
test-robot-unit: install-robot
	@echo "\n--- Running Robot Framework Helper Unit Tests (pytest) ---"
	$(ROBOT_VENV)/bin/pytest robot-framework-python-tests/pytest_unit/

.PHONY: test-robot-coverage
test-robot-coverage: install-robot
	@echo "\n--- Running Robot Framework Helper Code Coverage ---"
	$(ROBOT_VENV)/bin/pytest --cov=robot-framework-python-tests/resources robot-framework-python-tests/pytest_unit/

# --- Playwright / Node.js ---
.PHONY: test-playwright
test-playwright:
	@echo "\n--- Running Playwright TS API Tests ---"
	cd playwright_ts_api_test && npx playwright test

.PHONY: test-playwright-security
test-playwright-security: install-playwright
	@echo "\n--- Running Playwright Dependency Security Scan (npm audit) ---"
	cd playwright_ts_api_test && npm audit

# --- Go ---
.PHONY: test-go
test-go:
	@echo "\n--- Running Go API Tests (requires OWM_API_KEY env var) ---"
	cd go-api-tests && go test -v ./...

# --- Pact / Node.js ---
.PHONY: test-pact
test-pact:
	@echo "\n--- Running Pact Contract Tests ---"
	cd pact-contract-testing/consumer-frontend && npm test
	cd pact-contract-testing/provider-api && npm test

.PHONY: test-pact-security
test-pact-security: install-pact
	@echo "\n--- Running Pact Consumer Dependency Security Scan (npm audit) ---"
	cd pact-contract-testing/consumer-frontend && npm audit
	@echo "\n--- Running Pact Provider Dependency Security Scan (npm audit) ---"
	cd pact-contract-testing/provider-api && npm audit

# --- k6 ---
.PHONY: test-k6
test-k6:
	@echo "\n--- Running k6 Performance Tests ---"
	cd k6-performance-tests && k6 run smoke-test.js

# --- Elixir ---
.PHONY: test-elixir
test-elixir:
	@echo "\n--- Running Elixir API Tests ---"
	cd elixir-api-tests && mix test

# --- AI / Python ---
.PHONY: test-ai-unit
test-ai-unit: install-ai
	@echo "\n--- Running AI Script Unit Tests (pytest) ---"
	$(AI_VENV)/bin/pytest ai-test-observability/tests/

.PHONY: test-ai-coverage
test-ai-coverage: install-ai
	@echo "\n--- Running AI Script Code Coverage ---"
	$(AI_VENV)/bin/pytest --cov=ai-test-observability ai-test-observability/tests/

# --- Blockchain / Node.js ---
.PHONY: test-blockchain
test-blockchain:
	@echo "\n--- Running Blockchain Contract Tests ---"
	cd blockchain-smart-contracts-tests && npx hardhat test

.PHONY: test-blockchain-security
test-blockchain-security: install-blockchain
	@echo "\n--- Running Blockchain Dependency Security Scan (npm audit) ---"
	cd blockchain-smart-contracts-tests && npm audit

# --- Mobile / Python ---
.PHONY: test-mobile
test-mobile: install-mobile
	@echo "\n--- Running Mobile Appium Tests (using venv) ---"
	$(MOBILE_VENV)/bin/pytest mobile-appium-tests/

.PHONY: test-mobile-coverage
test-mobile-coverage: install-mobile
	@echo "\n--- Running Mobile Appium Helper Code Coverage ---"
	$(MOBILE_VENV)/bin/pytest --cov=mobile-appium-tests/helpers mobile-appium-tests/

# ==============================================================================
# Linting Targets
# ==============================================================================
# (Unchanged)
.PHONY: lint-robot
lint-robot: install-robot
	@echo "\n--- Running Robot Framework Linters (using venv) ---"
	$(ROBOT_VENV)/bin/robocop robot-framework-python-tests
	$(ROBOT_VENV)/bin/rflint robot-framework-python-tests

.PHONY: lint-playwright
lint-playwright:
	@echo "\n--- Running Playwright Linter (eslint) ---"
	cd playwright_ts_api_test && npm run lint

.PHONY: lint-go
lint-go:
	@echo "\n--- Running Go Linter (gofmt) ---"
	cd go-api-tests && gofmt -l .

# ==============================================================================
# Clean Target
# ==============================================================================
# (Unchanged)
.PHONY: clean
clean:
	@echo "--- Cleaning up build artifacts, caches, and virtual environments ---"
	# C#
	rm -rf csharp-specflow-api-tests/bin csharp-specflow-api-tests/obj csharp-specflow-api-tests/TestResults
	# Java
	cd java-event-driven-tests && mvn clean
	# Python
	rm -rf $(ROBOT_VENV) $(AI_VENV) $(MOBILE_VENV)
	find . -type d -name "__pycache__" -exec rm -r {} +
	find . -type d -name ".pytest_cache" -exec rm -r {} +
	find . -type f -name ".coverage" -delete
	rm -rf robot-framework-python-tests/reports
	# Node.js
	find . -type d -name "node_modules" -exec rm -r {} +
	rm -rf playwright_ts_api_test/test-results playwright_ts_api_test/playwright-report
	rm -rf pact-contract-testing/consumer-frontend/pacts
	rm -rf blockchain-smart-contracts-tests/artifacts blockchain-smart-contracts-tests/cache
	# Go
	cd go-api-tests && go clean
	@echo "Cleanup complete."