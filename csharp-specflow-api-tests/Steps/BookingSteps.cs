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
        private BookingDetails? _updatedDetails;
        private ApiException? _caughtException;

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

        [When(@"I create a new booking with default valid details")]
        public void WhenICreateANewBookingWithDefaultValidDetails()
        {
            _originalDetails = TestDataBuilder.CreateDefaultBookingDetails();
            var bookingResponseData = _bookingClient.CreateBooking(_originalDetails);

            bookingResponseData.Should().NotBeNull("CreateBooking response data is null.");
            bookingResponseData.BookingId.Should().BeGreaterThan(0, "Booking ID should be greater than zero.");

            _context["bookingId"] = bookingResponseData.BookingId;
            _context["originalDetails"] = _originalDetails;
        }

        [Then(@"the booking creation should be successful")]
        public void ThenTheBookingCreationShouldBeSuccessful()
        {
            _context.ContainsKey("bookingId").Should().BeTrue("booking should be created");
            var bookingId = (int)_context["bookingId"];
            bookingId.Should().BeGreaterThan(0);
        }

        [Then(@"the booking details should be stored")]
        public void ThenTheBookingDetailsShouldBeStored()
        {
            _context.ContainsKey("originalDetails").Should().BeTrue();
            _context["originalDetails"].Should().BeOfType<BookingDetails>();
        }
        [Then(@"the booking should exist in the system with the stored details")]
        public void ThenTheBookingShouldExistInTheSystemWithTheStoredDetails()
        {
            var bookingId = (int)_context["bookingId"];
            var originalDetails = (BookingDetails?)_context["originalDetails"];

            originalDetails.Should().NotBeNull("Original booking details should be in the context.");

            var bookingData = _bookingClient.GetBooking(bookingId);

            bookingData.Should().NotBeNull();
            bookingData.Firstname.Should().Be(originalDetails.Firstname);
            bookingData.Lastname.Should().Be(originalDetails.Lastname);
            bookingData.Totalprice.Should().Be(originalDetails.Totalprice);
            bookingData.Depositpaid.Should().Be(originalDetails.Depositpaid);
            bookingData.Additionalneeds.Should().Be(originalDetails.Additionalneeds);
            bookingData.Bookingdates.Should().NotBeNull();
            bookingData.Bookingdates!.Checkin.Should().Be(originalDetails.Bookingdates!.Checkin);
            bookingData.Bookingdates.Checkout.Should().Be(originalDetails.Bookingdates.Checkout);
        }

        [When(@"I update the booking with new checkin \"(.*)\", checkout \"(.*)\", and price (\d+)")]
        public void WhenIUpdateTheBooking(string newCheckin, string newCheckout, int newPrice)
        {
            var bookingId = (int)_context["bookingId"];
            var original = (BookingDetails)_context["originalDetails"]!;
            var payload = TestDataBuilder.CreateUpdatedBookingDetails(original, DateOnly.Parse(newCheckin), DateOnly.Parse(newCheckout), newPrice);
            _updatedDetails = _bookingClient.UpdateBooking(bookingId, payload);
        }

        [Then(@"the booking update should be successful")]
        public void ThenTheBookingUpdateShouldBeSuccessful()
        {
            _updatedDetails.Should().NotBeNull();
        }

        [Then(@"the updated booking details should be stored correctly")]
        public void ThenTheUpdatedBookingDetailsShouldBeStoredCorrectly()
        {
            var bookingId = (int)_context["bookingId"];
            var bookingData = _bookingClient.GetBooking(bookingId);
            bookingData.Should().NotBeNull();
            bookingData.Totalprice.Should().Be(_updatedDetails!.Totalprice);
            bookingData.Bookingdates!.Checkin.Should().Be(_updatedDetails.Bookingdates!.Checkin);
            bookingData.Bookingdates.Checkout.Should().Be(_updatedDetails.Bookingdates.Checkout);
        }

        [When(@"I attempt to update a booking with a non-existent ID (\d+) using new checkin \"(.*)\" and checkout \"(.*)\" and price (\d+)")]
        public void WhenIAttemptToUpdateANonExistentBooking(int bookingId, string checkin, string checkout, int price)
        {
            var payload = TestDataBuilder.CreateCustomBookingDetails(
                "Ghost", "User", price, true,
                DateOnly.Parse(checkin), DateOnly.Parse(checkout));
            try
            {
                _bookingClient.UpdateBooking(bookingId, payload);
            }
            catch (ApiException ex)
            {
                _caughtException = ex;
            }
        }

        [Then(@"the update operation should fail with status code (\d+)")]
        public void ThenTheUpdateOperationShouldFailWithStatusCode(int statusCode)
        {
            _caughtException.Should().NotBeNull("an exception should have been thrown");
            ((int)_caughtException!.StatusCode).Should().Be(statusCode);
        }

        [Given(@"a booking is created with default valid details and its ID stored")]
        public void GivenABookingIsCreatedWithDefaultValidDetailsAndItsIdStored()
        {
            WhenICreateANewBookingWithDefaultValidDetails();
        }

        [When(@"I attempt to partially update the booking with firstname \"(.*)\" and lastname \"(.*)\"")]
        public void WhenIAttemptToPartiallyUpdateTheBooking(string firstname, string lastname)
        {
            var bookingId = (int)_context["bookingId"];
            var payload = TestDataBuilder.CreatePartialBookingUpdateRequest(firstname, lastname);
            _updatedDetails = _bookingClient.PartialUpdateBooking(bookingId, payload);
        }

        [Then(@"the partial update operation should be successful")]
        public void ThenThePartialUpdateOperationShouldBeSuccessful()
        {
            _updatedDetails.Should().NotBeNull();
        }

        [Then(@"the booking retrieved by its stored ID should have firstname \"(.*)\" and lastname \"(.*)\"")]
        public void ThenTheBookingRetrievedByItsStoredIdShouldHaveFirstnameAndLastname(string firstname, string lastname)
        {
            var bookingId = (int)_context["bookingId"];
            var bookingData = _bookingClient.GetBooking(bookingId);
            bookingData.Firstname.Should().Be(firstname);
            bookingData.Lastname.Should().Be(lastname);
        }

        [Then(@"other details like total price and deposit paid for that booking should remain unchanged")]
        public void ThenOtherDetailsShouldRemainUnchanged()
        {
            var bookingId = (int)_context["bookingId"];
            var bookingData = _bookingClient.GetBooking(bookingId);
            var original = (BookingDetails)_context["originalDetails"]!;
            bookingData.Totalprice.Should().Be(original.Totalprice);
            bookingData.Depositpaid.Should().Be(original.Depositpaid);
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
