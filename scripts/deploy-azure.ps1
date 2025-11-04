# Script de deployment para Azure (PowerShell)
# Uso: .\scripts\deploy-azure.ps1 [environment]

param(
    [string]$Environment = "production",
    [string]$AzureWebAppName = $env:AZURE_WEBAPP_NAME ?? "licoreria-api",
    [string]$ResourceGroup = $env:RESOURCE_GROUP ?? "licoreria-rg"
)

Write-Host "🚀 Iniciando deployment a Azure..." -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "Web App: $AzureWebAppName" -ForegroundColor Cyan
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan

# Verificar que Azure CLI esté instalado
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Azure CLI no está instalado. Por favor instálalo primero." -ForegroundColor Red
    exit 1
}

# Verificar login
Write-Host "🔐 Verificando login en Azure..." -ForegroundColor Yellow
$account = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ No estás logueado en Azure. Ejecuta: az login" -ForegroundColor Red
    exit 1
}

# Build y publish
Write-Host "📦 Compilando aplicación..." -ForegroundColor Yellow
dotnet restore LicoreriaAPI.sln
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet build LicoreriaAPI.sln --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet publish src/LicoreriaAPI/LicoreriaAPI.csproj --configuration Release --output ./publish
if ($LASTEXITCODE -ne 0) { exit 1 }

# Crear zip
Write-Host "📦 Creando paquete..." -ForegroundColor Yellow
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force

# Deploy a Azure App Service
Write-Host "☁️ Desplegando a Azure App Service..." -ForegroundColor Yellow
az webapp deploy `
    --resource-group $ResourceGroup `
    --name $AzureWebAppName `
    --type zip `
    --src-path ./publish.zip

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Deployment completado exitosamente!" -ForegroundColor Green
    Write-Host "🌐 URL: https://$AzureWebAppName.azurewebsites.net" -ForegroundColor Cyan
} else {
    Write-Host "❌ Error en el deployment" -ForegroundColor Red
    exit 1
}

# Limpiar
Remove-Item ./publish.zip -ErrorAction SilentlyContinue


