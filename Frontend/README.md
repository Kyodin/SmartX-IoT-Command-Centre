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