# Script para Deploy Manual a Azure App Service
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\deploy-manual.ps1

param(
    [string]$WebAppName = "api-lagata",
    [string]$ResourceGroup = "la_gata_2",
    [string]$Configuration = "Release"
)

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Deploy Manual a Azure App Service" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "App Service: $WebAppName" -ForegroundColor Cyan
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host ""

# Verificar que Azure CLI está instalado
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Azure CLI no está instalado." -ForegroundColor Red
    Write-Host "   Instala Azure CLI desde: https://aka.ms/installazurecliwindows" -ForegroundColor Yellow
    exit 1
}

# Verificar login
Write-Host "🔐 Verificando login en Azure..." -ForegroundColor Yellow
$account = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ No estás logueado en Azure." -ForegroundColor Red
    Write-Host "   Ejecuta: az login" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Logueado correctamente" -ForegroundColor Green
Write-Host ""

# Verificar que el App Service existe
Write-Host "🔍 Verificando que el App Service existe..." -ForegroundColor Yellow
$webappExists = az webapp show --name $WebAppName --resource-group $ResourceGroup 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ El App Service '$WebAppName' no existe en el resource group '$ResourceGroup'" -ForegroundColor Red
    Write-Host "   Verifica el nombre y el resource group" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ App Service encontrado" -ForegroundColor Green
Write-Host ""

# Limpiar carpeta publish anterior
if (Test-Path "./publish") {
    Write-Host "🗑️  Eliminando carpeta publish anterior..." -ForegroundColor Yellow
    Remove-Item -Path "./publish" -Recurse -Force
    Write-Host "✅ Carpeta limpiada" -ForegroundColor Green
    Write-Host ""
}

# Restore
Write-Host "📦 Restaurando dependencias NuGet..." -ForegroundColor Yellow
dotnet restore LicoreriaAPI.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al restaurar dependencias" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Dependencias restauradas" -ForegroundColor Green
Write-Host ""

# Build
Write-Host "🔨 Compilando solución..." -ForegroundColor Yellow
dotnet build LicoreriaAPI.sln --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al compilar" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Compilación exitosa" -ForegroundColor Green
Write-Host ""

# Publish
Write-Host "📤 Publicando aplicación..." -ForegroundColor Yellow
dotnet publish src/LicoreriaAPI/LicoreriaAPI.csproj --configuration $Configuration --output ./publish
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al publicar" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Publicación exitosa" -ForegroundColor Green
Write-Host ""

# Deploy a Azure
Write-Host "🚀 Desplegando a Azure..." -ForegroundColor Yellow
Write-Host "   Esto puede tomar unos minutos..." -ForegroundColor Cyan

az webapp deploy `
    --name $WebAppName `
    --resource-group $ResourceGroup `
    --src-path "./publish" `
    --type zip

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al desplegar" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "  ✅ Deploy completado exitosamente!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 URL de la aplicación:" -ForegroundColor Cyan
$webappUrl = az webapp show --name $WebAppName --resource-group $ResourceGroup --query defaultHostName -o tsv
Write-Host "   https://$webappUrl" -ForegroundColor White
Write-Host ""
Write-Host "📊 Swagger UI:" -ForegroundColor Cyan
Write-Host "   https://$webappUrl/swagger" -ForegroundColor White
Write-Host ""

