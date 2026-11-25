# Smart-X IoT Monitoring System

## PROG7312 Part 1

Smart-X is an IoT monitoring application that I developed for my PROG7312 Part 1 project.

The purpose of the application is to simulate a system where different IoT sensors can be registered, generate telemetry data and send that data to a backend API. The system also stores the telemetry so that previous readings can be viewed and searched.

For this part of the project, I focused mainly on the first Smart-X pillar, which is Sensor Data Ingestion and Telemetry.

The application has a React frontend which acts as the main dashboard and an ASP.NET Core Web API which handles the backend functionality.


## Smart-X Architecture

The Smart-X system is divided into three main areas.

### Pillar 1: Sensor Data Ingestion and Telemetry

This is the main focus of Part 1.

It allows sensors to be registered and used to generate simulated telemetry readings. These readings can be generated individually or in batches and then sent to the ingestion API.

The telemetry is stored by the backend and can later be viewed or searched through the dashboard.


### Pillar 2: Real Time Command Stream and History

This area is intended for later development.

The purpose of this pillar is to allow commands to be sent to connected IoT devices and to keep a history of the commands that have been sent.


### Pillar 3: Network Topology and Mesh Routing

This area will also be developed further during the later stages of the POE.

It is intended to represent relationships between connected IoT devices and how information can move through the network.


## Technologies Used

The project was developed using the following technologies.

### Backend

ASP.NET Core Web API

C#

.NET 10

Swagger

### Frontend

React

JavaScript

Vite

CSS

### Development Tools

Visual Studio Code

Postman

Swagger UI

Git

GitHub


## Project Structure

The project is separated into a backend and frontend.

