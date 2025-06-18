namespace SpecFlowApiTests.Configuration
{
    public class ApiSettings
    {
        public string? BaseUrl { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
                throw new InvalidOperationException("BaseUrl is required.");
        }
    }
}
