# Technical Research: IoT Robot Control & Telemetry API

**Feature**: 001-iot-robot-api  
**Date**: 2025-11-10  
**Phase**: Phase 0 - Technical Research

## Overview

This document captures technical decisions, research findings, and architectural patterns for implementing the IoT Robot Control & Telemetry API using C# Minimal APIs with mocked data stores.

## Technology Stack Decisions

### 1. API Framework: ASP.NET Core Minimal APIs

**Decision**: Use Minimal APIs for lightweight, focused endpoint definitions.

**Rationale**:
- **Simplicity**: Minimal APIs reduce boilerplate compared to MVC controllers
- **Performance**: Lower overhead, faster startup time, reduced memory footprint
- **Code Quality**: Encourages focused, single-purpose endpoint handlers (aligns with Principle I)
- **Modern approach**: Idiomatic for .NET 8+ and microservices architecture

**Alternatives Considered**:
- **MVC Controllers**: More structure but adds unnecessary complexity for a pure API
- **gRPC**: Better performance but webhook integration and REST compatibility required by spec

**Best Practices**:
- Group related endpoints using `MapGroup()` for route prefixes
- Use endpoint filters for cross-cutting concerns (validation, logging)
- Keep handler functions small (delegate to services)
- Use `Results` helper methods for consistent response types

### 2. Validation: FluentValidation

**Decision**: Use FluentValidation for request validation.

**Rationale**:
- **Explicit rules**: No magic attributes, clear validation logic (Principle I: Code Quality)
- **Testability**: Validators are independently testable
- **Reusability**: Complex validation rules can be composed and reused
- **Rich API**: Supports async validation, custom rules, dependent properties

**Alternatives Considered**:
- **Data Annotations**: Less flexible, harder to test, magic attribute-based
- **Manual validation**: More code, harder to maintain, no consistency

**Best Practices**:
- One validator per request DTO
- Use `RuleFor()` with descriptive error messages
- Leverage built-in rules (`NotEmpty`, `GreaterThan`, `Must`, etc.)
- Register validators in DI container for automatic integration

### 3. Logging: Serilog

**Decision**: Use Serilog with structured logging for observability.

**Rationale**:
- **Structured logging**: JSON output for easy parsing and querying
- **Rich sinks**: Console, file, Elasticsearch, Application Insights support
- **Performance**: Async logging, minimal overhead
- **Context enrichment**: Automatic request IDs, correlation IDs

**Alternatives Considered**:
- **ILogger (default)**: Less flexible sinks, basic structured logging
- **NLog**: Similar features but Serilog has better .NET Core integration

**Best Practices**:
- Log structured data with `{Property}` syntax
- Use appropriate log levels (Information, Warning, Error, Debug)
- Enrich logs with request context (robot ID, operator ID, command ID)
- Never log sensitive data (auth tokens, passwords)

### 4. Testing Framework: xUnit + FluentAssertions

**Decision**: Use xUnit for test framework, FluentAssertions for readable assertions.

**Rationale**:
- **xUnit**: Industry standard for .NET, parallel execution by default, extensible
- **FluentAssertions**: Readable assertions improve test clarity (Principle II: TDD)
- **Microsoft.AspNetCore.Mvc.Testing**: In-memory API testing without network overhead
- **Strong ecosystem**: Good integration with CI/CD, coverage tools

**Alternatives Considered**:
- **NUnit/MSTest**: xUnit has better async support and is more modern
- **Standard assertions**: Less readable, harder to debug failing tests

**Best Practices**:
- Arrange-Act-Assert pattern in every test
- One assertion concept per test
- Use `theory` and `InlineData` for parameterized tests
- Mock external dependencies (webhooks) with test doubles

### 5. Mock Data Storage: In-Memory Concurrent Collections

**Decision**: Use `ConcurrentDictionary<TKey, TValue>` and `ConcurrentBag<T>` for thread-safe mock stores.

**Rationale**:
- **No infrastructure**: Aligns with Principle III (DX Consistency) - no setup needed
- **Fast development**: Immediate feedback, no database migrations
- **Test isolation**: Each test run gets fresh data
- **Thread-safe**: Supports concurrent API requests without race conditions
- **Future migration path**: Interface-based design allows swapping for EF Core later

