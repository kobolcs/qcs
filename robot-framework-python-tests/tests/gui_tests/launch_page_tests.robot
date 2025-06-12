*** Settings ***
Resource         ../../resources/variables/spacex_variables.robot
Resource         ../../resources/locators/ui_locators.robot
Resource         ../../resources/keywords/spacex_ui_keywords.robot
Suite Setup      Open Launch Page    ${LAUNCH_PAGE_URL}    ${BROWSER}    ${TIMEOUT}
Suite Teardown   Close Launch Browser

*** Test Cases ***
Verify Latest Launch Section Present
    Verify Latest Launch Section Visible

Capture Launch Patch Image
    ${current_screenshot}=    Set Variable    results/visual_latest_launch.png
    Capture Launch Patch Image    ${current_screenshot}
