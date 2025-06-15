*** Settings ***
Resource          ../../keywords.robot
Library           ../../helpers/csv_keywords.py

*** Variables ***
${CSV_PATH}       tests/api_tests/last_5_launches.csv

*** Test Cases ***
Validate Multiple Past Launches
    [Documentation]    This test reads launch data from a CSV and validates it.
    ${launches}=    Read CSV File To List    ${CSV_PATH}
    FOR    ${launch}    IN    @{launches}
        ${response}=    Get Launch By ID    ${launch}[id]
        Validate Launch Details    ${response}    ${launch}
    END