**Alternatives Considered**:
- **SQLite in-memory**: Adds database dependency, slower tests
- **Entity Framework In-Memory provider**: Not thread-safe, deprecated for testing

**Best Practices**:
- Define service interfaces (`IRobotService`, `ITelemetryService`)
- Implement mock stores with realistic data generation
- Use `DateTime.UtcNow` for timestamps (avoid time zone issues)
- Pre-seed stores with sample data for manual testing via Swagger

## Architectural Patterns

### Command Pattern for Robot Operations

**Pattern**: Encapsulate robot commands as request objects.

**Implementation**:
```csharp
public class CommandRequest
{
    public string RobotId { get; init; }
    public string CommandType { get; init; } // "move", "rotate", "stop"
    public Dictionary<string, object> Parameters { get; init; }
    public CommandPriority Priority { get; init; }
}
```

**Rationale**:
- Extensible: New command types don't require API changes
- Validation: Centralized validation per command type
- Audit trail: Commands are serializable for logging

### Repository Pattern for Data Access

**Decision**: Use service layer abstraction instead of full repository pattern.

**Rationale**:
- **Simpler**: Mocked data doesn't need repository abstraction overhead
- **YAGNI**: Repository pattern adds complexity without current benefit
- **Service interfaces**: Sufficient for dependency injection and testing
- **Future-ready**: Can introduce repositories when adding real persistence

### Webhook Delivery with Retry Logic

**Pattern**: Background service with exponential backoff for webhook delivery.

**Implementation Strategy**:
- Queue events in-memory (`Channel<T>` or `BlockingCollection<T>`)
- Background worker processes queue
- Retry failed deliveries with exponential backoff (1s, 2s, 4s, 8s, 16s max)
- Dead letter queue after 5 failed attempts
- Use `IHttpClientFactory` for HTTP calls with Polly for resilience

**Rationale**:
- **Reliability**: Ensures event delivery per FR-018 (5 second delivery)
- **Resilience**: Handles temporary network failures
- **Performance**: Async processing doesn't block API requests

## Performance Optimization Strategies

### 1. Async/Await Throughout

**Strategy**: Use async operations for all I/O-bound work.

**Rationale**:
- Frees threads for other requests (supports 100+ concurrent connections)
- Minimal APIs are async by default
- HttpClient webhook calls must be async

### 2. Pagination for Historical Queries

**Strategy**: Default page size 100, max 1000 per FR-015.

**Implementation**:
```csharp
public record TelemetryHistoryQuery(
    string RobotId,
    DateTime StartTime,
    DateTime EndTime,
    int Page = 1,
    int PageSize = 100);
```

**Rationale**:
- Prevents memory exhaustion with large datasets
- Meets SC-004 (2 second response for 24-hour queries)
- Standard REST pagination pattern

### 3. Response Compression

**Strategy**: Enable gzip compression for JSON responses.

**Rationale**:
- Reduces bandwidth for telemetry arrays
- Negligible CPU cost for significant transfer savings
- Built into ASP.NET Core via `UseResponseCompression()`

## Security Considerations

### Authentication: Bearer Token

**Decision**: JWT bearer tokens for stateless authentication.

**Rationale**:
- **Stateless**: No session storage, scales horizontally
- **Standard**: Industry-standard OAuth2 pattern
- **Role claims**: Supports RBAC (operator, viewer, admin) per FR-022

**Mock Implementation**:
- Accept any token format initially
- Validate token presence in middleware
- Extract role from `Authorization` header or use default "operator"

### Input Validation

**Strategy**: Validate all inputs at API boundary.

**Key Validations**:
- Robot ID exists and is accessible by operator
- Command parameters within operational limits (speed, distance)
- Date ranges for historical queries (max 90 days)
- Webhook URLs are valid HTTPS endpoints
- Pagination parameters within bounds

## Data Model Design Principles

### Use Records for DTOs

**Pattern**: C# records for request/response models.

**Rationale**:
- **Immutability**: Prevents accidental mutation (thread-safety)
- **Value equality**: Built-in comparison for testing
- **Concise syntax**: `init` properties, positional records

