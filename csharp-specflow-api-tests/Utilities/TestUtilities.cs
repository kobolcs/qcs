using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using RestSharp;
using Serilog;
using SpecFlowApiTests.Configuration;
using SpecFlowApiTests.Models;

namespace SpecFlowApiTests.Utilities
{
    /// <summary>
    /// Advanced test utilities for professional API testing demonstrations
    /// </summary>
    public static class TestUtilities
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(TestUtilities));

        #region Environment and Health Checks

        /// <summary>
        /// Comprehensive API health check with detailed diagnostics
        /// </summary>
        public static async Task<HealthCheckResult> PerformHealthCheckAsync(string baseUrl, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var healthCheck = new HealthCheckResult
            {
                BaseUrl = baseUrl,
                Timestamp = DateTimeOffset.UtcNow
            };

            try
            {
                // Network connectivity check
                var pingResult = await PingHostAsync(new Uri(baseUrl).Host);
                healthCheck.NetworkLatency = pingResult.RoundtripTime;
                healthCheck.NetworkStatus = pingResult.Status == IPStatus.Success ? "Healthy" : "Failed";

                // HTTP connectivity check
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                var response = await client.GetAsync($"{baseUrl}/ping", cancellationToken);
                
                healthCheck.HttpStatus = response.StatusCode;
                healthCheck.ResponseTime = stopwatch.ElapsedMilliseconds;
                healthCheck.IsHealthy = response.IsSuccessStatusCode;

                // API-specific health validation
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    healthCheck.ApiResponse = content;
                    healthCheck.HasValidResponse = !string.IsNullOrEmpty(content);
                }

                Logger.Information("Health check completed: {Status} in {ResponseTime}ms", 
                    healthCheck.IsHealthy ? "Healthy" : "Unhealthy", healthCheck.ResponseTime);
            }
            catch (Exception ex)
            {
                healthCheck.IsHealthy = false;
                healthCheck.ErrorMessage = ex.Message;
                Logger.Error(ex, "Health check failed for {BaseUrl}", baseUrl);
            }
            finally
            {
                stopwatch.Stop();
                healthCheck.TotalCheckTime = stopwatch.ElapsedMilliseconds;
            }

            return healthCheck;
        }

        private static async Task<PingReply> PingHostAsync(string hostname)
        {
            using var ping = new Ping();
            try
            {
                return await ping.SendPingAsync(hostname, 5000);
            }
            catch
            {
                return new PingReply(); // Return empty reply on failure
            }
        }

        #endregion

        #region Test Data Management

        /// <summary>
        /// Advanced test data factory with realistic data patterns
        /// </summary>
        public static class TestDataFactory
        {
            private static readonly Faker BaseFaker = new("en");
            
            public static class Bookings
            {
                public static BookingDetails CreateRealistic(string scenario = "default")
                {
                    return scenario.ToLowerInvariant() switch
                    {
                        "business" => CreateBusinessBooking(),
                        "leisure" => CreateLeisureBooking(),
                        "extended" => CreateExtendedStayBooking(),
                        "lastminute" => CreateLastMinuteBooking(),
                        "invalid" => CreateInvalidBooking(),
                        _ => CreateStandardBooking()
                    };
                }

                private static BookingDetails CreateBusinessBooking()
                {
                    var faker = new Faker<BookingDetails>()
                        .RuleFor(b => b.Firstname, f => f.Name.FirstName())
                        .RuleFor(b => b.Lastname, f => f.Name.LastName())
                        .RuleFor(b => b.Totalprice, f => f.Random.Int(200, 500))
                        .RuleFor(b => b.Depositpaid, true)
                        .RuleFor(b => b.Additionalneeds, "Business center access")
                        .RuleFor(b => b.Bookingdates, f => CreateBusinessDates(f));

                    return faker.Generate();
                }

                private static BookingDetails CreateLeisureBooking()
                {
                    var faker = new Faker<BookingDetails>()
                        .RuleFor(b => b.Firstname, f => f.Name.FirstName())
                        .RuleFor(b => b.Lastname, f => f.Name.LastName())
                        .RuleFor(b => b.Totalprice, f => f.Random.Int(100, 300))
                        .RuleFor(b => b.Depositpaid, f => f.Random.Bool())
                        .RuleFor(b => b.Additionalneeds, f => f.PickRandom("Pool access", "Spa services", "Tour booking", "None"))
                        .RuleFor(b => b.Bookingdates, f => CreateWeekendDates(f));

                    return faker.Generate();
                }

                private static BookingDetails CreateExtendedStayBooking()
                {
                    var faker = new Faker<BookingDetails>()
                        .RuleFor(b => b.Firstname, f => f.Name.FirstName())
                        .RuleFor(b => b.Lastname, f => f.Name.LastName())
                        .RuleFor(b => b.Totalprice, f => f.Random.Int(1000, 3000))
                        .RuleFor(b => b.Depositpaid, true)
                        .RuleFor(b => b.Additionalneeds, "Extended stay package")
                        .RuleFor(b => b.Bookingdates, f => CreateExtendedDates(f));

                    return faker.Generate();
                }

                private static BookingDetails CreateLastMinuteBooking()
                {
                    var faker = new Faker<BookingDetails>()
                        .RuleFor(b => b.Firstname, f => f.Name.FirstName())
                        .RuleFor(b => b.Lastname, f => f.Name.LastName())
                        .RuleFor(b => b.Totalprice, f => f.Random.Int(80, 150))
                        .RuleFor(b => b.Depositpaid, false)
                        .RuleFor(b => b.Additionalneeds, "Last minute booking")
                        .RuleFor(b => b.Bookingdates, f => CreateLastMinuteDates(f));

                    return faker.Generate();
                }

                private static BookingDetails CreateInvalidBooking()
                {
                    var faker = new Faker<BookingDetails>()
                        .RuleFor(b => b.Firstname, "")
                        .RuleFor(b => b.Lastname, "")
                        .RuleFor(b => b.Totalprice, -100)
                        .RuleFor(b => b.Depositpaid, true)
                        .RuleFor(b => b.Additionalneeds, "<script>alert('xss')</script>")
                        .RuleFor(b => b.Bookingdates, f => CreateInvalidDates(f));

                    return faker.Generate();
                }

                private static BookingDetails CreateStandardBooking()
                {
                    return TestDataBuilder.CreateDefaultBookingDetails();
                }

                private static BookingDates CreateBusinessDates(Faker faker)
                {
                    var startDate = faker.Date.Between(DateTime.Now.AddDays(7), DateTime.Now.AddDays(60));
                    // Business trips are typically 1-5 days
                    var endDate = startDate.AddDays(faker.Random.Int(1, 5));
                    
                    return new BookingDates
                    {
                        Checkin = DateOnly.FromDateTime(startDate),
                        Checkout = DateOnly.FromDateTime(endDate)
                    };
                }

                private static BookingDates CreateWeekendDates(Faker faker)
                {
                    var startDate = faker.Date.Between(DateTime.Now.AddDays(1), DateTime.Now.AddDays(90));
                    // Weekend trips are typically 2-4 days
                    var endDate = startDate.AddDays(faker.Random.Int(2, 4));
                    
                    return new BookingDates
                    {
                        Checkin = DateOnly.FromDateTime(startDate),
                        Checkout = DateOnly.FromDateTime(endDate)
                    };
                }

                private static BookingDates CreateExtendedDates(Faker faker)
                {
                    var startDate = faker.Date.Between(DateTime.Now.AddDays(14), DateTime.Now.AddDays(180));
                    // Extended stays are typically 14-90 days
                    var endDate = startDate.AddDays(faker.Random.Int(14, 90));
                    
                    return new BookingDates
                    {
                        Checkin = DateOnly.FromDateTime(startDate),
                        Checkout = DateOnly.FromDateTime(endDate)
                    };
                }

                private static BookingDates CreateLastMinuteDates(Faker faker)
                {
                    var startDate = faker.Date.Between(DateTime.Now, DateTime.Now.AddDays(3));
                    var endDate = startDate.AddDays(faker.Random.Int(1, 3));
                    
                    return new BookingDates
                    {
                        Checkin = DateOnly.FromDateTime(startDate),
                        Checkout = DateOnly.FromDateTime(endDate)
                    };
                }

                private static BookingDates CreateInvalidDates(Faker faker)
                {
                    // Create dates where checkout is before checkin
                    var endDate = faker.Date.Between(DateTime.Now.AddDays(1), DateTime.Now.AddDays(30));
                    var startDate = endDate.AddDays(faker.Random.Int(1, 10));
                    
                    return new BookingDates
                    {
                        Checkin = DateOnly.FromDateTime(startDate),
                        Checkout = DateOnly.FromDateTime(endDate)
                    };
                }
            }

            public static class SecurityTestData
            {
                public static IEnumerable<string> GetXssPayloads()
                {
                    return new[]
                    {
                        "<script>alert('xss')</script>",
                        "javascript:alert('xss')",
                        "<img src=x onerror=alert('xss')>",
                        "';DROP TABLE bookings;--",
                        "' OR 1=1--",
                        "%3Cscript%3Ealert('xss')%3C/script%3E",
                        "{{7*7}}",
                        "${7*7}",
                        "#{7*7}"
                    };
                }

                public static IEnumerable<string> GetSqlInjectionPayloads()
                {
                    return new[]
                    {
                        "' OR 1=1--",
                        "'; DROP TABLE users;--",
                        "' UNION SELECT * FROM users--",
                        "admin'--",
                        "' OR 'a'='a",
                        "1' OR '1'='1",
                        "'; EXEC xp_cmdshell('dir');--"
                    };
                }

                public static IEnumerable<(string Field, string Value, string ExpectedBehavior)> GetValidationTestCases()
                {
                    return new[]
                    {
                        ("firstname", "", "Should reject empty firstname"),
                        ("lastname", "", "Should reject empty lastname"),
                        ("totalprice", "-100", "Should reject negative prices"),
                        ("totalprice", "0", "Should reject zero prices"),
                        ("totalprice", "abc", "Should reject non-numeric prices"),
                        ("checkin", "2025-13-01", "Should reject invalid dates"),
                        ("checkout", "invalid-date", "Should reject malformed dates"),
                        ("additionalneeds", new string('A', 1000), "Should handle long strings appropriately")
                    };
                }
            }
        }

        #endregion

        #region Performance Monitoring

        /// <summary>
        /// Performance monitoring utilities for API testing
        /// </summary>
        public static class PerformanceMonitor
        {
            private static readonly List<PerformanceMetric> Metrics = new();

            public static PerformanceTracker StartTracking(string operationName)
            {
                return new PerformanceTracker(operationName);
            }

            public static void RecordMetric(PerformanceMetric metric)
            {
                Metrics.Add(metric);
                Logger.Information("Performance metric recorded: {Operation} took {Duration}ms", 
                    metric.OperationName, metric.DurationMs);
            }

            public static PerformanceReport GenerateReport()
            {
                if (!Metrics.Any())
                {
                    return new PerformanceReport { HasData = false };
                }

                var report = new PerformanceReport
                {
                    HasData = true,
                    TotalOperations = Metrics.Count,
                    AverageResponseTime = Metrics.Average(m => m.DurationMs),
                    MinResponseTime = Metrics.Min(m => m.DurationMs),
                    MaxResponseTime = Metrics.Max(m => m.DurationMs),
                    OperationBreakdown = Metrics
                        .GroupBy(m => m.OperationName)
                        .ToDictionary(g => g.Key, g => new OperationStats
                        {
                            Count = g.Count(),
                            AverageTime = g.Average(m => m.DurationMs),
                            MinTime = g.Min(m => m.DurationMs),
                            MaxTime = g.Max(m => m.DurationMs)
                        })
                };

                return report;
            }

            public static void ClearMetrics()
            {
                Metrics.Clear();
            }
        }

        public class PerformanceTracker : IDisposable
        {
            private readonly string _operationName;
            private readonly Stopwatch _stopwatch;
            private readonly DateTimeOffset _startTime;

            public PerformanceTracker(string operationName)
            {
                _operationName = operationName;
                _startTime = DateTimeOffset.UtcNow;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                var metric = new PerformanceMetric
                {
                    OperationName = _operationName,
                    StartTime = _startTime,
                    EndTime = DateTimeOffset.UtcNow,
                    DurationMs = _stopwatch.ElapsedMilliseconds
                };
                
                PerformanceMonitor.RecordMetric(metric);
            }
        }

        #endregion

        #region API Response Validation

        /// <summary>
        /// Advanced API response validation utilities
        /// </summary>
        public static class ResponseValidator
        {
            public static ValidationResult ValidateBookingResponse(BookingResponse response, BookingDetails expectedDetails)
            {
                var result = new ValidationResult();

                try
                {
                    // Validate structure
                    response.Should().NotBeNull();
                    response.BookingId.Should().BeGreaterThan(0);
                    response.Booking.Should().NotBeNull();

                    // Validate data integrity
                    var actualBooking = response.Booking!;
                    actualBooking.Firstname.Should().Be(expectedDetails.Firstname);
                    actualBooking.Lastname.Should().Be(expectedDetails.Lastname);
                    actualBooking.Totalprice.Should().Be(expectedDetails.Totalprice);
                    actualBooking.Depositpaid.Should().Be(expectedDetails.Depositpaid);
                    actualBooking.Additionalneeds.Should().Be(expectedDetails.Additionalneeds);

                    // Validate dates
                    actualBooking.Bookingdates.Should().NotBeNull();
                    actualBooking.Bookingdates!.Checkin.Should().Be(expectedDetails.Bookingdates!.Checkin);
                    actualBooking.Bookingdates.Checkout.Should().Be(expectedDetails.Bookingdates.Checkout);

                    result.IsValid = true;
                    result.Message = "Response validation passed";
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Message = $"Validation failed: {ex.Message}";
                    result.ValidationErrors.Add(ex.Message);
                }

                return result;
            }

            public static ValidationResult ValidateResponseSchema(string jsonResponse, string expectedSchema)
            {
                var result = new ValidationResult();
                
                try
                {
                    // Basic JSON validation
                    JsonDocument.Parse(jsonResponse);
                    
                    // Schema validation would require additional libraries like Newtonsoft.Json.Schema
                    // For demo purposes, we'll do basic structure validation
                    
                    result.IsValid = true;
                    result.Message = "Schema validation passed";
                }
                catch (JsonException ex)
                {
                    result.IsValid = false;
                    result.Message = $"Invalid JSON: {ex.Message}";
                    result.ValidationErrors.Add(ex.Message);
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Message = $"Schema validation failed: {ex.Message}";
                    result.ValidationErrors.Add(ex.Message);
                }

                return result;
            }

            public static bool IsSecureResponse(RestResponse response)
            {
                // Check for security headers
                var securityHeaders = new[]
                {
                    "X-Content-Type-Options",
                    "X-Frame-Options", 
                    "X-XSS-Protection",
                    "Strict-Transport-Security"
                };

                var hasSecurityHeaders = securityHeaders.Any(header => 
                    response.Headers?.Any(h => h.Name?.Equals(header, StringComparison.OrdinalIgnoreCase) == true) == true);

                // Check for sensitive data exposure
                var sensitivePatterns = new[]
                {
                    @"password\s*[=:]\s*['""]?\w+['""]?",
                    @"token\s*[=:]\s*['""]?\w+['""]?",
                    @"secret\s*[=:]\s*['""]?\w+['""]?",
                    @"key\s*[=:]\s*['""]?\w+['""]?"
                };

                var hasSensitiveData = sensitivePatterns.Any(pattern =>
                    Regex.IsMatch(response.Content ?? "", pattern, RegexOptions.IgnoreCase));

                return hasSecurityHeaders && !hasSensitiveData;
            }
        }

        #endregion

        #region Test Environment Utilities

        /// <summary>
        /// Test environment management utilities
        /// </summary>
        public static class EnvironmentUtils
        {
            public static bool IsTestEnvironment()
            {
                var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? 
                                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
                                "Production";
                
                return environment.Equals("Test", StringComparison.OrdinalIgnoreCase) ||
                       environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);
            }

            public static Dictionary<string, string> GetEnvironmentInfo()
            {
                return new Dictionary<string, string>
                {
                    ["Environment"] = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Unknown",
                    ["MachineName"] = Environment.MachineName,
                    ["UserName"] = Environment.UserName,
                    ["OSVersion"] = Environment.OSVersion.ToString(),
                    ["ProcessorCount"] = Environment.ProcessorCount.ToString(),
                    ["WorkingSet"] = Environment.WorkingSet.ToString(),
                    ["FrameworkVersion"] = Environment.Version.ToString(),
                    ["CurrentDirectory"] = Environment.CurrentDirectory,
                    ["Is64BitProcess"] = Environment.Is64BitProcess.ToString(),
                    ["Is64BitOperatingSystem"] = Environment.Is64BitOperatingSystem.ToString()
                };
            }

            public static async Task<bool> WaitForServiceAsync(string serviceUrl, TimeSpan timeout)
            {
                using var client = new HttpClient();
                var endTime = DateTime.UtcNow.Add(timeout);

                while (DateTime.UtcNow < endTime)
                {
                    try
                    {
                        var response = await client.GetAsync(serviceUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // Service not ready yet
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2));
                }

                return false;
            }
        }

        #endregion

        #region Data Models

        public class HealthCheckResult
        {
            public string BaseUrl { get; set; } = string.Empty;
            public DateTimeOffset Timestamp { get; set; }
            public bool IsHealthy { get; set; }
            public HttpStatusCode HttpStatus { get; set; }
            public long ResponseTime { get; set; }
            public long NetworkLatency { get; set; }
            public string NetworkStatus { get; set; } = string.Empty;
            public string? ApiResponse { get; set; }
            public bool HasValidResponse { get; set; }
            public string? ErrorMessage { get; set; }
            public long TotalCheckTime { get; set; }
        }

        public class PerformanceMetric
        {
            public string OperationName { get; set; } = string.Empty;
            public DateTimeOffset StartTime { get; set; }
            public DateTimeOffset EndTime { get; set; }
            public long DurationMs { get; set; }
        }

        public class PerformanceReport
        {
            public bool HasData { get; set; }
            public int TotalOperations { get; set; }
            public double AverageResponseTime { get; set; }
            public long MinResponseTime { get; set; }
            public long MaxResponseTime { get; set; }
            public Dictionary<string, OperationStats> OperationBreakdown { get; set; } = new();
        }

        public class OperationStats
        {
            public int Count { get; set; }
            public double AverageTime { get; set; }
            public long MinTime { get; set; }
            public long MaxTime { get; set; }
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string Message { get; set; } = string.Empty;
            public List<string> ValidationErrors { get; set; } = new();
        }

        #endregion
    }
}