using System.Text.Json.Serialization;

namespace SpecFlowApiTests.Models
{
    public class AuthResponse
    {
        // NOTE: setter needed for System.Text.Json deserialization
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}
