FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ECommerce.sln ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY src/ECommerce.Core/ECommerce.Core.csproj src/ECommerce.Core/
COPY src/ECommerce.Messaging.RabbitMq/ECommerce.Messaging.RabbitMq.csproj src/ECommerce.Messaging.RabbitMq/
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/Notification.Worker/Notification.Worker.csproj src/Notification.Worker/
COPY src/Orchestrator.Api/Orchestrator.Api.csproj src/Orchestrator.Api/
COPY src/Stock.Worker/Stock.Worker.csproj src/Stock.Worker/
RUN dotnet restore ECommerce.sln
COPY src/ECommerce.Core src/ECommerce.Core
COPY src/ECommerce.Messaging.RabbitMq src/ECommerce.Messaging.RabbitMq
COPY src/ECommerce.Shared src/ECommerce.Shared
COPY src/Orchestrator.Api src/Orchestrator.Api
RUN dotnet publish src/Orchestrator.Api/Orchestrator.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV Redis__Enabled="true" \
    Redis__Mode="Single" \
    Redis__ConnectionString="redis:6379" \
    RateLimiting__Enabled="true" \
    RateLimiting__Requests="100" \
    RateLimiting__WindowSeconds="60" \
    RateLimiting__KeyPrefix="orchestrator" \
    Logging__Elasticsearch__Enabled="true" \
    Logging__Elasticsearch__Uri="http://elasticsearch:9200" \
    Logging__Elasticsearch__IndexPrefix="logs" \
    Logging__Elasticsearch__MinimumLevel="Information" \
    Logging__Elasticsearch__AutoRegisterTemplate="true" \
    Logging__Elasticsearch__BatchPostingLimit="1000" \
    Logging__Elasticsearch__PeriodSeconds="2" \
    Correlation__HeaderName="X-Correlation-Id"
ENTRYPOINT ["dotnet", "Orchestrator.Api.dll"]
