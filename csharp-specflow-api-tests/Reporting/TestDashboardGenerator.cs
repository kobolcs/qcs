using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Serilog;

namespace SpecFlowApiTests.Reporting
{
    /// <summary>
    /// Advanced test execution dashboard generator for professional demonstrations
    /// </summary>
    public static class TestDashboardGenerator
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(TestDashboardGenerator));

        public static void GenerateComprehensiveDashboard(List<TestExecutionResult> results, string outputPath = "TestResults")
        {
            try
            {
                Directory.CreateDirectory(outputPath);
                
                // Generate multiple dashboard formats
                GenerateHtmlDashboard(results, Path.Combine(outputPath, "dashboard.html"));
                GenerateJsonReport(results, Path.Combine(outputPath, "test-metrics.json"));
                GenerateMarkdownSummary(results, Path.Combine(outputPath, "test-summary.md"));
                GenerateCsvMetrics(results, Path.Combine(outputPath, "performance-metrics.csv"));
                
                Logger.Information("Comprehensive test dashboard generated successfully at {OutputPath}", outputPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to generate test dashboard");
                throw;
            }
        }

        private static void GenerateHtmlDashboard(List<TestExecutionResult> results, string filePath)
        {
            var totalTests = results.Count;
            var passedTests = results.Count(r => r.Status == "Passed");
            var failedTests = results.Count(r => r.Status == "Failed");
            var skippedTests = results.Count(r => r.Status == "Skipped");
            var passRate = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0;
            
            var avgDuration = results.Any() ? results.Average(r => r.DurationMs) : 0;
            var totalDuration = results.Sum(r => r.DurationMs);

            var html = GenerateAdvancedHtmlContent(results, totalTests, passedTests, failedTests, skippedTests, passRate, avgDuration, totalDuration);
            
            File.WriteAllText(filePath, html, Encoding.UTF8);
            Logger.Information("HTML dashboard generated: {FilePath}", filePath);
        }

        private static string GenerateAdvancedHtmlContent(List<TestExecutionResult> results, int totalTests, int passedTests, int failedTests, int skippedTests, double passRate, double avgDuration, long totalDuration)
        {
            var performanceByTag = results
                .SelectMany(r => r.Tags.Select(tag => new { Tag = tag, Duration = r.DurationMs, Status = r.Status }))
                .GroupBy(x => x.Tag)
                .ToDictionary(g => g.Key, g => new
                {
                    Count = g.Count(),
                    AvgDuration = g.Average(x => x.Duration),
                    PassRate = (double)g.Count(x => x.Status == "Passed") / g.Count() * 100
                });

            var failedTestDetails = results.Where(r => r.Status == "Failed").ToList();
            var performanceOutliers = results.Where(r => r.DurationMs > avgDuration * 2).OrderByDescending(r => r.DurationMs).Take(10).ToList();

            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Professional Test Automation Dashboard</title>
    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }}
        .container {{ 
            max-width: 1400px; 
            margin: 0 auto; 
            background: white; 
            border-radius: 15px; 
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{ 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white; 
            padding: 30px; 
            text-align: center; 
        }}
        .header h1 {{ font-size: 2.5em; margin-bottom: 10px; }}
        .header p {{ font-size: 1.2em; opacity: 0.9; }}
        .stats-grid {{ 
            display: grid; 
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); 
            gap: 20px; 
            padding: 30px; 
            background: #f8f9fa;
        }}
        .stat-card {{ 
            background: white; 
            padding: 25px; 
            border-radius: 10px; 
            text-align: center; 
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
            transition: transform 0.3s ease;
        }}
        .stat-card:hover {{ transform: translateY(-5px); }}
        .stat-value {{ font-size: 2.5em; font-weight: bold; margin: 10px 0; }}
        .stat-label {{ color: #666; font-size: 1.1em; }}
        .passed {{ color: #28a745; }}
        .failed {{ color: #dc3545; }}
        .skipped {{ color: #ffc107; }}
        .total {{ color: #007bff; }}
        .content {{ padding: 30px; }}
        .section {{ margin-bottom: 40px; }}
        .section h2 {{ 
            font-size: 1.8em; 
            margin-bottom: 20px; 
            color: #333;
            border-bottom: 3px solid #667eea;
            padding-bottom: 10px;
        }}
        .chart-container {{ 
            position: relative; 
            height: 400px; 
            margin: 20px 0;
            background: white;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
        }}
        .progress-bar {{ 
            width: 100%; 
            height: 25px; 
            background: #e9ecef; 
            border-radius: 15px; 
            overflow: hidden;
            margin: 20px 0;
        }}
        .progress-fill {{ 
            height: 100%; 
            background: linear-gradient(90deg, #28a745, #20c997); 
            border-radius: 15px;
            transition: width 1s ease-in-out;
        }}
        .test-grid {{ 
            display: grid; 
            gap: 15px; 
            margin-top: 20px;
        }}
        .test-item {{ 
            padding: 15px; 
            border-radius: 8px; 
            border-left: 4px solid #ddd;
            background: #f8f9fa;
        }}
        .test-item.passed {{ border-left-color: #28a745; background: #d4edda; }}
        .test-item.failed {{ border-left-color: #dc3545; background: #f8d7da; }}
        .test-item.skipped {{ border-left-color: #ffc107; background: #fff3cd; }}
        .test-title {{ font-weight: bold; margin-bottom: 5px; }}
        .test-meta {{ font-size: 0.9em; color: #666; }}
        .performance-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin-top: 20px;
            background: white;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
        }}
        .performance-table th, .performance-table td {{ 
            padding: 12px; 
            text-align: left; 
            border-bottom: 1px solid #ddd;
        }}
        .performance-table th {{ 
            background: #667eea; 
            color: white; 
            font-weight: 600;
        }}
        .performance-table tr:hover {{ background: #f5f5f5; }}
        .tag-cloud {{ 
            display: flex; 
            flex-wrap: wrap; 
            gap: 10px; 
            margin-top: 20px;
        }}
        .tag {{ 
            padding: 8px 16px; 
            background: #e3f2fd; 
            border-radius: 20px; 
            font-size: 0.9em;
            border: 1px solid #2196f3;
            color: #1976d2;
        }}
        .footer {{ 
            background: #333; 
            color: white; 
            padding: 20px; 
            text-align: center; 
        }}
        .metric-highlight {{ 
            background: linear-gradient(135deg, #ff6b6b, #feca57); 
            color: white; 
            padding: 20px; 
            border-radius: 10px; 
            margin: 20px 0;
            text-align: center;
        }}
        @media (max-width: 768px) {{
            .stats-grid {{ grid-template-columns: 1fr 1fr; }}
            .content {{ padding: 15px; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🧪 Professional Test Automation Dashboard</h1>
            <p>Comprehensive API Testing Results & Analytics</p>
            <p style='font-size: 0.9em; margin-top: 10px;'>Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
        </div>

        <div class='stats-grid'>
            <div class='stat-card'>
                <div class='stat-value total'>{totalTests}</div>
                <div class='stat-label'>Total Tests</div>
            </div>
            <div class='stat-card'>
                <div class='stat-value passed'>{passedTests}</div>
                <div class='stat-label'>✅ Passed</div>
            </div>
            <div class='stat-card'>
                <div class='stat-value failed'>{failedTests}</div>
                <div class='stat-label'>❌ Failed</div>
            </div>
            <div class='stat-card'>
                <div class='stat-value skipped'>{skippedTests}</div>
                <div class='stat-label'>⏭️ Skipped</div>
            </div>
            <div class='stat-card'>
                <div class='stat-value'>{passRate:F1}%</div>
                <div class='stat-label'>Pass Rate</div>
            </div>
            <div class='stat-card'>
                <div class='stat-value'>{avgDuration:F0}ms</div>
                <div class='stat-label'>Avg Duration</div>
            </div>
        </div>

        <div class='metric-highlight'>
            <h3>🎯 Test Execution Summary</h3>
            <p>Total execution time: {TimeSpan.FromMilliseconds(totalDuration):mm\\:ss\\.fff} | Average test duration: {avgDuration:F0}ms | Success rate: {passRate:F1}%</p>
        </div>

        <div class='progress-bar'>
            <div class='progress-fill' style='width: {passRate}%'></div>
        </div>

        <div class='content'>
            <div class='section'>
                <h2>📊 Test Results Overview</h2>
                <div class='chart-container'>
                    <canvas id='resultsChart'></canvas>
                </div>
            </div>

            <div class='section'>
                <h2>⚡ Performance by Test Category</h2>
                <table class='performance-table'>
                    <thead>
                        <tr>
                            <th>Category</th>
                            <th>Test Count</th>
                            <th>Avg Duration (ms)</th>
                            <th>Pass Rate</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", performanceByTag.Select(kvp => $@"
                        <tr>
                            <td>@{kvp.Key}</td>
                            <td>{kvp.Value.Count}</td>
                            <td>{kvp.Value.AvgDuration:F0}</td>
                            <td>{kvp.Value.PassRate:F1}%</td>
                            <td>{(kvp.Value.PassRate >= 95 ? "🟢 Excellent" : kvp.Value.PassRate >= 80 ? "🟡 Good" : "🔴 Needs Attention")}</td>
                        </tr>"))}
                    </tbody>
                </table>
            </div>

            {(failedTestDetails.Any() ? $@"
            <div class='section'>
                <h2>🔍 Failed Test Analysis</h2>
                <div class='test-grid'>
                    {string.Join("", failedTestDetails.Select(test => $@"
                    <div class='test-item failed'>
                        <div class='test-title'>{test.ScenarioTitle}</div>
                        <div class='test-meta'>
                            Feature: {test.FeatureTitle} | Duration: {test.DurationMs}ms | Tags: {string.Join(", ", test.Tags)}
                        </div>
                        {(!string.IsNullOrEmpty(test.ErrorMessage) ? $"<div style='color: #dc3545; margin-top: 8px; font-size: 0.9em;'>Error: {test.ErrorMessage}</div>" : "")}
                    </div>"))}
                </div>
            </div>" : "")}

            {(performanceOutliers.Any() ? $@"
            <div class='section'>
                <h2>🐌 Performance Outliers</h2>
                <p style='margin-bottom: 15px; color: #666;'>Tests taking significantly longer than average ({avgDuration:F0}ms)</p>
                <div class='test-grid'>
                    {string.Join("", performanceOutliers.Select(test => $@"
                    <div class='test-item'>
                        <div class='test-title'>{test.ScenarioTitle}</div>
                        <div class='test-meta'>
                            Duration: {test.DurationMs}ms ({(test.DurationMs / avgDuration):F1}x average) | Feature: {test.FeatureTitle}
                        </div>
                    </div>"))}
                </div>
            </div>" : "")}

            <div class='section'>
                <h2>🏷️ Test Categories</h2>
                <div class='tag-cloud'>
                    {string.Join("", results.SelectMany(r => r.Tags).Distinct().OrderBy(t => t).Select(tag => $"<span class='tag'>@{tag}</span>"))}
                </div>
            </div>

            <div class='section'>
                <h2>📈 Performance Trends</h2>
                <div class='chart-container'>
                    <canvas id='performanceChart'></canvas>
                </div>
            </div>
        </div>

        <div class='footer'>
            <p>🚀 Professional Test Automation Framework | Built with C#, SpecFlow & NUnit</p>
            <p style='margin-top: 5px; font-size: 0.9em;'>Demonstrating enterprise-grade quality assurance practices</p>
        </div>
    </div>

    <script>
        // Results pie chart
        const resultsCtx = document.getElementById('resultsChart').getContext('2d');
        new Chart(resultsCtx, {{
            type: 'doughnut',
            data: {{
                labels: ['Passed', 'Failed', 'Skipped'],
                datasets: [{{
                    data: [{passedTests}, {failedTests}, {skippedTests}],
                    backgroundColor: ['#28a745', '#dc3545', '#ffc107'],
                    borderWidth: 3,
                    borderColor: '#fff'
                }}]
            }},
            options: {{
                responsive: true,
                maintainAspectRatio: false,
                plugins: {{
                    legend: {{ position: 'bottom', labels: {{ padding: 20, font: {{ size: 14 }} }} }},
                    title: {{ display: true, text: 'Test Results Distribution', font: {{ size: 16 }} }}
                }}
            }}
        }});

        // Performance chart
        const performanceCtx = document.getElementById('performanceChart').getContext('2d');
        const performanceData = {JsonSerializer.Serialize(results.Take(20).Select(r => new { x = r.ScenarioTitle.Length > 30 ? r.ScenarioTitle.Substring(0, 30) + "..." : r.ScenarioTitle, y = r.DurationMs }))};
        
        new Chart(performanceCtx, {{
            type: 'bar',
            data: {{
                labels: performanceData.map(d => d.x),
                datasets: [{{
                    label: 'Duration (ms)',
                    data: performanceData.map(d => d.y),
                    backgroundColor: 'rgba(102, 126, 234, 0.8)',
                    borderColor: 'rgba(102, 126, 234, 1)',
                    borderWidth: 1
                }}]
            }},
            options: {{
                responsive: true,
                maintainAspectRatio: false,
                scales: {{
                    y: {{ beginAtZero: true, title: {{ display: true, text: 'Duration (ms)' }} }},
                    x: {{ title: {{ display: true, text: 'Test Scenarios' }} }}
                }},
                plugins: {{
                    title: {{ display: true, text: 'Test Execution Performance', font: {{ size: 16 }} }}
                }}
            }}
        }});

        // Add some interactive animations
        document.addEventListener('DOMContentLoaded', function() {{
            const statCards = document.querySelectorAll('.stat-card');
            statCards.forEach((card, index) => {{
                setTimeout(() => {{
                    card.style.opacity = '0';
                    card.style.transform = 'translateY(20px)';
                    card.style.transition = 'all 0.5s ease';
                    setTimeout(() => {{
                        card.style.opacity = '1';
                        card.style.transform = 'translateY(0)';
                    }}, 50);
                }}, index * 100);
            }});
        }});
    </script>
</body>
</html>";
        }

        private static void GenerateJsonReport(List<TestExecutionResult> results, string filePath)
        {
            var report = new
            {
                GeneratedAt = DateTimeOffset.UtcNow,
                Summary = new
                {
                    TotalTests = results.Count,
                    PassedTests = results.Count(r => r.Status == "Passed"),
                    FailedTests = results.Count(r => r.Status == "Failed"),
                    SkippedTests = results.Count(r => r.Status == "Skipped"),
                    PassRate = results.Count > 0 ? (double)results.Count(r => r.Status == "Passed") / results.Count * 100 : 0,
                    TotalDuration = results.Sum(r => r.DurationMs),
                    AverageDuration = results.Any() ? results.Average(r => r.DurationMs) : 0
                },
                TestResults = results.Select(r => new
                {
                    r.FeatureTitle,
                    r.ScenarioTitle,
                    r.Status,
                    r.DurationMs,
                    r.StartTime,
                    r.EndTime,
                    r.Tags,
                    r.ErrorMessage
                }).ToArray(),
                PerformanceMetrics = results
                    .GroupBy(r => r.FeatureTitle)
                    .Select(g => new
                    {
                        Feature = g.Key,
                        TestCount = g.Count(),
                        AverageDuration = g.Average(r => r.DurationMs),
                        PassRate = (double)g.Count(r => r.Status == "Passed") / g.Count() * 100
                    }).ToArray()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(report, options);
            File.WriteAllText(filePath, json, Encoding.UTF8);
            Logger.Information("JSON report generated: {FilePath}", filePath);
        }

        private static void GenerateMarkdownSummary(List<TestExecutionResult> results, string filePath)
        {
            var totalTests = results.Count;
            var passedTests = results.Count(r => r.Status == "Passed");
            var failedTests = results.Count(r => r.Status == "Failed");
            var passRate = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0;

            var markdown = new StringBuilder();
            
            markdown.AppendLine("# 🧪 Test Execution Summary Report");
            markdown.AppendLine();
            markdown.AppendLine($"**Generated:** {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            markdown.AppendLine();
            markdown.AppendLine("## 📊 Overall Results");
            markdown.AppendLine();
            markdown.AppendLine($"| Metric | Value |");
            markdown.AppendLine($"|--------|--------|");
            markdown.AppendLine($"| Total Tests | {totalTests} |");
            markdown.AppendLine($"| ✅ Passed | {passedTests} |");
            markdown.AppendLine($"| ❌ Failed | {failedTests} |");
            markdown.AppendLine($"| 📈 Pass Rate | {passRate:F1}% |");
            markdown.AppendLine($"| ⏱️ Total Duration | {TimeSpan.FromMilliseconds(results.Sum(r => r.DurationMs)):mm\\:ss\\.fff} |");
            markdown.AppendLine();

            if (failedTests > 0)
            {
                markdown.AppendLine("## ❌ Failed Tests");
                markdown.AppendLine();
                var failedResults = results.Where(r => r.Status == "Failed").ToList();
                foreach (var failed in failedResults)
                {
                    markdown.AppendLine($"### {failed.FeatureTitle} > {failed.ScenarioTitle}");
                    markdown.AppendLine($"- **Duration:** {failed.DurationMs}ms");
                    markdown.AppendLine($"- **Tags:** {string.Join(", ", failed.Tags)}");
                    if (!string.IsNullOrEmpty(failed.ErrorMessage))
                    {
                        markdown.AppendLine($"- **Error:** {failed.ErrorMessage}");
                    }
                    markdown.AppendLine();
                }
            }

            markdown.AppendLine("## 🏷️ Results by Category");
            markdown.AppendLine();
            var tagResults = results
                .SelectMany(r => r.Tags.Select(tag => new { Tag = tag, Status = r.Status, Duration = r.DurationMs }))
                .GroupBy(x => x.Tag)
                .OrderBy(g => g.Key);

            markdown.AppendLine("| Category | Total | Passed | Failed | Pass Rate | Avg Duration |");
            markdown.AppendLine("|----------|-------|--------|--------|-----------|--------------|");

            foreach (var tagGroup in tagResults)
            {
                var tagTotal = tagGroup.Count();
                var tagPassed = tagGroup.Count(x => x.Status == "Passed");
                var tagFailed = tagGroup.Count(x => x.Status == "Failed");