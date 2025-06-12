*** Settings ***
Library    ImageHorizonLibrary
Library    OperatingSystem

*** Keywords ***
Compare With Baseline
    [Arguments]    ${baseline}    ${current}    ${tolerance}
    ${result}=    Compare Images    ${baseline}    ${current}    ${tolerance}
    Run Keyword If    '${result}' != 'PASS'    Fail    Visual comparison failed: ${result}
