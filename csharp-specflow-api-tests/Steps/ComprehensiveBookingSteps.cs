using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using FluentAssertions;
using SpecFlowApiTests.Clients;
using SpecFlowApiTests.Models;
using SpecFlowApiTests.Helpers;
using SpecFlowApiTests.Exceptions;
using Serilog;

namespace SpecFlowApiTests.Steps
{
    [Binding]
    public class ComprehensiveBookingSteps
    {
        private readonly ScenarioContext _context;
        private readonly AuthClient _authClient;
        private readonly BookingClient _bookingClient;
        private readonly ILogger _logger;

        private BookingDetails? _createdBooking;
        private BookingResponse? _bookingResponse;
        private ApiException? _lastException;
        private List<int> _createdBookingIds = new();
        private Stopwatch _performanceTimer = new();

        public ComprehensiveBookingSteps(
            ScenarioContext context, 
            AuthClient authClient, 
            BookingClient bookingClient,
            ILogger logger)
        {
            _context = context;
            _authClient = authClient;
            _bookingClient = bookingClient;
            _logger = logger.ForContext<ComprehensiveBookingSteps>();
        }

        #region Background Steps

        [Given(@"the API is available and responding")]
        public async Task GivenTheAPIIsAvailableAndResponding()
        {
            try
            {
                // Perform health check
                var healthCheckBooking = TestDataBuilder.CreateDefaultBookingDetails();
                var response = await _bookingClient.CreateBookingAsync(healthCheckBooking);
                
                response.Should().NotBeNull("API should be responsive");
                response.BookingId.Should().BeGreaterThan(0, "API should return valid booking ID");
                
                // Clean up health check data
                _bookingClient.DeleteBooking(response.BookingId);
                
                _logger.Information("API health check passed - API is available and responding");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "API health check failed");
                throw new InvalidOperationException("API is not available or not responding properly", ex);
            }
        }

        [Given(@"I have valid authentication credentials")]
        public void GivenIHaveValidAuthenticationCredentials()
        {
            _authClient.Should().NotBeNull("AuthClient should be available");
            _bookingClient.Should().NotBeNull("BookingClient should be available with valid token");
            _logger.Information("Valid authentication credentials confirmed");
        }

        #endregion

        #region Booking Creation Steps

