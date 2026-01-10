FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ECommerce.sln ./
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/Notification.Api/Notification.Api.csproj src/Notification.Api/
RUN dotnet restore src/Notification.Api/Notification.Api.csproj
COPY src/ECommerce.Shared src/ECommerce.Shared
COPY src/Notification.Api src/Notification.Api
RUN dotnet publish src/Notification.Api/Notification.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notification.Api.dll"]
