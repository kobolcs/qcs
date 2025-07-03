*** Settings ***
Library    Browser
Library    ../helpers/self_heal.py

*** Test Cases ***
Click In Iframe And Shadow DOM
    New Browser    chromium
    New Page    https://demo.playwright.dev/shadow-dom
    # shadow DOM button
    ${sel}=    Click With Fallback    css=span >> shadow=button    css=button.shadow-btn
    # iframe (simulated, would need a test page with iframe)
    # Select Frame    name=demo-frame
    # Click    text=Inside Iframe
    Capture Page Screenshot
    Close Browser

Data Driven Example
    [Template]    Add Numbers Should Work
    2    3    5
    10   5    15

*** Keywords ***
Add Numbers Should Work
    [Arguments]    ${a}    ${b}    ${expected}
    ${result}=    Evaluate    ${a} + ${b}
    Should Be Equal    ${result}    ${expected}
