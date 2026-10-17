# Generates a new RSA key for signing Users.Api JWTs and stores it in user secrets (Rider, dotnet ef) and .env (Docker).
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File scripts/new-jwt-key.ps1
# Any previous key is replaced, so all existing tokens stop being valid.

$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

$openssl = Join-Path $env:ProgramFiles 'Git\mingw64\bin\openssl.exe'
if (-not (Test-Path $openssl)) {
    throw "openssl not found at $openssl. It ships with Git for Windows."
}
if (-not (Test-Path '.env')) {
    throw 'Missing .env file. Copy .env.example to .env and set MSSQL_SA_PASSWORD first.'
}

# Temporary files instead of pipes: Windows PowerShell 5.1 pipelines corrupt binary data.
$pemFile = New-TemporaryFile
$derFile = New-TemporaryFile
try {
    Write-Host '==> Generating a 2048-bit RSA key (PKCS#8)...'
    & $openssl genpkey -quiet -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out $pemFile.FullName
    if ($LASTEXITCODE -ne 0) { throw 'openssl genpkey failed.' }

    & $openssl pkcs8 -topk8 -nocrypt -in $pemFile.FullName -outform DER -out $derFile.FullName
    if ($LASTEXITCODE -ne 0) { throw 'openssl pkcs8 failed.' }

    $key = [Convert]::ToBase64String([IO.File]::ReadAllBytes($derFile.FullName))
}
finally {
    Remove-Item $pemFile.FullName, $derFile.FullName -Force -ErrorAction SilentlyContinue
}

Write-Host '==> Storing it in user secrets (Jwt:SigningKey)...'
dotnet user-secrets set 'Jwt:SigningKey' $key --project src/Services/Users/Users.Api | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'dotnet user-secrets set failed.' }

Write-Host '==> Storing it in .env (JWT_SIGNING_KEY)...'
$envPath = (Resolve-Path '.env').Path
$lines = @([IO.File]::ReadAllLines($envPath) | Where-Object { $_ -notmatch '^JWT_SIGNING_KEY=' })
$lines += "JWT_SIGNING_KEY=$key"
# UTF-8 without BOM: a BOM would corrupt the first variable name for Docker Compose.
[IO.File]::WriteAllLines($envPath, $lines, (New-Object System.Text.UTF8Encoding($false)))

Write-Host '==> Done. Run "Start (Docker + migrations)" and restart Users.Api to use the new key.'