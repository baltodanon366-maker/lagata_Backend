# Script para desplegar a tu Web App existente
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\deploy-to-existing-webapp.ps1

param(
    [string]$ResourceGroup = "RG_Licoreria",
    [string]$WebAppName = "api-lagata"
)

Write-Host "Iniciando deployment a Web App existente..." -ForegroundColor Green
Write-Host "Web App: $WebAppName" -ForegroundColor Cyan
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan

# Build y publish
Write-Host "`nCompilando aplicacion..." -ForegroundColor Yellow
dotnet restore LicoreriaAPI.sln
if ($LASTEXITCODE -ne 0) { 
    Write-Host "ERROR: Error en restore" -ForegroundColor Red
    exit 1 
}

dotnet build LicoreriaAPI.sln --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { 
    Write-Host "ERROR: Error en build" -ForegroundColor Red
    exit 1 
}

Write-Host "OK: Build exitoso" -ForegroundColor Green

Write-Host "`nPublicando aplicacion..." -ForegroundColor Yellow
dotnet publish src/LicoreriaAPI/LicoreriaAPI.csproj --configuration Release --output ./publish
if ($LASTEXITCODE -ne 0) { 
    Write-Host "ERROR: Error en publish" -ForegroundColor Red
    exit 1 
}

Write-Host "OK: Publish exitoso" -ForegroundColor Green

# Crear zip
Write-Host "`nCreando paquete ZIP..." -ForegroundColor Yellow
if (Test-Path ./publish.zip) {
    Remove-Item ./publish.zip -Force
}
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force

# Deploy
Write-Host "`nDesplegando a Azure App Service..." -ForegroundColor Yellow
az webapp deploy `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --type zip `
    --src-path ./publish.zip

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nOK: Deployment completado exitosamente!" -ForegroundColor Green
    Write-Host "`nURLs:" -ForegroundColor Cyan
    Write-Host "  - API: https://$WebAppName.azurewebsites.net" -ForegroundColor Yellow
    Write-Host "  - Swagger: https://$WebAppName.azurewebsites.net" -ForegroundColor Yellow
    Write-Host "`nVer logs:" -ForegroundColor Cyan
    Write-Host "  az webapp log tail --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor Gray
} else {
    Write-Host "`nERROR: Error en el deployment" -ForegroundColor Red
    Write-Host "Verifica que el Web App existe y que tienes permisos" -ForegroundColor Yellow
    exit 1
}

# Limpiar
Write-Host "`nLimpiando archivos temporales..." -ForegroundColor Yellow
Remove-Item ./publish.zip -ErrorAction SilentlyContinue
Write-Host "OK: Limpieza completada" -ForegroundColor Green
