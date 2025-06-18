namespace SpecFlowApiTests.Models
{
    using System;
    using System.Text.Json.Serialization;

    public class BookingDates
    {
        [JsonPropertyName("checkin")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly Checkin { get; set; }

        [JsonPropertyName("checkout")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly Checkout { get; set; }
    }
}