```text
SmartX-POE
│
├── Backend
│   ├── Controllers
│   ├── Models
│   ├── Services
│   ├── Properties
│   └── Program.cs
│
├── Frontend
│   ├── src
│   │   ├── App.jsx
│   │   └── App.css
│   ├── package.json
│   └── vite.config.js
│
└── README.md

The backend contains the API, models and services used by Smart-X.

The frontend contains the React dashboard used to interact with the system.


## Sensor Registration

Sensors can be registered directly from the Smart-X dashboard.

When registering a sensor, the user can enter information such as the sensor name, category, MAC address and deployment location.

Each registered sensor receives an ID which is then used when generating telemetry.

The dashboard also displays the registered devices so that their information can easily be viewed.


## Sensor Categories

Different sensor categories can be selected when registering a device.

Examples include temperature, humidity, environmental, power, consumption and actuator sensors.

This makes the system more flexible because it is not limited to only one type of IoT device.


## Telemetry Generation

Smart-X can simulate telemetry readings for registered sensors.

The generated readings contain information such as the sensor ID, temperature, humidity, battery level and the time the reading was recorded.

The system supports both individual telemetry generation and batch generation.


## Batch Telemetry

Batch processing was included because an IoT system may need to process a large number of readings instead of sending every reading separately.

The user selects a sensor and chooses the number of readings that should be generated.

The generator creates the batch and the frontend then sends the readings to the telemetry ingestion API.

The API receives the batch and stores each reading.

This allowed me to demonstrate the complete process from telemetry generation to transmission, ingestion and storage.


## Telemetry Ingestion API

The backend contains API endpoints that receive telemetry from the frontend.

The ingestion process follows this general flow.


Sensor
   ↓
Telemetry Generator
   ↓
Telemetry Batch
   ↓
ASP.NET Core API
   ↓
Telemetry Storage
   ↓
Dashboard and Historical Search

The dashboard displays counters for generated, transmitted and received telemetry so that the process can be seen while the application is running.


## Historical Telemetry

Telemetry that has been received by the API is stored so that it can be accessed again.

The dashboard contains a historical telemetry search where a sensor ID can be entered.

The system then searches the stored readings and returns telemetry belonging to that sensor.

This is useful for checking previous activity instead of only viewing the newest reading.


## Search Algorithm

A linear search approach is used when searching through the telemetry collection.

The collection is checked for readings that match the requested sensor ID.

The application also measures how long the search takes.

The search time is displayed on the dashboard to give some evidence of how the data structure performs.


## Data Structures

Data structures are important in this project because telemetry needs to be generated, transmitted, stored and searched.

### List Collection

A `List<T>` is used to store collections such as telemetry readings and registered sensors.

I used a list because the amount of telemetry changes while the application is running. New readings can therefore be added without needing to know the final size beforehand.

This is particularly useful for batch telemetry where the number of stored readings can grow quickly.


### Arrays

Arrays are also used as part of the data structure implementation.

They are useful when working with a known or fixed group of values and help demonstrate the difference between fixed size structures and dynamic collections such as `List<T>`.


### Jagged Arrays

Jagged arrays are included to demonstrate a structure where each row can contain a different number of values.

This is useful for understanding how telemetry or sensor information can be grouped when each group does not necessarily contain the same amount of data.


## Generic Telemetry Packet

The project contains a generic class called `TelemetryPacket<T>`.

Using a generic type means that the packet is not permanently tied to only one specific type of data.

For the current Smart-X implementation it can be used with sensor readings, while still keeping the class reusable for other data types if the application is expanded later.


## Operator Overloading

Operator overloading is demonstrated in the `SensorReading` model.

This allows sensor reading objects to work with an overloaded operator rather than only using standard methods.

I included this to demonstrate how operators can be given behaviour for custom C# objects.


## Recursion

Recursion is implemented in the telemetry analysis functionality.

A recursive method is used to process readings by calling itself while moving through the collection.

The method continues until it reaches its stopping condition.

This demonstrates recursion using the telemetry data from the Smart-X project rather than using an unrelated example.


## Performance Testing

The dashboard includes a performance test that can be run using different dataset sizes.

The available test sizes include smaller collections as well as larger collections containing thousands of records.

The test measures values such as insertion time and search time.

This makes it possible to see how the selected data structure behaves as the amount of telemetry increases.

The performance information is displayed directly on the dashboard so that it can be demonstrated during the project presentation.


## Dashboard

The frontend was designed as a monitoring dashboard so that the main Smart-X functionality can be demonstrated from one place.

The dashboard displays information such as:

API status

Total registered sensors

Online sensors

Telemetry generated

Telemetry transmitted

Telemetry received

Total telemetry records stored

Latest telemetry readings

Last telemetry reading

Search algorithm

Last search

Search time

Insertion time

Performance test results

Registered sensor information


## File Upload

The application also includes a file upload section.

Files such as logs, CSV files and image evidence can be selected and uploaded.

An image preview is displayed when an image is selected.

This gives the system a way of attaching additional information that may be related to a sensor or IoT environment.


## API Status

The dashboard checks whether the backend API can be reached.

When the API is available, the interface displays that the system is online.

This is useful when demonstrating the project because it gives a quick indication that communication between the React frontend and ASP.NET Core backend is working.


## API Endpoints

The backend contains controllers for the different parts of the application.

These include endpoints for sensor management, telemetry generation, telemetry ingestion, telemetry retrieval, historical search, analysis, performance testing and other supporting functionality.

Swagger can also be used to view and test the available endpoints directly.


## Running the Project

The backend and frontend need to be running at the same time.


### Starting the Backend

Open a terminal in the project folder and run:

```powershell
cd Backend
dotnet run
```

The backend should start on:

```text
http://localhost:5090
```

Swagger can then be opened using:

```text
http://localhost:5090/swagger
```


### Starting the Frontend

Open another terminal and run:

```powershell
cd Frontend
npm install
npm run dev
```

The frontend should start on:

```text
http://localhost:5173
```

The Smart-X dashboard can then be opened in the browser.


## Testing the Application

A basic test of the system can be done by following these steps.

1. Start the ASP.NET Core backend.

2. Start the React frontend.

3. Check that the dashboard shows the API as online.

4. Register a sensor.

5. Select the registered sensor.

6. Generate a telemetry batch.

7. Transmit the batch to the API.

8. Check that the telemetry counters increase.

9. View the latest telemetry readings.

10. Search the historical telemetry using a sensor ID.

11. Check the search time displayed by the system.

12. Run the performance test using different dataset sizes.

13. Test the file upload functionality.

## Code Attribution

1. Microsoft Learn. ASP.NET Core Web API documentation.  
   Used to understand controller based APIs, routing, HTTP methods and the structure of the backend.  
   https://learn.microsoft.com/en-us/aspnet/core/web-api/

2. Microsoft Learn. Create a controller based Web API with ASP.NET Core.  
   Used as a reference when building and testing the Smart-X API controllers.  
   https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api

3. Microsoft Learn. Dependency injection in ASP.NET Core.  
   Used to understand how services such as the telemetry generator and analysis services are registered in Program.cs and injected into controllers.  
   https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection

4. Microsoft Learn. C# generic types and methods.  
   Used to understand generics and the implementation of the TelemetryPacket<T> class.  
   https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics

5. Microsoft Learn. C# collections.  
   Used to understand List<T> and why a dynamic collection is suitable for telemetry readings and sensor records.  
   https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/collections

6. Microsoft Learn. C# arrays.  
   Used to understand standard arrays, multidimensional arrays and jagged arrays used in the telemetry analysis functionality.  
   https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/arrays

7. Microsoft Learn. C# operator overloading.  
   Used to understand how the + operator can be overloaded for the SensorReading model.  
   https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading

8. Microsoft Learn. C# methods.  
   Used to support the understanding of methods and recursive method calls used in CountReadingsRecursively.  
   https://learn.microsoft.com/en-us/dotnet/csharp/methods

9. Microsoft Learn. System.Diagnostics.Stopwatch.  
   Used to understand how to measure search and insertion performance in the telemetry analysis and search functionality.  
   https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch

10. React Documentation. useState.  
    Used to understand how frontend state is managed for sensors, telemetry, registration forms, search results and dashboard statistics.  
    https://react.dev/reference/react/useState

11. React Documentation. useEffect.  
    Used to understand how dashboard data can be loaded when the React component starts.  
    https://react.dev/reference/react/useEffect

12. MDN Web Docs. Fetch API.  
    Used to understand frontend communication with the ASP.NET Core API using HTTP requests.  
    https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API

13. Vite Documentation. Getting Started.  
    Used to understand the React development environment, project setup and the npm run dev development server.  
    https://vite.dev/guide/

14. GitHub Docs. Using Git and GitHub.  
    Used to understand repository management, commits, pushing changes and maintaining the project version history.  
    https://docs.github.com/en/get-started/using-git/about-git


---

# PROG7312 Part 2

## Part 2 Overview

For Part 2 of the Smart-X POE, I continued developing the application by adding more functionality to the existing React frontend and ASP.NET Core Web API.

The main focus of this part was to introduce additional data structures and algorithms that could be used to manage IoT devices, process telemetry, handle commands and alerts, and analyse historical information.

I also improved the dashboard so that the different parts of the system could be accessed and demonstrated more easily.

The Part 1 functionality was kept as part of the application and expanded during Part 2.

## Smart-X Command Centre

The frontend was updated with a Command Centre design.

The purpose of this was to make the application feel more like a central monitoring system where the user can manage sensors, view telemetry, control devices and monitor alerts.

The dashboard uses a dark colour scheme with blue and cyan elements to help separate the different sections.

The Command Centre brings together the existing telemetry functionality and the additional Part 2 features.

## Real Time Command Processing

The application allows commands to be sent to registered devices.

The available commands include:

- On
- Off
- Restart
- Disconnect
- Connect

When a command is sent, the backend checks the device and updates its current state.

The command is also recorded so that previous actions can be viewed.

I included an undo feature which allows the most recent command to be reversed.

This is useful when a command was selected incorrectly or when a previous device state needs to be restored.

## Part 2 Data Structures

Several data structures were implemented to support the new functionality.

### Dictionary

A `Dictionary<int, SmartXDevice>` is used to store device information using the sensor ID as the key.

I used a dictionary because it allows a device to be found directly using its ID instead of searching through every device.

This is useful when commands need to be sent to a particular sensor.

### Queue

A `Queue<TelemetryReading>` is used for telemetry processing.

New readings are added to the end of the queue and the oldest reading is processed first.

This follows the First-In, First-Out (FIFO) principle.

I also included batch queueing and the ability to view pending readings without removing them.

### PriorityQueue

A `PriorityQueue` is used to manage device alerts.

Alerts are assigned a priority value, with lower numbers representing more urgent alerts.

The system processes the most urgent alert first.

A sequence number is also used to preserve the order of alerts that have the same priority.

### Stack

A `Stack<SmartXCommand>` is used to support the undo functionality.

Each command stores the previous device state before the new command is applied.

When undo is selected, the most recent command is removed from the stack and the previous device state is restored.

This follows the Last-In, First-Out (LIFO) principle.

### SortedDictionary

A `SortedDictionary<DateTime, SmartXEvent>` is used to maintain the operational event history.

The history records commands, undo actions and alert activity.

The events can be retrieved in chronological order, with the newest events displayed first.

This also allows the recommendation engine to analyse previous activity.

### HashSet

HashSets are used to track unique active alerts and disconnected devices.

The active alerts HashSet helps prevent duplicate alerts of the same type from being added repeatedly for the same sensor.

The disconnected devices HashSet keeps track of devices that have been disconnected.

## Predictive Recommendations

A recommendation feature was added to Smart-X to analyse device activity and identify possible problems.

The system uses existing device information, recorded commands, alert history and telemetry readings to generate recommendations.

Examples include:

- Devices that are disconnected
- Devices that are switched off
- Repeated restart commands
- Repeated disconnect commands
- Recurring alert patterns
- High temperature readings
- High humidity readings

The recommendations contain information about the issue and suggest an action that could be taken.

For historical patterns, the system can also provide the reason for the recommendation and the number of recorded events that support it.

This makes the recommendations easier to understand rather than only displaying a warning.

## Search Intelligence

I also added a search intelligence service.

This records sensor searches performed through the dashboard and keeps track of how often particular sensors are searched.

A queue is used to maintain recent search activity, while a dictionary is used to count searches for each sensor.

When a sensor has been searched repeatedly, the system can generate a recommendation.

If that sensor is also offline, the recommendation can be escalated to a critical severity.

This allows Smart-X to use historical user activity as another source of information.

## Automated Testing

Automated testing was added using xUnit.

The tests were separated into different files so that the functionality could be checked independently.

### Search Intelligence Tests

Eight automated tests were created to check search recording, search counts, history limits, recommendation thresholds, offline escalation and duplicate prevention.

### Operations Tests

Ten automated tests were created to check device lookup, command validation, connectivity, undo functionality, alert handling, priority processing, event history and recommendation duplication.

### Telemetry Queue Tests

Ten automated tests were created to check FIFO processing, queue counts, batch ordering, empty queue handling and viewing pending readings.

### Test Results

The automated tests were run using:

```powershell
dotnet test Tests/SmartX.Tests.csproj
```

The most recent test execution returned:

- Total tests: 28
- Passed: 28
- Failed: 0
- Skipped: 0

Both the backend and test project compiled successfully.

These tests provide evidence that the tested services and data structures are behaving as expected.

## Part 2 API Functionality

The backend was extended with additional endpoints for device operations, alert processing, queue management and search intelligence.

The telemetry queue endpoints include:

- `POST /api/Telemetry/queue`
- `GET /api/Telemetry/queue`
- `POST /api/Telemetry/queue/process-next`

The search intelligence endpoints include:

- `POST /api/search-intelligence/record`
- `GET /api/search-intelligence/history`
- `GET /api/search-intelligence/recommendations`

The additional controllers allow the React dashboard to communicate with the new backend services.

Swagger can also be used to test the available API functionality.

## Running the Part 2 Application

The backend and frontend are started in separate terminals, as explained in the Part 1 setup instructions.

Backend:

```powershell
cd Backend
dotnet run
```

Frontend:

```powershell
cd Frontend
npm install
npm run dev
```

The backend runs at:

`http://localhost:5090`

