# Starts the local environment (SQL Server, Users.Api, Transactions.Api) and applies both services' database migrations.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File scripts/dev-up.ps1

$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

if (-not (Test-Path '.env')) {
    throw 'Missing .env file. Copy .env.example to .env and set MSSQL_SA_PASSWORD.'
}

# Read .env so the migrations use the same password and key as the containers, without depending on user secrets.
$settings = @{}
foreach ($line in Get-Content '.env') {
    if ($line -match '^\s*([A-Za-z_][A-Za-z0-9_]*)=(.*)$') { $settings[$Matches[1]] = $Matches[2] }
}

Write-Host '==> Starting containers (waiting until SQL Server is healthy)...'
docker compose up --detach --build --wait
if ($LASTEXITCODE -ne 0) { throw 'docker compose up failed.' }

Write-Host '==> Restoring .NET tools and packages...'
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed.' }
dotnet restore MoneyTracker.slnx
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }

# Environment variables take precedence over user secrets, so these reach both startup projects.
$sqlServer = "Server=localhost,1433;User Id=sa;Password=$($settings['MSSQL_SA_PASSWORD']);TrustServerCertificate=True"
$env:ConnectionStrings__UsersDb = "$sqlServer;Database=UsersDb"
$env:ConnectionStrings__TransactionsDb = "$sqlServer;Database=TransactionsDb"
$env:Jwt__SigningKey = $settings['JWT_SIGNING_KEY']

Write-Host '==> Applying Users database migrations...'
dotnet ef database update --project src/Services/Users/Users.Infrastructure --startup-project src/Services/Users/Users.Api
if ($LASTEXITCODE -ne 0) { throw 'Users database migration failed.' }

Write-Host '==> Applying Transactions database migrations...'
dotnet ef database update --project src/Services/Transactions/Transactions.Infrastructure --startup-project src/Services/Transactions/Transactions.Api
if ($LASTEXITCODE -ne 0) { throw 'Transactions database migration failed.' }

Write-Host '==> Ready.'
Write-Host '    Users.Api:        http://localhost:5100/scalar/v1'
Write-Host '    Transactions.Api: http://localhost:5101/scalar/v1'