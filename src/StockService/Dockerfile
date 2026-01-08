FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ECommerce.sln ./
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/StockService/StockService.csproj src/StockService/
RUN dotnet restore src/StockService/StockService.csproj
COPY src/ECommerce.Shared src/ECommerce.Shared
COPY src/StockService src/StockService
RUN dotnet publish src/StockService/StockService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StockService.dll"]
