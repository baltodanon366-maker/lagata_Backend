# Script para verificar configuración completa en Azure
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\verificar-config-azure.ps1

param(
    [string]$ResourceGroup = "RG_Licoreria",
    [string]$WebAppName = "api-lagata"
)

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Verificación de Configuración Azure" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

# 1. Connection Strings
Write-Host "1. Connection Strings:" -ForegroundColor Yellow
$sqlConn = az webapp config appsettings list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[?name=='ConnectionStrings__SqlServerConnection'].value" `
    --output tsv

if ([string]::IsNullOrEmpty($sqlConn)) {
    Write-Host "   ❌ ConnectionStrings__SqlServerConnection NO configurada" -ForegroundColor Red
} else {
    Write-Host "   ✅ ConnectionStrings__SqlServerConnection configurada" -ForegroundColor Green
    if ($sqlConn -match "Database=([^;]+)") {
        Write-Host "   Base de datos: $($matches[1])" -ForegroundColor Cyan
    }
}

# 2. JWT Settings
Write-Host "`n2. JWT Settings:" -ForegroundColor Yellow
$jwtSecret = az webapp config appsettings list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[?name=='JwtSettings__SecretKey'].value" `
    --output tsv

if ([string]::IsNullOrEmpty($jwtSecret)) {
    Write-Host "   ❌ JwtSettings__SecretKey NO configurada" -ForegroundColor Red
} else {
    Write-Host "   ✅ JwtSettings__SecretKey configurada" -ForegroundColor Green
    Write-Host "   Longitud: $($jwtSecret.Length) caracteres" -ForegroundColor Cyan
    if ($jwtSecret.Length -lt 32) {
        Write-Host "   ⚠️  ADVERTENCIA: La clave debe tener al menos 32 caracteres" -ForegroundColor Yellow
    }
}

# 3. Environment
Write-Host "`n3. Environment:" -ForegroundColor Yellow
$env = az webapp config appsettings list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[?name=='ASPNETCORE_ENVIRONMENT'].value" `
    --output tsv

if ([string]::IsNullOrEmpty($env)) {
    Write-Host "   ⚠️  ASPNETCORE_ENVIRONMENT no configurado (usará Production por defecto)" -ForegroundColor Yellow
} else {
    Write-Host "   ✅ ASPNETCORE_ENVIRONMENT: $env" -ForegroundColor Green
}

# 4. URL
Write-Host "`n4. URL de la API:" -ForegroundColor Yellow
$webappUrl = az webapp show --name $WebAppName --resource-group $ResourceGroup --query defaultHostName -o tsv
Write-Host "   https://$webappUrl" -ForegroundColor Cyan
Write-Host "   Swagger: https://$webappUrl/swagger" -ForegroundColor Cyan

# 5. Estado del App Service
Write-Host "`n5. Estado del App Service:" -ForegroundColor Yellow
$state = az webapp show --name $WebAppName --resource-group $ResourceGroup --query state -o tsv
Write-Host "   Estado: $state" -ForegroundColor $(if ($state -eq "Running") { "Green" } else { "Red" })

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host "  Para ver logs:" -ForegroundColor Cyan
Write-Host "  az webapp log tail --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Green

