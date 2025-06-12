*** Settings ***
Library    BrowserLibrary
Resource   ../../resources/variables/spacex_variables.robot
Resource   ../../resources/locators/ui_locators.robot

*** Variables ***
${LAUNCH_PAGE}    ${LAUNCH_PAGE_URL}

*** Test Cases ***
Open Latest Launch Page Using Playwright
    New Browser    chromium
    New Context
    New Page       ${LAUNCH_PAGE}
    Wait For Selector   ${LAUNCH_SECTION}
    Page Should Contain Element   ${LAUNCH_TITLE}
    Capture Page Screenshot   results/playwright_latest_launch.png
    Close Browser
