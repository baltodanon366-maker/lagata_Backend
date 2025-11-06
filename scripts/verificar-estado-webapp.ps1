# Script para verificar el estado del Web App
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\verificar-estado-webapp.ps1

param(
    [string]$ResourceGroup = "RG_Licoreria",
    [string]$WebAppName = "api-lagata"
)

Write-Host "Verificando estado del Web App..." -ForegroundColor Green
Write-Host "Web App: $WebAppName" -ForegroundColor Cyan
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan

# Verificar estado
Write-Host "`nEstado del Web App:" -ForegroundColor Yellow
az webapp show `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "{State:state, DefaultHostName:defaultHostName, Location:location, ResourceGroup:resourceGroup}" `
    --output table

# Verificar si está corriendo
Write-Host "`nEstado de ejecucion:" -ForegroundColor Yellow
$state = az webapp show --resource-group $ResourceGroup --name $WebAppName --query "state" -o tsv
if ($state -eq "Running") {
    Write-Host "OK: Web App esta corriendo" -ForegroundColor Green
} else {
    Write-Host "ADVERTENCIA: Web App esta en estado: $state" -ForegroundColor Yellow
    Write-Host "Iniciando Web App..." -ForegroundColor Yellow
    az webapp start --resource-group $ResourceGroup --name $WebAppName
}

# Verificar URL
Write-Host "`nURL del Web App:" -ForegroundColor Yellow
$url = az webapp show --resource-group $ResourceGroup --name $WebAppName --query "defaultHostName" -o tsv
Write-Host "https://$url" -ForegroundColor Cyan

# Ver logs recientes
Write-Host "`nVer logs recientes (presiona Ctrl+C para salir):" -ForegroundColor Yellow
Write-Host "Ejecutando: az webapp log tail --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor Gray
az webapp log tail --resource-group $ResourceGroup --name $WebAppName

