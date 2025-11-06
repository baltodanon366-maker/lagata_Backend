# Script simplificado para crear recursos de Azure
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\create-azure-resources-simple.ps1

param(
    [string]$Location = "eastus",
    [string]$ResourceGroup = "licoreria-rg",
    [string]$AppServicePlan = "licoreria-plan",
    [string]$WebAppName = "licoreria-api"
)

Write-Host "🚀 Creando recursos de Azure para Licoreria API..." -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan
Write-Host "Location: $Location" -ForegroundColor Cyan

# Verificar login
Write-Host "`n🔐 Verificando login en Azure..." -ForegroundColor Yellow
$account = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ No estás logueado en Azure. Ejecuta: az login" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Logueado correctamente" -ForegroundColor Green

# Crear Resource Group
Write-Host "`n📦 Creando Resource Group..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location $Location
if ($LASTEXITCODE -ne 0) { 
    Write-Host "⚠️ El Resource Group ya existe o hubo un error" -ForegroundColor Yellow
}

# Crear App Service Plan
Write-Host "`n📦 Creando App Service Plan..." -ForegroundColor Yellow
az appservice plan create `
    --name $AppServicePlan `
    --resource-group $ResourceGroup `
    --location $Location `
    --sku B1 `
    --is-linux
if ($LASTEXITCODE -ne 0) { 
    Write-Host "⚠️ El App Service Plan ya existe o hubo un error" -ForegroundColor Yellow
}

# Crear Web App
Write-Host "`n📦 Creando Web App..." -ForegroundColor Yellow
az webapp create `
    --resource-group $ResourceGroup `
    --plan $AppServicePlan `
    --name $WebAppName `
    --runtime '"DOTNET|8.0"'
if ($LASTEXITCODE -ne 0) { 
    Write-Host "⚠️ La Web App ya existe o hubo un error" -ForegroundColor Yellow
}

# Configurar puerto 8080
Write-Host "`n⚙️ Configurando Web App..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings ASPNETCORE_URLS="http://+:8080" ASPNETCORE_ENVIRONMENT="Production"
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n✅ Recursos creados exitosamente!" -ForegroundColor Green
Write-Host "`n📋 Resumen de recursos:" -ForegroundColor Cyan
Write-Host "  - Resource Group: $ResourceGroup"
Write-Host "  - Web App: $WebAppName"
Write-Host "  - URL: https://$WebAppName.azurewebsites.net" -ForegroundColor Yellow
Write-Host "`n📝 Próximos pasos:" -ForegroundColor Yellow
Write-Host "  1. Configurar connection string de SQL Server"
Write-Host "  2. Configurar App Settings (JWT, etc.)"
Write-Host "  3. Desplegar la aplicación"

