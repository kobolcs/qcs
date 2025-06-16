const { spawnSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const repoRoot = path.resolve(__dirname, '..');
const reportPath = path.join(repoRoot, 'go-api-tests', 'weather_test_report.json');

function run(cmd, args, cwd) {
  const result = spawnSync(cmd, args, { cwd, stdio: 'inherit', shell: process.platform === 'win32' });
  if (result.error) throw result.error;
  if (result.status !== 0) process.exit(result.status);
}

console.log('=== Running Go API tests ===');
if (fs.existsSync(reportPath)) fs.unlinkSync(reportPath);
run('go', ['test', '-v', './...'], path.join(repoRoot, 'go-api-tests'));

if (!fs.existsSync(reportPath)) {
  console.error('❌ Go API tests did not produce report. Aborting.');
  process.exit(1);
}

console.log('✅ Go API tests passed. Running Pact contract tests.');
run('npm', ['test'], path.join(repoRoot, 'pact-contract-testing', 'consumer-frontend'));
run('npm', ['test'], path.join(repoRoot, 'pact-contract-testing', 'provider-api'));

console.log('🎉 Integration scenario complete.');

