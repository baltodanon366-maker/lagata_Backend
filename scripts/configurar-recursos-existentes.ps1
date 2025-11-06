# Script para configurar los recursos existentes de Azure
# Usa los recursos que ya tienes creados

param(
    [string]$ResourceGroup = "RG_Licoreria",
    [string]$WebAppName = "api-lagata"
)

$sqlConnectionString = "Server=tcp:sqlserverjuan123.database.windows.net,1433;Database=dbLicoreriaLaGata;User ID=adminjuan;Password=LicoreriaLaGata2025!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "Configurando recursos existentes de Azure..." -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan
Write-Host "Web App: $WebAppName" -ForegroundColor Cyan

# Verificar que el Web App existe
Write-Host "`nVerificando que el Web App existe..." -ForegroundColor Yellow
$webApp = az webapp show --name $WebAppName --resource-group $ResourceGroup 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: El Web App '$WebAppName' no existe en el Resource Group '$ResourceGroup'" -ForegroundColor Red
    Write-Host "`nOpciones:" -ForegroundColor Yellow
    Write-Host "  1. Crear un nuevo Web App con el script create-new-webapp.ps1"
    Write-Host "  2. Usar un Web App existente (actualiza los nombres en este script)"
    exit 1
}
Write-Host "OK: Web App encontrado" -ForegroundColor Green

# Configurar puerto 8080
Write-Host "`nConfigurando puerto y entorno..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings ASPNETCORE_URLS="http://+:8080" ASPNETCORE_ENVIRONMENT="Production"
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Configuracion de puerto completada" -ForegroundColor Green
} else {
    Write-Host "ADVERTENCIA: Error al configurar puerto" -ForegroundColor Yellow
}

# Configurar SQL Connection String
Write-Host "`nConfigurando SQL Server Connection String..." -ForegroundColor Yellow
az webapp config connection-string set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --connection-string-type SQLServer `
    --settings SqlServerConnection="$sqlConnectionString"
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: SQL Connection String configurado" -ForegroundColor Green
} else {
    Write-Host "ADVERTENCIA: Error al configurar SQL Connection String" -ForegroundColor Yellow
}

# Configurar App Settings
Write-Host "`nConfigurando App Settings..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings `
        JwtSettings__SecretKey="LicoreriaLaGata2025SuperSecretKeyForJWTTokenGenerationMin32Chars" `
        JwtSettings__Issuer="LicoreriaAPI" `
        JwtSettings__Audience="LicoreriaAPIUsers" `
        JwtSettings__ExpirationMinutes="60" `
        MongoDBSettings__DatabaseName="LicoreriaMongoDB" `
        ConnectionStrings__SqlServerConnection="$sqlConnectionString"
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: App Settings configurados" -ForegroundColor Green
} else {
    Write-Host "ADVERTENCIA: Error al configurar App Settings" -ForegroundColor Yellow
}

# Desconectar el deployment source si existe
Write-Host "`nDesconectando deployment source anterior..." -ForegroundColor Yellow
az webapp deployment source delete `
    --resource-group $ResourceGroup `
    --name $WebAppName 2>&1 | Out-Null
# Ignorar errores si no existe

Write-Host "`nOK: Configuracion completada exitosamente!" -ForegroundColor Green
Write-Host "`nResumen:" -ForegroundColor Cyan
Write-Host "  - Resource Group: $ResourceGroup"
Write-Host "  - Web App: $WebAppName"
Write-Host "  - SQL Server: sqlserverjuan123"
Write-Host "  - SQL Database: dbLicoreriaLaGata"
Write-Host "  - URL: https://$WebAppName.azurewebsites.net" -ForegroundColor Yellow
Write-Host "`nProximo paso:" -ForegroundColor Yellow
Write-Host "  Ejecutar: powershell -ExecutionPolicy Bypass -File .\scripts\deploy-to-existing-webapp.ps1"
