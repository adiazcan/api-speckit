# Implementation Plan: IoT Robot Control & Telemetry API

**Branch**: `001-iot-robot-api` | **Date**: 2025-11-10 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-iot-robot-api/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a REST API for IoT robot control and telemetry monitoring using C# Minimal APIs. The system enables operators to send control commands (movement, rotation, sensor activation), retrieve real-time and historical telemetry data (position, battery, temperature, sensors), manage event subscriptions via webhooks, and handle multiple robots with role-based access control. Data will be mocked initially for rapid prototyping and testing.

## Technical Context

**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**: ASP.NET Core Minimal APIs, System.Text.Json, FluentValidation, Serilog  
**Storage**: In-memory mock data stores (future: Entity Framework Core with PostgreSQL/SQL Server)  
**Testing**: xUnit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing (integration tests)  
**Target Platform**: Linux/Windows server (containerized with Docker)  
**Project Type**: Single API project  
**Performance Goals**: <500ms command acknowledgment, <300ms telemetry response, 100+ concurrent connections, 1000 data points/sec/robot ingestion  
**Constraints**: <200ms p95 latency for API endpoints, <512MB memory baseline, stateless API design for horizontal scaling  
**Scale/Scope**: 100+ robots, 90 days telemetry retention, ~20 API endpoints, webhook delivery with retry logic

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle I: Code Quality First
- ✅ **PASS**: C# with StyleCop analyzers, editorconfig for formatting
- ✅ **PASS**: Minimal APIs encourage simple, focused endpoint handlers
- ✅ **PASS**: FluentValidation for explicit validation rules (no magic)
- ✅ **PASS**: Serilog for structured logging

### Principle II: Test-Driven Development (TDD)
- ✅ **PASS**: xUnit test framework with contract, integration, and unit test structure
- ✅ **PASS**: Microsoft.AspNetCore.Mvc.Testing for in-memory API testing
- ✅ **PASS**: FluentAssertions for readable test assertions
- ✅ **PASS**: Mocked data enables test-first development without infrastructure dependencies

### Principle III: Development Experience Consistency
- ✅ **PASS**: Single command setup: `dotnet restore && dotnet run`
- ✅ **PASS**: .NET SDK provides consistent tooling (build, test, format)
- ✅ **PASS**: appsettings.json for configuration, supports environment-specific overrides
- ✅ **PASS**: No external dependencies required for local development (mocked data)
- ✅ **PASS**: Swagger/OpenAPI auto-generated for API documentation

### Principle IV: Performance by Design
- ✅ **PASS**: Spec includes quantified performance requirements (SC-001 through SC-005)
- ✅ **PASS**: Async/await patterns in Minimal APIs for non-blocking I/O
- ✅ **PASS**: Pagination built into historical telemetry queries (FR-015)
- ✅ **PASS**: In-memory storage eliminates database latency during development

**GATE RESULT**: ✅ ALL CHECKS PASSED - Proceed to Phase 0

**POST-DESIGN RE-CHECK**: ✅ ALL CHECKS PASSED
- All principles remain satisfied after Phase 1 design
- OpenAPI contract defines 20+ endpoints with clear request/response schemas
- Data model includes 7 entities with validation rules and state machines
- Quickstart guide provides executable examples for all 4 user stories
- Test strategy documented with unit, integration, and contract test examples

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── RobotApi/                      # Main API project
│   ├── Program.cs                 # Application entry point, Minimal API endpoint registration
│   ├── Models/                    # Domain entities and DTOs
│   │   ├── Robot.cs
│   │   ├── Command.cs
│   │   ├── Telemetry.cs
│   │   ├── TelemetryHistory.cs
│   │   ├── Event.cs
│   │   ├── Operator.cs
│   │   └── Subscription.cs
│   ├── Services/                  # Business logic and data access
│   │   ├── IRobotService.cs
│   │   ├── RobotService.cs
│   │   ├── ICommandService.cs
│   │   ├── CommandService.cs
│   │   ├── ITelemetryService.cs
│   │   ├── TelemetryService.cs
│   │   ├── IEventService.cs
│   │   ├── EventService.cs
│   │   ├── IWebhookService.cs
│   │   └── WebhookService.cs
│   ├── Data/                      # Mock data stores
│   │   ├── MockRobotStore.cs
│   │   ├── MockCommandStore.cs
│   │   ├── MockTelemetryStore.cs
│   │   ├── MockEventStore.cs
│   │   └── MockSubscriptionStore.cs
│   ├── Validators/                # FluentValidation validators
│   │   ├── CommandRequestValidator.cs
│   │   ├── SubscriptionRequestValidator.cs
│   │   └── QueryParameterValidators.cs
│   ├── Middleware/                # Custom middleware
│   │   ├── AuthenticationMiddleware.cs
│   │   └── ExceptionHandlingMiddleware.cs
│   └── RobotApi.csproj            # Project file with dependencies

tests/
├── RobotApi.Tests.Contract/      # Contract tests (API interface compliance)
│   ├── RobotEndpointContractTests.cs
│   ├── CommandEndpointContractTests.cs
│   ├── TelemetryEndpointContractTests.cs
│   └── RobotApi.Tests.Contract.csproj
├── RobotApi.Tests.Integration/    # Integration tests (full API scenarios)
│   ├── RobotCommandIntegrationTests.cs
│   ├── TelemetryQueryIntegrationTests.cs
│   ├── WebhookNotificationTests.cs
│   ├── WebApplicationFactory/
│   │   └── CustomWebApplicationFactory.cs
│   └── RobotApi.Tests.Integration.csproj
└── RobotApi.Tests.Unit/           # Unit tests (service logic)
    ├── Services/
    │   ├── RobotServiceTests.cs
    │   ├── CommandServiceTests.cs
    │   ├── TelemetryServiceTests.cs
    │   └── WebhookServiceTests.cs
    ├── Validators/
    │   └── CommandRequestValidatorTests.cs
    └── RobotApi.Tests.Unit.csproj

# Configuration and deployment
├── appsettings.json               # Base configuration
├── appsettings.Development.json   # Development overrides
├── .editorconfig                  # Code style rules
├── Directory.Build.props          # Shared MSBuild properties
├── Dockerfile                     # Container image definition
└── RobotApi.sln                   # Solution file
```

**Structure Decision**: Single API project structure selected because this is a pure REST API with no separate frontend. The C# Minimal APIs approach keeps endpoint definitions co-located in Program.cs for simplicity, while business logic is organized in the Services layer. Mock data stores in the Data folder enable test-first development without database dependencies. Test projects are separated by type (contract, integration, unit) to support the TDD principle and enable independent execution.

## Complexity Tracking

> **No violations detected** - All constitution principles are satisfied by the chosen architecture.
