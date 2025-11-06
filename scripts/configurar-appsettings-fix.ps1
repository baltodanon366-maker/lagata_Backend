# Script corregido para configurar App Settings correctamente
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\configurar-appsettings-fix.ps1

param(
    [string]$ResourceGroup = "RG_Licoreria",
    [string]$WebAppName = "api-lagata"
)

$sqlConnectionString = "Server=tcp:sqlserverjuan123.database.windows.net,1433;Database=dbLicoreriaLaGata;User ID=adminjuan;Password=LicoreriaLaGata2025!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$dataWarehouseConnectionString = "Server=tcp:sqlserverjuan123.database.windows.net,1433;Database=dbLicoreriaDW;User ID=adminjuan;Password=LicoreriaLaGata2025!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "Configurando App Settings correctamente..." -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan
Write-Host "Web App: $WebAppName" -ForegroundColor Cyan
Write-Host ""

# Configurar cada setting individualmente para asegurar que se guarde
Write-Host "`nConfigurando puerto..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings ASPNETCORE_URLS="http://+:8080"

Write-Host "Configurando entorno..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings ASPNETCORE_ENVIRONMENT="Production"

Write-Host "Configurando JWT Secret Key..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings JwtSettings__SecretKey="LicoreriaLaGata2025SuperSecretKeyForJWTTokenGenerationMin32Chars"

Write-Host "Configurando JWT Issuer..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings JwtSettings__Issuer="LicoreriaAPI"

Write-Host "Configurando JWT Audience..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings JwtSettings__Audience="LicoreriaAPIUsers"

Write-Host "Configurando JWT Expiration..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings JwtSettings__ExpirationMinutes="60"

Write-Host "Configurando MongoDB Database Name..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings MongoDBSettings__DatabaseName="LicoreriaMongoDB"

Write-Host "Configurando SQL Connection String (App Setting)..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings ConnectionStrings__SqlServerConnection="$sqlConnectionString"

Write-Host "Configurando Data Warehouse Connection String..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings ConnectionStrings__DataWarehouseConnection="$dataWarehouseConnectionString"

# Configurar Connection String como Connection String (no como App Setting) - Opcional pero recomendado
Write-Host "`nConfigurando SQL Connection String (tipo SQLServer)..." -ForegroundColor Yellow
az webapp config connection-string set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --connection-string-type SQLServer `
    --settings SqlServerConnection="$sqlConnectionString"

Write-Host "`nVerificando configuracion..." -ForegroundColor Yellow
az webapp config appsettings list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[?name=='ASPNETCORE_URLS' || name=='JwtSettings__SecretKey' || name=='ConnectionStrings__SqlServerConnection' || name=='ConnectionStrings__DataWarehouseConnection']" `
    --output table

Write-Host "`nOK: Configuracion completada!" -ForegroundColor Green
Write-Host "`nReiniciando Web App para aplicar cambios..." -ForegroundColor Yellow
az webapp restart --resource-group $ResourceGroup --name $WebAppName

Write-Host "`nOK: Web App reiniciado" -ForegroundColor Green
Write-Host "URL: https://$WebAppName.azurewebsites.net" -ForegroundColor Cyan

