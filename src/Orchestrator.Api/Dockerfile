FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ECommerce.sln ./
COPY src/ECommerce.Core/ECommerce.Core.csproj src/ECommerce.Core/
COPY src/ECommerce.Messaging.RabbitMq/ECommerce.Messaging.RabbitMq.csproj src/ECommerce.Messaging.RabbitMq/
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/Orchestrator.Api/Orchestrator.Api.csproj src/Orchestrator.Api/
RUN dotnet restore src/Orchestrator.Api/Orchestrator.Api.csproj
COPY src/ECommerce.Core src/ECommerce.Core
COPY src/ECommerce.Messaging.RabbitMq src/ECommerce.Messaging.RabbitMq
COPY src/ECommerce.Shared src/ECommerce.Shared
COPY src/Orchestrator.Api src/Orchestrator.Api
RUN dotnet publish src/Orchestrator.Api/Orchestrator.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Orchestrator.Api.dll"]
