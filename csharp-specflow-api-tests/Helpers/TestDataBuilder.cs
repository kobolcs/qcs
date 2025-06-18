using Bogus; // NOTE: Bogus generates realistic random values for independence
using SpecFlowApiTests.Models;
using System;

namespace SpecFlowApiTests.Helpers
{
    public static class TestDataBuilder
    {
        public static BookingDetails CreateDefaultBookingDetails()
        {
            // NOTE: Generate unique booking data so tests do not share state
            var bookingFaker = new Faker<BookingDetails>()
                .RuleFor(b => b.Firstname, f => f.Name.FirstName())
                .RuleFor(b => b.Lastname, f => f.Name.LastName())
                .RuleFor(b => b.Totalprice, f => f.Random.Int(100, 2000))
                .RuleFor(b => b.Depositpaid, f => f.Random.Bool())
                .RuleFor(b => b.Additionalneeds, f => f.PickRandom("Breakfast", "Early check-in", "None", "Sea view"))
                .RuleFor(b => b.Bookingdates, (f, b) =>
                {
                    var checkinDate = f.Date.Future(1, DateTime.Now.AddMonths(2));
                    var checkoutDate = checkinDate.AddDays(f.Random.Int(2, 14));
                    return new BookingDates
                    {
                        Checkin = DateOnly.FromDateTime(checkinDate),
                        Checkout = DateOnly.FromDateTime(checkoutDate)
                    };
                });

            return bookingFaker.Generate();
        }

        public static BookingDetails CreateCustomBookingDetails(
            string firstname,
            string lastname,
            int totalprice,
            bool depositpaid,
            DateOnly checkin,
            DateOnly checkout,
            string? additionalneeds = null)
        {
            return new BookingDetails
            {
                Firstname = firstname,
                Lastname = lastname,
                Totalprice = totalprice,
                Depositpaid = depositpaid,
                Bookingdates = new BookingDates
                {
                    Checkin = checkin,
                    Checkout = checkout
                },
                Additionalneeds = additionalneeds
            };
        }

        public static BookingDetails CreateUpdatedBookingDetails(BookingDetails originalDetails, DateOnly newCheckin, DateOnly newCheckout, int newPrice)
        {
            return new BookingDetails
            {
                Firstname = originalDetails.Firstname,
                Lastname = originalDetails.Lastname,
                Totalprice = newPrice,
                Depositpaid = !originalDetails.Depositpaid,
                Bookingdates = new BookingDates
                {
                    Checkin = newCheckin,
                    Checkout = newCheckout
                },
                Additionalneeds = (originalDetails.Additionalneeds ?? string.Empty) + " (Updated)"
            };
        }

        public static PartialBookingUpdateRequest CreatePartialBookingUpdateRequest(string? firstname = null, string? lastname = null, int? totalprice = null)
        {
            return new PartialBookingUpdateRequest
            {
                Firstname = firstname,
                Lastname = lastname,
                Totalprice = totalprice
            };
        }
    }
}
