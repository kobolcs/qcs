Feature: Comprehensive Booking API Test Suite
  As a QA Engineer demonstrating professional test automation
  I want to showcase comprehensive API testing scenarios
  To validate functionality, security, performance, and reliability

  Background:
    Given the API is available and responding
    And I have valid authentication credentials

  @smoke @critical_path
  Scenario: Complete booking lifecycle with data validation
    Given I am an authenticated user
    When I create a booking with the following details:
      | Field           | Value              |
      | firstname       | John               |
      | lastname        | Doe                |
      | totalprice      | 150                |
      | depositpaid     | true               |
      | checkin         | 2025-08-01         |
      | checkout        | 2025-08-05         |
      | additionalneeds | Breakfast          |
    Then the booking should be created successfully
    And the booking ID should be a positive integer
    And all booking details should match the input data
    When I retrieve the booking by ID
    Then the retrieved data should exactly match the created booking
    When I update the booking with price 200 and checkout date "2025-08-06"
    Then the booking should be updated successfully
    And the price should be 200
    And the checkout date should be "2025-08-06"
    When I delete the booking
    Then the booking should be deleted successfully
    And retrieving the deleted booking should return 404

  @negative @validation
  Scenario Outline: Data validation for booking creation
    Given I am an authenticated user
    When I attempt to create a booking with <field> set to "<invalid_value>"
    Then the booking creation should fail
    And the response should contain an appropriate error message

    Examples:
      | field        | invalid_value           | description                    |
      | firstname    |                         | Empty firstname                |
      | lastname     |                         | Empty lastname                 |
      | totalprice   | -50                     | Negative price                 |
      | totalprice   | 0                       | Zero price                     |
      | totalprice   | abc                     | Non-numeric price              |
      | checkin      | 2025-13-01              | Invalid month                  |
      | checkin      | 2025-02-30              | Invalid date                   |
      | checkout     | 2025-07-31              | Checkout before checkin        |
      | additionalneeds | <script>alert('xss')</script> | XSS attempt                   |

  @security @authentication
  Scenario: Authentication and authorization edge cases
    Given I have invalid credentials
    When I attempt to authenticate with username "invalid" and password "wrong"
    Then authentication should fail with 401 status
    And no token should be returned
    
    Given I am an authenticated user
    When I wait for the token to expire
    And I attempt to create a booking with an expired token
    Then the request should fail with 401 status
    
    When I attempt to access bookings without authentication
    Then some operations should be restricted
    And read operations should still work

  @boundary @edge_cases
  Scenario Outline: Boundary value testing for booking fields
    Given I am an authenticated user
    When I create a booking with <field> set to "<boundary_value>"
    Then the booking creation should <expected_result>

    Examples:
      | field        | boundary_value    | expected_result |
      | firstname    | A                 | succeed         |
      | firstname    | [50 char string] | succeed         |
      | lastname     | B                 | succeed         |
      | lastname     | [50 char string] | succeed         |
      | totalprice   | 1                 | succeed         |
      | totalprice   | 999999            | succeed         |
      | checkin      | 2025-01-01        | succeed         |
      | checkout     | 2030-12-31        | succeed         |

  @concurrency @performance
  Scenario: Concurrent booking operations
    Given I am an authenticated user
    When I create 5 bookings simultaneously
    Then all bookings should be created successfully
    And each booking should have a unique ID
    And no data corruption should occur
    When I update all 5 bookings simultaneously
    Then all updates should succeed
    And no conflicts should occur

  @api_contract @schema_validation
  Scenario: API response schema validation
    Given I am an authenticated user
    When I create a new booking
    Then the response should conform to the booking creation schema
    And all required fields should be present
    And field types should match the specification
    When I retrieve a booking
    Then the response should conform to the booking retrieval schema
    And the data should be consistent with creation response

  @error_handling @resilience
  Scenario: API resilience and error recovery
    Given the API is under load
    When I make multiple rapid requests
    Then the API should handle rate limiting gracefully
    And appropriate HTTP status codes should be returned
    
    Given I send malformed JSON in the request body
    When I attempt to create a booking
    Then the API should return 400 Bad Request
    And the error message should be descriptive

  @data_integrity @cleanup
  Scenario: Test data management and cleanup
    Given I create multiple test bookings
    When the test suite completes
    Then all test data should be cleaned up
    And no orphaned records should remain
    And the system should be in a clean state

  @monitoring @metrics
  Scenario: API performance monitoring
    Given I am measuring API response times
    When I perform standard booking operations
    Then response times should be within acceptable limits
    And performance metrics should be recorded
    And any degradation should be detected

  @regression @comprehensive
  Scenario: Full API regression test
    Given I have a comprehensive test dataset
    When I execute all booking operations in sequence
    Then all operations should complete successfully
    And system state should remain consistent
    And no side effects should occur

  @accessibility @usability
  Scenario: API documentation and usability validation
    Given I am testing API documentation examples
    When I execute documented API calls
    Then all examples should work correctly
    And responses should match documentation
    And error codes should be as documented