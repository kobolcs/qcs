# Project Vision: AI for Test Observability

**Status: Experimental – Work in Progress**

This project aims to demonstrate the application of AI and Machine Learning techniques to solve common, difficult problems in QA and test automation, transforming raw test output into actionable insights.

It explores how data science techniques can reduce triage time and highlight flaky tests before they erode trust in automation.
---
### When to Use This Approach

This is an advanced strategy for mature QA organizations facing specific challenges at scale:

* **Large, Slow Test Suites**: When test suites run for hours, manual triage of failures becomes a significant bottleneck. This approach automates the initial analysis.
* **Flaky Test Problems**: Flakiness undermines trust in automation. An AI model can objectively identify and flag statistically flaky tests, separating them from legitimate regressions.
* **Root Cause Analysis**: When a single backend issue causes dozens of tests to fail, NLP clustering can group them by their error messages, immediately pointing teams to the likely root cause.

### Similar Tooling in Other Languages
### Strategic Advantage
- Provides a glimpse into data-driven QA strategies our clients are beginning to request.
- Helps prioritize failures and focus engineering effort on what matters most.
- Architecture concepts shared in [../../ARCHITECTURAL_PRINCIPLES.md](../../ARCHITECTURAL_PRINCIPLES.md).


This project demonstrates the core principles behind commercial AI-in-testing platforms.
* **Commercial Tools**: `Launchable`, `Testim`, `Applitools` (for visual AI), and others use similar ML techniques for test selection, root cause analysis, and self-healing.
* **Open Source**: `Healenium` is an open-source tool specifically focused on self-healing locators, a subset of this broader vision.

### (Proposed) Installation and Running

**Prerequisites:**
* Python (version 3.10 or later)
* Git

#### 1. Local Machine (Windows/macOS/Linux)

1.  **Set up a Python virtual environment**:
    * **macOS/Linux**:
        ```bash
        python3 -m venv venv
        source venv/bin/activate
        ```
    * **Windows (Command Prompt/PowerShell)**:
        ```cmd
        python -m venv venv
        .\venv\Scripts\activate
        ```
2.  **Install dependencies**:
    ```bash
    pip install -r requirements.txt
    ```
3.  **Convert a raw test report to CSV** (pointing to a `weather_test_report.json` file from the Go tests):
    ```bash
    python process_results.py --input-file ../../go-api-tests/weather_test_report.json --output-file processed_results.csv
    ```
4.  **Launch the interactive dashboard**:
    ```bash
    streamlit run dashboard.py
    ```
