using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using System;
using SpecFlowApiTests.Configuration;
using Serilog;

namespace SpecFlowApiTests.Helpers
{
    public static class ConfigurationHelper
    {
        private static readonly Lazy<ApiSettings> _apiSettings = new Lazy<ApiSettings>(() =>
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<ApiSettings>()
                .AddEnvironmentVariables();

            var keyVaultUri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
            if (!string.IsNullOrWhiteSpace(keyVaultUri))
            {
                builder.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
            }

            var configuration = builder.Build();

            var settings = new ApiSettings();
            configuration.GetSection("ApiSettings").Bind(settings);

            if (string.IsNullOrEmpty(settings.BaseUrl))
            {
                Log.Logger.Error("ApiSettings:BaseUrl is missing in appsettings.json or environment variables.");
                throw new InvalidOperationException("ApiSettings:BaseUrl is required in appsettings.json or as an environment variable.");
            }
            if (string.IsNullOrEmpty(settings.Username))
            {
                Log.Logger.Error("ApiSettings:Username is missing in appsettings.json or environment variables.");
                throw new InvalidOperationException("ApiSettings:Username is required in appsettings.json or as an environment variable.");
            }
            if (string.IsNullOrEmpty(settings.Password))
            {
                Log.Logger.Error("ApiSettings:Password is missing in appsettings.json or environment variables.");
                throw new InvalidOperationException("ApiSettings:Password is required in appsettings.json or as an environment variable.");
            }

            return settings;
        });

        public static ApiSettings GetApiSettings() => _apiSettings.Value;
    }
}
