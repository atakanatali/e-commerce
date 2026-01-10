FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ECommerce.sln ./
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/Stock.Api/Stock.Api.csproj src/Stock.Api/
RUN dotnet restore src/Stock.Api/Stock.Api.csproj
COPY src/ECommerce.Shared src/ECommerce.Shared
COPY src/Stock.Api src/Stock.Api
RUN dotnet publish src/Stock.Api/Stock.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Stock.Api.dll"]
