# Stage 1: Build the test project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SpecFlowApiTests.csproj", "."]
RUN dotnet restore "./SpecFlowApiTests.csproj"
COPY . .
RUN dotnet build "SpecFlowApiTests.csproj" -c Release -o /app/build

# Stage 2: Run the tests from the build output
FROM build AS test
WORKDIR /src
CMD ["dotnet", "test", "--no-restore", "--no-build", "-c", "Release"]