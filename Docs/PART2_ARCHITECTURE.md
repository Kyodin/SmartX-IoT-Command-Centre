
# Smart-X Part 2 - System Architecture

## Introduction

For Part 2 of the Smart-X POE, I expanded the existing application by adding device management, command processing, alert handling, telemetry queues and predictive recommendations.

The system continues to use a React frontend and an ASP.NET Core Web API backend.

The purpose of the architecture is to keep the different responsibilities separated so that the application is easier to develop, test and maintain.

## Application Structure

The Smart-X application consists of the following main components:

- React frontend
- ASP.NET Core Web API
- Controllers
- Services
- Models
- In-memory data collections
- Automated xUnit tests

The frontend communicates with the backend through HTTP requests.

The controllers receive requests and use the relevant services to perform operations.

The services contain the main application logic and data structure implementations.

## Smart-X Pillars

### Pillar 1: Sensor Data Ingestion and Telemetry

This pillar handles sensor registration, telemetry generation, batch ingestion and historical telemetry.

Telemetry readings can be generated, transmitted to the backend and stored for later retrieval.

### Pillar 2: Real Time Command Stream and History

This pillar handles device commands, operational history and alert processing.

Users can switch devices on or off, restart them, disconnect or reconnect them, and undo recent commands.

The system also records command and alert activity.

### Pillar 3: Network Topology and Mesh Routing

This pillar represents the relationships and communication paths between IoT devices.

The current project includes sensor registration and device connectivity status, which provide a foundation for further topology and routing functionality.

A complete network graph and mesh-routing algorithm have not yet been verified as implemented.

## Data Structures

### Dictionary

Used for device lookup by sensor ID.

### Queue

Used for FIFO telemetry processing and recent search activity.

### PriorityQueue

Used to process urgent alerts before lower-priority alerts.

### Stack

Used to reverse recent device commands.

### SortedDictionary

Used to maintain operational events in chronological order.

### HashSet

Used to prevent duplicate active alerts and track disconnected devices.

## Telemetry Processing Flow

1. A sensor generates telemetry.
2. The frontend sends the reading to the backend.
3. The reading can be placed into the telemetry processing queue.
4. The oldest queued reading is processed first.
5. The processed reading is stored in the telemetry collection.
6. The stored telemetry can be retrieved and searched.

## Command Processing Flow

1. A device is selected.
2. A command is submitted.
3. The backend validates the device and command.
4. The previous device state is added to the undo stack.
5. The new device state is applied.
6. The sensor's connectivity status is updated.
7. The command is recorded in the operational history.

## Alert Processing Flow

1. An alert is submitted.
2. The HashSet checks whether the same alert is already active.
3. A unique alert is added to the priority queue.
4. The priority queue determines the processing order.
5. The alert is processed and removed from the active alert collection.
6. The event is recorded in the history.

## Predictive Recommendations

The recommendation engine analyses:

- Device connectivity
- Device power state
- Repeated commands
- Recurring alerts
- Temperature and humidity thresholds
- Repeated sensor searches

Recommendations are generated using rule-based conditions and recorded historical patterns.

The system provides suggestions and severity levels to help users identify possible problems.

## Testing

The backend contains automated xUnit tests for:

- Search intelligence
- Device operations
- Data structure behaviour
- FIFO telemetry processing

The latest verified test execution returned 28 passed, 0 failed and 0 skipped.

Tests can be run using:

    dotnet test Tests/SmartX.Tests.csproj

## Development Resources

The project was developed with reference to the lecturer-provided slides, class notes, practical exercises, assessment instructions and other module resources.

Additional technical documentation and AI tool attribution are included in the main README.md file.

## References

Microsoft Learn. C# Collections.
https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/collections

Microsoft Learn. ASP.NET Core Web API.
https://learn.microsoft.com/en-us/aspnet/core/web-api/

Microsoft Learn. Dependency Injection.
https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection

Microsoft Learn. PriorityQueue.
https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.priorityqueue-2

xUnit.net. Official Documentation.
https://xunit.net/

React Documentation.
https://react.dev/
