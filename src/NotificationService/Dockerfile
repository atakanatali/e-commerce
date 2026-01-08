FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ECommerce.sln ./
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/NotificationService/NotificationService.csproj src/NotificationService/
RUN dotnet restore src/NotificationService/NotificationService.csproj
COPY src/ECommerce.Shared src/ECommerce.Shared
COPY src/NotificationService src/NotificationService
RUN dotnet publish src/NotificationService/NotificationService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.dll"]
