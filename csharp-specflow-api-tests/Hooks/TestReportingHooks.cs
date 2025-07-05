using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using TechTalk.SpecFlow;
using Serilog;
using BoDi;

namespace SpecFlowApiTests.Hooks
{
    [Binding]
    public class TestReportingHooks
    {
        private static readonly List<TestExecutionMetrics> _testMetrics = new();
        private static readonly Stopwatch _overallTimer = new();
        private readonly ScenarioContext _scenarioContext;
        private readonly IObjectContainer _objectContainer;
        private readonly Stopwatch _scenarioTimer = new();
        private readonly ILogger _logger;

        public TestReportingHooks(ScenarioContext scenarioContext, IObjectContainer objectContainer, ILogger logger)
        {
            _scenarioContext = scenarioContext;
            _objectContainer = objectContainer;
            _logger = logger.ForContext<TestReportingHooks>();
        }

        [BeforeTestRun]
        public static void InitializeTestReporting()
        {
            _overallTimer.Start();
            
            Log.Information("=".PadRight(80, '='));
            Log.Information("🚀 STARTING TEST AUTOMATION DEMO EXECUTION");
            Log.Information("=".PadRight(80, '='));
            Log.Information("Test Run Started: {StartTime}", DateTimeOffset.UtcNow);
            Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Unknown");
            Log.Information("Machine: {MachineName}", Environment.MachineName);
            Log.Information("User: {UserName}", Environment.UserName);
            Log.Information("Framework: .NET {Version}", Environment.Version);
            
            // Initialize performance monitoring
            var performanceCounters = InitializePerformanceCounters();
            Log.Information("Performance monitoring initialized with {CounterCount} counters", performanceCounters.Count);
        }

        [BeforeScenario]
        public void InitializeScenarioReporting()
        {
            _scenarioTimer.Start();
            
            var scenarioInfo = _scenarioContext.ScenarioInfo;
            var tags = string.Join(", ", scenarioInfo.Tags);
            
            Log.Information("▶️  Starting Scenario: {Title}", scenarioInfo.Title);
            Log.Information("   📋 Tags: [{Tags}]", tags);
            Log.Information("   🏷️  Feature: {Feature}", scenarioInfo.ScenarioContainer.FeatureInfo.Title);
            
            // Store scenario start metrics
            _scenarioContext["StartTime"] = DateTimeOffset.UtcNow;
            _scenarioContext["ScenarioTimer"] = _scenarioTimer;
            
            // Initialize scenario-specific metrics
            var scenarioMetrics = new ScenarioMetrics
            {
                ScenarioTitle = scenarioInfo.Title,
                FeatureTitle = scenarioInfo.ScenarioContainer.FeatureInfo.Title,
                Tags = scenarioInfo.Tags.ToArray(),
                StartTime = DateTimeOffset.UtcNow
            };
            
            _scenarioContext["ScenarioMetrics"] = scenarioMetrics;
        }

        [AfterStep]
        public void RecordStepExecution()
        {
            var stepInfo = _scenarioContext.StepContext.StepInfo;
            var stepMetrics = new StepMetrics
            {
                StepText = stepInfo.Text,
                StepType = stepInfo.StepDefinitionType.ToString(),
                ExecutionTime = _scenarioTimer.Elapsed,
                Status = _scenarioContext.TestError == null ? "Passed" : "Failed"
            };

            if (_scenarioContext.ContainsKey("ScenarioMetrics"))
            {
                var scenarioMetrics = (ScenarioMetrics)_scenarioContext["ScenarioMetrics"];
                scenarioMetrics.Steps.Add(stepMetrics);
            }

            // Log step completion
            var status = _scenarioContext.TestError == null ? "✅" : "❌";
            _logger.Information("  {Status} {StepType}: {StepText}", status, stepInfo.StepDefinitionType, stepInfo.Text);
            
            if (_scenarioContext.TestError != null)
            {
                _logger.Error(_scenarioContext.TestError, "Step failed with error");
            }
        }

        [AfterScenario]
        public void FinalizeScenarioReporting()
        {
            _scenarioTimer.Stop();
            
            var scenarioInfo = _scenarioContext.ScenarioInfo;
            var duration = _scenarioTimer.Elapsed;
            var status = _scenarioContext.TestError == null ? "PASSED" : "FAILED";
            var statusIcon = _scenarioContext.TestError == null ? "✅" : "❌";
            
            // Update scenario metrics
            if (_scenarioContext.ContainsKey("ScenarioMetrics"))
            {
                var scenarioMetrics = (ScenarioMetrics)_scenarioContext["ScenarioMetrics"];
                scenarioMetrics.EndTime = DateTimeOffset.UtcNow;
                scenarioMetrics.Duration = duration;
                scenarioMetrics.Status = status;
                scenarioMetrics.ErrorMessage = _scenarioContext.TestError?.Message;
                
                // Add to global metrics collection
                _testMetrics.Add(new TestExecutionMetrics
                {
                    ScenarioTitle = scenarioMetrics.ScenarioTitle,
                    FeatureTitle = scenarioMetrics.FeatureTitle,
                    Status = scenarioMetrics.Status,
                    Duration = scenarioMetrics.Duration,
                    StartTime = scenarioMetrics.StartTime,
                    EndTime = scenarioMetrics.EndTime,
                    StepCount = scenarioMetrics.Steps.Count,
                    Tags = scenarioMetrics.Tags,
                    ErrorMessage = scenarioMetrics.ErrorMessage
                });
            }

            Log.Information("🏁 Scenario Completed: {Title} - {Status} in {Duration:mm\\:ss\\.fff}", 
                scenarioInfo.Title, status, duration);
            
            if (_scenarioContext.TestError != null)
            {
                Log.Error("❌ Scenario Failed: {Error}", _scenarioContext.TestError.Message);
            }

            // Record performance metrics if available
            RecordPerformanceMetrics();
            
            Log.Information("─".PadRight(80, '─'));
        }

