namespace SpecFlowApiTests.Models
{
    public class AuthRequest
    {
        public string Username { get; }
        public string Password { get; }

        public AuthRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}