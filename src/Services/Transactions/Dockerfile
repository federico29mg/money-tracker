# Build context: repository root (see docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Shared build configuration first: global.json, props and the central package versions.
COPY global.json Directory.Build.props Directory.Packages.props ./

# Project files only, so the restore layer is cached until a dependency changes.
COPY src/Services/Transactions/Transactions.Domain/Transactions.Domain.csproj src/Services/Transactions/Transactions.Domain/
COPY src/Services/Transactions/Transactions.Application/Transactions.Application.csproj src/Services/Transactions/Transactions.Application/
COPY src/Services/Transactions/Transactions.Infrastructure/Transactions.Infrastructure.csproj src/Services/Transactions/Transactions.Infrastructure/
COPY src/Services/Transactions/Transactions.Api/Transactions.Api.csproj src/Services/Transactions/Transactions.Api/
RUN dotnet restore src/Services/Transactions/Transactions.Api/Transactions.Api.csproj

# Source code, then publish.
COPY src/Services/Transactions/ src/Services/Transactions/
RUN dotnet publish src/Services/Transactions/Transactions.Api/Transactions.Api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Transactions.Api.dll"]