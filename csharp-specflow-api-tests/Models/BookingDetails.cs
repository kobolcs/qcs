namespace SpecFlowApiTests.Models
{
    public class BookingDetails
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public int Totalprice { get; set; }
        public bool Depositpaid { get; set; }
        public BookingDates? Bookingdates { get; set; }
        public string? Additionalneeds { get; set; }
    }
}