# Data Model: IoT Robot Control & Telemetry API

**Feature**: 001-iot-robot-api  
**Date**: 2025-11-10  
**Phase**: Phase 1 - Data Model Design

## Overview

This document defines the core entities, their relationships, validation rules, and state transitions for the IoT Robot Control & Telemetry API.

## Entity Definitions

### Robot

Represents a physical IoT robot device registered in the system.

**Properties**:
- `Id` (string, required): Unique robot identifier (assigned by manufacturer or provisioning system)
- `Name` (string, required): Human-readable robot name (max 100 characters)
- `ModelType` (string, required): Robot model/SKU identifier (e.g., "RoboX-3000")
- `FirmwareVersion` (string, required): Current firmware version (semantic versioning format)
- `ConnectionStatus` (enum, required): Current connection state (Online, Offline)
- `LastSeenAt` (DateTime, required): UTC timestamp of last communication
- `RegisteredAt` (DateTime, required): UTC timestamp when robot was registered
- `Capabilities` (object, required): Dictionary of supported features

**Validation Rules**:
- `Id`: 1-50 characters, alphanumeric plus hyphens, must be unique
- `Name`: 1-100 characters, not empty
- `ModelType`: 1-50 characters, not empty
- `FirmwareVersion`: Must match semver pattern (e.g., "1.2.3")
- `LastSeenAt`: Cannot be in the future
- `Capabilities`: Must include at least `commands` array and `sensors` array

**Relationships**:
- One Robot has many Commands
- One Robot has many Telemetry snapshots
- One Robot has many Events

**Example**:
```json
{
  "id": "robot-42",
  "name": "Warehouse Robot Alpha",
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
```

---

### Command

Represents a control instruction sent to a robot.

**Properties**:
- `Id` (string, required): Unique command identifier (GUID)
- `RobotId` (string, required): Target robot ID (foreign key)
- `CommandType` (string, required): Command name (e.g., "move", "rotate", "stop")
- `Parameters` (object, required): Command-specific parameters
- `Priority` (enum, required): Command priority level (Normal, High, Emergency)
- `Status` (enum, required): Current status (Pending, Executing, Completed, Failed)
- `CreatedAt` (DateTime, required): UTC timestamp when command was created
- `ExecutedAt` (DateTime, nullable): UTC timestamp when execution started
- `CompletedAt` (DateTime, nullable): UTC timestamp when execution finished
- `OperatorId` (string, required): ID of operator who issued command
- `Result` (object, nullable): Execution result data
- `ErrorMessage` (string, nullable): Error details if status is Failed

**Validation Rules**:
- `Id`: Must be valid GUID format
- `RobotId`: Must reference existing robot
- `CommandType`: Must be in robot's capability.commands array
- `Priority`: One of Normal, High, Emergency
- `Status`: Valid state transition (see state machine below)
- `Parameters`: Must be valid for command type (validated per command)
- `OperatorId`: Must reference valid operator
- `ExecutedAt`: Must be >= CreatedAt if set
- `CompletedAt`: Must be >= ExecutedAt if set

**State Transitions**:
```
Pending → Executing → Completed
                    → Failed
Pending → Failed (validation error, robot offline)
Executing → Failed (runtime error)
```

**Command-Specific Parameters**:

**Move Command**:
```json
{
  "commandType": "move",
  "parameters": {
    "direction": "forward",  // forward, backward, left, right
    "distance": 10.0,        // meters, 0-100
    "speed": 2.0             // m/s, 0-maxSpeed
  }
}
```

**Rotate Command**:
```json
{
  "commandType": "rotate",
  "parameters": {
    "direction": "left",     // left, right
    "degrees": 90            // 0-360
  }
}
```

**Stop Command**:
```json
{
  "commandType": "stop",
  "parameters": {}
}
```

**Sensor Activate Command**:
```json
{
  "commandType": "sensor_activate",
  "parameters": {
    "sensorName": "proximity",
    "enabled": true
  }
}
```

**Relationships**:
- Many Commands belong to one Robot
- One Command generates zero or more Events (command_completed, command_failed)

---

### Telemetry

Represents a snapshot of robot sensor data at a point in time.

