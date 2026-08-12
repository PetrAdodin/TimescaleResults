# TimescaleResults

ASP.NET Core Web API for uploading CSV measurement files, calculating statistics and storing results in PostgreSQL.

## Technologies

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL 16
- CsvHelper
- Swagger / OpenAPI
- xUnit
- Docker Compose

## Requirements

- .NET 10 SDK
- Docker Desktop

## Run locally

### 1. Start PostgreSQL

From the repository root:

```bash
docker compose up -d