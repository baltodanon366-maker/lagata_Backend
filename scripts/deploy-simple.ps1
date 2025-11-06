# Script simplificado para deployment
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\deploy-simple.ps1

param(
    [string]$ResourceGroup = "licoreria-rg",
    [string]$WebAppName = "licoreria-api"
)

Write-Host "🚀 Iniciando deployment..." -ForegroundColor Green

# Build y publish
Write-Host "`n📦 Compilando aplicación..." -ForegroundColor Yellow
dotnet restore LicoreriaAPI.sln
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet build LicoreriaAPI.sln --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet publish src/LicoreriaAPI/LicoreriaAPI.csproj --configuration Release --output ./publish
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "✅ Compilación exitosa" -ForegroundColor Green

# Crear zip
Write-Host "`n📦 Creando paquete..." -ForegroundColor Yellow
if (Test-Path ./publish.zip) {
    Remove-Item ./publish.zip
}
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force

# Deploy
Write-Host "`n☁️ Desplegando a Azure App Service..." -ForegroundColor Yellow
az webapp deploy `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --type zip `
    --src-path ./publish.zip

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Deployment completado exitosamente!" -ForegroundColor Green
    Write-Host "🌐 URL: https://$WebAppName.azurewebsites.net" -ForegroundColor Cyan
    Write-Host "📝 Swagger: https://$WebAppName.azurewebsites.net" -ForegroundColor Cyan
} else {
    Write-Host "`n❌ Error en el deployment" -ForegroundColor Red
    exit 1
}

# Limpiar
Remove-Item ./publish.zip -ErrorAction SilentlyContinue
Write-Host "`n🧹 Archivos temporales eliminados" -ForegroundColor Yellow

