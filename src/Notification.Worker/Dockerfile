FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ECommerce.sln ./
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/Notification.Worker/Notification.Worker.csproj src/Notification.Worker/
RUN dotnet restore src/Notification.Worker/Notification.Worker.csproj
COPY src/ECommerce.Shared src/ECommerce.Shared
COPY src/Notification.Worker src/Notification.Worker
RUN dotnet publish src/Notification.Worker/Notification.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
