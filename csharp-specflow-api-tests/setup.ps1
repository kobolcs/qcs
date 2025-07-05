# Professional C# SpecFlow API Test Automation Demo - Setup Script
# This script sets up the complete optimized test automation framework

param(
    [string]$Environment = "Test",
    [switch]$SkipDependencies,
    [switch]$RunTests,
    [switch]$GenerateReports,
    [switch]$Docker,
    [switch]$Help
)

if ($Help) {
    Write-Host @"
🧪 Professional C# SpecFlow API Test Automation Demo Setup
========================================================

Usage: .\setup.ps1 [OPTIONS]

Options:
  -Environment <env>     Set environment (Test, Development, Production) [Default: Test]
  -SkipDependencies     Skip dependency installation
  -RunTests             Run tests after setup
  -GenerateReports      Generate comprehensive reports
  -Docker               Use Docker for execution
  -Help                 Show this help message

Examples:
  .\setup.ps1                                    # Basic setup
  .\setup.ps1 -RunTests -GenerateReports        # Setup and run full demo
  .\setup.ps1 -Docker                           # Setup with Docker
  .\setup.ps1 -Environment Development          # Setup for development

Requirements:
  - .NET 8.0 SDK
  - PowerShell 5.1+ or PowerShell Core
  - (Optional) Docker for containerized execution
  - (Optional) Visual Studio or VS Code for development

"@
    exit 0
}

# Color functions for better output
function Write-ColorOutput($ForegroundColor, $Message) {
    $previousColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $ForegroundColor
    Write-Output $Message
    $Host.UI.RawUI.ForegroundColor = $previousColor
}

function Write-Success($Message) { Write-ColorOutput Green "✅ $Message" }
function Write-Info($Message) { Write-ColorOutput Cyan "ℹ️  $Message" }
function Write-Warning($Message) { Write-ColorOutput Yellow "⚠️  $Message" }
function Write-Error($Message) { Write-ColorOutput Red "❌ $Message" }

# Banner
Write-Host @"
╔════════════════════════════════════════════════════════════════╗
║                                                                ║
║    🧪 Professional C# SpecFlow API Test Automation Demo       ║
║                                                                ║
║    Enterprise-grade test automation framework                  ║
║    Demonstrating best practices and modern techniques          ║
║                                                                ║
╚════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Magenta

Write-Info "Starting setup for environment: $Environment"

# Check prerequisites
Write-Info "Checking prerequisites..."

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($dotnetVersion -and $dotnetVersion.StartsWith("8.")) {
        Write-Success ".NET 8.0 SDK found: $dotnetVersion"
    } else {
        Write-Warning ".NET 8.0 SDK not found. Current version: $dotnetVersion"
        Write-Info "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
        if (-not $SkipDependencies) {
            Write-Error "Cannot continue without .NET 8.0 SDK"
            exit 1
        }
    }
} catch {
    Write-Error ".NET SDK not found. Please install .NET 8.0 SDK"
    exit 1
}

# Check PowerShell version
$psVersion = $PSVersionTable.PSVersion
if ($psVersion.Major -ge 5) {
    Write-Success "PowerShell version: $($psVersion.ToString())"
} else {
    Write-Warning "PowerShell 5.1+ recommended. Current: $($psVersion.ToString())"
}

# Check Docker (if requested)
if ($Docker) {
    try {
        $dockerVersion = docker --version 2>$null
        if ($dockerVersion) {
            Write-Success "Docker found: $dockerVersion"
        } else {
            Write-Error "Docker not found but -Docker flag specified"
            exit 1
        }
    } catch {
        Write-Error "Docker not available"
        exit 1
    }
}

# Set up directory structure
Write-Info "Setting up directory structure..."

$directories = @(
    "TestResults",
    "logs",
    "bin",
    "obj",
    "Features",
    "Steps", 
    "Clients",
    "Configuration",
    "Helpers",
    "Utilities",
    "Hooks",
    "Models",
    "Exceptions",
    "Reporting"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Success "Created directory: $dir"
    }
}

# Install dependencies
if (-not $SkipDependencies) {
    Write-Info "Installing/updating dependencies..."
    
    try {
        Write-Info "Restoring NuGet packages..."
        dotnet restore --verbosity minimal
        if ($LASTEXITCODE -eq 0) {
            Write-Success "NuGet packages restored successfully"
        } else {
            Write-Error "Failed to restore NuGet packages"
            exit 1
        }
        
        Write-Info "Installing global tools..."
        dotnet tool install --global SpecFlow.Plus.LivingDoc.CLI --verbosity minimal 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "SpecFlow LivingDoc CLI installed"
        } else {
            Write-Info "SpecFlow LivingDoc CLI already installed or failed to install"
        }
        
    } catch {
        Write-Error "Failed to install dependencies: $_"
        exit 1
    }
}

# Configure environment
Write-Info "Configuring environment for: $Environment"

# Set environment variables
$env:DOTNET_ENVIRONMENT = $Environment
$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"

# Configure user secrets for demo
Write-Info "Configuring user secrets..."
try {
    dotnet user-secrets init --verbosity minimal 2>$null
    dotnet user-secrets set "ApiSettings:Password" "password123" --verbosity minimal 2>$null
    Write-Success "User secrets configured for demo environment"
} catch {
    Write-Warning "Failed to configure user secrets. Manual setup may be required."
}

