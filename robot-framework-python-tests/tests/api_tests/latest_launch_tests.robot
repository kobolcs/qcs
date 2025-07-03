*** Settings ***
Documentation       This data-driven suite verifies rocket details for multiple SpaceX launches.
...                 It reads launch IDs from a CSV and uses a template to run validations.
Library             OperatingSystem
Library             Collections
Library             RESTinstance    ${BASE_URL}
Library             resources.keywords.extended_spacex_helpers.ExtendedSpaceX
Suite Setup         Load Launch IDs From CSV
Test Template       Verify Launch Data

*** Test Cases ***
# The test cases will be dynamically created from the launch IDs in the CSV.
# To add more tests, simply add more IDs to last_5_launches.csv.

*** Keywords ***
Load Launch IDs From CSV
    [Documentation]    Reads the launch IDs and creates a test case for each one.
    ${csv_path}=    Normalize Path    ${CURDIR}/last_5_launches.csv
    ${csv_content}=    Get File    ${csv_path}
    @{lines}=    Split To Lines    ${csv_content}
    Remove From List    ${lines}    0    # Remove the header line

    FOR    ${launch_id}    IN    @{lines}
        Create Test    Launch ${launch_id}    Verify Launch Data    ${launch_id}
    END

Verify Launch Data
    [Arguments]    ${launch_id}
    [Documentation]    This is the template keyword that validates a single launch.
    ${resp}=    GET    https://api.spacexdata.com/v4/launches/${launch_id}
    Should Be Equal As Strings    ${resp.status_code}    200
    ${launch_json}=    Set Variable    ${resp.json()}

    # Step 1: Validate key fields exist (similar to your original test)
    Should Not Be Empty    ${launch_json['name']}
    Should Not Be Empty    ${launch_json['date_utc']}
    Should Not Be Empty    ${launch_json['rocket']}

    # Step 2: Use the powerful custom Python keyword for combined data
    ${combined_data}=    Get Combined Launch And Rocket    ${launch_json}
    Should Not Be Empty    ${combined_data['rocket_name']}
    Log    Verified Data: ${combined_data}

    # Step 3: Validate the image URL
    ${patch_url}=    Set Variable    ${launch_json['links']['patch']['small']}
    Should Match Regexp    ${patch_url}    ^https://.*\\.png$