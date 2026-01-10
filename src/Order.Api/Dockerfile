FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ECommerce.sln ./
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/Order.Api/Order.Api.csproj src/Order.Api/
RUN dotnet restore src/Order.Api/Order.Api.csproj
COPY src/ECommerce.Shared src/ECommerce.Shared
COPY src/Order.Api src/Order.Api
RUN dotnet publish src/Order.Api/Order.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Order.Api.dll"]
