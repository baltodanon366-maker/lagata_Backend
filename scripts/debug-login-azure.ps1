# Script para debug del login en Azure
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\debug-login-azure.ps1

param(
    [string]$ResourceGroup = "RG_Licoreria",
    [string]$WebAppName = "api-lagata"
)

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Debug: Login en Azure" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

# 1. Verificar Connection String
Write-Host "1. Verificando Connection String..." -ForegroundColor Yellow
$connectionString = az webapp config appsettings list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[?name=='ConnectionStrings__SqlServerConnection'].value" `
    --output tsv

if ([string]::IsNullOrEmpty($connectionString)) {
    Write-Host "   ❌ ConnectionStrings__SqlServerConnection NO configurada" -ForegroundColor Red
    Write-Host "   Ejecuta: scripts/configurar-appsettings-fix.ps1" -ForegroundColor Yellow
} else {
    Write-Host "   ✅ ConnectionStrings__SqlServerConnection configurada" -ForegroundColor Green
    $dbName = if ($connectionString -match "Database=([^;]+)") { $matches[1] } else { "No encontrado" }
    Write-Host "   Base de datos: $dbName" -ForegroundColor Cyan
}

# 2. Verificar JWT Secret
Write-Host "`n2. Verificando JWT Secret..." -ForegroundColor Yellow
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
}

# 3. Obtener URL
Write-Host "`n3. URL de la API:" -ForegroundColor Yellow
$webappUrl = az webapp show --name $WebAppName --resource-group $ResourceGroup --query defaultHostName -o tsv
Write-Host "   https://$webappUrl" -ForegroundColor Cyan

# 4. Probar login
Write-Host "`n4. Probando login..." -ForegroundColor Yellow
$loginUrl = "https://$webappUrl/api/Auth/login"
$body = @{
    nombreUsuario = "admin"
    password = "Admin123!"
} | ConvertTo-Json

Write-Host "   URL: $loginUrl" -ForegroundColor Cyan
Write-Host "   Body: $body" -ForegroundColor Cyan
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Host "   ✅ Login exitoso!" -ForegroundColor Green
    Write-Host "   Token recibido: $($response.Token.Substring(0, [Math]::Min(50, $response.Token.Length)))..." -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Error en login:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "   Detalles: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}

# 5. Ver logs
Write-Host "`n5. Para ver logs detallados:" -ForegroundColor Yellow
Write-Host "   az webapp log tail --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor Cyan
Write-Host ""

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Verificaciones completadas" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

