# Tasks: IoT Robot Control & Telemetry API

**Feature**: 001-iot-robot-api  
**Date**: 2025-11-10  
**Phase**: Phase 2 - Task Breakdown

**Input**: Design documents from `/specs/001-iot-robot-api/`  
**Prerequisites**: ✅ plan.md, ✅ spec.md, ✅ research.md, ✅ data-model.md, ✅ contracts/openapi.yaml

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Per plan.md structure:
- `src/RobotApi/` - Main API project
- `tests/RobotApi.Tests.Contract/` - Contract tests (OpenAPI compliance)
- `tests/RobotApi.Tests.Integration/` - Integration tests (end-to-end scenarios)
- `tests/RobotApi.Tests.Unit/` - Unit tests (business logic)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create project directory structure (src/RobotApi/, tests/ with 3 projects)
- [X] T002 Initialize .NET 8 solution with `dotnet new sln -n RobotApi`
- [X] T003 [P] Create RobotApi project with `dotnet new web -n RobotApi -o src/RobotApi`
- [X] T004 [P] Create contract test project with `dotnet new xunit -n RobotApi.Tests.Contract -o tests/RobotApi.Tests.Contract`
- [X] T005 [P] Create integration test project with `dotnet new xunit -n RobotApi.Tests.Integration -o tests/RobotApi.Tests.Integration`
- [X] T006 [P] Create unit test project with `dotnet new xunit -n RobotApi.Tests.Unit -o tests/RobotApi.Tests.Unit`
- [X] T007 Add projects to solution with `dotnet sln add` commands
- [X] T008 [P] Add FluentValidation package to src/RobotApi/RobotApi.csproj
- [X] T009 [P] Add Serilog packages (Serilog.AspNetCore, Serilog.Sinks.Console) to src/RobotApi/RobotApi.csproj
- [X] T010 [P] Add FluentAssertions package to all test projects
- [X] T011 [P] Add Microsoft.AspNetCore.Mvc.Testing to tests/RobotApi.Tests.Integration/
- [X] T012 Create appsettings.json configuration file in src/RobotApi/
- [X] T013 Create appsettings.Development.json for local development in src/RobotApi/
- [X] T014 Create .editorconfig with C# formatting rules at repository root
- [X] T015 Create .gitignore for .NET projects at repository root
- [X] T016 Create README.md with project overview and setup instructions at repository root

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T017 [P] Create base model interfaces in src/RobotApi/Models/IEntity.cs
- [X] T018 [P] Create Operator model in src/RobotApi/Models/Operator.cs (Id, Username, Email, Role enum)
- [X] T019 [P] Create Robot model in src/RobotApi/Models/Robot.cs (Id, Name, ModelType, FirmwareVersion, ConnectionStatus enum, LastSeenAt, RegisteredAt, Capabilities)
- [X] T020 Configure Serilog in src/RobotApi/Program.cs (console sink, structured logging, request logging)
- [X] T021 Implement mock authentication middleware in src/RobotApi/Middleware/MockAuthenticationMiddleware.cs (JWT simulation with hardcoded operators)
- [X] T022 Create authentication helpers in src/RobotApi/Extensions/AuthenticationExtensions.cs (claims extraction, operator context)
- [X] T023 Create in-memory data store interface in src/RobotApi/Data/IDataStore.cs
- [X] T024 Implement OperatorStore in src/RobotApi/Data/OperatorStore.cs (ConcurrentDictionary with 3 seeded operators)
- [X] T025 Implement RobotStore in src/RobotApi/Data/RobotStore.cs (ConcurrentDictionary with 5 seeded robots)
- [X] T026 Create data seeding service in src/RobotApi/Data/DataSeeder.cs (initialize all stores with test data)
- [X] T027 Create ProblemDetails factory in src/RobotApi/Extensions/ProblemDetailsExtensions.cs (RFC 7807 format)
- [X] T028 Implement global exception handler middleware in src/RobotApi/Middleware/ExceptionHandlerMiddleware.cs
- [X] T029 Create validation extensions in src/RobotApi/Extensions/ValidationExtensions.cs (FluentValidation integration)
- [X] T030 Configure CORS policy in src/RobotApi/Program.cs (allow localhost for development)
- [X] T031 Setup API versioning in src/RobotApi/Program.cs (/v1 prefix for all endpoints)
- [X] T032 Create base integration test fixture in tests/RobotApi.Tests.Integration/Fixtures/ApiFixture.cs (WebApplicationFactory)
- [X] T033 Create test authentication helper in tests/RobotApi.Tests.Integration/Helpers/AuthHelper.cs (generate test JWT tokens)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Send Robot Commands (Priority: P1) 🎯 MVP