The frontend runs at:

`http://localhost:5173`

The application can then be tested through the Smart-X dashboard.

## Lecturer Resources and Learning Materials

The development of this project was supported by the learning materials and resources provided by the lecturer throughout the PROG7312 module.

These included the lecture slides, class notes, practical examples, assessment instructions, prescribed learning material and additional resources made available during the module.

I used these resources to understand the data structures and algorithms covered in class and how they could be applied to the Smart-X application.

The lecturer-provided material was used alongside the official technical documentation and other references listed in this README.

## Additional Part 2 References

The references below was used

15. Microsoft Learn. Queue Class.
    Used to understand FIFO queue processing for telemetry readings.
    https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1

16. Microsoft Learn. Stack Class.
    Used to understand LIFO processing and the implementation of the undo feature.
    https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.stack-1

17. Microsoft Learn. Dictionary Class.
    Used to understand key-based storage and device lookup.
    https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2

18. Microsoft Learn. SortedDictionary Class.
    Used to understand how events can be stored using sorted keys.
    https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.sorteddictionary-2

19. Microsoft Learn. HashSe Class.
    Used to understand unique collections and duplicate prevention.
    https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1

20. Microsoft Learn. PriorityQueue Class.
    Used to understand priority-based alert processing.
    https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.priorityqueue-2

