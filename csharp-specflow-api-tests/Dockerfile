# Multi-stage Dockerfile for SpecFlow API Tests
# Optimized for caching, security, and performance

# Stage 1: Restore dependencies (leverages Docker layer caching)
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS restore
WORKDIR /src

# Copy only the project file first for better caching
COPY ["SpecFlowApiTests.csproj", "."]

# Restore dependencies with optimizations
RUN dotnet restore "./SpecFlowApiTests.csproj" \
    --runtime linux-musl-x64 \
    --no-cache \
    --verbosity minimal

# Stage 2: Build the application
FROM restore AS build

# Copy the source code
COPY . .

# Build the project
RUN dotnet build "SpecFlowApiTests.csproj" \
    --configuration Release \
    --output /app/build \
    --no-restore \
    --runtime linux-musl-x64 \
    --verbosity minimal

# Stage 3: Test stage (can be run independently)
FROM build AS test
WORKDIR /src

# Set environment variables for test execution
ENV DOTNET_ENVIRONMENT=Test
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Default API settings for demo environment
ENV ApiSettings__BaseUrl=https://restful-booker.herokuapp.com
ENV ApiSettings__Username=admin
ENV ApiSettings__Password=password123
ENV ApiSettings__TimeoutSeconds=60
ENV ApiSettings__MaxRetries=3

# Create non-root user for security
RUN addgroup -g 1001 -S testgroup && \
    adduser -S testuser -G testgroup -u 1001

# Create logs directory and set permissions
RUN mkdir -p /src/logs && \
    chown -R testuser:testgroup /src

# Switch to non-root user
USER testuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD dotnet --version || exit 1

# Default command to run tests
CMD ["dotnet", "test", \
     "--configuration", "Release", \
     "--no-restore", \
     "--no-build", \
     "--logger:console;verbosity=normal", \
     "--logger:trx;LogFileName=TestResults.trx", \
     "--results-directory", "TestResults"]

# Stage 4: Runtime stage (for production deployment scenarios)
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS runtime
WORKDIR /app

# Install curl for health checks in runtime
RUN apk add --no-cache curl

# Copy built application
COPY --from=build /app/build .

# Create non-root user
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -G appgroup -u 1001

# Set ownership
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Health check for runtime
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Runtime entry point
ENTRYPOINT ["dotnet", "SpecFlowApiTests.dll"]

# Stage 5: Development stage (for development purposes)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /src

# Install debugging tools
RUN dotnet tool install --global dotnet-dump && \
    dotnet tool install --global dotnet-trace

# Copy source for development
COPY . .

# Restore and build
RUN dotnet restore && \
    dotnet build --configuration Debug

# Set development environment
ENV DOTNET_ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose common debugging ports
EXPOSE 5000 5001

# Development command
CMD ["dotnet", "watch", "test", "--verbosity", "normal"]