**Goal**: Enable operators to send control commands (move, rotate, stop, sensor_activate) to robots and track command status

**Independent Test**: Send POST request to `/v1/robots/{robotId}/commands` with valid command, receive 202 Accepted with command ID, then GET `/v1/robots/{robotId}/commands/{commandId}` to verify status transitions from Pending → Executing → Completed

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T034 [P] [US1] Contract test for POST /robots/{robotId}/commands in tests/RobotApi.Tests.Contract/CommandsContractTests.cs (OpenAPI schema validation)
- [ ] T035 [P] [US1] Contract test for GET /robots/{robotId}/commands/{commandId} in tests/RobotApi.Tests.Contract/CommandsContractTests.cs
- [ ] T036 [P] [US1] Contract test for GET /robots/{robotId}/commands with filtering in tests/RobotApi.Tests.Contract/CommandsContractTests.cs
- [ ] T037 [P] [US1] Contract test for DELETE /robots/{robotId}/commands/{commandId} in tests/RobotApi.Tests.Contract/CommandsContractTests.cs
- [ ] T038 [P] [US1] Integration test for send move command scenario in tests/RobotApi.Tests.Integration/CommandScenarioTests.cs
- [ ] T039 [P] [US1] Integration test for send rotate command scenario in tests/RobotApi.Tests.Integration/CommandScenarioTests.cs
- [ ] T040 [P] [US1] Integration test for send stop command scenario in tests/RobotApi.Tests.Integration/CommandScenarioTests.cs
- [ ] T041 [P] [US1] Integration test for command status tracking in tests/RobotApi.Tests.Integration/CommandScenarioTests.cs
- [ ] T042 [P] [US1] Integration test for command cancellation in tests/RobotApi.Tests.Integration/CommandScenarioTests.cs
- [ ] T043 [P] [US1] Integration test for offline robot rejection in tests/RobotApi.Tests.Integration/CommandScenarioTests.cs
- [ ] T044 [P] [US1] Unit test for CommandService.SendCommandAsync in tests/RobotApi.Tests.Unit/Services/CommandServiceTests.cs
- [ ] T045 [P] [US1] Unit test for command validation rules in tests/RobotApi.Tests.Unit/Validators/CommandValidatorTests.cs
- [ ] T046 [P] [US1] Unit test for command state machine transitions in tests/RobotApi.Tests.Unit/Models/CommandTests.cs

### Implementation for User Story 1

- [X] T047 [P] [US1] Create Command model in src/RobotApi/Models/Command.cs (Id, RobotId, CommandType, Parameters, Priority enum, Status enum, timestamps, OperatorId, Result, ErrorMessage)
- [X] T048 [P] [US1] Create CommandRequest DTO in src/RobotApi/Models/DTOs/CommandRequest.cs (CommandType, Parameters, Priority)
- [X] T049 [P] [US1] Create CommandResponse DTO in src/RobotApi/Models/DTOs/CommandResponse.cs (map from Command entity)
- [X] T050 [US1] Create CommandRequestValidator in src/RobotApi/Validators/CommandRequestValidator.cs (FluentValidation rules for command type, parameters, priority)
- [X] T051 [US1] Create MoveCommandParametersValidator in src/RobotApi/Validators/MoveCommandParametersValidator.cs (direction, distance 0-100, speed validation)
- [X] T052 [US1] Create RotateCommandParametersValidator in src/RobotApi/Validators/RotateCommandParametersValidator.cs (direction, degrees 0-360)
- [X] T053 [US1] Create SensorActivateCommandParametersValidator in src/RobotApi/Validators/SensorActivateCommandParametersValidator.cs (sensorName, enabled validation)
- [X] T054 [US1] Implement CommandStore in src/RobotApi/Data/CommandStore.cs (ConcurrentDictionary with CRUD operations, status filtering)
- [X] T055 [US1] Create ICommandService interface in src/RobotApi/Services/ICommandService.cs
- [X] T056 [US1] Implement CommandService in src/RobotApi/Services/CommandService.cs (SendCommandAsync, GetCommandAsync, ListCommandsAsync, CancelCommandAsync with business logic)
- [X] T057 [US1] Create background command executor in src/RobotApi/Services/CommandExecutorService.cs (IHostedService that simulates command execution with state transitions)
- [X] T058 [US1] Implement POST /v1/robots/{robotId}/commands endpoint in src/RobotApi/Endpoints/CommandEndpoints.cs (validate robot online, create command, return 202)
- [X] T059 [US1] Implement GET /v1/robots/{robotId}/commands/{commandId} endpoint in src/RobotApi/Endpoints/CommandEndpoints.cs (retrieve command with status)
- [X] T060 [US1] Implement GET /v1/robots/{robotId}/commands endpoint in src/RobotApi/Endpoints/CommandEndpoints.cs (list with status filter, limit parameter)
- [X] T061 [US1] Implement DELETE /v1/robots/{robotId}/commands/{commandId} endpoint in src/RobotApi/Endpoints/CommandEndpoints.cs (cancel command if Pending or Executing)
- [X] T062 [US1] Add command-specific logging in src/RobotApi/Services/CommandService.cs (command sent, status changes, errors)
- [X] T063 [US1] Add authorization checks in src/RobotApi/Endpoints/CommandEndpoints.cs (Operator role or higher for commands)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Read Real-time Telemetry (Priority: P2)

