Feature: API Security Testing
  As a security-conscious QA engineer
  I want to validate API security measures
  To ensure the application is protected against common vulnerabilities

  Background:
    Given the API security testing environment is prepared
    And I have appropriate security testing credentials

  @security @authentication @critical
  Scenario: Authentication security validation
    Given I am testing authentication security
    When I attempt to authenticate with invalid credentials
    Then the authentication should fail securely
    And no sensitive information should be exposed in the response
    And appropriate security headers should be present
    
  @security @authentication @edge_case
  Scenario: Token expiration handling
    Given I have a valid authentication token
    When I wait for the token to expire
    And I attempt to make an authenticated request
    Then the request should be rejected with 401 status
    And the error message should not reveal internal details
    
  @security @authentication @brute_force
  Scenario: Brute force protection
    Given I am testing brute force protection
    When I make multiple failed authentication attempts
    Then the system should implement rate limiting
    And further attempts should be temporarily blocked
    And security events should be logged appropriately

  @security @input_validation @xss
  Scenario Outline: Cross-Site Scripting (XSS) prevention
    Given I am testing XSS vulnerability protection
    When I submit a booking request with <field> containing XSS payload "<xss_payload>"
    Then the API should sanitize or reject the malicious input
    And the response should not contain unescaped script content
    And the data should be safely stored without executing scripts

    Examples:
      | field           | xss_payload                              |
      | firstname       | <script>alert('xss')</script>          |
      | lastname        | javascript:alert('xss')                 |
      | additionalneeds | <img src=x onerror=alert('xss')>       |
      | firstname       | ';DROP TABLE users;--                   |
      | additionalneeds | {{7*7}}                                 |

  @security @input_validation @sql_injection
  Scenario Outline: SQL Injection prevention
    Given I am testing SQL injection vulnerability protection
    When I submit a booking request with <field> containing SQL injection payload "<sql_payload>"
    Then the API should properly sanitize the input
    And no database errors should be exposed
    And the malicious SQL should not be executed

    Examples:
      | field      | sql_payload                    |
      | firstname  | ' OR 1=1--                    |
      | lastname   | '; DROP TABLE bookings;--     |
      | firstname  | ' UNION SELECT * FROM users-- |
      | lastname   | admin'--                       |

  @security @data_validation @boundary
  Scenario Outline: Input validation boundary testing
    Given I am testing input validation boundaries
    When I submit a booking with <field> containing "<boundary_value>"
    Then the API should handle the boundary value appropriately
    And provide clear validation error messages if rejected
    And not expose internal system information

    Examples:
      | field        | boundary_value                           |
      | firstname    | [empty string]                          |
      | firstname    | [single character]                      |
      | firstname    | [1000 character string]                 |
      | totalprice   | -999999999                              |
      | totalprice   | 999999999                               |
      | totalprice   | [non-numeric string]                    |
      | checkin      | 1900-01-01                              |
      | checkout     | 2100-12-31                              |

  @security @authorization @access_control
  Scenario: Unauthorized access prevention
    Given I am testing unauthorized access prevention
    When I attempt to access protected endpoints without authentication
    Then access should be denied with appropriate HTTP status codes
    And error messages should not reveal system architecture
    And no sensitive data should be accessible

  @security @authorization @privilege_escalation
  Scenario: Privilege escalation prevention
    Given I am authenticated as a regular user
    When I attempt to access administrative functions
    Then the access should be denied
    And appropriate authorization errors should be returned
    And the attempt should be logged for security monitoring

  @security @data_exposure @information_disclosure
  Scenario: Sensitive data exposure prevention
    Given I am testing for information disclosure
    When I make various API requests
    Then responses should not contain sensitive information
    And error messages should not reveal internal details
    And system paths or configuration should not be exposed
    And stack traces should not be visible in production

  @security @headers @transport_security
  Scenario: Security headers validation
    Given I am testing HTTP security headers
    When I make requests to the API
    Then appropriate security headers should be present
    And Content-Type headers should be correctly set
    And X-Content-Type-Options should prevent MIME sniffing
    And X-Frame-Options should prevent clickjacking
    And security headers should follow best practices

  @security @session_management @token_security
  Scenario: Token security validation
    Given I have a valid authentication token
    When I analyze the token structure and handling
    Then the token should be properly formatted
    And token transmission should be secure
    And token storage recommendations should be followed
    And token expiration should be implemented

  @security @rate_limiting @dos_protection
  Scenario: Rate limiting and DoS protection
    Given I am testing rate limiting mechanisms
    When I make rapid successive requests to the API
    Then rate limiting should be enforced
    And appropriate HTTP status codes should be returned
    And legitimate requests should not be affected
    And the system should remain stable under load

  @security @error_handling @exception_management
  Scenario: Secure error handling
    Given I am testing error handling security
    When I trigger various error conditions
    Then error responses should not expose sensitive information
    And stack traces should not be visible
    And error codes should be consistent and documented
    And internal system details should remain hidden

  @security @logging @audit_trail
  Scenario: Security event logging
    Given I am testing security event logging
    When I perform various security-relevant actions
    Then appropriate events should be logged
    And log entries should contain sufficient detail for investigation
    And sensitive data should not be logged in plain text
    And audit trails should be tamper-evident

  @security @encryption @data_protection
  Scenario: Data protection in transit and at rest
    Given I am testing data protection mechanisms
    When I submit sensitive data through the API
    Then data transmission should be encrypted
    And sensitive data should be properly protected
    And encryption standards should be appropriate
    And data integrity should be maintained

  @security @business_logic @workflow_security
  Scenario: Business logic security validation
    Given I am testing business logic security
    When I attempt to manipulate business workflows
    Then business rules should be consistently enforced
    And workflow bypass attempts should be prevented
    And state transitions should be properly validated
    And business constraints should be maintained

  @security @regression @vulnerability_retest
  Scenario: Security regression testing
    Given I am performing security regression testing
    When I test previously identified security issues
    Then all known vulnerabilities should remain fixed
    And new security measures should be functioning
    And security controls should not have been weakened
    And compliance requirements should still be met