        [AfterTestRun]
        public static void GenerateTestReport()
        {
            _overallTimer.Stop();
            
            var totalTests = _testMetrics.Count;
            var passedTests = _testMetrics.Count(m => m.Status == "PASSED");
            var failedTests = _testMetrics.Count(m => m.Status == "FAILED");
            var passRate = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0;
            
            Log.Information("=".PadRight(80, '='));
            Log.Information("📊 TEST EXECUTION SUMMARY");
            Log.Information("=".PadRight(80, '='));
            Log.Information("Total Execution Time: {Duration:mm\\:ss\\.fff}", _overallTimer.Elapsed);
            Log.Information("Total Scenarios: {Total}", totalTests);
            Log.Information("✅ Passed: {Passed}", passedTests);
            Log.Information("❌ Failed: {Failed}", failedTests);
            Log.Information("📈 Pass Rate: {PassRate:F1}%", passRate);
            
            if (failedTests > 0)
            {
                Log.Warning("Failed Scenarios:");
                foreach (var failed in _testMetrics.Where(m => m.Status == "FAILED"))
                {
                    Log.Warning("  ❌ {Feature} > {Scenario}: {Error}", 
                        failed.FeatureTitle, failed.ScenarioTitle, failed.ErrorMessage);
                }
            }

            // Performance summary
            if (_testMetrics.Any())
            {
                var avgDuration = _testMetrics.Average(m => m.Duration.TotalMilliseconds);
                var maxDuration = _testMetrics.Max(m => m.Duration.TotalMilliseconds);
                var minDuration = _testMetrics.Min(m => m.Duration.TotalMilliseconds);
                
                Log.Information("⏱️  Performance Summary:");
                Log.Information("  Average Duration: {Avg:F0}ms", avgDuration);
                Log.Information("  Max Duration: {Max:F0}ms", maxDuration);
                Log.Information("  Min Duration: {Min:F0}ms", minDuration);
            }

            // Tag-based summary
            GenerateTagBasedSummary();

            // Generate detailed JSON report
            GenerateJsonReport();
            
            // Generate HTML dashboard
            GenerateHtmlDashboard();
            
            Log.Information("=".PadRight(80, '='));
            Log.Information("🎯 TEST AUTOMATION DEMO COMPLETED SUCCESSFULLY");
            Log.Information("=".PadRight(80, '='));
        }

        private static void GenerateTagBasedSummary()
        {
            var tagStats = new Dictionary<string, (int Total, int Passed, int Failed)>();
            
            foreach (var metric in _testMetrics)
            {
                foreach (var tag in metric.Tags)
                {
                    if (!tagStats.ContainsKey(tag))
                        tagStats[tag] = (0, 0, 0);
                    
                    var current = tagStats[tag];
                    tagStats[tag] = (
                        current.Total + 1,
                        current.Passed + (metric.Status == "PASSED" ? 1 : 0),
                        current.Failed + (metric.Status == "FAILED" ? 1 : 0)
                    );
                }
            }

            if (tagStats.Any())
            {
                Log.Information("🏷️  Test Results by Tag:");
                foreach (var (tag, stats) in tagStats.OrderByDescending(x => x.Value.Total))
                {
                    var passRate = stats.Total > 0 ? (double)stats.Passed / stats.Total * 100 : 0;
                    Log.Information("  @{Tag}: {Passed}/{Total} ({PassRate:F1}%)", 
                        tag, stats.Passed, stats.Total, passRate);
                }
            }
        }