**Properties**:
- `Id` (string, required): Unique telemetry snapshot ID (GUID)
- `RobotId` (string, required): Source robot ID (foreign key)
- `Timestamp` (DateTime, required): UTC timestamp when data was captured
- `Position` (object, required): Robot position coordinates
- `Orientation` (object, required): Robot orientation (pitch, roll, yaw)
- `Speed` (double, required): Current speed in m/s
- `BatteryLevel` (int, required): Battery percentage (0-100)
- `Temperature` (double, required): Internal temperature in Celsius
- `SensorReadings` (object, required): Dictionary of sensor-specific values
- `DataFreshness` (double, computed): Seconds since timestamp (computed at query time)

**Validation Rules**:
- `Id`: Must be valid GUID
- `RobotId`: Must reference existing robot
- `Timestamp`: Cannot be in the future
- `Position.latitude`: -90 to 90 (if using GPS)
- `Position.longitude`: -180 to 180 (if using GPS)
- `Position.x`, `Position.y`: Any double (if using local coordinates)
- `Speed`: >= 0
- `BatteryLevel`: 0 to 100
- `Temperature`: -50 to 100 (reasonable operating range)
- `SensorReadings`: Keys must match robot.capabilities.sensors

**Position Formats**:

**GPS Coordinates**:
```json
{
  "position": {
    "type": "gps",
    "latitude": 37.7749,
    "longitude": -122.4194,
    "altitude": 10.5
  }
}
```

**Local Coordinates**:
```json
{
  "position": {
    "type": "local",
    "x": 42.5,
    "y": 18.3,
    "z": 0.0
  }
}
```

**Relationships**:
- Many Telemetry snapshots belong to one Robot
- Telemetry data is aggregated in TelemetryHistory for historical queries

**Example**:
```json
{
  "id": "telem-abc123",
  "robotId": "robot-42",
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
  }
}
```

---

### TelemetryHistory

Logical collection representing time-series telemetry data for historical queries.

**Properties**:
- This is not a separate entity but a query view over Telemetry collection
- Filtered by `RobotId`, `StartTime`, `EndTime`
- Ordered by `Timestamp` ascending
- Supports pagination (page, pageSize)

**Query Parameters**:
- `robotId` (required): Robot to query
- `startTime` (required): Start of time range (ISO 8601)
- `endTime` (required): End of time range (ISO 8601)
- `telemetryTypes` (optional): Array of fields to include (e.g., ["battery", "temperature"])
- `page` (optional, default=1): Page number
- `pageSize` (optional, default=100, max=1000): Items per page

**Validation Rules**:
- `startTime` must be before `endTime`
- Time range cannot exceed 90 days
- `page` must be >= 1
- `pageSize` must be 1-1000

---

### Event

Represents a significant occurrence requiring notification.

**Properties**:
- `Id` (string, required): Unique event ID (GUID)
- `RobotId` (string, required): Source robot ID (foreign key)
- `EventType` (string, required): Event category (battery_low, error, command_completed, sensor_threshold)
- `Severity` (enum, required): Event importance (Info, Warning, Error, Critical)
- `Timestamp` (DateTime, required): UTC timestamp when event occurred
- `Message` (string, required): Human-readable description
- `Data` (object, required): Event-specific payload

**Validation Rules**:
- `Id`: Must be valid GUID
- `RobotId`: Must reference existing robot
- `EventType`: One of predefined types
- `Severity`: One of Info, Warning, Error, Critical
- `Timestamp`: Cannot be in the future
- `Message`: 1-500 characters

**Event Types and Data**:

**Battery Low**:
```json
{
  "eventType": "battery_low",
  "severity": "Warning",
  "message": "Battery level below 20%",
  "data": {
    "currentLevel": 18,
    "threshold": 20
  }
}
```

**Command Completed**:
```json
{
  "eventType": "command_completed",
  "severity": "Info",
  "message": "Move command completed successfully",
  "data": {
    "commandId": "cmd-xyz",
    "commandType": "move",
    "duration": 15.3
  }
}
```

**Sensor Threshold Exceeded**:
```json
{
  "eventType": "sensor_threshold",
  "severity": "Warning",
  "message": "Temperature exceeds 45°C",
  "data": {
    "sensorName": "temperature",
    "currentValue": 47.2,
    "threshold": 45.0
  }
}
```