**Goal**: Enable operators to retrieve current telemetry data (position, battery, temperature, sensors) from robots

**Independent Test**: Send GET request to `/v1/robots/{robotId}/telemetry` for online robot, receive 200 OK with telemetry snapshot containing position, orientation, speed, batteryLevel, temperature, sensorReadings with dataFreshness < 5 seconds

### Tests for User Story 2

- [ ] T064 [P] [US2] Contract test for GET /robots/{robotId}/telemetry in tests/RobotApi.Tests.Contract/TelemetryContractTests.cs (OpenAPI schema validation)
- [ ] T065 [P] [US2] Contract test for telemetry data structure validation in tests/RobotApi.Tests.Contract/TelemetryContractTests.cs
- [ ] T066 [P] [US2] Integration test for retrieve current telemetry scenario in tests/RobotApi.Tests.Integration/TelemetryScenarioTests.cs
- [ ] T067 [P] [US2] Integration test for telemetry freshness validation in tests/RobotApi.Tests.Integration/TelemetryScenarioTests.cs
- [ ] T068 [P] [US2] Integration test for offline robot telemetry unavailable in tests/RobotApi.Tests.Integration/TelemetryScenarioTests.cs
- [ ] T069 [P] [US2] Unit test for TelemetryService.GetCurrentTelemetryAsync in tests/RobotApi.Tests.Unit/Services/TelemetryServiceTests.cs
- [ ] T070 [P] [US2] Unit test for telemetry data freshness calculation in tests/RobotApi.Tests.Unit/Models/TelemetryTests.cs

### Implementation for User Story 2

- [X] T071 [P] [US2] Create Telemetry model in src/RobotApi/Models/Telemetry.cs (Id, RobotId, Timestamp, Position object, Orientation object, Speed, BatteryLevel, Temperature, SensorReadings dictionary)
- [X] T072 [P] [US2] Create Position value object in src/RobotApi/Models/Position.cs (GPS and local coordinate support with Type property)
- [X] T073 [P] [US2] Create Orientation value object in src/RobotApi/Models/Orientation.cs (Pitch, Roll, Yaw)
- [X] T074 [P] [US2] Create TelemetryResponse DTO in src/RobotApi/Models/DTOs/TelemetryResponse.cs (map from Telemetry entity with computed DataFreshness)
- [X] T075 [US2] Implement TelemetryStore in src/RobotApi/Data/TelemetryStore.cs (ConcurrentDictionary with latest telemetry per robot and historical storage)
- [X] T076 [US2] Create ITelemetryService interface in src/RobotApi/Services/ITelemetryService.cs
- [X] T077 [US2] Implement TelemetryService in src/RobotApi/Services/TelemetryService.cs (GetCurrentTelemetryAsync with freshness logic)
- [X] T078 [US2] Create background telemetry generator in src/RobotApi/Services/TelemetryGeneratorService.cs (IHostedService that simulates telemetry updates every 2 seconds for online robots)
- [X] T079 [US2] Implement GET /v1/robots/{robotId}/telemetry endpoint in src/RobotApi/Endpoints/TelemetryEndpoints.cs (return current snapshot with freshness)
- [X] T080 [US2] Add telemetry-specific logging in src/RobotApi/Services/TelemetryService.cs (telemetry retrieved, stale data warnings)
- [X] T081 [US2] Add authorization checks in src/RobotApi/Endpoints/TelemetryEndpoints.cs (Viewer role or higher for telemetry read)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Query Historical Telemetry (Priority: P3)

**Goal**: Enable operators to query historical telemetry data with time range filtering, field selection, pagination, and CSV export

**Independent Test**: Send GET request to `/v1/robots/{robotId}/telemetry/history?startTime=2025-11-10T00:00:00Z&endTime=2025-11-10T23:59:59Z&page=1&pageSize=50`, receive 200 OK with paginated array of telemetry snapshots. Then GET `/v1/robots/{robotId}/telemetry/export?startTime=...&endTime=...&format=csv` and verify CSV download

