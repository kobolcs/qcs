#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

REPORT_PATH="go-api-tests/weather_test_report.json"

# Run Go API tests and generate report
echo "=== Running Go API tests ==="
rm -f "$REPORT_PATH"
(cd go-api-tests && go test -v ./...)

if [ ! -f "$REPORT_PATH" ]; then
  echo "❌ Go API tests did not produce report. Aborting."
  exit 1
fi

echo "✅ Go API tests passed. Running Pact contract tests."
(cd pact-contract-testing/consumer-frontend && npm test)
(cd pact-contract-testing/provider-api && npm test)

echo "🎉 Integration scenario complete."
