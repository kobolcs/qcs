using System.Text.Json.Serialization;

namespace SpecFlowApiTests.Models
{
    public class AuthRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; }

        [JsonPropertyName("password")]
        public string Password { get; }

        public AuthRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