**Relationships**:
- Many Events belong to one Robot
- Many Events trigger Webhook deliveries via Subscriptions

---

### Operator

Represents a user with authentication credentials and permissions.

**Properties**:
- `Id` (string, required): Unique operator identifier (GUID or username)
- `Username` (string, required): Login username
- `Email` (string, required): Contact email
- `Role` (enum, required): Permission level (Viewer, Operator, Administrator)
- `CreatedAt` (DateTime, required): UTC timestamp of account creation
- `LastLoginAt` (DateTime, nullable): UTC timestamp of last login

**Validation Rules**:
- `Id`: 1-100 characters, unique
- `Username`: 3-50 characters, alphanumeric plus underscore/hyphen
- `Email`: Valid email format
- `Role`: One of Viewer, Operator, Administrator

**Permissions by Role**:
- **Viewer**: Read telemetry, view robots (no commands)
- **Operator**: Viewer + send commands, manage subscriptions
- **Administrator**: Operator + register/deregister robots, manage operators

**Relationships**:
- One Operator creates many Commands
- One Operator has many Subscriptions

---

### Subscription

Represents an operator's registration for event notifications.

**Properties**:
- `Id` (string, required): Unique subscription ID (GUID)
- `OperatorId` (string, required): Subscriber operator ID (foreign key)
- `RobotId` (string, nullable): Specific robot to monitor (null = all robots)
- `EventTypes` (array, required): Event types to receive (e.g., ["battery_low", "error"])
- `WebhookUrl` (string, required): HTTPS endpoint for notifications
- `IsActive` (bool, required): Whether subscription is enabled
- `FilterCriteria` (object, nullable): Optional filters (e.g., severity >= Warning)
- `CreatedAt` (DateTime, required): UTC timestamp of subscription creation

**Validation Rules**:
- `Id`: Must be valid GUID
- `OperatorId`: Must reference existing operator
- `RobotId`: Must reference existing robot if set
- `EventTypes`: At least one type, all types must be valid
- `WebhookUrl`: Must be valid HTTPS URL (no HTTP for security)
- `FilterCriteria.minSeverity`: One of Info, Warning, Error, Critical

**Example**:
```json
{
  "id": "sub-123",
  "operatorId": "operator-alice",
  "robotId": "robot-42",
  "eventTypes": ["battery_low", "error"],
  "webhookUrl": "https://alerts.example.com/robot-events",
  "isActive": true,
  "filterCriteria": {
    "minSeverity": "Warning"
  },
  "createdAt": "2025-11-10T10:00:00Z"
}
```

**Relationships**:
- Many Subscriptions belong to one Operator
- One Subscription filters Events for one Robot (or all robots if null)

---

## Entity Relationship Diagram

```
┌─────────────┐
│   Operator  │
└─────┬───────┘
      │
      │ 1:N (creates)
      │
┌─────▼───────┐        ┌─────────────┐
│   Command   │───────▶│    Event    │
└─────┬───────┘   N:N  └─────┬───────┘
      │                       │
      │ N:1                   │ N:1
      │                       │
┌─────▼───────┐               │
│    Robot    │◀──────────────┘
└─────┬───────┘
      │
      │ 1:N
      │
┌─────▼────────┐
│  Telemetry   │
└──────────────┘

┌─────────────┐        ┌─────────────┐
│   Operator  │───────▶│Subscription │
└─────────────┘  1:N   └─────┬───────┘
                              │ N:1
                              │
                        ┌─────▼───────┐
                        │    Robot    │
                        └─────────────┘
```

## State Machines

### Command State Machine

```
┌─────────┐
│ Pending │
└────┬────┘
     │
     │ (robot online, validated)
     ▼
┌──────────┐         ┌───────────┐
│Executing │────────▶│ Completed │
└────┬─────┘         └───────────┘
     │
     │ (error occurs)
     ▼
┌──────────┐
│  Failed  │
└──────────┘

Transitions from Pending:
- → Executing: Robot online, command valid, resources available
- → Failed: Robot offline, validation error, robot busy (depends on priority)

Transitions from Executing:
- → Completed: Command executed successfully
- → Failed: Runtime error, robot disconnected, timeout

Terminal states: Completed, Failed
```

