# Quick Start Guide: IoT Robot Control & Telemetry API

**Feature**: 001-iot-robot-api  
**Date**: 2025-11-10  
**Audience**: Developers integrating with the API

## Prerequisites

- .NET 8 SDK installed
- Basic understanding of REST APIs
- HTTP client (curl, Postman, or Swagger UI)

## Quick Start

### 1. Run the API Locally

```bash
# Clone and navigate to project
cd src/RobotApi

# Restore dependencies
dotnet restore

# Run the API (starts on http://localhost:5000)
dotnet run
```

The API will start and display:
```
info: Now listening on: http://localhost:5000
info: Swagger UI available at: http://localhost:5000/swagger
```

### 2. Access Swagger UI

Open your browser to `http://localhost:5000/swagger` to explore the API interactively.

### 3. Authenticate

All API requests require a Bearer token. For development with mocked data, use any non-empty token:

```bash
export API_TOKEN="dev-token-12345"
```

### 4. Test Basic Operations

#### List All Robots

```bash
curl -X GET "http://localhost:5000/v1/robots" \
  -H "Authorization: Bearer $API_TOKEN"
```

**Expected Response**:
```json
[
  {
    "id": "robot-1",
    "name": "Warehouse Bot Alpha",
    "modelType": "RoboX-3000",
    "firmwareVersion": "2.5.1",
    "connectionStatus": "Online",
    "lastSeenAt": "2025-11-10T14:30:00Z",
    "registeredAt": "2025-11-01T09:00:00Z",
    "capabilities": {
      "commands": ["move", "rotate", "stop", "sensor_activate"],
      "sensors": ["battery", "temperature", "position", "proximity"],
      "maxSpeed": 5.0,
      "maxDistance": 100.0
    }
  }
]
```

#### Get Robot Details

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-1" \
  -H "Authorization: Bearer $API_TOKEN"
```

## User Story Walkthroughs

### User Story 1: Send Robot Commands

#### Send a Movement Command

```bash
curl -X POST "http://localhost:5000/v1/robots/robot-1/commands" \
  -H "Authorization: Bearer $API_TOKEN" \
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

**Expected Response** (202 Accepted):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "robotId": "robot-1",
  "commandType": "move",
  "parameters": {
    "direction": "forward",
    "distance": 10.0,
    "speed": 2.0
  },
  "priority": "Normal",
  "status": "Pending",
  "createdAt": "2025-11-10T14:35:00Z",
  "executedAt": null,
  "completedAt": null,
  "operatorId": "operator-alice",
  "result": null,
  "errorMessage": null
}
```

#### Check Command Status

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-1/commands/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer $API_TOKEN"
```

**Expected Response** (when executing):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Executing",
  "executedAt": "2025-11-10T14:35:01Z",
  ...
}
```

#### Send Other Command Types

**Rotation Command**:
```bash
curl -X POST "http://localhost:5000/v1/robots/robot-1/commands" \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "commandType": "rotate",
    "parameters": {
      "direction": "left",
      "degrees": 90
    }
  }'
```

**Emergency Stop**:
```bash
curl -X POST "http://localhost:5000/v1/robots/robot-1/commands" \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "commandType": "stop",
    "parameters": {},
    "priority": "Emergency"
  }'
```

#### Cancel a Command

```bash
curl -X DELETE "http://localhost:5000/v1/robots/robot-1/commands/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer $API_TOKEN"
```

---

### User Story 2: Read Real-Time Telemetry

#### Get Current Telemetry

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-1/telemetry" \
  -H "Authorization: Bearer $API_TOKEN"
```

**Expected Response**:
```json
{
  "id": "telem-abc123",
  "robotId": "robot-1",
  "timestamp": "2025-11-10T14:30:15Z",
  "position": {
    "type": "local",
    "x": 45.2,
    "y": 23.8,
    "z": 0.0
  },
  "orientation": {
    "pitch": 0.0,
    "roll": 0.0,
    "yaw": 90.0
  },
  "speed": 2.5,
  "batteryLevel": 75,
  "temperature": 38.5,
  "sensorReadings": {
    "proximity": 2.3,
    "vibration": 0.02,
    "humidity": 45
  },
  "dataFreshness": 2.5
}
```

#### Get Filtered Telemetry (Battery and Temperature Only)

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-1/telemetry?fields=battery,temperature" \
  -H "Authorization: Bearer $API_TOKEN"
