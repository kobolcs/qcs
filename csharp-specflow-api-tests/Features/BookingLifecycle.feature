Feature: Booking Lifecycle Management
  As a user of the Restful Booker API
  I want to perform various operations on bookings
  To ensure the booking lifecycle, validation rules, and security are working correctly

  @booking_lifecycle @smoke @regression
  Scenario: Authenticated user can manage the full booking lifecycle
    Given I am an authenticated user
    When I create a new booking with default valid details
    Then the booking creation should be successful
    And the booking details should be stored
    And the booking should exist in the system with the stored details
    When I update the booking with new checkin "2028-01-01", checkout "2028-01-05", and price 250
    Then the booking update should be successful
    And the updated booking details should be stored correctly
    When I delete the booking
    Then the booking should no longer exist

  @booking_update @negative @regression
  Scenario: Attempt to update a non-existent booking
    Given I am an authenticated user
    When I attempt to update a booking with a non-existent ID 88888888 using new checkin "2029-02-01" and checkout "2029-02-05" and price 300
    Then the update operation should fail with status code 405

  @booking_partial_update @regression
  Scenario: Partially update a booking with only firstname and lastname
    Given I am an authenticated user
    And a booking is created with default valid details and its ID stored
    When I attempt to partially update the booking with firstname "PatchedFirst" and lastname "PatchedLast"
    Then the partial update operation should be successful
    And the booking retrieved by its stored ID should have firstname "PatchedFirst" and lastname "PatchedLast"
    And other details like total price and deposit paid for that booking should remain unchanged
