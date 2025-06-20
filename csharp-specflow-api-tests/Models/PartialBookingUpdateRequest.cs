using System.Text.Json.Serialization;

namespace SpecFlowApiTests.Models
{
    public class PartialBookingUpdateRequest
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("firstname")]
        public string? Firstname { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("lastname")]
        public string? Lastname { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("totalprice")]
        public int? Totalprice { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("depositpaid")]
        public bool? Depositpaid { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bookingdates")]
        public BookingDates? Bookingdates { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("additionalneeds")]
        public string? Additionalneeds { get; set; }
    }
}
