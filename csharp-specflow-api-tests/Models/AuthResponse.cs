namespace SpecFlowApiTests.Models
{
    public class AuthResponse
    {
        // NOTE: setter needed for System.Text.Json deserialization
        public string? Token { get; set; }
    }
}