```

**Expected Response**:
```json
{
  "id": "telem-abc123",
  "robotId": "robot-1",
  "timestamp": "2025-11-10T14:30:15Z",
  "batteryLevel": 75,
  "temperature": 38.5
}
```

---

### User Story 3: Monitor Historical Telemetry

#### Query Last 1 Hour of Telemetry

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-1/telemetry/history?startTime=2025-11-10T13:30:00Z&endTime=2025-11-10T14:30:00Z" \
  -H "Authorization: Bearer $API_TOKEN"
```

**Expected Response**:
```json
{
  "data": [
    {
      "id": "telem-001",
      "robotId": "robot-1",
      "timestamp": "2025-11-10T13:30:00Z",
      "batteryLevel": 80,
      "temperature": 37.2,
      ...
    },
    {
      "id": "telem-002",
      "robotId": "robot-1",
      "timestamp": "2025-11-10T13:40:00Z",
      "batteryLevel": 79,
      "temperature": 37.5,
      ...
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 100,
    "totalRecords": 7,
    "totalPages": 1
  }
}
```

#### Query with Pagination

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-1/telemetry/history?startTime=2025-11-09T00:00:00Z&endTime=2025-11-10T00:00:00Z&page=2&pageSize=50" \
  -H "Authorization: Bearer $API_TOKEN"
```

#### Export Telemetry as CSV

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-1/telemetry/export?startTime=2025-11-10T00:00:00Z&endTime=2025-11-10T23:59:59Z&format=csv" \
  -H "Authorization: Bearer $API_TOKEN" \
  -o robot-1-telemetry.csv
```

---

### User Story 4: Receive Real-Time Event Notifications

#### Create a Webhook Subscription

```bash
curl -X POST "http://localhost:5000/v1/subscriptions" \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "robotId": "robot-1",
    "eventTypes": ["battery_low", "error"],
    "webhookUrl": "https://alerts.example.com/robot-events",
    "filterCriteria": {
      "minSeverity": "Warning"
    }
  }'
```

**Expected Response** (201 Created):
```json
{
  "id": "sub-123",
  "operatorId": "operator-alice",
  "robotId": "robot-1",
  "eventTypes": ["battery_low", "error"],
  "webhookUrl": "https://alerts.example.com/robot-events",
  "isActive": true,
  "filterCriteria": {
    "minSeverity": "Warning"
  },
  "createdAt": "2025-11-10T14:40:00Z"
}
```

#### List Your Subscriptions

```bash
curl -X GET "http://localhost:5000/v1/subscriptions" \
  -H "Authorization: Bearer $API_TOKEN"
```

#### Disable a Subscription

```bash
curl -X PATCH "http://localhost:5000/v1/subscriptions/sub-123" \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "isActive": false
  }'
```

#### Delete a Subscription

```bash
curl -X DELETE "http://localhost:5000/v1/subscriptions/sub-123" \
  -H "Authorization: Bearer $API_TOKEN"
```

#### Webhook Payload Format

When an event occurs, your webhook endpoint will receive a POST request:

```json
{
  "id": "event-789",
  "robotId": "robot-1",
  "eventType": "battery_low",
  "severity": "Warning",
  "timestamp": "2025-11-10T14:45:00Z",
  "message": "Battery level below 20%",
  "data": {
    "currentLevel": 18,
    "threshold": 20
  }
}
```

**Your webhook should respond with 200 OK. Failed deliveries will be retried with exponential backoff (1s, 2s, 4s, 8s, 16s).**

---

## Common Scenarios

### Scenario: Monitor Multiple Robots

```bash
# Get all robots
curl -X GET "http://localhost:5000/v1/robots" \
  -H "Authorization: Bearer $API_TOKEN"

# Subscribe to all robot events (no robotId specified)
curl -X POST "http://localhost:5000/v1/subscriptions" \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "eventTypes": ["battery_low", "error", "sensor_threshold"],
    "webhookUrl": "https://monitoring.example.com/all-robots"
  }'
```

### Scenario: Emergency Stop All Robots

```bash
# Send emergency stop to each robot
for robot in robot-1 robot-2 robot-3; do
  curl -X POST "http://localhost:5000/v1/robots/$robot/commands" \
    -H "Authorization: Bearer $API_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"commandType": "stop", "parameters": {}, "priority": "Emergency"}'
done
```

### Scenario: Generate Daily Telemetry Report

```bash
# Export yesterday's data for each robot
START_TIME=$(date -u -d "yesterday 00:00" +"%Y-%m-%dT%H:%M:%SZ")
END_TIME=$(date -u -d "today 00:00" +"%Y-%m-%dT%H:%M:%SZ")

curl -X GET "http://localhost:5000/v1/robots/robot-1/telemetry/export?startTime=$START_TIME&endTime=$END_TIME&format=csv" \
  -H "Authorization: Bearer $API_TOKEN" \
  -o "robot-1-$(date -u -d yesterday +%Y-%m-%d).csv"
```