21. Microsoft Learn. Unit testing C# with xUnit.
    Used as a reference for organising and running automated tests.
    https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit

22. xUnit.net. Official Documentation.
    Used as a reference for writing and executing xUnit tests.
    https://xunit.net/



23. The lecturer-provided lecture slides, class notes, practical examples, assessment instructions and additional learning resources were used throughout the development and testing of the project.

The technical references and AI tool attribution are included in the earlier sections of this README.




## Part 2 Data Structure Testing Instructions

For Part 2, I tested the new data structures through the Smart-X dashboard, backend API and automated xUnit tests.

### Testing the Telemetry Queue

The telemetry queue follows the First-In, First-Out (FIFO) principle.

To test the queue:

1. Start the backend and frontend.
2. Open Swagger at http://localhost:5090/swagger.
3. Open the Telemetry endpoints.
4. Use `GET /api/Telemetry/queue` to view pending readings.
5. Add telemetry readings through the queue endpoint using the request format shown in Swagger.
6. Use `GET /api/Telemetry/queue` again to confirm that the readings were added.
7. Use `POST /api/Telemetry/queue/process-next` to process the oldest reading.
8. Check the queue again to confirm that the oldest reading was removed first.

This demonstrates FIFO processing.

### Testing the Device Dictionary

The dictionary stores devices using their sensor IDs as keys.

