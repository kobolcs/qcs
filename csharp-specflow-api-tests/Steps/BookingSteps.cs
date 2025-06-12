using System.Net;
using TechTalk.SpecFlow;
using FluentAssertions;
using SpecFlowApiTests.Clients;
using SpecFlowApiTests.Models;
using SpecFlowApiTests.Helpers;
using SpecFlowApiTests.Exceptions; // Import the custom exception

namespace SpecFlowApiTests.Steps
{
    [Binding]
    public class BookingSteps
    {
        private readonly ScenarioContext _context;
        private readonly AuthClient _authClient;
        private readonly BookingClient _bookingClient;

        private BookingDetails? _originalDetails;

        public BookingSteps(ScenarioContext context, AuthClient authClient, BookingClient bookingClient)
        {
            _context = context;
            _authClient = authClient;
            _bookingClient = bookingClient;
        }

        [Given(@"I am an authenticated user")]
        public void GivenIAmAnAuthenticatedUser()
        {
            _bookingClient.Should().NotBeNull("BookingClient was not injected.");
            _authClient.Should().NotBeNull("AuthClient was not injected.");
        }

        [When(@"I create a new booking with valid details")]
        public void WhenICreateANewBookingWithValidDetails()
        {
            _originalDetails = TestDataBuilder.CreateDefaultBookingDetails();
            // The CreateBooking method now returns the BookingResponse object directly.
            var bookingResponseData = _bookingClient.CreateBooking(_originalDetails);

            bookingResponseData.Should().NotBeNull("CreateBooking response data is null.");
            bookingResponseData.BookingId.Should().BeGreaterThan(0, "Booking ID should be greater than zero.");

            _context["bookingId"] = bookingResponseData.BookingId;
            _context["originalDetails"] = _originalDetails;
        }

        [Then(@"the booking should exist in the system")]
        public void ThenTheBookingShouldExistInTheSystem()
        {
            var bookingId = (int)_context["bookingId"];
            var originalDetails = (BookingDetails?)_context["originalDetails"];

            originalDetails.Should().NotBeNull("Original booking details should be in the context.");

            // The GetBooking method now returns the BookingDetails object directly.
            var bookingData = _bookingClient.GetBooking(bookingId);

            bookingData.Should().NotBeNull();
            bookingData.Firstname.Should().Be(originalDetails.Firstname);
            bookingData.Lastname.Should().Be(originalDetails.Lastname);
        }

        [When(@"I delete the booking")]
        public void WhenIDeleteTheBooking()
        {
            var bookingId = (int)_context["bookingId"];
            // DeleteBooking now returns void on success or throws an exception on failure.
            _bookingClient.DeleteBooking(bookingId);
        }

        [Then(@"the booking should no longer exist")]
        public void ThenTheBookingShouldNoLongerExist()
        {
            var bookingId = (int)_context["bookingId"];

            // This is the new pattern for testing expected errors.
            // We define the action that should fail.
            Action act = () => _bookingClient.GetBooking(bookingId);

            // Assert that the action throws our custom exception.
            var exception = act.Should().Throw<ApiException>("getting a deleted booking should fail").And;

            // We can now assert on the details of the exception itself.
            exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}