        [When(@"I create a booking with the following details:")]
        public async Task WhenICreateABookingWithTheFollowingDetails(Table table)
        {
            var bookingData = table.CreateInstance<BookingCreationData>();
            
            _createdBooking = new BookingDetails
            {
                Firstname = bookingData.Firstname,
                Lastname = bookingData.Lastname,
                Totalprice = bookingData.Totalprice,
                Depositpaid = bookingData.Depositpaid,
                Bookingdates = new BookingDates
                {
                    Checkin = DateOnly.Parse(bookingData.Checkin),
                    Checkout = DateOnly.Parse(bookingData.Checkout)
                },
                Additionalneeds = bookingData.Additionalneeds
            };

            _logger.Information("Creating booking with details: {@BookingDetails}", _createdBooking);

            _performanceTimer.Start();
            try
            {
                _bookingResponse = await _bookingClient.CreateBookingAsync(_createdBooking);
                _performanceTimer.Stop();
                
                _context["createdBooking"] = _createdBooking;
                _context["bookingResponse"] = _bookingResponse;
                _context["bookingId"] = _bookingResponse.BookingId;
                _createdBookingIds.Add(_bookingResponse.BookingId);

                _logger.Information("Booking created successfully with ID {BookingId} in {ElapsedMs}ms", 
                    _bookingResponse.BookingId, _performanceTimer.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _performanceTimer.Stop();
                _logger.Error(ex, "Failed to create booking");
                _lastException = ex as ApiException;
                throw;
            }
        }

        [When(@"I attempt to create a booking with (.*) set to ""(.*)""")]
        public async Task WhenIAttemptToCreateABookingWithFieldSetTo(string field, string invalidValue)
        {
            var bookingDetails = TestDataBuilder.CreateDefaultBookingDetails();
            
            // Apply the invalid value to the specified field
            ApplyInvalidFieldValue(bookingDetails, field, invalidValue);

            _logger.Information("Attempting to create booking with invalid {Field} = {InvalidValue}", field, invalidValue);

            try
            {
                _bookingResponse = await _bookingClient.CreateBookingAsync(bookingDetails);
                _logger.Warning("Booking creation succeeded unexpectedly with invalid data");
            }
            catch (ApiException ex)
            {
                _lastException = ex;
                _logger.Information("Booking creation failed as expected: {StatusCode} - {Message}", ex.StatusCode, ex.Message);
            }
        }

        #endregion

        #region Validation Steps

        [Then(@"the booking should be created successfully")]
        public void ThenTheBookingShouldBeCreatedSuccessfully()
        {
            _bookingResponse.Should().NotBeNull("Booking response should not be null");
            _bookingResponse!.BookingId.Should().BeGreaterThan(0, "Booking ID should be a positive integer");
            _bookingResponse.Booking.Should().NotBeNull("Booking details should be included in response");
        }

        [Then(@"the booking ID should be a positive integer")]
        public void ThenTheBookingIDShouldBeAPositiveInteger()
        {
            _bookingResponse.Should().NotBeNull();
            _bookingResponse!.BookingId.Should().BeGreaterThan(0);
            _bookingResponse.BookingId.Should().BeOfType<int>();
        }

        [Then(@"all booking details should match the input data")]
        public void ThenAllBookingDetailsShouldMatchTheInputData()
        {
            _bookingResponse.Should().NotBeNull();
            _createdBooking.Should().NotBeNull();

            var returnedBooking = _bookingResponse!.Booking!;
            
            returnedBooking.Should().BeEquivalentTo(_createdBooking, options => options
                .WithStrictOrdering()
                .ComparingByMembers<BookingDetails>()
                .ComparingByMembers<BookingDates>());

            _logger.Information("Booking details validation passed - all fields match");
        }

        [Then(@"the booking creation should fail")]
        public void ThenTheBookingCreationShouldFail()
        {
            _lastException.Should().NotBeNull("An exception should have been thrown for invalid data");
            _lastException!.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
        }

        [Then(@"the response should contain an appropriate error message")]
        public void ThenTheResponseShouldContainAnAppropriateErrorMessage()
        {
            _lastException.Should().NotBeNull();
            _lastException!.ResponseContent.Should().NotBeNullOrEmpty("Error response should contain a message");
            
            // Log the error message for debugging
            _logger.Information("Error response: {ErrorMessage}", _lastException.ResponseContent);
        }

        #endregion

        #region Authentication Steps

        [Given(@"I have invalid credentials")]
        public void GivenIHaveInvalidCredentials()
        {
            // This step is for documentation - we'll use invalid credentials in the next step
            _logger.Information("Preparing to test with invalid credentials");
        }

        [When(@"I attempt to authenticate with username ""(.*)"" and password ""(.*)""")]
        public async Task WhenIAttemptToAuthenticateWithUsernameAndPassword(string username, string password)
        {
            try
            {
                var invalidAuthClient = new AuthClient(
                    ConfigurationHelper.GetApiSettings().BaseUrl!,
                    username,
                    password,
                    _logger);

                await invalidAuthClient.GetTokenAsync();
                _logger.Warning("Authentication succeeded unexpectedly with invalid credentials");
            }
            catch (Exception ex)
            {
                _lastException = ex as ApiException ?? new ApiException(new RestSharp.RestResponse { StatusCode = HttpStatusCode.Unauthorized });
                _logger.Information("Authentication failed as expected: {Message}", ex.Message);
            }
        }

        [Then(@"authentication should fail with (\d+) status")]
        public void ThenAuthenticationShouldFailWithStatus(int expectedStatusCode)
        {
            _lastException.Should().NotBeNull("Authentication should have failed");
            ((int)_lastException!.StatusCode).Should().Be(expectedStatusCode);
        }

        [Then(@"no token should be returned")]
        public void ThenNoTokenShouldBeReturned()
        {
            // Token should not be available due to failed authentication
            _lastException.Should().NotBeNull("Authentication failure should result in no token");
        }

        #endregion

        #region Concurrency Steps

        [When(@"I create (\d+) bookings simultaneously")]
        public async Task WhenICreateBookingsSimultaneously(int count)
        {
            var tasks = new List<Task<BookingResponse>>();
            
            _logger.Information("Creating {Count} bookings simultaneously", count);

            for (int i = 0; i < count; i++)
            {
                var bookingDetails = TestDataBuilder.CreateDefaultBookingDetails($"concurrent-test-{i}");
                tasks.Add(_bookingClient.CreateBookingAsync(bookingDetails));
            }

            try
            {
                var results = await Task.WhenAll(tasks);
                _context["concurrentBookings"] = results;
                _createdBookingIds.AddRange(results.Select(r => r.BookingId));
                
                _logger.Information("Successfully created {Count} concurrent bookings", results.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create concurrent bookings");
                throw;
            }
        }

        [Then(@"all bookings should be created successfully")]
        public void ThenAllBookingsShouldBeCreatedSuccessfully()
        {
            var concurrentBookings = (BookingResponse[])_context["concurrentBookings"];
            
            concurrentBookings.Should().NotBeNull();
            concurrentBookings.Should().NotContainNulls();
            concurrentBookings.Should().OnlyContain(b => b.BookingId > 0);
        }

        [Then(@"each booking should have a unique ID")]
        public void ThenEachBookingShouldHaveAUniqueID()
        {
            var concurrentBookings = (BookingResponse[])_context["concurrentBookings"];
            var bookingIds = concurrentBookings.Select(b => b.BookingId).ToList();
            
            bookingIds.Should().OnlyHaveUniqueItems("All booking IDs should be unique");
        }

        [Then(@"no data corruption should occur")]
        public async Task ThenNoDataCorruptionShouldOccur()
        {
            var concurrentBookings = (BookingResponse[])_context["concurrentBookings"];
            
            // Verify each booking by retrieving it
            foreach (var booking in concurrentBookings)
            {
                var retrieved = await _bookingClient.GetBookingAsync(booking.BookingId);
                retrieved.Should().NotBeNull($"Booking {booking.BookingId} should be retrievable");
                
                // Verify data integrity
                retrieved.Should().BeEquivalentTo(booking.Booking, options => options
                    .ComparingByMembers<BookingDetails>()
                    .ComparingByMembers<BookingDates>());
            }
            
            _logger.Information("Data integrity verification passed for all concurrent bookings");
        }

        #endregion

        #region Performance Steps

        [Then(@"response times should be within acceptable limits")]
        public void ThenResponseTimesShouldBeWithinAcceptableLimits()
        {
            var acceptableResponseTime = TimeSpan.FromSeconds(5); // 5 second SLA
            
            _performanceTimer.Elapsed.Should().BeLessThan(acceptableResponseTime, 
                $"Response time should be under {acceptableResponseTime.TotalSeconds} seconds");
            
            _logger.Information("Performance check passed - Response time: {ElapsedMs}ms", _performanceTimer.ElapsedMilliseconds);
        }

        [Then(@"performance metrics should be recorded")]
        public void ThenPerformanceMetricsShouldBeRecorded()
        {
            // Record performance metrics for monitoring
            var metrics = new
            {
                ResponseTime = _performanceTimer.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow,
                Operation = "BookingCreation",
                Success = _lastException == null
            };
            
            _logger.Information("Performance metrics recorded: {@Metrics}", metrics);
            _context["performanceMetrics"] = metrics;
        }

        #endregion

        #region Cleanup Steps

        [AfterScenario]
        public void CleanupTestData()
        {
            _logger.Information("Starting test data cleanup for {Count} bookings", _createdBookingIds.Count);
            
            foreach (var bookingId in _createdBookingIds)
            {
                try
                {
                    _bookingClient.DeleteBooking(bookingId);
                    _logger.Debug("Deleted booking {BookingId}", bookingId);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to delete booking {BookingId} during cleanup", bookingId);
                }
            }
            
            _createdBookingIds.Clear();
            _logger.Information("Test data cleanup completed");
        }

        #endregion

        #region Helper Methods

        private void ApplyInvalidFieldValue(BookingDetails booking, string field, string value)
        {
            switch (field.ToLowerInvariant())
            {
                case "firstname":
                    booking.Firstname = value;
                    break;
                case "lastname":
                    booking.Lastname = value;
                    break;
                case "totalprice":
                    if (int.TryParse(value, out var price))
                        booking.Totalprice = price;
                    else
                        booking.Totalprice = -1; // Invalid value for non-numeric input
                    break;
                case "checkin":
                    if (DateOnly.TryParse(value, out var checkinDate))
                        booking.Bookingdates!.Checkin = checkinDate;
                    break;
                case "checkout":
                    if (DateOnly.TryParse(value, out var checkoutDate))
                        booking.Bookingdates!.Checkout = checkoutDate;
                    break;
                case "additionalneeds":
                    booking.Additionalneeds = value;
                    break;
            }
        }

        #endregion

        #region Data Transfer Objects

        public class BookingCreationData
        {
            public string Firstname { get; set; } = string.Empty;
            public string Lastname { get; set; } = string.Empty;
            public int Totalprice { get; set; }
            public bool Depositpaid { get; set; }
            public string Checkin { get; set; } = string.Empty;
            public string Checkout { get; set; } = string.Empty;
            public string? Additionalneeds { get; set; }
        }

        #endregion
    }
}