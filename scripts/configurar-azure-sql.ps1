# Script para configurar la connection string de SQL Server en Azure
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\configurar-azure-sql.ps1

param(
    [string]$ResourceGroup = "licoreria-rg",
    [string]$WebAppName = "licoreria-api"
)

$sqlConnectionString = "Server=tcp:sqlserverjuan123.database.windows.net,1433;Database=dbLicoreriaLaGata;User ID=adminjuan;Password=LicoreriaLaGata2025!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "⚙️ Configurando connection strings y app settings..." -ForegroundColor Yellow

# Configurar SQL Connection String
Write-Host "`n📝 Configurando SQL Server Connection String..." -ForegroundColor Cyan
az webapp config connection-string set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --connection-string-type SQLServer `
    --settings SqlServerConnection="$sqlConnectionString"
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SQL Connection String configurado" -ForegroundColor Green
} else {
    Write-Host "❌ Error al configurar SQL Connection String" -ForegroundColor Red
    exit 1
}

# Configurar App Settings
Write-Host "`n📝 Configurando App Settings..." -ForegroundColor Cyan
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings `
        JwtSettings__SecretKey="LicoreriaLaGata2025SuperSecretKeyForJWTTokenGeneration" `
        JwtSettings__Issuer="LicoreriaAPI" `
        JwtSettings__Audience="LicoreriaAPIUsers" `
        JwtSettings__ExpirationMinutes="60" `
        MongoDBSettings__DatabaseName="LicoreriaMongoDB" `
        ConnectionStrings__SqlServerConnection="$sqlConnectionString"
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ App Settings configurados" -ForegroundColor Green
} else {
    Write-Host "❌ Error al configurar App Settings" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Configuración completada exitosamente!" -ForegroundColor Green
Write-Host "`n📋 Verificación:" -ForegroundColor Cyan
Write-Host "  - SQL Connection String: ✅ Configurado"
Write-Host "  - JWT Settings: ✅ Configurado"
Write-Host "  - MongoDB: ⏳ Pendiente (cuando tengas la connection string)"

