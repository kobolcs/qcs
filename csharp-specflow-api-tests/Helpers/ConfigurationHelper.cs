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
        private static readonly Lazy<IConfiguration> _configuration = new(() =>
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{GetEnvironment()}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<ApiSettings>(optional: true)
                .AddEnvironmentVariables();

            // Only add Key Vault if explicitly configured and credentials are available
            var keyVaultUri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            
            if (!string.IsNullOrWhiteSpace(keyVaultUri) && !string.IsNullOrWhiteSpace(clientId))
            {
                try
                {
                    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ExcludeEnvironmentCredential = false,
                        ExcludeInteractiveBrowserCredential = true,
                        ExcludeAzureCliCredential = false,
                        ExcludeManagedIdentityCredential = false,
                        ExcludeSharedTokenCacheCredential = true,
                        ExcludeVisualStudioCredential = true,
                        ExcludeVisualStudioCodeCredential = true
                    });

                    builder.AddAzureKeyVault(new Uri(keyVaultUri), credential);
                    Log.Information("Azure Key Vault configuration loaded successfully from {KeyVaultUri}", keyVaultUri);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to load Azure Key Vault configuration from {KeyVaultUri}, falling back to other sources", keyVaultUri);
                }
            }
            else if (!string.IsNullOrWhiteSpace(keyVaultUri))
            {
                Log.Warning("Key Vault URI provided but AZURE_CLIENT_ID is missing. Skipping Key Vault configuration.");
            }

            return builder.Build();
        });

        private static readonly Lazy<ApiSettings> _apiSettings = new(() =>
        {
            var configuration = _configuration.Value;
            var settings = new ApiSettings();
            
            try
            {
                configuration.GetSection("ApiSettings").Bind(settings);
                settings.Validate();
                
                Log.Information("API configuration loaded successfully. BaseUrl: {BaseUrl}, Username: {Username}", 
                    settings.BaseUrl, settings.Username);
                
                return settings;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to load and validate API settings");
                throw new InvalidOperationException("API configuration is invalid or missing. Please check your appsettings.json, user secrets, or environment variables.", ex);
            }
        });

        /// <summary>
        /// Gets the current environment name from environment variables
        /// </summary>
        private static string GetEnvironment()
        {
            return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                   ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                   ?? "Production";
        }

        /// <summary>
        /// Gets the fully configured IConfiguration instance
        /// </summary>
        public static IConfiguration GetConfiguration() => _configuration.Value;

        /// <summary>
        /// Gets the validated API settings
        /// </summary>
        public static ApiSettings GetApiSettings() => _apiSettings.Value;

        /// <summary>
        /// Gets a configuration value with optional default
        /// </summary>
        public static T GetValue<T>(string key, T defaultValue = default!)
        {
            return _configuration.Value.GetValue<T>(key, defaultValue);
        }

        /// <summary>
        /// Checks if the configuration is running in a test environment
        /// </summary>
        public static bool IsTestEnvironment()
        {
            var environment = GetEnvironment();
            return string.Equals(environment, "Test", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(environment, "Testing", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates that all required configuration is present
        /// </summary>
        public static void ValidateConfiguration()
        {
            try
            {
                var _ = GetApiSettings(); // This will trigger validation
                Log.Information("Configuration validation completed successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Configuration validation failed");
                throw;
            }
        }
    }
}