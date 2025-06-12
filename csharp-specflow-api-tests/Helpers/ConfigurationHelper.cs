using Microsoft.Extensions.Configuration;
using SpecFlowApiTests.Configuration;
using System;

namespace SpecFlowApiTests.Helpers
{
    public static class ConfigurationHelper
    {
        private static readonly Lazy<ApiSettings> _apiSettings = new Lazy<ApiSettings>(() =>
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<ApiSettings>()
                .AddEnvironmentVariables()
                .Build();

            var settings = new ApiSettings();
            configuration.GetSection("ApiSettings").Bind(settings);

            if (string.IsNullOrEmpty(settings.BaseUrl))
                throw new InvalidOperationException("ApiSettings:BaseUrl is required in appsettings.json or as an environment variable.");
            if (string.IsNullOrEmpty(settings.Username))
                throw new InvalidOperationException("ApiSettings:Username is required in appsettings.json or as an environment variable.");
            if (string.IsNullOrEmpty(settings.Password))
                throw new InvalidOperationException("ApiSettings:Password is required in appsettings.json or as an environment variable.");

            return settings;
        });

        public static ApiSettings GetApiSettings() => _apiSettings.Value;
    }
}