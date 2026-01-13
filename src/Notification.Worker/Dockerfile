FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ECommerce.sln ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY src/ECommerce.Core/ECommerce.Core.csproj src/ECommerce.Core/
COPY src/ECommerce.Messaging.RabbitMq/ECommerce.Messaging.RabbitMq.csproj src/ECommerce.Messaging.RabbitMq/
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/Orchestrator.Api/Orchestrator.Api.csproj src/Orchestrator.Api/
COPY src/Notification.Worker/Notification.Worker.csproj src/Notification.Worker/
COPY src/Stock.Worker/Stock.Worker.csproj src/Stock.Worker/
RUN dotnet restore ECommerce.sln
COPY src/ECommerce.Core src/ECommerce.Core
COPY src/ECommerce.Messaging.RabbitMq src/ECommerce.Messaging.RabbitMq
COPY src/ECommerce.Shared src/ECommerce.Shared
COPY src/Notification.Worker src/Notification.Worker
RUN dotnet publish src/Notification.Worker/Notification.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