### Robot Connection Status

```
┌────────┐         ┌─────────┐
│ Online │◀───────▶│ Offline │
└────────┘         └─────────┘

Transitions:
- Online → Offline: No telemetry received for 60 seconds
- Offline → Online: Telemetry received
```

## Indexes and Query Optimization

### Primary Queries

1. **Get Robot by ID**: `robots[id]` - O(1) dictionary lookup
2. **Get Commands by Robot**: `commands.Where(c => c.RobotId == id).OrderByDescending(c => c.CreatedAt)`
3. **Get Current Telemetry**: `telemetry.Where(t => t.RobotId == id).OrderByDescending(t => t.Timestamp).First()`
4. **Historical Telemetry**: `telemetry.Where(t => t.RobotId == id && t.Timestamp >= start && t.Timestamp <= end).OrderBy(t => t.Timestamp)`
5. **Active Subscriptions**: `subscriptions.Where(s => s.IsActive && (s.RobotId == id || s.RobotId == null))`

### Mock Store Indexes

For in-memory mock stores:
- `Dictionary<string, Robot>` keyed by RobotId
- `Dictionary<string, Command>` keyed by CommandId
- `Dictionary<string, Telemetry>` keyed by TelemetryId
- `List<Telemetry>` secondary index for time-range queries (sorted by Timestamp)
- `Dictionary<string, Subscription>` keyed by SubscriptionId

## Data Seeding Strategy

### Pre-populated Mock Data

For manual testing via Swagger UI:

**Robots**:
- `robot-1`: "Warehouse Bot Alpha" (Online)
- `robot-2`: "Delivery Bot Beta" (Online)
- `robot-3`: "Inspection Bot Gamma" (Offline)

**Commands**: 5 sample commands per robot (mix of Completed/Failed/Pending)

**Telemetry**: 100 snapshots per robot over past 24 hours (10-minute intervals)

**Operators**:
- `operator-admin`: Role = Administrator
- `operator-alice`: Role = Operator
- `operator-viewer`: Role = Viewer

**Subscriptions**: 2 subscriptions (one for battery alerts, one for errors)

### Data Generation Rules

- Robot IDs: `robot-{n}` format
- Command IDs: GUID v4
- Telemetry IDs: GUID v4
- Timestamps: Use `DateTime.UtcNow` for current, subtract TimeSpan for historical
- Battery levels: Random 10-100, decrease over time
- Positions: Random walk simulation (incremental changes)
- Temperature: Random 35-42°C (normal operating range)

## Validation Summary

| Entity | Required Fields | Unique Constraints | Referential Integrity |
|--------|----------------|-------------------|----------------------|
| Robot | Id, Name, ModelType, FirmwareVersion, ConnectionStatus, LastSeenAt | Id | - |
| Command | Id, RobotId, CommandType, Priority, Status, CreatedAt, OperatorId | Id | RobotId → Robot, OperatorId → Operator |
| Telemetry | Id, RobotId, Timestamp, Position, BatteryLevel, Temperature | Id | RobotId → Robot |
| Event | Id, RobotId, EventType, Severity, Timestamp, Message | Id | RobotId → Robot |
| Operator | Id, Username, Email, Role, CreatedAt | Id, Username, Email | - |
| Subscription | Id, OperatorId, EventTypes, WebhookUrl, IsActive, CreatedAt | Id | OperatorId → Operator, RobotId → Robot (if not null) |

## Future Considerations

### Database Migration

When moving from mocked to persistent storage:

1. **Entity Framework Core Entities**: Add `[Key]`, `[Required]`, `[ForeignKey]` attributes
2. **DbContext**: Define `DbSet<T>` for each entity
3. **Migrations**: Use EF migrations to generate schema
4. **Indexes**: Add indexes on foreign keys, timestamp columns, status enums
5. **Relationships**: Define navigation properties (Robot.Commands, Robot.Telemetry)

### Time-Series Optimization

For high-volume telemetry data:

- Consider TimescaleDB extension for PostgreSQL
- Or use dedicated time-series database (InfluxDB, TimescaleDB)
- Implement data retention policy (auto-delete data older than 90 days)
- Use partitioning by date for historical tables
