# tests/performance/api_performance_tests.robot
*** Settings ***
Library    Collections
Library    DateTime
Library    ../../helpers/performance_helper.PerformanceHelper
Resource   ../../resources/keywords/spacex_api_keywords.robot

*** Variables ***
${ITERATIONS}       10
${SLA_P95_MS}      500
${SLA_P99_MS}      1000

*** Test Cases ***
SpaceX API Performance SLA Validation
    [Documentation]    Validates API performance against defined SLAs
    [Tags]    performance    sla

    ${results}=    Create List

    FOR    ${i}    IN RANGE    ${ITERATIONS}
        ${start}=    Get Current Date    result_format=epoch
        Get Latest Launch JSON
        ${end}=      Get Current Date    result_format=epoch
        ${duration}=    Evaluate    (${end} - ${start}) * 1000  # Convert to ms
        Append To List    ${results}    ${duration}
    END

    ${p95}=    Calculate Percentile    ${results}    95
    ${p99}=    Calculate Percentile    ${results}    99

    Should Be True    ${p95} < ${SLA_P95_MS}
    ...    P95 latency ${p95}ms exceeds SLA of ${SLA_P95_MS}ms

    Log Performance Report    ${results}    ${p95}    ${p99}