### Timestamps in ISO 8601 Format

**Decision**: Use `DateTime` with UTC, serialize as ISO 8601.

**Rationale**:
- Meets FR-008 requirement
- Avoids time zone ambiguity
- Standard format for APIs

### Enums for Fixed Value Sets

**Examples**:
```csharp
public enum CommandStatus { Pending, Executing, Completed, Failed }
public enum CommandPriority { Normal, High, Emergency }
public enum RobotConnectionStatus { Online, Offline }
```

**Rationale**:
- Type-safe vs magic strings
- Compile-time validation
- Better tooling support (IntelliSense)

## Error Handling Strategy

### Problem Details (RFC 7807)

**Decision**: Return RFC 7807 Problem Details for errors.

**Rationale**:
- Standard format for HTTP API errors
- Includes error type, title, detail, status, instance
- Supports extension members for additional context

**Example**:
```json
{
  "type": "https://api.example.com/problems/robot-offline",
  "title": "Robot Unavailable",
  "status": 503,
  "detail": "Robot 'robot-42' is currently offline",
  "instance": "/api/robots/robot-42/commands",
  "robotId": "robot-42",
  "lastSeen": "2025-11-10T10:30:00Z"
}
```

### Global Exception Handler

**Pattern**: Middleware catches unhandled exceptions.

**Implementation**:
- Log exception with full context
- Return Problem Details response
- Hide internal error details from clients (security)
- Include trace ID for support correlation

## Deployment Considerations

### Docker Container

**Decision**: Package as Docker container for deployment flexibility.

**Benefits**:
- Consistent environment (dev, test, prod)
- Easy scaling (Kubernetes, Docker Swarm, Azure Container Apps)
- Portable across hosting platforms

### Environment Configuration

**Pattern**: appsettings.json with environment-specific overrides.

**Configuration Sources** (priority order):
1. Environment variables (highest)
2. appsettings.{Environment}.json
3. appsettings.json (lowest)

**Key Settings**:
- API listening ports
- CORS origins
- Log levels per namespace
- Webhook retry configuration
- Mock data seed options

## Testing Strategy

### Contract Tests

**Scope**: Verify API contract compliance (endpoints, request/response schemas).

**Tools**: xUnit, FluentAssertions, JSON schema validation.

**Examples**:
- POST /api/robots/{id}/commands returns 202 Accepted
- GET /api/robots/{id}/telemetry returns Telemetry schema
- Invalid requests return 400 with Problem Details

### Integration Tests

**Scope**: End-to-end user story validation with in-memory API.

**Tools**: Microsoft.AspNetCore.Mvc.Testing, WebApplicationFactory.

**Examples**:
- Send command → verify command stored → check status
- Subscribe to webhook → trigger event → verify webhook called
- Query historical telemetry → verify pagination works

### Unit Tests

**Scope**: Service logic, validation rules, business rules.

**Tools**: xUnit, FluentAssertions, Moq (for dependencies).

**Examples**:
- CommandService.CreateCommandAsync validates robot exists
- TelemetryService.GetHistoricalAsync respects date ranges
- WebhookService.RetryDeliveryAsync implements exponential backoff

## Open Questions / Future Enhancements

### When to Migrate from Mocked Data?

**Trigger**: When persistence across restarts is required or data volume exceeds memory limits.

**Migration Path**:
1. Install Entity Framework Core packages
2. Define `DbContext` and entities
3. Implement repository classes
4. Update DI registration to swap implementations
5. Add migrations for database schema

### Webhook Delivery Monitoring

**Future Enhancement**: Dashboard showing delivery success rates, retries, failures.

**Implementation**: Could add metrics endpoint exposing Prometheus metrics.

### Authentication Provider Integration

**Future Enhancement**: Integrate with actual OAuth2/OIDC provider (Auth0, Azure AD, Keycloak).

**Current**: Mocked token validation sufficient for initial development.

## References

- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Best-Practices)
- [xUnit Documentation](https://xunit.net/)
- [RFC 7807 Problem Details](https://www.rfc-editor.org/rfc/rfc7807)
- [Microsoft Testing Documentation](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
