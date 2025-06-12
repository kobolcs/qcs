# Robot Framework: Addressing Common Concerns

This document shows how a senior automation engineer mitigates pain points of Robot Framework, using modern best practices.

## 1. "Too Much Logic in .robot Files"

### Anti-pattern
```robot
*** Test Cases ***
Bad Example
    ${price}=    Evaluate    ${cost} * 1.2 + (${fee} or 0)
    Run Keyword If    ${price} > 100    Log    Expensive
```
### Alleviation
Move logic to Python custom keywords:
```python
# helpers/pricing.py
def calculate_total(cost, fee=0):
    return cost * 1.2 + fee
```
```robot
Library    helpers/pricing.py
*** Test Cases ***
Better Example
    ${total}=    Calculate Total    50    5
    Should Be True    ${total} == 65
```

## 2. "Can't Handle Modern Web (iframes, Shadow DOM)"

### Anti-pattern
```robot
# Fails: can't find nested/shadow element
Click Element    css=.my-button
```
### Alleviation
Use Browser library, direct selectors:
```robot
*** Settings ***
Library    Browser

*** Test Cases ***
Click Button In Shadow DOM
    New Page    https://modern-app.test/shadow
    Click    css=custom-element >> shadow=button
    # For iframe:
    Select Frame    iframe[name="frame1"]
    Click           text=Confirm
```

## 3. "No Rich Reporting"

### Anti-pattern
Default logs, no screenshots or extra context.

### Alleviation
Capture and attach artifacts:
```robot
*** Test Cases ***
Always Capture Screenshot
    [Teardown]    Capture Page Screenshot
    Open Browser    https://example.com    chromium
    # ...test steps...
```
Python log/file attach:
```python
from robot.libraries.BuiltIn import BuiltIn
from robot.api import logger

def attach_log_file(filepath):
    with open(filepath) as f:
        logger.info(f.read(), html=True)
        BuiltIn().log("Attached custom log: %s" % filepath)
```

## 4. "No Test Parametrization"

### Anti-pattern
Duplicated test cases for each input.

### Alleviation
Gherkin/BDD or Templates:
```robot
*** Test Cases ***
[Template]    Add Numbers Should Work
    1    2    3
    10   5    15

*** Keywords ***
Add Numbers Should Work
    [Arguments]    ${a}    ${b}    ${expected}
    ${result}=    Evaluate    ${a} + ${b}
    Should Be Equal    ${result}    ${expected}
```

## 5. "Not Pythonic, Can't Use Modern Tools"

Use pytest for unit tests, add robocop/rflint for static analysis.
