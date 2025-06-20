using BoDi;
using SpecFlowApiTests.Clients;
using SpecFlowApiTests.Configuration;
using SpecFlowApiTests.Helpers;
using System;
using TechTalk.SpecFlow;
using Serilog;
using System.Threading.Tasks;

namespace SpecFlowApiTests.Hooks
{
    [Binding]
    public class TestHooks
    {
        private readonly IObjectContainer _objectContainer;

        public TestHooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeTestRun]
        public static void InitializeLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/api_tests.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("====== Test Run Started ======");

            // Capture any unhandled exceptions to avoid silent test failures
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                Log.Fatal(args.ExceptionObject as Exception, "Unhandled exception occurred");
            };
        }

        [BeforeScenario(Order = 0)]
        public void RegisterApiSettings()
        {
            var apiSettings = ConfigurationHelper.GetApiSettings();
            _objectContainer.RegisterInstanceAs(apiSettings);
        }

        [BeforeScenario(Order = 1)]
        public async Task InitializeAndAuthenticateClients()
        {
            var apiSettings = _objectContainer.Resolve<ApiSettings>();
            var logger = Log.Logger.ForContext<TestHooks>();

            var authClient = new AuthClient(apiSettings.BaseUrl!, apiSettings.Username!, apiSettings.Password!, logger);
            string token;

            try
            {
                token = await authClient.GetTokenAsync();
                logger.Information("Successfully authenticated and retrieved token.");
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Authentication failed for user '{User}'. Cannot proceed with tests.", apiSettings.Username);
                throw new InvalidOperationException($"Failed to authenticate user '{apiSettings.Username}'.", ex);
            }

            var bookingClientWithAuth = new BookingClient(apiSettings.BaseUrl!, token, logger);

            _objectContainer.RegisterInstanceAs<AuthClient>(authClient);
            _objectContainer.RegisterInstanceAs<BookingClient>(bookingClientWithAuth);

            logger.Debug("Registered AuthClient and BookingClient instances in the container.");
        }
    }
}
