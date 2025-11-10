# API Usage Guide

Complete guide to using the IoT Robot Control & Telemetry API.

## Table of Contents

- [Authentication](#authentication)
- [Command Management](#command-management)
- [Telemetry Monitoring](#telemetry-monitoring)
- [Event Subscriptions](#event-subscriptions)
- [Robot Management](#robot-management)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)
- [Best Practices](#best-practices)

## Authentication

The API uses Bearer token authentication. Include the token in the `Authorization` header:

```bash
Authorization: Bearer <token>
```

### Mock Tokens (Development)

For development, use these mock tokens:

- **Admin**: `admin-token` - Full access (create, read, update, delete)
- **Operator**: `operator-token` - Create and read operations
- **Viewer**: `viewer-token` - Read-only access

### Example

```bash
curl -H "Authorization: Bearer operator-token" \
  http://localhost:5000/v1/robots
```

### Role-Based Access

| Endpoint | Admin | Operator | Viewer |
|----------|-------|----------|--------|
| GET /v1/robots | ✓ | ✓ | ✓ |
| POST /v1/robots | ✓ | ✗ | ✗ |
| DELETE /v1/robots/{id} | ✓ | ✗ | ✗ |
| POST /v1/robots/{id}/commands | ✓ | ✓ | ✗ |
| GET /v1/robots/{id}/telemetry | ✓ | ✓ | ✓ |

## Command Management

Send commands to robots and monitor their execution.

### Send Command

**Endpoint**: `POST /v1/robots/{robotId}/commands`

**Required Role**: Admin, Operator

#### Move Command

```bash
curl -X POST http://localhost:5000/v1/robots/robot-001/commands \
  -H "Authorization: Bearer operator-token" \
  -H "Content-Type: application/json" \
  -d '{
    "commandType": "move",
    "parameters": {
      "direction": "forward",
      "distance": 10.5,
      "speed": 2.0
    },
    "priority": "Normal"
  }'
```

**Response** (202 Accepted):
```json
{
  "id": "cmd-123",
  "robotId": "robot-001",
  "commandType": "move",
  "parameters": {
    "direction": "forward",
    "distance": 10.5,
    "speed": 2.0
  },
  "status": "Pending",
  "priority": "Normal",
  "createdAt": "2024-01-15T10:30:00Z",
  "createdBy": "operator-user"
}
```

#### Rotate Command

```bash
curl -X POST http://localhost:5000/v1/robots/robot-001/commands \
  -H "Authorization: Bearer operator-token" \
  -H "Content-Type: application/json" \
  -d '{
    "commandType": "rotate",
    "parameters": {
      "direction": "left",
      "degrees": 90
    }
  }'
```

#### Stop Command

```bash
curl -X POST http://localhost:5000/v1/robots/robot-001/commands \
  -H "Authorization: Bearer operator-token" \
  -H "Content-Type: application/json" \
  -d '{
    "commandType": "stop",
    "parameters": {},
    "priority": "High"
  }'
```

#### Sensor Activate Command

```bash
curl -X POST http://localhost:5000/v1/robots/robot-001/commands \
  -H "Authorization: Bearer operator-token" \
  -H "Content-Type: application/json" \
  -d '{
    "commandType": "sensor_activate",
    "parameters": {
      "sensorName": "camera",
      "enabled": true
    }
  }'
```

### List Commands

**Endpoint**: `GET /v1/robots/{robotId}/commands`

**Query Parameters**:
- `status` (optional): Filter by status (Pending, InProgress, Completed, Failed)
- `limit` (optional): Number of results (default: 50, max: 100)

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-001/commands?status=Completed&limit=10" \
  -H "Authorization: Bearer viewer-token"
```

**Response**:
```json
[
  {
    "id": "cmd-123",
    "robotId": "robot-001",
    "commandType": "move",
    "parameters": {...},
    "status": "Completed",
    "priority": "Normal",
    "createdAt": "2024-01-15T10:30:00Z",
    "completedAt": "2024-01-15T10:30:05Z",
    "createdBy": "operator-user"
  }
]
```

### Get Command Details

**Endpoint**: `GET /v1/robots/{robotId}/commands/{commandId}`

```bash
curl -X GET http://localhost:5000/v1/robots/robot-001/commands/cmd-123 \
  -H "Authorization: Bearer viewer-token"
```

## Telemetry Monitoring

Monitor real-time and historical telemetry data from robots.

### Get Latest Telemetry

**Endpoint**: `GET /v1/robots/{robotId}/telemetry`

```bash
curl -X GET http://localhost:5000/v1/robots/robot-001/telemetry \
  -H "Authorization: Bearer viewer-token"
```

**Response**:
```json
{
  "id": "telem-456",
  "robotId": "robot-001",
  "timestamp": "2024-01-15T10:35:00Z",
  "batteryLevel": 87.5,
  "location": {
    "x": 12.5,
    "y": 8.3,
    "z": 0.0
  },
  "sensors": {
    "temperature": 25.5,
    "humidity": 45.2,
    "pressure": 1013.25
  },
  "status": "Active"
}
```

### Get Historical Telemetry

**Endpoint**: `GET /v1/robots/{robotId}/telemetry/history`

**Query Parameters**:
- `from` (required): Start timestamp (ISO 8601)
- `to` (required): End timestamp (ISO 8601)
- `limit` (optional): Max records (default: 100, max: 1000)

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-001/telemetry/history?from=2024-01-15T10:00:00Z&to=2024-01-15T11:00:00Z&limit=50" \
  -H "Authorization: Bearer viewer-token"
```

**Response**:
```json
{
  "robotId": "robot-001",
  "from": "2024-01-15T10:00:00Z",
  "to": "2024-01-15T11:00:00Z",
  "totalRecords": 42,
  "telemetry": [
    {
      "timestamp": "2024-01-15T10:00:00Z",
      "batteryLevel": 90.0,
      "location": {...},
      "sensors": {...}
    }
  ]
}
```

### Export Telemetry as CSV

**Endpoint**: `GET /v1/robots/{robotId}/telemetry/export`

**Query Parameters**: Same as history endpoint

```bash
curl -X GET "http://localhost:5000/v1/robots/robot-001/telemetry/export?from=2024-01-15T10:00:00Z&to=2024-01-15T11:00:00Z" \
  -H "Authorization: Bearer viewer-token" \
  -o telemetry.csv
```

**Response** (text/csv):
```csv
Timestamp,BatteryLevel,LocationX,LocationY,LocationZ,Temperature,Humidity,Pressure,Status
2024-01-15T10:00:00Z,90.0,12.5,8.3,0.0,25.5,45.2,1013.25,Active
2024-01-15T10:01:00Z,89.8,12.6,8.4,0.0,25.6,45.1,1013.26,Active
```

## Event Subscriptions

Subscribe to events and receive webhook notifications.

### Create Subscription

**Endpoint**: `POST /v1/subscriptions`

```bash
curl -X POST http://localhost:5000/v1/subscriptions \
  -H "Authorization: Bearer operator-token" \
  -H "Content-Type: application/json" \
  -d '{
    "webhookUrl": "https://your-app.com/webhooks/robot-events",
    "eventTypes": ["BatteryLow", "CommandCompleted"],
    "description": "Production monitoring webhook"
  }'
```

**Response** (201 Created):
```json
{
  "id": "sub-789",
  "webhookUrl": "https://your-app.com/webhooks/robot-events",
  "eventTypes": ["BatteryLow", "CommandCompleted"],
  "description": "Production monitoring webhook",
  "isActive": true,
  "createdAt": "2024-01-15T10:40:00Z",
  "createdBy": "operator-user"
}
```

### Event Types

- `BatteryLow`: Battery level below 20%
- `BatteryCharging`: Robot started charging
- `CommandCompleted`: Command execution completed
- `CommandFailed`: Command execution failed
- `RobotOffline`: Robot went offline
- `RobotOnline`: Robot came back online

### List Subscriptions

**Endpoint**: `GET /v1/subscriptions`

```bash
curl -X GET http://localhost:5000/v1/subscriptions \
  -H "Authorization: Bearer viewer-token"
```

### Get Subscription Details

**Endpoint**: `GET /v1/subscriptions/{subscriptionId}`

```bash
curl -X GET http://localhost:5000/v1/subscriptions/sub-789 \
  -H "Authorization: Bearer viewer-token"
```

### Update Subscription

**Endpoint**: `PUT /v1/subscriptions/{subscriptionId}`

```bash
curl -X PUT http://localhost:5000/v1/subscriptions/sub-789 \
  -H "Authorization: Bearer operator-token" \
  -H "Content-Type: application/json" \
  -d '{
    "webhookUrl": "https://your-app.com/webhooks/robot-events",
    "eventTypes": ["BatteryLow", "CommandCompleted", "RobotOffline"],
    "isActive": false,
    "description": "Temporarily disabled"
  }'
```

### Delete Subscription

**Endpoint**: `DELETE /v1/subscriptions/{subscriptionId}`

```bash
curl -X DELETE http://localhost:5000/v1/subscriptions/sub-789 \
  -H "Authorization: Bearer operator-token"
```

### Webhook Payload

When an event occurs, the API sends a POST request to your webhook URL:

```json
{
  "eventId": "evt-999",
  "eventType": "BatteryLow",
  "severity": "Warning",
  "robotId": "robot-001",
  "timestamp": "2024-01-15T10:45:00Z",
  "payload": {
    "batteryLevel": 18.5,
    "location": {
      "x": 12.5,
      "y": 8.3,
      "z": 0.0
    }
  },
  "subscriptionId": "sub-789"
}
```

### Webhook Best Practices

1. **Return 200 OK** quickly (within 5 seconds)
2. **Process asynchronously**: Queue the event for background processing
3. **Validate signature**: Check the X-Webhook-Signature header (if implemented)
4. **Idempotency**: Handle duplicate events using eventId
5. **Retry logic**: The API retries failed webhooks with exponential backoff

## Robot Management

Manage robot registrations and metadata.

### List Robots

**Endpoint**: `GET /v1/robots`

```bash
curl -X GET http://localhost:5000/v1/robots \
  -H "Authorization: Bearer viewer-token"
```

**Response**:
```json
[
  {
    "id": "robot-001",
    "name": "Explorer-1",
    "model": "EX-2024",
    "firmwareVersion": "1.2.3",
    "capabilities": {
      "commands": ["move", "rotate", "stop"],
      "sensors": ["camera", "lidar", "temperature"],
      "maxSpeed": 5.0,
      "maxDistance": 100.0
    },
    "status": "Active",
    "location": {
      "x": 12.5,
      "y": 8.3,
      "z": 0.0
    },
    "registeredAt": "2024-01-01T00:00:00Z",
    "lastSeenAt": "2024-01-15T10:50:00Z"
  }
]
```

### Get Robot Details

**Endpoint**: `GET /v1/robots/{robotId}`

```bash
curl -X GET http://localhost:5000/v1/robots/robot-001 \
  -H "Authorization: Bearer viewer-token"
```

### Register New Robot

**Endpoint**: `POST /v1/robots`

**Required Role**: Admin

```bash
curl -X POST http://localhost:5000/v1/robots \
  -H "Authorization: Bearer admin-token" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "robot-004",
    "name": "Scout-4",
    "model": "SC-2024",
    "firmwareVersion": "2.0.0",
    "capabilities": {
      "commands": ["move", "rotate", "stop", "sensor_activate"],
      "sensors": ["camera", "gps", "temperature", "humidity"],
      "maxSpeed": 8.0,
      "maxDistance": 200.0
    }
  }'
```

### Delete Robot

**Endpoint**: `DELETE /v1/robots/{robotId}`

**Required Role**: Admin

```bash
curl -X DELETE http://localhost:5000/v1/robots/robot-004 \
  -H "Authorization: Bearer admin-token"
```

## Error Handling

The API returns standard HTTP status codes and error details:

### Error Response Format

```json
{
  "error": "Validation failed",
  "message": "Invalid command parameters",
  "details": {
    "commandType": "Required field",
    "parameters.distance": "Must be between 0 and 100"
  },
  "timestamp": "2024-01-15T11:00:00Z",
  "path": "/v1/robots/robot-001/commands"
}
```

### HTTP Status Codes

| Code | Meaning | Description |
|------|---------|-------------|
| 200 | OK | Request successful |
| 201 | Created | Resource created successfully |
| 202 | Accepted | Command accepted for processing |
| 400 | Bad Request | Invalid request parameters |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |

### Common Errors

#### Validation Error (400)

```json
{
  "error": "Validation failed",
  "message": "CommandType is required"
}
```

#### Authentication Error (401)

```json
{
  "error": "Unauthorized",
  "message": "Missing or invalid authentication token"
}
```

#### Permission Error (403)

```json
{
  "error": "Forbidden",
  "message": "Insufficient permissions for this operation"
}
```

#### Not Found (404)

```json
{
  "error": "Not Found",
  "message": "Robot with id 'robot-999' not found"
}
```

#### Rate Limit (429)

```json
{
  "error": "Rate limit exceeded",
  "message": "Maximum 100 requests per minute allowed",
  "retryAfter": "60 seconds"
}
```

## Rate Limiting

The API enforces rate limits per operator:

- **Limit**: 100 requests per minute per operator
- **Scope**: Per authentication token
- **Exclusions**: Health and documentation endpoints are not rate-limited

### Rate Limit Headers

Response headers indicate your rate limit status:

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1704800000
```

### Handling Rate Limits

When rate limited, wait for the duration specified in the `Retry-After` header:

```python
import requests
import time

def call_api_with_retry(url, headers):
    response = requests.get(url, headers=headers)
    
    if response.status_code == 429:
        retry_after = int(response.headers.get('Retry-After', 60))
        print(f"Rate limited. Waiting {retry_after} seconds...")
        time.sleep(retry_after)
        return call_api_with_retry(url, headers)
    
    return response
```

## Best Practices

### 1. Use Appropriate Roles

- Use **viewer-token** for read-only operations
- Use **operator-token** for command execution
- Reserve **admin-token** for administrative tasks

### 2. Handle Webhooks Reliably

```javascript
// Express.js webhook handler
app.post('/webhooks/robot-events', async (req, res) => {
  // Respond quickly
  res.status(200).send('OK');
  
  // Process asynchronously
  await queue.add('process-robot-event', req.body);
});
```

### 3. Poll for Command Status

```bash
# Send command
COMMAND_ID=$(curl -X POST ... | jq -r '.id')

# Poll for completion
while true; do
  STATUS=$(curl -X GET /v1/robots/robot-001/commands/$COMMAND_ID | jq -r '.status')
  
  if [ "$STATUS" = "Completed" ] || [ "$STATUS" = "Failed" ]; then
    break
  fi
  
  sleep 1
done
```

### 4. Use Time Windows for Historical Data

```bash
# Query last hour instead of all time
FROM=$(date -u -d '1 hour ago' +%Y-%m-%dT%H:%M:%SZ)
TO=$(date -u +%Y-%m-%dT%H:%M:%SZ)

curl -X GET "/v1/robots/robot-001/telemetry/history?from=$FROM&to=$TO"
```

### 5. Export Large Datasets as CSV

```bash
# More efficient for large datasets
curl -X GET "/v1/robots/robot-001/telemetry/export?from=$FROM&to=$TO" \
  -H "Authorization: Bearer viewer-token" \
  -o data.csv
```

### 6. Implement Exponential Backoff

```python
import time
import requests

def call_api_with_backoff(url, headers, max_retries=5):
    for attempt in range(max_retries):
        try:
            response = requests.get(url, headers=headers, timeout=10)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            if attempt == max_retries - 1:
                raise
            wait_time = 2 ** attempt  # 1, 2, 4, 8, 16 seconds
            time.sleep(wait_time)
```

### 7. Monitor API Health

```bash
# Check API health before making requests
curl -f http://localhost:5000/v1/health || exit 1
```

## Additional Resources

- **OpenAPI Specification**: GET `/v1/openapi.json`
- **Health Check**: GET `/v1/health`
- **Deployment Guide**: `docs/DEPLOYMENT.md`
- **Quickstart**: `specs/001-iot-robot-api/quickstart.md`