To test the dictionary:

1. Start the Smart-X application.
2. Open the Sensor Data & Telemetry page.
3. Register a sensor or use an existing sensor.
4. Open the Real-Time Command Stream page.
5. Select the registered device.
6. Send an ON, OFF or RESTART command.
7. Confirm that the correct device state changes.
8. Return to the telemetry dashboard and confirm that the registered sensor is still available.

This demonstrates how the system identifies and updates devices using their IDs.

### Testing the Priority Queue

1. Open the Real-Time Command Stream page.
2. Create alerts with different priority levels.
3. Submit the alerts.
4. Process the next alert.
5. Confirm that the highest-priority alert is processed first.

### Testing the Undo Stack

1. Select a registered device.
2. Send a command that changes its state.
3. Send another command.
4. Select Undo.
5. Confirm that the most recent command is reversed first.

### Testing Duplicate Prevention

1. Create an alert for a particular sensor.
2. Submit the same active alert again.
3. Confirm that the duplicate alert is not added to the active alert queue.

### Running the Automated Tests

Open a terminal in the main SmartX-POE directory and run:

```powershell
dotnet test Tests/SmartX.Tests.csproj
```

The latest verified execution returned:

- 28 tests passed
- 0 tests failed
- 0 tests skipped

The automated tests cover search intelligence, operations and telemetry queue functionality.


