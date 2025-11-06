# Script para verificar configuración y hacer debug del login
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\verificar-login-azure.ps1

param(
    [string]$ResourceGroup = "RG_Licoreria",
    [string]$WebAppName = "api-lagata"
)

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Verificación de Configuración Azure" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

# Verificar Connection Strings
Write-Host "1. Verificando Connection Strings..." -ForegroundColor Yellow
$sqlConnection = az webapp config appsettings list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[?name=='ConnectionStrings__SqlServerConnection'].value" `
    --output tsv

if ([string]::IsNullOrEmpty($sqlConnection)) {
    Write-Host "   ❌ ConnectionStrings__SqlServerConnection NO está configurada" -ForegroundColor Red
} else {
    Write-Host "   ✅ ConnectionStrings__SqlServerConnection configurada" -ForegroundColor Green
    Write-Host "   Valor: $($sqlConnection.Substring(0, [Math]::Min(50, $sqlConnection.Length)))..." -ForegroundColor Gray
}

# Verificar JWT Settings
Write-Host "`n2. Verificando JWT Settings..." -ForegroundColor Yellow
$jwtSecret = az webapp config appsettings list `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query "[?name=='JwtSettings__SecretKey'].value" `
    --output tsv

if ([string]::IsNullOrEmpty($jwtSecret)) {
    Write-Host "   ❌ JwtSettings__SecretKey NO está configurada" -ForegroundColor Red
} else {
    Write-Host "   ✅ JwtSettings__SecretKey configurada" -ForegroundColor Green
}

# Verificar logs recientes
Write-Host "`n3. Obteniendo logs recientes..." -ForegroundColor Yellow
Write-Host "   Ejecutando: az webapp log tail --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor Cyan
Write-Host "   (Presiona Ctrl+C para salir de los logs)" -ForegroundColor Gray
Write-Host ""

# Mostrar URL de la API
Write-Host "`n4. URL de la API:" -ForegroundColor Yellow
$webappUrl = az webapp show --name $WebAppName --resource-group $ResourceGroup --query defaultHostName -o tsv
Write-Host "   https://$webappUrl" -ForegroundColor Cyan
Write-Host "   Swagger: https://$webappUrl/swagger" -ForegroundColor Cyan
Write-Host ""

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Para ver logs en tiempo real:" -ForegroundColor Cyan
Write-Host "  az webapp log tail --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Green

