# resources/integrations/github_integration.py
import os
import json
import requests
from typing import Dict, Optional

class GitHubIntegration:
    """Integrates test results with GitHub Issues for defect tracking."""

    def __init__(self):
        self.token = os.getenv("GITHUB_TOKEN")
        self.repo = os.getenv("GITHUB_REPOSITORY", "kobolcs/qcs")
        self.headers = {
            "Authorization": f"token {self.token}",
            "Accept": "application/vnd.github.v3+json"
        }

    def create_issue_from_failure(self, test_name: str, error_message: str,
                                 test_output: str) -> Optional[Dict]:
        """Create GitHub issue for test failure."""
        if not self.token:
            return None

        issue_data = {
            "title": f"Test Failure: {test_name}",
            "body": self._format_issue_body(test_name, error_message, test_output),
            "labels": ["test-failure", "automated"],
            "assignees": []
        }

        response = requests.post(
            f"https://api.github.com/repos/{self.repo}/issues",
            headers=self.headers,
            json=issue_data
        )

        if response.status_code == 201:
            return response.json()
        return None

    def _format_issue_body(self, test_name: str, error: str, output: str) -> str:
        """Format issue body with test details."""
        return f"""## Automated Test Failure Report

**Test Name:** `{test_name}`
**Timestamp:** {datetime.now().isoformat()}
**Environment:** {os.getenv('TEST_ENV', 'development')}

### Error Message
{error}

### Test Output
<details>
<summary>Click to expand</summary>
{output}
</details>

### Reproduction Steps
1. Run: `robot -t "{test_name}" tests/`
2. Check the generated report in `reports/log.html`

---
*This issue was automatically created by the test automation framework.*
"""