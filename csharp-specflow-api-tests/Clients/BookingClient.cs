using RestSharp;
using SpecFlowApiTests.Helpers;
using SpecFlowApiTests.Models;
using System.Text.Json;
using Serilog;
using SpecFlowApiTests.Exceptions;
using Polly;

namespace SpecFlowApiTests.Clients
{
    public class BookingClient
    {
        private readonly RestClient _client;
        private readonly string? _token;
        private readonly ILogger _logger;

        public BookingClient(string baseUrl, string? token, ILogger logger)
        {
            _client = ApiClient.GetClient(baseUrl);
            _token = token;
            _logger = logger.ForContext<BookingClient>();
        }

        private void AddAuthHeaders(RestRequest request)
        {
            if (!string.IsNullOrEmpty(_token))
            {
                // For Restful-Booker, the auth token is sent as a cookie.
                request.AddHeader("Cookie", $"token={_token}");
            }
        }

        public BookingResponse CreateBooking(BookingDetails payload)
        {
            var request = new RestRequest("/booking", Method.Post);
            request.AddJsonBody(payload);

            LogRequest("CreateBooking", request, payload);
            var response = _client.Execute<BookingResponse>(request);
            LogResponse("CreateBooking", response);

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new ApiException(response);
            }
            return response.Data;
        }

        public async Task<BookingResponse> CreateBookingAsync(BookingDetails payload)
        {
            var request = new RestRequest("/booking", Method.Post);
            request.AddJsonBody(payload);
            AddAuthHeaders(request);
            LogRequest("CreateBooking", request, payload);

            var policy = Policy
                .Handle<Exception>()
                .OrResult<RestResponse<BookingResponse>>(r => r.StatusCode == System.Net.HttpStatusCode.RequestTimeout || (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryCount, context) =>
                    {
                        _logger.Warning($"Retry {retryCount} for CreateBooking due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });

            var response = await policy.ExecuteAsync(() => _client.ExecuteAsync<BookingResponse>(request));
            LogResponse("CreateBooking", response);

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new ApiException(response);
            }
            return response.Data;
        }

        public BookingDetails GetBooking(int bookingId)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Get);
            LogRequest("GetBooking", request);
            var response = _client.Execute<BookingDetails>(request);
            LogResponse("GetBooking", response);

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new ApiException(response);
            }
            return response.Data;
        }

        public async Task<BookingDetails> GetBookingAsync(int bookingId)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Get);
            AddAuthHeaders(request);
            LogRequest("GetBooking", request);

            var policy = Policy
                .Handle<Exception>()
                .OrResult<RestResponse<BookingDetails>>(r => r.StatusCode == System.Net.HttpStatusCode.RequestTimeout || (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryCount, context) =>
                    {
                        _logger.Warning($"Retry {retryCount} for GetBooking due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });

            var response = await policy.ExecuteAsync(() => _client.ExecuteAsync<BookingDetails>(request));
            LogResponse("GetBooking", response);

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new ApiException(response);
            }
            return response.Data;
        }

        public BookingDetails UpdateBooking(int bookingId, BookingDetails payload)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Put);
            AddAuthHeaders(request);
            request.AddJsonBody(payload);

            LogRequest("UpdateBooking", request, payload);
            var response = _client.Execute<BookingDetails>(request);
            LogResponse("UpdateBooking", response);

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new ApiException(response);
            }
            return response.Data;
        }

        public BookingDetails PartialUpdateBooking(int bookingId, PartialBookingUpdateRequest payload)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Patch);
            AddAuthHeaders(request);
            request.AddJsonBody(payload);

            LogRequest("PartialUpdateBooking", request, payload);
            var response = _client.Execute<BookingDetails>(request);
            LogResponse("PartialUpdateBooking", response);

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new ApiException(response);
            }
            return response.Data;
        }

        public void DeleteBooking(int bookingId)
        {
            var request = new RestRequest($"/booking/{bookingId}", Method.Delete);
            AddAuthHeaders(request);
            LogRequest("DeleteBooking", request);
            var response = _client.Execute(request);
            LogResponse("DeleteBooking", response);

            if (!response.IsSuccessful)
            {
                throw new ApiException(response);
            }
        }

        private void LogRequest(string endpointName, RestRequest request, object? payload = null)
        {
            _logger.Debug("[{Endpoint}] Request: {Method} {Uri}", endpointName, request.Method, _client.BuildUri(request));
            if (payload != null && request.Method != Method.Get)
            {
                _logger.Debug("[{Endpoint}] Request Body: {Payload}", endpointName, JsonSerializer.Serialize(payload));
            }
        }

        private void LogResponse(string endpointName, RestResponse response)
        {
            if (response.IsSuccessful)
            {
                _logger.Information("[{Endpoint}] Response Status: {StatusCode}", endpointName, response.StatusCode);
                _logger.Debug("[{Endpoint}] Response Body: {Content}", endpointName, response.Content);
            }
            else
            {
                _logger.Error("[{Endpoint}] Response Status: {StatusCode}. Content: {Content}", endpointName, response.StatusCode, response.Content);
            }

            if (response.ErrorException != null)
            {
                _logger.Error(response.ErrorException, "[{Endpoint}] An exception occurred during the request.", endpointName);
            }
        }
    }
}
