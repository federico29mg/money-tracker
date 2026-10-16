# Starts the local environment (SQL Server + Users.Api) and applies the Users database migrations.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File scripts/dev-up.ps1

$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

if (-not (Test-Path '.env')) {
    throw 'Missing .env file. Copy .env.example to .env and set MSSQL_SA_PASSWORD.'
}

Write-Host '==> Starting containers (waiting until SQL Server is healthy)...'
docker compose up --detach --build --wait
if ($LASTEXITCODE -ne 0) { throw 'docker compose up failed.' }

Write-Host '==> Restoring .NET tools and packages...'
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed.' }
dotnet restore MoneyTracker.slnx
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }

Write-Host '==> Applying Users database migrations...'
dotnet ef database update --project src/Services/Users/Users.Infrastructure --startup-project src/Services/Users/Users.Api
if ($LASTEXITCODE -ne 0) { throw 'Database migration failed.' }

Write-Host '==> Ready. Users.Api: http://localhost:5100'