#!/usr/bin/env bash
set -euo pipefail

# Ensure we're at repo root
cd "$(dirname "$0")"

TEST_DIR="robot-framework-python-tests"
REQUIREMENTS="$TEST_DIR/requirements.txt"
REPORTS_DIR="$TEST_DIR/reports"
RESULTS_DIR="$TEST_DIR/results"
PROCESSES="${PROCESSES:-4}"

echo "=== Installing dependencies ==="
pip install -r "$REQUIREMENTS"

echo "=== Preparing report directories ==="
mkdir -p "$REPORTS_DIR/api" \
         "$REPORTS_DIR/performance" \
         "$REPORTS_DIR/gui" \
         "$REPORTS_DIR/all" \
         "$RESULTS_DIR"

echo "=== Running API tests ==="
robot --outputdir "$REPORTS_DIR/api" "$TEST_DIR/tests/api_tests"

echo "=== Running performance tests ==="
robot --outputdir "$REPORTS_DIR/performance" "$TEST_DIR/tests/performance_tests"

echo "=== Running GUI tests ==="
robot --outputdir "$REPORTS_DIR/gui" "$TEST_DIR/tests/ui_demo.robot" "$TEST_DIR/tests/visual_tests"

echo "=== Running full test suite in parallel ==="
pabot --processes "$PROCESSES" --outputdir "$REPORTS_DIR/all" "$TEST_DIR/tests"

