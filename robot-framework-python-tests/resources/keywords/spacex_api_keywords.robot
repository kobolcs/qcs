*** Settings ***
Library     REST    ${BASE_URL}

*** Keywords ***
Get Latest Launch JSON
    [Documentation]    Calls SpaceX API latest launch endpoint.
    GET    ${LAUNCH_ENDPOINT}
    Status    200
    ${json}=    Output    body
    RETURN    ${json}

Get Launch By ID
    [Arguments]    ${launch_id}
    [Documentation]    Calls SpaceX API to get a launch by its ID.
    GET    /launches/${launch_id}
    Status    200
    ${json}=    Output    body
    RETURN    ${json}

Validate Launch Fields
    [Arguments]    ${launch_json}
    Should Not Be Empty    ${launch_json['name']}
    Should Match Regexp    ${launch_json['date_utc']}    \d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+Z
    Should Be True    type(${launch_json['success']}) is bool or ${launch_json['success']} is None
    Should Contain    ${launch_json}    rocket
    Should Contain    ${launch_json}    details
    Should Contain    ${launch_json}    links

Get Launch Image URL
    [Arguments]    ${launch_json}
    RETURN    ${launch_json['links']['patch']['small']}
