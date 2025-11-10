# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["RobotApi.sln", "./"]
COPY ["src/RobotApi/RobotApi.csproj", "src/RobotApi/"]
COPY ["tests/RobotApi.Tests.Unit/RobotApi.Tests.Unit.csproj", "tests/RobotApi.Tests.Unit/"]
COPY ["tests/RobotApi.Tests.Contract/RobotApi.Tests.Contract.csproj", "tests/RobotApi.Tests.Contract/"]
COPY ["tests/RobotApi.Tests.Integration/RobotApi.Tests.Integration.csproj", "tests/RobotApi.Tests.Integration/"]

# Restore dependencies
RUN dotnet restore "src/RobotApi/RobotApi.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/src/RobotApi"
RUN dotnet build "RobotApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RobotApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/v1/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "RobotApi.dll"]
