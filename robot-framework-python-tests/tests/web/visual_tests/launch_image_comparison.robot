*** Settings ***
Resource         ../../resources/variables/spacex_variables.robot
Resource         ../../resources/keywords/spacex_ui_keywords.robot
Resource         ../../resources/keywords/visual_keywords.robot
Suite Setup      Open Launch Page    ${LAUNCH_PAGE_URL}    ${BROWSER}    ${TIMEOUT}
Suite Teardown   Close Launch Browser

*** Variables ***
${BASELINE}      baselines/latest_launch_baseline.png
${CURRENT}       results/latest_launch_current.png
${TOL}           ${TOLERANCE}

*** Test Cases ***
Homepage Launch Patch Visual Comparison
    Capture Page Screenshot    ${CURRENT}
    Compare With Baseline      ${BASELINE}    ${CURRENT}    ${TOL}