---

## Error Handling Examples

### Invalid Command Parameters

```bash
curl -X POST "http://localhost:5000/v1/robots/robot-1/commands" \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "commandType": "move",
    "parameters": {
      "direction": "forward",
      "distance": 500.0,
      "speed": 10.0
    }
  }'
```

**Response** (400 Bad Request):
```json
{
  "type": "https://api.example.com/problems/validation-error",
  "title": "Validation Failed",
  "status": 400,
  "detail": "Command parameters exceed robot capabilities",
  "instance": "/v1/robots/robot-1/commands",
  "errors": {
    "parameters.distance": ["Distance 500.0 exceeds maximum 100.0"],
    "parameters.speed": ["Speed 10.0 exceeds maximum 5.0"]
  }
}
```

### Robot Offline

```bash
curl -X POST "http://localhost:5000/v1/robots/robot-3/commands" \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"commandType": "move", "parameters": {"direction": "forward", "distance": 5.0}}'
```

**Response** (503 Service Unavailable):
```json
{
  "type": "https://api.example.com/problems/robot-offline",
  "title": "Robot Unavailable",
  "status": 503,
  "detail": "Robot 'robot-3' is currently offline",
  "instance": "/v1/robots/robot-3/commands",
  "robotId": "robot-3",
  "lastSeen": "2025-11-09T18:30:00Z"
}
```

### Invalid Time Range

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-1/telemetry/history?startTime=2025-08-01T00:00:00Z&endTime=2025-11-10T00:00:00Z" \
  -H "Authorization: Bearer $API_TOKEN"
```

**Response** (400 Bad Request):
```json
{
  "type": "https://api.example.com/problems/invalid-query",
  "title": "Invalid Query Parameters",
  "status": 400,
  "detail": "Time range exceeds maximum allowed duration of 90 days",
  "instance": "/v1/robots/robot-1/telemetry/history",
  "errors": {
    "timeRange": ["Range of 101 days exceeds limit of 90 days"]
  }
}
```

---

## Testing Strategy

### Unit Tests Example

```csharp
[Fact]
public async Task SendCommand_WithValidParameters_ReturnsAccepted()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new CommandRequest
    {
        CommandType = "move",
        Parameters = new Dictionary<string, object>
        {
            ["direction"] = "forward",
            ["distance"] = 10.0,
            ["speed"] = 2.0
        },
        Priority = "Normal"
    };

    // Act
    var response = await client.PostAsJsonAsync("/v1/robots/robot-1/commands", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    var command = await response.Content.ReadFromJsonAsync<Command>();
    command.Status.Should().Be("Pending");
    command.RobotId.Should().Be("robot-1");
}
```

### Integration Test Example

```csharp
[Fact]
public async Task CompleteUserStory_SendCommandAndCheckStatus()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act 1: Send command
    var commandRequest = new CommandRequest { CommandType = "move", Parameters = new { direction = "forward", distance = 5.0 } };
    var sendResponse = await client.PostAsJsonAsync("/v1/robots/robot-1/commands", commandRequest);
    var command = await sendResponse.Content.ReadFromJsonAsync<Command>();

    // Act 2: Check status
    await Task.Delay(100); // Simulate execution time
    var statusResponse = await client.GetAsync($"/v1/robots/robot-1/commands/{command.Id}");
    var updatedCommand = await statusResponse.Content.ReadFromJsonAsync<Command>();

    // Assert
    updatedCommand.Status.Should().BeOneOf("Executing", "Completed");
}
```

---

## Performance Expectations

Based on success criteria:

- **Command acknowledgment**: < 500ms response time
- **Telemetry queries**: < 300ms for current data
- **Historical queries**: < 2 seconds for 24-hour ranges
- **Webhook delivery**: < 5 seconds from event occurrence
- **Concurrent connections**: Support 100+ simultaneous robots
- **Throughput**: Handle 1000 data points/second/robot

---

## Next Steps

1. **Explore Swagger UI** at `http://localhost:5000/swagger` for interactive API testing
2. **Review OpenAPI spec** in `contracts/openapi.yaml` for complete API documentation
3. **Check data model** in `data-model.md` for entity schemas and validation rules
4. **Run tests** with `dotnet test` to verify all user stories
5. **Set up webhooks** using a service like webhook.site for testing notifications

## Support

- **OpenAPI Specification**: `/contracts/openapi.yaml`
- **Data Model Documentation**: `/data-model.md`
- **Research & Architecture**: `/research.md`
- **Implementation Plan**: `/plan.md`
