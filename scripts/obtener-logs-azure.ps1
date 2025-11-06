# Script para obtener logs de Azure y buscar errores de login
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\obtener-logs-azure.ps1

param(
    [string]$ResourceGroup = "RG_Licoreria",
    [string]$WebAppName = "api-lagata",
    [int]$Lines = 100
)

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Obteniendo logs de Azure" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

Write-Host "Descargando últimos $Lines líneas de logs..." -ForegroundColor Yellow
Write-Host ""

# Obtener logs recientes
az webapp log tail `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --lines $Lines

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host "  Buscar errores relacionados con:" -ForegroundColor Cyan
Write-Host "  - LoginAsync" -ForegroundColor White
Write-Host "  - BCrypt" -ForegroundColor White
Write-Host "  - Connection" -ForegroundColor White
Write-Host "  - SqlException" -ForegroundColor White
Write-Host "  - VerifyPassword" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Green

