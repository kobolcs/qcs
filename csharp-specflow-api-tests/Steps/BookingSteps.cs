using System.Net;
using TechTalk.SpecFlow;
using FluentAssertions;
using SpecFlowApiTests.Clients;
using SpecFlowApiTests.Models;
using SpecFlowApiTests.Helpers;
using SpecFlowApiTests.Exceptions; // NOTE: custom exception exposes status code for API failures

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

            var bookingData = _bookingClient.GetBooking(bookingId);

            bookingData.Should().NotBeNull();
            bookingData.Firstname.Should().Be(originalDetails.Firstname);
            bookingData.Lastname.Should().Be(originalDetails.Lastname);
        }

        [When(@"I delete the booking")]
        public void WhenIDeleteTheBooking()
        {
            var bookingId = (int)_context["bookingId"];
            _bookingClient.DeleteBooking(bookingId);
        }

        [Then(@"the booking should no longer exist")]
        public void ThenTheBookingShouldNoLongerExist()
        {
            var bookingId = (int)_context["bookingId"];

            // NOTE: capturing the failing action lets us assert on ApiException details
            Action act = () => _bookingClient.GetBooking(bookingId);
            var exception = act.Should().Throw<ApiException>("getting a deleted booking should fail").And;
            exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}