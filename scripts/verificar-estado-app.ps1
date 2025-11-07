# Script para verificar el estado de la App Service
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\verificar-estado-app.ps1

param(
    [string]$ResourceGroup = "RG_Licoreria",
    [string]$WebAppName = "api-lagata"
)

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Verificación de Estado de App Service" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

# Intentar obtener información (puede fallar si la suscripción está deshabilitada)
Write-Host "1. Verificando estado del App Service..." -ForegroundColor Yellow
try {
    $webapp = az webapp show --name $WebAppName --resource-group $ResourceGroup 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✅ App Service encontrado" -ForegroundColor Green
        $state = az webapp show --name $WebAppName --resource-group $ResourceGroup --query state -o tsv 2>&1
        $url = az webapp show --name $WebAppName --resource-group $ResourceGroup --query defaultHostName -o tsv 2>&1
        Write-Host "   Estado: $state" -ForegroundColor $(if ($state -eq "Running") { "Green" } else { "Red" })
        Write-Host "   URL: https://$url" -ForegroundColor Cyan
    } else {
        Write-Host "   ⚠️  No se pudo obtener información (puede ser problema de permisos)" -ForegroundColor Yellow
        Write-Host "   Error: $webapp" -ForegroundColor Red
    }
} catch {
    Write-Host "   ⚠️  Error al verificar estado" -ForegroundColor Yellow
}

Write-Host "`n2. Probando endpoints..." -ForegroundColor Yellow
$url = "https://api-lagata-f2afdpf8cqcngrbm.canadacentral-01.azurewebsites.net"

# Probar raíz
Write-Host "   Probando raíz: $url" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
    Write-Host "   ✅ Respuesta: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Probar Swagger JSON
Write-Host "`n   Probando Swagger JSON: $url/swagger/v1/swagger.json" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$url/swagger/v1/swagger.json" -Method Get -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
    Write-Host "   ✅ Swagger JSON disponible: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Esto indica que la aplicación no está iniciando correctamente" -ForegroundColor Yellow
}

# Probar endpoint de login
Write-Host "`n   Probando endpoint de login: $url/api/Auth/login" -ForegroundColor Cyan
try {
    $body = @{
        nombreUsuario = "admin"
        password = "Admin123!"
    } | ConvertTo-Json
    
    $response = Invoke-WebRequest -Uri "$url/api/Auth/login" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
    Write-Host "   ✅ Login endpoint responde: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ⚠️  Login endpoint: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host "  Para ver logs detallados:" -ForegroundColor Cyan
Write-Host "  az webapp log tail --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Green

