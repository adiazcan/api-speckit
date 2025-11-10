# IoT Robot Control & Telemetry API

REST API for controlling IoT robots and monitoring telemetry data. Built with C# 12, .NET 8, and ASP.NET Core Minimal APIs.

## Features

- **Robot Command Control** (P1/MVP): Send control commands (move, rotate, stop, sensor_activate) to robots and track command status
- **Real-time Telemetry** (P2): Retrieve current telemetry data (position, battery, temperature, sensors) from online robots
- **Historical Telemetry** (P3): Query historical telemetry with time range filtering, pagination, and CSV export
- **Event Notifications** (P4): Subscribe to robot events via webhooks (battery_low, command_completed, sensor_threshold, errors)
- **Robot Management**: Register and deregister robots (Administrator role)

## Technology Stack

- **Framework**: .NET 8 with ASP.NET Core Minimal APIs
- **Validation**: FluentValidation for request validation
- **Logging**: Serilog with structured logging
- **Testing**: xUnit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing
- **Data Storage**: In-memory mock data stores (ConcurrentDictionary/ConcurrentBag)
- **Authentication**: JWT bearer tokens (mocked initially)
- **API Specification**: OpenAPI 3.0

## Project Structure

```
src/
  RobotApi/                    # Main API project
    Models/                    # Entity models and DTOs
    Services/                  # Business logic services
    Data/                      # In-memory data stores
    Validators/                # FluentValidation validators
    Middleware/                # Custom middleware
    Endpoints/                 # Minimal API endpoints
    Extensions/                # Extension methods
    Program.cs                 # Application entry point

tests/
  RobotApi.Tests.Contract/     # Contract tests (OpenAPI compliance)
  RobotApi.Tests.Integration/  # Integration tests (end-to-end scenarios)
  RobotApi.Tests.Unit/         # Unit tests (business logic)
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd api-speckit
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the API**
   ```bash
   cd src/RobotApi
   dotnet run
   ```

   The API will be available at:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`

5. **Run tests**
   ```bash
   # Run all tests
   dotnet test

   # Run specific test project
   dotnet test tests/RobotApi.Tests.Unit
   dotnet test tests/RobotApi.Tests.Integration
   dotnet test tests/RobotApi.Tests.Contract
   ```

### Quick Test

Once the API is running, you can test it with curl:

```bash
# List all robots
curl http://localhost:5000/v1/robots

# Get robot details
curl http://localhost:5000/v1/robots/robot-42

# Get current telemetry
curl http://localhost:5000/v1/robots/robot-42/telemetry

# Send a command
curl -X POST http://localhost:5000/v1/robots/robot-42/commands \
  -H "Content-Type: application/json" \
  -d '{
    "commandType": "move",
    "parameters": {
      "direction": "forward",
      "distance": 10.0,
      "speed": 2.0
    },
    "priority": "Normal"
  }'
```

## API Documentation

Once the API is running, you can access:

- **OpenAPI Specification**: `http://localhost:5000/v1/openapi.json`
- **Health Check**: `http://localhost:5000/v1/health`

For detailed API usage examples, see [specs/001-iot-robot-api/quickstart.md](specs/001-iot-robot-api/quickstart.md).

## Configuration

### appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  }
}
```

### appsettings.Development.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  },
  "Cors": {
    "AllowedOrigins": ["*"]
  }
}
```

## Authentication

The API uses JWT bearer token authentication. In the current mocked implementation, use these test operators:

- **Admin**: `admin` (Administrator role - full access)
- **Operator**: `operator` (Operator role - commands and telemetry)
- **Viewer**: `viewer` (Viewer role - read-only)

## Performance Targets

Per constitution Principle IV (Performance by Design):

- Command latency: < 500ms
- Telemetry retrieval: < 300ms
- Historical query: Efficient for 1000+ records
- Webhook delivery: < 10 seconds (including retries)
- Concurrent robot support: 100+ simultaneous connections

## Development Principles

This project follows the [API-SpecKit Constitution](specs/memory/constitution.md):

1. **Code Quality First**: Linting, SRP, no magic values
2. **TDD**: 80% test coverage, Red-Green-Refactor
3. **DX Consistency**: Single-command setup, 30min onboarding
4. **Performance by Design**: Quantified targets, async patterns

## Contributing

1. Follow the constitution principles
2. Write tests first (TDD approach)
3. Ensure all tests pass before submitting
4. Follow C# coding conventions in `.editorconfig`
5. Update documentation as needed

## Project Status

✅ **Phase 1: Setup** - Complete  
⏳ **Phase 2: Foundational** - In Progress  
⏳ **Phase 3: User Story 1 (MVP)** - Pending  
⏳ **Phase 4-6: User Stories 2-4** - Pending  
⏳ **Phase 7: Robot Management** - Pending  
⏳ **Phase 8: Polish** - Pending

See [specs/001-iot-robot-api/tasks.md](specs/001-iot-robot-api/tasks.md) for detailed task breakdown.

## License

[Your License Here]

## Support

For issues or questions, please contact the development team or create an issue in the repository.
