#!/usr/bin/env bash
set -euo pipefail

# Ensure we're at repo root
cd "$(dirname "$0")"

TEST_DIR="robot-framework-python-tests"
REQUIREMENTS="$TEST_DIR/requirements.txt"

echo "=== Installing dependencies ==="
pip install -r "$REQUIREMENTS"

echo "=== Preparing report directories ==="
mkdir -p "$TEST_DIR/reports/api" \
         "$TEST_DIR/reports/performance" \
         "$TEST_DIR/reports/gui" \
         "$TEST_DIR/reports/all" \
         "$TEST_DIR/results"

echo "=== Running API tests ==="
robot --outputdir "$TEST_DIR/reports/api" "$TEST_DIR/tests/api_tests"

echo "=== Running performance tests ==="
robot --outputdir "$TEST_DIR/reports/performance" "$TEST_DIR/tests/performance_tests"

echo "=== Running GUI tests ==="
robot --outputdir "$TEST_DIR/reports/gui" "$TEST_DIR/tests/ui_demo.robot" "$TEST_DIR/tests/visual_tests"

echo "=== Running full test suite in parallel ==="
pabot --processes 4 --outputdir "$TEST_DIR/reports/all" "$TEST_DIR/tests"

