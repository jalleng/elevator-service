# Elevator Service API

A minimal testing API for elevator floor request management. This service provides endpoints for queuing and managing elevator floor requests, designed to help frontend developers test their elevator control applications locally.

## Overview

The Elevator Service API simulates an elevator's floor request system with the following features:

- **Floor Request Management**: Add, remove, and clear floor requests
- **Smart Queue Processing**: Handles internal (inside elevator) and external (hall call) requests
- **Direction-Aware**: Processes requests based on current direction and optimizes travel
- **Real-time Status**: Get the next floor to service and current request queue
- **Interactive Documentation**: Swagger UI for easy testing and exploration

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A code editor (e.g., [VS Code](https://code.visualstudio.com/))
- A web browser

### Installing .NET

**macOS:**

```bash
brew install dotnet
```

**Windows:**
Download and install from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

**Linux:**
Follow instructions at [https://learn.microsoft.com/en-us/dotnet/core/install/linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux)

Verify installation:

```bash
dotnet --version
```

## Getting Started

### Clone the Repository

```bash
git clone <repository-url>
cd elevator-service
```

### Run the API

```bash
cd ElevatorAPI
dotnet run
```

The API will start on `http://localhost:8080`

### Access Swagger UI

Open your browser and navigate to:

```
http://localhost:8080
```

You'll see the interactive Swagger documentation where you can test all endpoints.

## Configuration

### Floor Range

Configure the minimum and maximum floor numbers in `appsettings.json`:

```json
{
  "Elevator": {
    "MinFloor": 1,
    "MaxFloor": 100
  }
}
```

### Port Configuration

To change the port, edit `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:8080"
    }
  }
}
```

## API Endpoints

### GET /floorrequests

Get all floor requests in the queue.

### GET /floorrequests/internal

Get only internal (inside elevator) floor requests.

### GET /floorrequests/next

Get the next floor to service based on current direction.

### POST /floorrequests

Add a new floor request.

**Request Body:**

```json
{
  "floor": 5,
  "direction": "Up",
  "origin": "External"
}
```

**Enum Values:**

- `direction`: `"Up"`, `"Down"`, `"Both"`
- `origin`: `"Internal"`, `"External"`

### DELETE /floorrequests/{floor}

Remove a floor request by floor number.

### POST /floorrequests/clear

Clear all floor requests.

### POST /floorrequests/servicecurrentfloor

Mark the current floor as serviced and move to the next request.

## Running Tests

```bash
cd ElevatorAPI.Tests
dotnet test
```

Or run from the solution root:

```bash
dotnet test
```

## Project Structure

```
elevator-service/
├── ElevatorAPI/              # Main API project
│   ├── Filters/              # Swagger filters
│   ├── Models/               # Data models
│   ├── Routes/               # API endpoints
│   └── Services/             # Business logic
└── ElevatorAPI.Tests/        # Test project
    ├── Filters/              # Filter tests
    ├── Routes/               # Integration tests
    └── Services/             # Unit tests
```

## Development Features

- **CORS Enabled**: All origins allowed for local development
- **HTTP Logging**: Request/response logging in console
- **Developer Exception Page**: Detailed error information
- **String Enums**: Enums serialized as readable strings in JSON

## License

See [LICENSE](LICENSE) file for details.
