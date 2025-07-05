using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SpecFlowApiTests.Configuration
{
    /// <summary>
    /// Configuration settings for API testing with comprehensive validation
    /// </summary>
    public class ApiSettings
    {
        /// <summary>
        /// Base URL for the API under test
        /// </summary>
        [Required]
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Username for API authentication
        /// </summary>
        [Required]
        public string? Username { get; set; }

        /// <summary>
        /// Password for API authentication
        /// </summary>
        [Required]
        public string? Password { get; set; }

        /// <summary>
        /// HTTP client timeout in seconds (default: 30)
        /// </summary>
        [Range(5, 300)]
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum number of retry attempts for failed requests (default: 3)
        /// </summary>
        [Range(0, 10)]
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Enable detailed logging for debugging (default: false)
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;

        /// <summary>
        /// API rate limit per minute (default: 60)
        /// </summary>
        [Range(1, 1000)]
        public int RateLimitPerMinute { get; set; } = 60;

        /// <summary>
        /// Connection pool size for HTTP client (default: 10)
        /// </summary>
        [Range(1, 100)]
        public int ConnectionPoolSize { get; set; } = 10;

        /// <summary>
        /// Enable request/response logging (default: true for test environments)
        /// </summary>
        public bool EnableRequestLogging { get; set; } = true;

        /// <summary>
        /// Custom headers to include with all requests
        /// </summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new();

        /// <summary>
        /// Validates all configuration settings
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
        public void Validate()
        {
            var errors = new List<string>();

            // Validate BaseUrl
            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                errors.Add("BaseUrl is required");
            }
            else if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) || 
                     (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                errors.Add("BaseUrl must be a valid HTTP/HTTPS URL");
            }

            // Validate Username
            if (string.IsNullOrWhiteSpace(Username))
            {
                errors.Add("Username is required");
            }
            else if (Username.Length < 2)
            {
                errors.Add("Username must be at least 2 characters long");
            }

            // Validate Password
            if (string.IsNullOrWhiteSpace(Password))
            {
                errors.Add("Password is required");
            }
            else if (Password.Length < 3)
            {
                errors.Add("Password must be at least 3 characters long");
            }

            // Validate TimeoutSeconds
            if (TimeoutSeconds < 5 || TimeoutSeconds > 300)
            {
                errors.Add("TimeoutSeconds must be between 5 and 300 seconds");
            }

            // Validate MaxRetries
            if (MaxRetries < 0 || MaxRetries > 10)
            {
                errors.Add("MaxRetries must be between 0 and 10");
            }

            // Validate RateLimitPerMinute
            if (RateLimitPerMinute < 1 || RateLimitPerMinute > 1000)
            {
                errors.Add("RateLimitPerMinute must be between 1 and 1000");
            }

            // Validate ConnectionPoolSize
            if (ConnectionPoolSize < 1 || ConnectionPoolSize > 100)
            {
                errors.Add("ConnectionPoolSize must be between 1 and 100");
            }

            // Validate CustomHeaders
            if (CustomHeaders.Any(header => string.IsNullOrWhiteSpace(header.Key)))
            {
                errors.Add("Custom header names cannot be null or whitespace");
            }

            if (errors.Any())
            {
                throw new InvalidOperationException($"Configuration validation failed: {string.Join(", ", errors)}");
            }
        }

        /// <summary>
        /// Gets the timeout as a TimeSpan
        /// </summary>
        public TimeSpan GetTimeout() => TimeSpan.FromSeconds(TimeoutSeconds);

        /// <summary>
        /// Checks if the configuration is for a secure endpoint
        /// </summary>
        public bool IsSecureEndpoint()
        {
            return Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) && 
                   uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a sanitized version of the settings for logging (without sensitive data)
        /// </summary>
        public object ToLogSafeObject()
        {
            return new
            {
                BaseUrl,
                Username,
                Password = "***", // Masked for security
                TimeoutSeconds,
                MaxRetries,
                EnableDetailedLogging,
                RateLimitPerMinute,
                ConnectionPoolSize,
                EnableRequestLogging,
                CustomHeadersCount = CustomHeaders.Count,
                IsSecure = IsSecureEndpoint()
            };
        }

        /// <summary>
        /// Creates a copy of the settings with environment-specific overrides
        /// </summary>
        public ApiSettings WithEnvironmentOverrides(string environment)
        {
            var copy = new ApiSettings
            {
                BaseUrl = BaseUrl,
                Username = Username,
                Password = Password,
                TimeoutSeconds = TimeoutSeconds,
                MaxRetries = MaxRetries,
                EnableDetailedLogging = EnableDetailedLogging,
                RateLimitPerMinute = RateLimitPerMinute,
                ConnectionPoolSize = ConnectionPoolSize,
                EnableRequestLogging = EnableRequestLogging,
                CustomHeaders = new Dictionary<string, string>(CustomHeaders)
            }

            // Apply environment-specific settings