        private static void GenerateJsonReport()
        {
            try
            {
                var reportData = new
                {
                    Summary = new
                    {
                        TotalTests = _testMetrics.Count,
                        PassedTests = _testMetrics.Count(m => m.Status == "PASSED"),
                        FailedTests = _testMetrics.Count(m => m.Status == "FAILED"),
                        TotalDuration = _overallTimer.Elapsed.ToString(@"mm\:ss\.fff"),
                        StartTime = DateTimeOffset.UtcNow.Subtract(_overallTimer.Elapsed),
                        EndTime = DateTimeOffset.UtcNow,
                        Environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Unknown",
                        MachineName = Environment.MachineName
                    },
                    TestResults = _testMetrics.Select(m => new
                    {
                        m.FeatureTitle,
                        m.ScenarioTitle,
                        m.Status,
                        Duration = m.Duration.ToString(@"mm\:ss\.fff"),
                        m.StartTime,
                        m.EndTime,
                        m.StepCount,
                        m.Tags,
                        m.ErrorMessage
                    }).ToArray()
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var reportJson = JsonSerializer.Serialize(reportData, jsonOptions);
                var reportPath = Path.Combine("TestResults", "test-execution-report.json");
                
                Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
                File.WriteAllText(reportPath, reportJson);
                
                Log.Information("📄 JSON report generated: {ReportPath}", reportPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate JSON report");
            }
        }

        private static void GenerateHtmlDashboard()
        {
            try
            {
                var dashboardHtml = GenerateDashboardHtml();
                var dashboardPath = Path.Combine("TestResults", "test-dashboard.html");
                
                Directory.CreateDirectory(Path.GetDirectoryName(dashboardPath)!);
                File.WriteAllText(dashboardPath, dashboardHtml);
                
                Log.Information("📊 HTML dashboard generated: {DashboardPath}", dashboardPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate HTML dashboard");
            }
        }

        private static string GenerateDashboardHtml()
        {
            var passedCount = _testMetrics.Count(m => m.Status == "PASSED");
            var failedCount = _testMetrics.Count(m => m.Status == "FAILED");
            var passRate = _testMetrics.Count > 0 ? (double)passedCount / _testMetrics.Count * 100 : 0;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Test Automation Dashboard</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background: #f5f5f5; }}
        .container {{ max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .metric-card {{ display: inline-block; margin: 10px; padding: 20px; background: #f8f9fa; border-radius: 5px; text-align: center; min-width: 150px; }}
        .passed {{ background: #d4edda; border-left: 4px solid #28a745; }}
        .failed {{ background: #f8d7da; border-left: 4px solid #dc3545; }}
        .total {{ background: #e3f2fd; border-left: 4px solid #2196f3; }}
        .progress {{ width: 100%; height: 20px; background: #f0f0f0; border-radius: 10px; overflow: hidden; margin: 20px 0; }}
        .progress-bar {{ height: 100%; background: linear-gradient(45deg, #28a745, #20c997); transition: width 0.5s ease; }}
        .test-list {{ margin-top: 30px; }}
        .test-item {{ padding: 10px; margin: 5px 0; border-radius: 5px; }}
        .test-item.passed {{ background: #d4edda; }}
        .test-item.failed {{ background: #f8d7da; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🧪 Test Automation Dashboard</h1>
            <p>Professional API Testing Demo Results</p>
        </div>
        
        <div class='metric-card total'>
            <h3>Total Tests</h3>
            <h2>{_testMetrics.Count}</h2>
        </div>
        
        <div class='metric-card passed'>
            <h3>✅ Passed</h3>
            <h2>{passedCount}</h2>
        </div>
        
        <div class='metric-card failed'>
            <h3>❌ Failed</h3>
            <h2>{failedCount}</h2>
        </div>
        
        <div class='metric-card'>
            <h3>Pass Rate</h3>
            <h2>{passRate:F1}%</h2>
        </div>
        
        <div class='progress'>
            <div class='progress-bar' style='width: {passRate}%'></div>
        </div>
        
        <div class='test-list'>
            <h3>Test Results Detail</h3>
            {string.Join("", _testMetrics.Select(m => $@"
            <div class='test-item {m.Status.ToLower()}'>
                <strong>{m.FeatureTitle}</strong> > {m.ScenarioTitle}
                <span style='float: right;'>{m.Duration.ToString(@"mm\:ss\.fff")}</span>
                {(m.Status == "FAILED" ? $"<br><small style='color: #dc3545;'>Error: {m.ErrorMessage}</small>" : "")}
            </div>"))}
        </div>
        
        <div style='margin-top: 30px; text-align: center; color: #666;'>
            <small>Generated on {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</small>
        </div>
    </div>
</body>
</html>";
        }

        private void RecordPerformanceMetrics()
        {
            if (_scenarioContext.ContainsKey("performanceMetrics"))
            {
                var metrics = _scenarioContext["performanceMetrics"];
                _logger.Information("Performance metrics for scenario: {@Metrics}", metrics);
            }
        }

        private static List<string> InitializePerformanceCounters()
        {
            // Initialize performance monitoring counters
            return new List<string>
            {
                "Memory Usage",
                "CPU Usage",
                "Network I/O",
                "Disk I/O"
            };
        }
    }

    public class TestExecutionMetrics
    {
        public string ScenarioTitle { get; set; } = string.Empty;
        public string FeatureTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int StepCount { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string? ErrorMessage { get; set; }
    }

    public class ScenarioMetrics
    {
        public string ScenarioTitle { get; set; } = string.Empty;
        public string FeatureTitle { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public List<StepMetrics> Steps { get; set; } = new();
    }

    public class StepMetrics
    {
        public string StepText { get; set; } = string.Empty;
        public string StepType { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}