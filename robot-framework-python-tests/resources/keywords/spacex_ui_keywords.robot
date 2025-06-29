*** Settings ***
Library    SeleniumLibrary

*** Keywords ***
Open Launch Page
    [Arguments]    ${url}    ${browser}    ${timeout}
    ${unique_dir}=    Evaluate    __import__('uuid').uuid4().hex
    &{options}=    Create Dictionary    args=--user-data-dir=/tmp/chrome_${unique_dir}
    Open Browser    ${url}    ${browser}    options=${options}
    Maximize Browser Window
    Set Selenium Timeout    ${timeout}

Verify Latest Launch Section Visible
    Wait Until Element Is Visible    ${LAUNCH_SECTION}
    Page Should Contain Element      ${LAUNCH_TITLE}

Switch To Launch Details Iframe
    Wait Until Element Is Visible    ${IFRAME_FRAME}
    Select Frame    ${IFRAME_FRAME}

Verify Content Inside Iframe
    Wait Until Element Is Visible    ${IFRAME_CONTENT}
    Page Should Contain Element      ${IFRAME_CONTENT}

Capture Launch Patch Image
    [Arguments]    ${save_path}
    Wait Until Element Is Visible    ${LAUNCH_IMG}
    Capture Element Screenshot       ${LAUNCH_IMG}    ${save_path}

Close Launch Browser
    Close Browser
