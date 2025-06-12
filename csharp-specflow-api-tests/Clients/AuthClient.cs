using RestSharp;
using SpecFlowApiTests.Helpers;
using SpecFlowApiTests.Models;
using System.Net;
using System.Threading.Tasks;
using Serilog;
using System;

namespace SpecFlowApiTests.Clients
{
    public class AuthClient
    {
        private readonly RestClient _client;
        private readonly string _username;
        private readonly string _password;
        private readonly string _authEndpoint;
        private readonly ILogger _logger;

        public AuthClient(string baseUrl, string username, string password, ILogger logger, string authEndpoint = "/auth")
        {
            _client = ApiClient.GetClient(baseUrl);
            _username = username;
            _password = password;
            _authEndpoint = authEndpoint;
            _logger = logger.ForContext<AuthClient>();
        }

        public async Task<string> GetTokenAsync()
        {
            var authRequestPayload = new AuthRequest(_username, _password);
            var request = new RestRequest(_authEndpoint, Method.Post);
            request.AddJsonBody(authRequestPayload);

            _logger.Information("Attempting to authenticate user: {Username}", _username);

            var response = await _client.ExecuteAsync<AuthResponse>(request);

            if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Data?.Token))
            {
                _logger.Information("Authentication successful for user: {Username}", _username);
                return response.Data.Token;
            }

            _logger.Error("Authentication failed. Status: {StatusCode}, Content: {Content}", response.StatusCode, response.Content);
            throw new System.Security.Authentication.AuthenticationException($"Authentication failed. Status: {response.StatusCode}. Content: {response.Content}");
        }
    }
}