## Part 3: Network Topology and Historical Telemetry

Part 3 extends the Smart-X IoT Monitoring System by integrating advanced tree and graph data structures into the existing application.

The final system brings together telemetry ingestion, real-time device commands, historical queries and simulated mesh network routing within one React dashboard.

### AVL Tree Historical Telemetry

A self-balancing AVL tree was implemented to index telemetry readings using their timestamps.

The implementation includes:

- Automatic balancing through AVL rotations
- Support for multiple readings with identical timestamps
- Historical searches between selected dates and times
- Optional sensor ID filtering
- Tree statistics, including height and node count
- Integration with telemetry ingestion and FIFO queue processing

The AVL tree enables efficient timestamp-based searches without scanning the entire telemetry list.

File: Backend/DataStructures/TelemetryAvlTree.cs

API endpoints:
- GET /api/History/range
- GET /api/History/statistics

### Mesh Network Graph

The simulated Smart-X network uses a weighted, undirected graph represented by adjacency lists.

Each registered sensor represents a graph vertex, while wireless connections are represented by weighted edges.

The following algorithms were implemented:

**Breadth-First Search (BFS)**

Uses a queue to traverse connected devices level by level.

**Depth-First Search (DFS)**

Uses a stack to explore connected devices through successive branches.

**Prim's Minimum Spanning Tree**

Uses a priority queue to select connections that minimize the total link cost while connecting reachable online sensors.

When the network is disconnected, the implementation returns a minimum spanning forest.

Offline sensors are excluded from active network traversal and spanning-tree calculations.

Files:
- Backend/Services/MeshNetworkService.cs
- Backend/Controllers/NetworkController.cs

### Interactive Network Dashboard

The React dashboard provides a visual representation of registered sensors and their connections.

Users can create and remove weighted connections, execute BFS and DFS, calculate the minimum spanning tree, and view changes in connectivity when sensors go offline.

Files:
- Frontend/src/MeshNetworkPanel.jsx
- Frontend/src/MeshNetworkPanel.css

### Testing

Automated testing uses xUnit.

At the latest verified backend test run:
- Total automated tests: 49
- Passed: 49
- Failed: 0

Manual testing also confirmed that all nine Network Topology dashboard checks passed.

The manual checks covered graph rendering, connection management, BFS, DFS, minimum spanning tree highlighting, sensor connectivity and integration with earlier Smart-X features.

### Current Limitations

Telemetry and graph connections are stored in memory, so they do not persist after a backend restart.

The network represents simulated IoT connectivity. It does not establish physical ESP32 wireless connections.

Link weights represent user-defined simulated connection costs, not measured wireless signal strength.

### Final System Integration

The Smart-X dashboard integrates all three POE development stages:

Part 1: Sensor registration, telemetry generation, ingestion and analysis.

Part 2: Device commands, queue management, alerts and operational history.

Part 3: AVL tree historical queries, graph traversal and minimum spanning tree routing.

The completed system demonstrates the practical application of data structures and algorithms within a connected IoT monitoring environment.

