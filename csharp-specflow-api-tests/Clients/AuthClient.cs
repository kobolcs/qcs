using RestSharp;
using SpecFlowApiTests.Helpers;
using SpecFlowApiTests.Models;
using System.Net;
using System.Threading.Tasks;
using Serilog;
using System;
using Polly;

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

            var policy = Policy
                .Handle<Exception>()
                .OrResult<RestResponse<AuthResponse>>(r => r.StatusCode == HttpStatusCode.RequestTimeout || (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryCount, context) =>
                    {
                        _logger.Warning($"Retry {retryCount} for authentication due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });

            var response = await policy.ExecuteAsync(() => _client.ExecuteAsync<AuthResponse>(request));

            if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Data?.Token))
            {
                _logger.Information("Authentication successful for user: {Username}", _username);
                return response.Data.Token;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.Error("Authentication failed: Unauthorized. Check credentials for user: {Username}", _username);
            }
            else if ((int)response.StatusCode == 418)
            {
                _logger.Error("Authentication failed: HTTP 418 (I'm a teapot) for user: {Username}", _username);
            }
            else
            {
                _logger.Error("Authentication failed. Status: {StatusCode}, Content: {Content}", response.StatusCode, response.Content);
            }
            throw new System.Security.Authentication.AuthenticationException($"Authentication failed. Status: {response.StatusCode}. Content: {response.Content}");
        }
    }
}
