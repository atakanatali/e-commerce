# Commercify Microservices Solution

<p align="center">
  <img width="456" height="456" alt="Cachify Logo" src="https://github.com/user-attachments/assets/d330816b-851b-4e83-a1d9-4e4bd2bee6ed" />
</p>

This repository contains a .NET 9 microservices sample that models a simple e-commerce flow (order creation → stock reservation → notification). Services communicate asynchronously via RabbitMQ and each service has its own PostgreSQL database.

## Table of Contents
- [Architecture Overview](#architecture-overview)
- [Projects](#projects)
- [Prerequisites](#prerequisites)
- [Quick Start (Docker)](#quick-start-docker)
- [Run Locally](#run-locally)
- [Solution and Package Management](#solution-and-package-management)
- [Directory Layout](#directory-layout)

## Architecture Overview
The system is composed of three HTTP API services. They exchange events over RabbitMQ and keep separate PostgreSQL data stores.

Flow summary:
1. **Orchestrator.Api** creates orders and publishes stock reservation requests.
2. **Stock.Worker** processes reservations and publishes the outcome.
3. **Notification.Worker** records notifications when orders are confirmed.

## Projects
- **Orchestrator.Api**: Order creation and reservation result handling. Port: `5001` (docker).
- **Stock.Worker**: Stock reservation processing.
- **Notification.Worker**: Order confirmation notifications.
- **ECommerce.Shared**: Shared models/contracts and helpers.

## Prerequisites
- .NET SDK 9.0
- Docker & Docker Compose (optional, recommended)
- PostgreSQL and RabbitMQ (automatically provided via Docker)

## Quick Start (Docker)
Run all services and dependencies:

```bash
docker compose up --build
```

Default endpoints:
- RabbitMQ management UI: `http://localhost:15672` (user: `guest`, password: `guest`)
- Orchestrator.Api: `http://localhost:5001`

> The other services do not expose host ports; they communicate over the Docker network.

## Run Locally
You can run each service in a separate terminal:

```bash
dotnet restore
```

```bash
dotnet run --project src/Orchestrator.Api/Orchestrator.Api.csproj
```

```bash
dotnet run --project src/Stock.Worker/Stock.Worker.csproj
```

```bash
dotnet run --project src/Notification.Worker/Notification.Worker.csproj
```

When running locally, use the following host values for infrastructure:

- RabbitMQ host: `localhost`
- PostgreSQL host: `localhost`

## Database Migrations (Per Service)
Each service owns its own EF Core migrations and history table. On startup, each service applies
its pending migrations automatically with retry logic, so the database schema stays in sync.

History tables:
- Orchestrator.Api: `__EFMigrationsHistory_Orchestrator`
- Stock.Worker: `__EFMigrationsHistory_Stock`
- Notification.Worker: `__EFMigrationsHistory_Notification`

### Add a migration
Run migrations from the repository root:

```bash
dotnet ef migrations add <Name> \
  --project src/Orchestrator.Api \
  --startup-project src/Orchestrator.Api \
  --context OrderDbContext \
  --output-dir Migrations
```

```bash
dotnet ef migrations add <Name> \
  --project src/Stock.Worker \
  --startup-project src/Stock.Worker \
  --context StockDbContext \
  --output-dir Migrations
```

```bash
dotnet ef migrations add <Name> \
  --project src/Notification.Worker \
  --startup-project src/Notification.Worker \
  --context NotificationDbContext \
  --output-dir Migrations
```

### Auto-apply on startup
Each service calls `Database.Migrate()` with retries on startup. This is idempotent and uses
service-specific migration history tables to prevent cross-service collisions.

## Solution and Package Management
The root `Directory.Build.props` enables **restore on build** so Rider's **Rebuild Solution** downloads NuGet packages automatically via `RestoreOnBuild`.

## Directory Layout
```
.
├── Directory.Build.props
├── ECommerce.sln
├── docker-compose.yml
├── docker/
│   └── init.sql
└── src/
    ├── ECommerce.Shared/
    ├── Orchestrator.Api/
    ├── Stock.Worker/
    └── Notification.Worker/
```

## License
This project is distributed under the license in `LICENSE`.