### Tests for User Story 3

- [ ] T082 [P] [US3] Contract test for GET /robots/{robotId}/telemetry/history in tests/RobotApi.Tests.Contract/TelemetryContractTests.cs
- [ ] T083 [P] [US3] Contract test for GET /robots/{robotId}/telemetry/export in tests/RobotApi.Tests.Contract/TelemetryContractTests.cs
- [ ] T084 [P] [US3] Integration test for query historical telemetry with time range in tests/RobotApi.Tests.Integration/TelemetryHistoryScenarioTests.cs
- [ ] T085 [P] [US3] Integration test for pagination with page and pageSize in tests/RobotApi.Tests.Integration/TelemetryHistoryScenarioTests.cs
- [ ] T086 [P] [US3] Integration test for field selection with telemetryTypes filter in tests/RobotApi.Tests.Integration/TelemetryHistoryScenarioTests.cs
- [ ] T087 [P] [US3] Integration test for CSV export scenario in tests/RobotApi.Tests.Integration/TelemetryHistoryScenarioTests.cs
- [ ] T088 [P] [US3] Integration test for 90-day time range limit enforcement in tests/RobotApi.Tests.Integration/TelemetryHistoryScenarioTests.cs
- [ ] T089 [P] [US3] Unit test for TelemetryService.GetHistoricalTelemetryAsync in tests/RobotApi.Tests.Unit/Services/TelemetryServiceTests.cs
- [ ] T090 [P] [US3] Unit test for TelemetryExportService.ExportToCsvAsync in tests/RobotApi.Tests.Unit/Services/TelemetryExportServiceTests.cs

### Implementation for User Story 3

- [X] T091 [P] [US3] Create TelemetryHistoryRequest DTO in src/RobotApi/Models/DTOs/TelemetryHistoryRequest.cs (StartTime, EndTime, TelemetryTypes array, Page, PageSize)
- [X] T092 [P] [US3] Create TelemetryHistoryResponse DTO in src/RobotApi/Models/DTOs/TelemetryHistoryResponse.cs (Data array, TotalCount, Page, PageSize)
- [X] T093 [US3] Create TelemetryHistoryRequestValidator in src/RobotApi/Validators/TelemetryHistoryRequestValidator.cs (time range validation, 90-day limit, pagination rules)
- [X] T094 [US3] Extend TelemetryService with GetHistoricalTelemetryAsync in src/RobotApi/Services/TelemetryService.cs (time range filtering, pagination, field projection)
- [X] T095 [US3] Create ITelemetryExportService interface in src/RobotApi/Services/ITelemetryExportService.cs
- [X] T096 [US3] Implement TelemetryExportService in src/RobotApi/Services/TelemetryExportService.cs (ExportToCsvAsync method with CSV generation)
- [X] T097 [US3] Implement GET /v1/robots/{robotId}/telemetry/history endpoint in src/RobotApi/Endpoints/TelemetryEndpoints.cs (query parameters, return paginated response)
- [X] T098 [US3] Implement GET /v1/robots/{robotId}/telemetry/export endpoint in src/RobotApi/Endpoints/TelemetryEndpoints.cs (format parameter, return CSV file with content disposition header)
- [X] T099 [US3] Add historical query logging in src/RobotApi/Services/TelemetryService.cs (query executed, record count, time range)
- [X] T100 [US3] Add authorization checks in src/RobotApi/Endpoints/TelemetryEndpoints.cs (Viewer role or higher for historical data)

**Checkpoint**: All primary user stories (US1, US2, US3) should now be independently functional

---

## Phase 6: User Story 4 - Receive Event Notifications (Priority: P4)

**Goal**: Enable operators to subscribe to robot events (battery_low, command_completed, sensor_threshold, errors) via webhooks with configurable filters

**Independent Test**: POST subscription to `/v1/subscriptions` with webhook URL and event filters. Trigger event (e.g., send command that completes), verify webhook receives POST with event payload. GET `/v1/subscriptions/{id}` to verify subscription state. DELETE to unsubscribe.

### Tests for User Story 4

