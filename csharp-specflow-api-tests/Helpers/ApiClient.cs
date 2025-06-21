using RestSharp;
using System;
using System.Net;

namespace SpecFlowApiTests.Helpers
{
    public static class ApiClient
    {
        /// <summary>
        /// Creates and configures a RestClient with the specified base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL for the API.</param>
        /// <returns>A configured RestClient instance.</returns>
        /// <exception cref="ArgumentException">Thrown if baseUrl is null or whitespace.</exception>
        public static RestClient GetClient(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL cannot be null or whitespace.", nameof(baseUrl));

            // Enforce TLS 1.2 for all outbound requests
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            int timeoutSeconds = 30;
            var timeoutEnv = Environment.GetEnvironmentVariable("API_CLIENT_TIMEOUT_SECONDS");
            if (!string.IsNullOrEmpty(timeoutEnv) && int.TryParse(timeoutEnv, out var parsedTimeout))
            {
                timeoutSeconds = parsedTimeout;
            }

            var options = new RestClientOptions(baseUrl)
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };

            var client = new RestClient(options);
            // Ensure the API receives headers expected by the Restful Booker service
            client.AddDefaultHeader("Accept", "application/json");
            client.AddDefaultHeader("User-Agent", "Mozilla/5.0");

            return client;
        }
    }
}
