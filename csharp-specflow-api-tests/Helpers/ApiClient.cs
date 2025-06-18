using RestSharp;
using System;

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

            var options = new RestClientOptions(baseUrl)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            return new RestClient(options);
        }
    }
}
