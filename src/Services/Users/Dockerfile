# Build context: repository root (see docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Shared build configuration first: global.json, props and the central package versions.
COPY global.json Directory.Build.props Directory.Packages.props ./

# Project files only, so the restore layer is cached until a dependency changes.
COPY src/Services/Users/Users.Domain/Users.Domain.csproj src/Services/Users/Users.Domain/
COPY src/Services/Users/Users.Application/Users.Application.csproj src/Services/Users/Users.Application/
COPY src/Services/Users/Users.Infrastructure/Users.Infrastructure.csproj src/Services/Users/Users.Infrastructure/
COPY src/Services/Users/Users.Api/Users.Api.csproj src/Services/Users/Users.Api/
RUN dotnet restore src/Services/Users/Users.Api/Users.Api.csproj

# Source code, then publish.
COPY src/Services/Users/ src/Services/Users/
RUN dotnet publish src/Services/Users/Users.Api/Users.Api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Users.Api.dll"]