# Build the project
Write-Info "Building the project..."
try {
    dotnet build --configuration Release --verbosity minimal --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Project built successfully"
    } else {
        Write-Error "Build failed"
        exit 1
    }
} catch {
    Write-Error "Build failed: $_"
    exit 1
}

# Run tests if requested
if ($RunTests) {
    Write-Info "Running test suite..."
    
    if ($Docker) {
        Write-Info "Building Docker image..."
        docker build -t specflow-api-tests:latest --target test .
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Docker image built successfully"
            
            Write-Info "Running tests in Docker container..."
            docker run --rm --name specflow-tests -e ApiSettings__Password=password123 specflow-api-tests:latest
        } else {
            Write-Error "Docker build failed"
            exit 1
        }
    } else {
        Write-Info "Running tests locally..."
        dotnet test --configuration Release --no-build --verbosity normal --logger:"console;verbosity=normal" --logger:"trx;LogFileName=TestResults.trx" --results-directory TestResults
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Tests completed successfully"
        } else {
            Write-Warning "Some tests may have failed. Check results for details."
        }
    }
}

# Generate reports if requested
if ($GenerateReports -and -not $Docker) {
    Write-Info "Generating comprehensive reports..."
    
    # Generate Living Documentation
    if (Test-Path "TestResults/TestResults.trx") {
        try {
            livingdoc test-assembly "bin/Release/net8.0/SpecFlowApiTests.dll" --test-results "TestResults/TestResults.trx" --output "TestResults/LivingDoc.html"
            Write-Success "Living Documentation generated: TestResults/LivingDoc.html"
        } catch {
            Write-Warning "Failed to generate Living Documentation"
        }
    }
    
    # Create summary report
    $reportContent = @"
# 🧪 Test Automation Demo - Execution Summary

## Setup Information
- **Environment**: $Environment
- **Execution Time**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
- **Framework**: .NET 8.0 + SpecFlow + NUnit
- **Platform**: $($env:OS) - $($env:PROCESSOR_ARCHITECTURE)

## Available Reports
- **Living Documentation**: TestResults/LivingDoc.html
- **Test Results**: TestResults/TestResults.trx
- **Logs**: logs/api_tests.log

## Demo Features Included
✅ Comprehensive test scenarios (Functional, Security, Performance)
✅ Professional reporting and dashboards
✅ CI/CD pipeline integration
✅ Docker containerization
✅ Advanced test utilities and helpers
✅ Performance monitoring and metrics
✅ Security testing examples
✅ Real-world business scenarios

## Next Steps
1. Open TestResults/LivingDoc.html to view interactive documentation
2. Review logs/api_tests.log for detailed execution logs
3. Check TestResults/ folder for all generated artifacts
4. Customize scenarios in Features/ folder for specific requirements

## Professional Services
This demo showcases enterprise-grade test automation capabilities.
Contact for consulting, training, and custom implementation services.
"@

    $reportContent | Out-File -FilePath "TestResults/ExecutionSummary.md" -Encoding UTF8
    Write-Success "Execution summary generated: TestResults/ExecutionSummary.md"
}

# Final setup validation
Write-Info "Validating setup..."

$validationItems = @(
    @{ Path = "SpecFlowApiTests.csproj"; Description = "Project file" },
    @{ Path = "appsettings.json"; Description = "Configuration file" },
    @{ Path = "Features"; Description = "Features directory" },
    @{ Path = "Steps"; Description = "Step definitions directory" }
)

$validationPassed = $true
foreach ($item in $validationItems) {
    if (Test-Path $item.Path) {
        Write-Success "$($item.Description) found"
    } else {
        Write-Error "$($item.Description) not found: $($item.Path)"
        $validationPassed = $false
    }
}

if ($validationPassed) {
    Write-Success "Setup validation completed successfully"
} else {
    Write-Error "Setup validation failed. Please check missing items."
    exit 1
}

# Success message
Write-Host @"

╔════════════════════════════════════════════════════════════════╗
║                                                                ║
║    🎉 SETUP COMPLETED SUCCESSFULLY!                           ║
║                                                                ║
║    Your professional test automation demo is ready!           ║
║                                                                ║
╚════════════════════════════════════════════════════════════════╝

🚀 Quick Start Commands:
  dotnet test                          # Run all tests
  dotnet test --filter Category=smoke  # Run smoke tests only
  docker build -t tests . && docker run tests  # Run in Docker

📊 View Results:
  TestResults/LivingDoc.html          # Interactive documentation
  TestResults/ExecutionSummary.md     # Summary report
  logs/api_tests.log                  # Detailed logs

📋 Demo Highlights:
  ✨ 25+ comprehensive test scenarios
  ✨ Security and performance testing
  ✨ Professional reporting dashboards
  ✨ CI/CD pipeline integration
  ✨ Docker containerization
  ✨ Enterprise-grade architecture

🎯 Perfect for showcasing professional test automation expertise!

"@ -ForegroundColor Green

Write-Info "Setup completed in environment: $Environment"
Write-Info "Framework ready for demonstration and professional use"