# TicTacToe API

[English Version](README_EN.md) | [Russian Version](README_RU.md)

## Table of Contents
- [Architectural Decisions](#architectural-decisions)
  - [Clean Architecture](#clean-architecture)
  - [CQRS](#cqrs-command-query-responsibility-segregation)
  - [Optimistic Concurrency](#optimistic-concurrency)
- [API Endpoints](#api-endpoints)
  - [GET /api/games](#get-apigames)
  - [POST /api/games](#post-apigames)
  - [GET /api/games/{id}](#get-apigamesid)
  - [POST /api/games/{id}/moves](#post-apigamesidmoves)
- [Data Model](#data-model)
  - [GameDto](#gamedto)
  - [Game Rules](#game-rules)
- [Game Settings](#game-settings)
- [Application Configuration](#application-configuration)
  - [appsettings.json](#appsettingsjson)
  - [Environment Variables](#environment-variables)
  - [Docker Configuration](#docker-configuration)
- [Running the Project](#running-the-project)
  - [Requirements](#requirements)
  - [Local Run](#local-run)
  - [Environment Profile](#environment-profile)
  - [Docker Run](#docker-run)
- [API Testing](#api-testing)
- [Running Tests](#running-tests)
- [Test Coverage Report](#test-coverage-report)
- [Troubleshooting](#troubleshooting)

## Architectural Decisions
This REST API application for playing Tic-Tac-Toe was developed using .NET 9.0 and ASP.NET Core with the following architectural approaches:

### Clean Architecture
The project is divided into several layers:
- **Domain**: Contains core business logic and domain models
- **Application**: Contains use cases and application logic
- **Infrastructure**: Contains implementations for external system interactions
- **API**: Provides HTTP API for application interaction

### CQRS (Command Query Responsibility Segregation)
The project uses CQRS pattern with MediatR library:
- **Commands**: For state changes (CreateGame, MakeMove)
- **Queries**: For data retrieval (GetGame, GetAllGames)

### Optimistic Concurrency
For data consistency during parallel access, optimistic concurrency with versioning (ETag) is used:
- Each game has its version
- When making a move, client must provide current game version
- If versions don't match, conflict error (409 Conflict) is returned

## API Endpoints
### GET /api/games
Gets list of all games.

**Response:**
- `200 OK`: Array of game objects

### POST /api/games
Creates a new game.

**Response:**
- `201 Created`: Created game object

### GET /api/games/{id}
Gets information about specific game.

**Parameters:**
- `id`: Game GUID identifier

**Response:**
- `200 OK`: Game object
- `404 Not Found`: Game not found

### POST /api/games/{id}/moves
Makes a move in the game.

**Parameters:**
- `id`: Game GUID identifier
- Header `If-Match`: Game version in `W/"version"` format

**Request Body:**
```json
{
  "player": "string",
  "row": 0,
  "col": 0
}
```

**Response:**
- `200 OK`: Updated game object
- `400 Bad Request`: Invalid move or data format
- `404 Not Found`: Game not found
- `409 Conflict`: Version conflict (game was modified since last request)

## Data Model
**GameDto**
```json
{
"id": "guid",
"board": [["", "", ""], ["", "", ""], ["", "", ""]],
"currentPlayer": "X|O",
"status": "InProgress|Draw|XPlayerWon|OPlayerWon",
"version": 0
}
```
## Game Rules
- Standard Tic-Tac-Toe rules
- Configurable board size and winning condition
- Minimum board size: 3x3
- Winning condition: 3 to board size symbols in a row
- 10% chance every 3 moves for a "random move" (player symbol change)

## Game Settings
Game settings are configured via ``GameSettings`` options:
- ``BoardSize``: Game board size
- ``WinCondition``: Number of symbols in row to win

## Application Configuration
**appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "GameSettings": {
    "BoardSize": 3,
    "WinCondition": 3
  }
}
```
**Environment Variables**

| Variable |     Description     |  Example   |
|---|:-------------------:|:----------:|
| ASPNETCORE_ENVIRONMENT | Runtime environment | Production |
| GameSettings__BoardSize |     	Board size     |     3      |
| GameSettings__WinCondition |  Winning condition  |     	3     |

**Docker Configuration**
```sh
docker run -p 8080:80 \
-e GameSettings__BoardSize=5 \
-e GameSettings__WinCondition=4 \
tictactoe-api
```

## Running the Project
**Requirements**

- .NET 9.0 SDK
- Docker (optionally)

**Local Rub**
```shell
git clone https://github.com/dmuka/TicTacToe.git
cd TicTacToe
dotnet restore
dotnet build
cd src/API
dotnet run
```

**Docker**
```shell
docker-compose up -d
```

## API Testing
You can use any http client or file ``API.http`` in the solution root.

```shell
# Create game
curl -X POST http://localhost:8080/api/games -H "Content-Type: application/json"

# Move
curl -X POST http://localhost:8080/api/games/{id}/moves \
  -H "Content-Type: application/json" \
  -H "If-Match: W/\"1\"" \
  -d '{"player": "X", "row": 0, "col": 0}'
```

## Testing
```shell
dotnet test
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Troubleshooting
- Check the ports and firewall settings
- Ensure that ``BoardSize`` is ≥ 3
- For debugging, set ``LogLevel: Debug`` in ``appsettings.Development.json``