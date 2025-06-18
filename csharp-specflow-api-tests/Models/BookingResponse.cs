using System.Text.Json.Serialization;

namespace SpecFlowApiTests.Models
{
    public class BookingResponse
    {
        [JsonPropertyName("bookingid")]
        public int BookingId { get; set; }

        [JsonPropertyName("booking")]
        public BookingDetails? Booking { get; set; }
    }
}
