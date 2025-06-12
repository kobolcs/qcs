*** Settings ***
Library    ../../resources/keywords/custom_playwright.py
Resource   ../../resources/variables/spacex_variables.robot

*** Variables ***
${HOST}    css=custom-element-host
${BTN}     css=custom-element-host >> shadow=button

*** Test Cases ***
Click Button Inside Shadow DOM
    Click Shadow Button    ${HOST}    ${BTN}
    Wait Until Page Contains    Modal Opened
