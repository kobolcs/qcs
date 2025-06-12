using System;
using System.Net;
using RestSharp;

namespace SpecFlowApiTests.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ResponseContent { get; }

        public ApiException(RestResponse response)
            : base(BuildErrorMessage(response))
        {
            StatusCode = response.StatusCode;
            ResponseContent = response.Content;
        }

        private static string BuildErrorMessage(RestResponse response)
        {
            return $"API call failed with status {response.StatusCode}. " +
                   $"URI: {response.ResponseUri}. " +
                   $"Content: {response.Content ?? "No Content"}.";
        }
    }
}