- [ ] T101 [P] [US4] Contract test for POST /subscriptions in tests/RobotApi.Tests.Contract/SubscriptionsContractTests.cs
- [ ] T102 [P] [US4] Contract test for GET /subscriptions in tests/RobotApi.Tests.Contract/SubscriptionsContractTests.cs
- [ ] T103 [P] [US4] Contract test for GET /subscriptions/{id} in tests/RobotApi.Tests.Contract/SubscriptionsContractTests.cs
- [ ] T104 [P] [US4] Contract test for PUT /subscriptions/{id} in tests/RobotApi.Tests.Contract/SubscriptionsContractTests.cs
- [ ] T105 [P] [US4] Contract test for DELETE /subscriptions/{id} in tests/RobotApi.Tests.Contract/SubscriptionsContractTests.cs
- [ ] T106 [P] [US4] Contract test for GET /events in tests/RobotApi.Tests.Contract/EventsContractTests.cs
- [ ] T107 [P] [US4] Integration test for create subscription scenario in tests/RobotApi.Tests.Integration/SubscriptionScenarioTests.cs
- [ ] T108 [P] [US4] Integration test for webhook delivery with event trigger in tests/RobotApi.Tests.Integration/WebhookScenarioTests.cs
- [ ] T109 [P] [US4] Integration test for webhook retry on failure in tests/RobotApi.Tests.Integration/WebhookScenarioTests.cs
- [ ] T110 [P] [US4] Integration test for event filtering by type and severity in tests/RobotApi.Tests.Integration/SubscriptionScenarioTests.cs
- [ ] T111 [P] [US4] Unit test for SubscriptionService.CreateSubscriptionAsync in tests/RobotApi.Tests.Unit/Services/SubscriptionServiceTests.cs
- [ ] T112 [P] [US4] Unit test for EventService.CreateEventAsync in tests/RobotApi.Tests.Unit/Services/EventServiceTests.cs
- [ ] T113 [P] [US4] Unit test for WebhookDeliveryService with retry logic in tests/RobotApi.Tests.Unit/Services/WebhookDeliveryServiceTests.cs

### Implementation for User Story 4

