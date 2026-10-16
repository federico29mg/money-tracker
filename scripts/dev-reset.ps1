# Deletes the local environment, including the database volume, then recreates it from scratch.

$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

Write-Host '==> Removing containers and volumes (all local data will be deleted)...'
docker compose down --volumes
if ($LASTEXITCODE -ne 0) { throw 'docker compose down failed.' }

& (Join-Path $PSScriptRoot 'dev-up.ps1')