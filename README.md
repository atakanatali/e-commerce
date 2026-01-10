# E-Commerce Microservices Solution

This repository contains a .NET 8 microservices sample that models a simple e-commerce flow (order creation → stock reservation → notification). Services communicate asynchronously via RabbitMQ and each service has its own PostgreSQL database.

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
1. **Order.Api** creates orders and publishes stock reservation requests.
2. **Stock.Api** processes reservations and publishes the outcome.
3. **Notification.Api** records notifications when orders are confirmed.

## Projects
- **Order.Api**: Order creation and reservation result handling. Port: `5001` (docker).
- **Stock.Api**: Stock reservation processing.
- **Notification.Api**: Order confirmation notifications.
- **ECommerce.Shared**: Shared models/contracts and helpers.

## Prerequisites
- .NET SDK 8.0
- Docker & Docker Compose (optional, recommended)
- PostgreSQL and RabbitMQ (automatically provided via Docker)

## Quick Start (Docker)
Run all services and dependencies:

```bash
docker compose up --build
```

Default endpoints:
- RabbitMQ management UI: `http://localhost:15672` (user: `guest`, password: `guest`)
- Order.Api: `http://localhost:5001`

> The other services do not expose host ports; they communicate over the Docker network.

## Run Locally
You can run each service in a separate terminal:

```bash
dotnet restore
```

```bash
dotnet run --project src/Order.Api/Order.Api.csproj
```

```bash
dotnet run --project src/Stock.Api/Stock.Api.csproj
```

```bash
dotnet run --project src/Notification.Api/Notification.Api.csproj
```

When running locally, use the following host values for infrastructure:

- RabbitMQ host: `localhost`
- PostgreSQL host: `localhost`

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
    ├── Order.Api/
    ├── Stock.Api/
    └── Notification.Api/
```

## License
This project is distributed under the license in `LICENSE`.