- [X] T114 [P] [US4] Create Event model in src/RobotApi/Models/Event.cs (Id, RobotId, EventType, Severity enum, Timestamp, Message, Data dictionary)
- [X] T115 [P] [US4] Create Subscription model in src/RobotApi/Models/Subscription.cs (Id, OperatorId, RobotId nullable, EventTypes array, WebhookUrl, IsActive, FilterCriteria, CreatedAt)
- [X] T116 [P] [US4] Create SubscriptionRequest DTO in src/RobotApi/Models/DTOs/SubscriptionRequest.cs (RobotId, EventTypes, WebhookUrl, FilterCriteria)
- [X] T117 [P] [US4] Create SubscriptionResponse DTO in src/RobotApi/Models/DTOs/SubscriptionResponse.cs
- [X] T118 [P] [US4] Create EventResponse DTO in src/RobotApi/Models/DTOs/EventResponse.cs
- [X] T119 [P] [US4] Create WebhookPayload DTO in src/RobotApi/Models/DTOs/WebhookPayload.cs (Event, SubscriptionId, DeliveredAt)
- [X] T120 [US4] Create SubscriptionRequestValidator in src/RobotApi/Validators/SubscriptionRequestValidator.cs (webhook URL must be HTTPS, event types valid)
- [X] T121 [US4] Implement EventStore in src/RobotApi/Data/EventStore.cs (ConcurrentBag with time-based filtering)
- [X] T122 [US4] Implement SubscriptionStore in src/RobotApi/Data/SubscriptionStore.cs (ConcurrentDictionary with active subscription queries)
- [X] T123 [US4] Create IEventService interface in src/RobotApi/Services/IEventService.cs
- [X] T124 [US4] Implement EventService in src/RobotApi/Services/EventService.cs (CreateEventAsync, ListEventsAsync, event generation from command/telemetry changes)
- [X] T125 [US4] Create ISubscriptionService interface in src/RobotApi/Services/ISubscriptionService.cs
- [X] T126 [US4] Implement SubscriptionService in src/RobotApi/Services/SubscriptionService.cs (CRUD operations, filter matching logic)
- [X] T127 [US4] Create IWebhookDeliveryService interface in src/RobotApi/Services/IWebhookDeliveryService.cs
- [X] T128 [US4] Implement WebhookDeliveryService in src/RobotApi/Services/WebhookDeliveryService.cs (HttpClient-based delivery with exponential backoff retry, max 3 attempts)
- [X] T129 [US4] Create background webhook processor in src/RobotApi/Services/WebhookProcessorService.cs (IHostedService that monitors EventStore and delivers to matching subscriptions)
- [X] T130 [US4] Integrate event generation in CommandService in src/RobotApi/Services/CommandService.cs (emit command_completed, command_failed events)
- [X] T131 [US4] Integrate event generation in TelemetryService in src/RobotApi/Services/TelemetryService.cs (emit battery_low, sensor_threshold events)
- [X] T132 [US4] Implement POST /v1/subscriptions endpoint in src/RobotApi/Endpoints/SubscriptionEndpoints.cs (create subscription)
- [X] T133 [US4] Implement GET /v1/subscriptions endpoint in src/RobotApi/Endpoints/SubscriptionEndpoints.cs (list operator's subscriptions)
- [X] T134 [US4] Implement GET /v1/subscriptions/{id} endpoint in src/RobotApi/Endpoints/SubscriptionEndpoints.cs (retrieve subscription details)
- [X] T135 [US4] Implement PUT /v1/subscriptions/{id} endpoint in src/RobotApi/Endpoints/SubscriptionEndpoints.cs (update subscription filters or webhook URL)
- [X] T136 [US4] Implement DELETE /v1/subscriptions/{id} endpoint in src/RobotApi/Endpoints/SubscriptionEndpoints.cs (deactivate subscription)
- [X] T137 [US4] Implement GET /v1/events endpoint in src/RobotApi/Endpoints/EventEndpoints.cs (list events with filtering by robot, type, severity, time range)
- [X] T138 [US4] Add webhook delivery logging in src/RobotApi/Services/WebhookDeliveryService.cs (delivery attempts, successes, failures)
- [X] T139 [US4] Add authorization checks in src/RobotApi/Endpoints/SubscriptionEndpoints.cs (Operator role or higher)

**Checkpoint**: All user stories (US1, US2, US3, US4) should now be independently functional

---

## Phase 7: Robot Management Endpoints (Foundational Extension)

**Goal**: Complete robot CRUD operations for administrator role

**Independent Test**: POST new robot to `/v1/robots`, verify 201 Created. GET `/v1/robots` to see in list. GET `/v1/robots/{id}` for details. DELETE `/v1/robots/{id}` and verify 204 No Content.

### Tests for Robot Management

- [ ] T140 [P] Contract test for GET /robots in tests/RobotApi.Tests.Contract/RobotsContractTests.cs
- [ ] T141 [P] Contract test for POST /robots in tests/RobotApi.Tests.Contract/RobotsContractTests.cs
- [ ] T142 [P] Contract test for GET /robots/{id} in tests/RobotApi.Tests.Contract/RobotsContractTests.cs
- [ ] T143 [P] Contract test for DELETE /robots/{id} in tests/RobotApi.Tests.Contract/RobotsContractTests.cs
- [ ] T144 [P] Integration test for register robot scenario in tests/RobotApi.Tests.Integration/RobotManagementScenarioTests.cs
- [ ] T145 [P] Integration test for list robots scenario in tests/RobotApi.Tests.Integration/RobotManagementScenarioTests.cs
- [ ] T146 [P] Integration test for deregister robot scenario in tests/RobotApi.Tests.Integration/RobotManagementScenarioTests.cs
- [ ] T147 [P] Unit test for RobotService CRUD operations in tests/RobotApi.Tests.Unit/Services/RobotServiceTests.cs

### Implementation for Robot Management

- [X] T148 [P] Create RobotRegistrationRequest DTO in src/RobotApi/Models/DTOs/RobotRegistrationRequest.cs (Name, ModelType, FirmwareVersion, Capabilities)
- [X] T149 [P] Create RobotResponse DTO in src/RobotApi/Models/DTOs/RobotResponse.cs (map from Robot entity)
- [X] T150 Create RobotRegistrationRequestValidator in src/RobotApi/Validators/RobotRegistrationRequestValidator.cs (name length, firmware semver, capabilities structure)
- [X] T151 Create IRobotService interface in src/RobotApi/Services/IRobotService.cs
- [X] T152 Implement RobotService in src/RobotApi/Services/RobotService.cs (RegisterRobotAsync, GetRobotAsync, ListRobotsAsync, DeregisterRobotAsync)
- [X] T153 Implement GET /v1/robots endpoint in src/RobotApi/Endpoints/RobotEndpoints.cs (list all robots)
- [X] T154 Implement POST /v1/robots endpoint in src/RobotApi/Endpoints/RobotEndpoints.cs (register robot, Administrator only)
- [X] T155 Implement GET /v1/robots/{robotId} endpoint in src/RobotApi/Endpoints/RobotEndpoints.cs (get robot details)
- [X] T156 Implement DELETE /v1/robots/{robotId} endpoint in src/RobotApi/Endpoints/RobotEndpoints.cs (deregister robot, Administrator only)
- [X] T157 Add robot management logging in src/RobotApi/Services/RobotService.cs (registered, deregistered)
- [X] T158 Add authorization checks in src/RobotApi/Endpoints/RobotEndpoints.cs (Administrator for POST/DELETE, Viewer for GET)

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T159 [P] Create API documentation endpoint GET /v1/openapi.json in src/RobotApi/Endpoints/DocumentationEndpoints.cs (serve OpenAPI spec)
- [ ] T160 [P] Create health check endpoint GET /v1/health in src/RobotApi/Endpoints/HealthEndpoints.cs
- [ ] T161 [P] Add rate limiting middleware in src/RobotApi/Middleware/RateLimitingMiddleware.cs (100 requests per minute per operator)
- [ ] T162 [P] Add request/response logging middleware in src/RobotApi/Middleware/RequestLoggingMiddleware.cs (structured logs with timing)
- [ ] T163 [P] Create Dockerfile for containerization at repository root
- [ ] T164 [P] Create docker-compose.yml for local development at repository root
- [ ] T165 [P] Add performance benchmarks in tests/RobotApi.Tests.Integration/PerformanceBenchmarkTests.cs (verify <500ms command latency, <300ms telemetry latency)
- [ ] T166 [P] Create deployment guide in docs/DEPLOYMENT.md
- [ ] T167 [P] Create API usage guide in docs/API_USAGE.md
- [ ] T168 [P] Update README.md with architecture diagram and quick start
- [ ] T169 [P] Add code coverage configuration to test projects (coverlet.collector)
- [ ] T170 Validate quickstart.md examples against running API (manual test of all curl commands)
- [ ] T171 Run constitution compliance check (code quality, TDD coverage, performance targets)
- [ ] T172 Create release notes in docs/RELEASE_NOTES.md for v1.0.0

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories and robot management
- **User Stories (Phases 3-6)**: All depend on Foundational phase completion
  - US1 (Send Robot Commands - P1): Can start after Foundational - No dependencies on other stories
  - US2 (Read Real-time Telemetry - P2): Can start after Foundational - Independent of US1
  - US3 (Query Historical Telemetry - P3): Depends on US2 (extends TelemetryService)
  - US4 (Receive Event Notifications - P4): Integrates with US1 and US2 (event generation) but can be built independently
- **Robot Management (Phase 7)**: Depends on Foundational - Can run in parallel with user stories
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational - No dependencies on other stories
- **US2 (P2)**: Can start after Foundational - Independent of US1
- **US3 (P3)**: Soft dependency on US2 (extends TelemetryService with historical query methods)
- **US4 (P4)**: Soft dependency on US1 and US2 (integrates event generation) but services are independently testable

### Within Each User Story

- Tests MUST be written and FAIL before implementation (per constitution Principle II: TDD)
- Contract tests before integration tests (API contract validation first)
- Models before services (data structures before business logic)
- Validators before endpoints (request validation before routing)
- Services before endpoints (business logic before HTTP layer)
- Core implementation before integration (story complete before cross-story dependencies)

### Parallel Opportunities

#### Setup Phase (Phase 1)
- T003, T004, T005, T006 (create all 4 projects in parallel)
- T008, T009, T010, T011 (add NuGet packages to projects in parallel)

#### Foundational Phase (Phase 2)
- T017, T018, T019 (create base models in parallel)
- T021, T023 (authentication and data store interfaces in parallel)
- T024, T025 (operator and robot stores in parallel)

#### User Story 1 (Phase 3)
- T034-T046 (all test files in parallel - 13 test tasks)
- T047, T048, T049 (Command model and DTOs in parallel)
- T050-T053 (all validators in parallel)

#### User Story 2 (Phase 4)
- T064-T070 (all test files in parallel - 7 test tasks)
- T071, T072, T073, T074 (Telemetry model, Position, Orientation, DTO in parallel)

#### User Story 3 (Phase 5)
- T082-T090 (all test files in parallel - 9 test tasks)
- T091, T092 (history request and response DTOs in parallel)

#### User Story 4 (Phase 6)
- T101-T113 (all test files in parallel - 13 test tasks)
- T114, T115 (Event and Subscription models in parallel)
- T116, T117, T118, T119 (all DTOs in parallel)
- T121, T122 (EventStore and SubscriptionStore in parallel)

#### Robot Management (Phase 7)
- T140-T147 (all test files in parallel - 8 test tasks)
- T148, T149 (registration request and response DTOs in parallel)

#### Polish (Phase 8)
- T159-T172 (most polish tasks can run in parallel, except T170-T172 which require completed API)

### Critical Path (Minimum for MVP - US1 Only)

1. Complete Phase 1: Setup (T001-T016) - 16 tasks
2. Complete Phase 2: Foundational (T017-T033) - 17 tasks
3. Complete Phase 3: US1 Tests (T034-T046) - 13 tasks
4. Complete Phase 3: US1 Implementation (T047-T063) - 17 tasks
5. Validate US1 independently - MVP READY

**Total MVP Tasks**: 63 tasks

**Full Feature Tasks**: 172 tasks

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (16 tasks)
2. Complete Phase 2: Foundational (17 tasks) - BLOCKS all stories
3. Complete Phase 3: User Story 1 (30 tasks)
4. **STOP and VALIDATE**: Test User Story 1 independently with quickstart.md examples
5. Deploy/demo command API with status tracking

### Incremental Delivery (Priority Order)

1. Complete Setup + Foundational → Foundation ready (33 tasks)
2. Add US1 (Send Robot Commands - P1) → Test independently → Deploy/Demo (63 total tasks) 🎯 MVP
3. Add US2 (Read Real-time Telemetry - P2) → Test independently → Deploy/Demo (81 total tasks)
4. Add US3 (Query Historical Telemetry - P3) → Test independently → Deploy/Demo (100 total tasks)
5. Add US4 (Receive Event Notifications - P4) → Test independently → Deploy/Demo (139 total tasks)
6. Add Robot Management (Phase 7) → Complete CRUD operations (158 total tasks)
7. Polish & Cross-cutting (Phase 8) → Production ready (172 total tasks) 🚀

### Parallel Team Strategy

With 3 developers after Foundational phase:

1. **Team completes Setup + Foundational together** (33 tasks)
2. **Once Foundational is done**:
   - **Developer A**: US1 (Send Robot Commands) - 30 tasks
   - **Developer B**: US2 (Read Real-time Telemetry) - 18 tasks
   - **Developer C**: Robot Management (Phase 7) - 19 tasks
3. **After first wave**:
   - **Developer A**: US3 (Query Historical Telemetry) - 19 tasks (extends US2)
   - **Developer B**: US4 (Receive Event Notifications) - 39 tasks
   - **Developer C**: Polish tasks - 13 tasks

---

## Test Coverage Requirements

Per constitution Principle II (TDD):

- **Target**: 80% code coverage minimum
- **Contract Tests**: Validate all OpenAPI endpoints against schema
- **Integration Tests**: Validate end-to-end user scenarios for each story
- **Unit Tests**: Validate business logic in services, validators, models

**Test Execution Order**:
1. Write contract tests for endpoints → Run → Should FAIL (endpoints don't exist yet)
2. Write integration tests for scenarios → Run → Should FAIL (no implementation)
3. Write unit tests for services/validators → Run → Should FAIL (no implementation)
4. Implement models, validators, services, endpoints
5. Re-run tests → Should PASS (green)
6. Refactor for quality → Tests remain GREEN

---

## Performance Validation Checkpoints

Per constitution Principle IV (Performance by Design):

- [ ] After US1 implementation: Verify command latency <500ms (T165)
- [ ] After US2 implementation: Verify telemetry retrieval latency <300ms (T165)
- [ ] After US3 implementation: Verify historical query handles 1000 records efficiently (T165)
- [ ] After US4 implementation: Verify webhook delivery with retry completes within 10 seconds (T165)
- [ ] Final validation: Verify system handles 100+ concurrent robot connections (T165)

---

## Completion Criteria

**Phase 1 Complete**: All projects created, dependencies installed, structure ready
**Phase 2 Complete**: Authentication, data stores, middleware, error handling operational
**US1 Complete**: Can send commands, track status, cancel commands - MVP READY
**US2 Complete**: Can retrieve current telemetry with freshness validation
**US3 Complete**: Can query historical telemetry with pagination and CSV export
**US4 Complete**: Can subscribe to events and receive webhook notifications
**Robot Management Complete**: Administrator can register/deregister robots
**Polish Complete**: Documentation, performance validation, containerization, release notes

**Final Validation**:
- [ ] All quickstart.md curl examples execute successfully
- [ ] All constitution principles satisfied (quality gates pass)
- [ ] All OpenAPI contract tests pass
- [ ] 80%+ code coverage achieved
- [ ] Performance benchmarks meet targets (<500ms commands, <300ms telemetry)
- [ ] Docker container runs successfully
- [ ] Release notes document all features

---

## Notes

- **[P] tasks** = different files, no dependencies - can execute in parallel
- **[Story] label** maps task to specific user story for traceability (US1, US2, US3, US4)
- Each user story should be independently completable and testable
- TDD workflow: Write tests FIRST (contract → integration → unit), watch them FAIL, then implement, watch them PASS
- Commit after each task or logical group of related tasks
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Mock data approach: ConcurrentDictionary for synchronous CRUD, ConcurrentBag for append-only events
- Background services: CommandExecutorService, TelemetryGeneratorService, WebhookProcessorService run as IHostedService instances
