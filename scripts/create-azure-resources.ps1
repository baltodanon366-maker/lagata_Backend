# Script para crear todos los recursos de Azure necesarios (PowerShell)
# Uso: .\scripts\create-azure-resources.ps1

param(
    [string]$Location = "eastus",
    [string]$ResourceGroup = "licoreria-rg",
    [string]$AppServicePlan = "licoreria-plan",
    [string]$WebAppName = "licoreria-api",
    [string]$SqlServerName = "licoreria-sql-server",
    [string]$SqlAdminUser = "sqladmin",
    [string]$SqlDbName = "LicoreriaDB",
    [string]$CosmosAccount = "licoreria-cosmos",
    [string]$CosmosDb = "LicoreriaMongoDB",
    [string]$KeyVaultName = "licoreria-keyvault"
)

Write-Host "🚀 Creando recursos de Azure para Licoreria API..." -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan
Write-Host "Location: $Location" -ForegroundColor Cyan

# Verificar login
Write-Host "🔐 Verificando login en Azure..." -ForegroundColor Yellow
$account = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ No estás logueado en Azure. Ejecuta: az login" -ForegroundColor Red
    exit 1
}

# Crear Resource Group
Write-Host "📦 Creando Resource Group..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location $Location
if ($LASTEXITCODE -ne 0) { exit 1 }

# Crear App Service Plan
Write-Host "📦 Creando App Service Plan..." -ForegroundColor Yellow
az appservice plan create `
    --name $AppServicePlan `
    --resource-group $ResourceGroup `
    --location $Location `
    --sku B1 `
    --is-linux
if ($LASTEXITCODE -ne 0) { exit 1 }

# Crear Web App
Write-Host "📦 Creando Web App..." -ForegroundColor Yellow
az webapp create `
    --resource-group $ResourceGroup `
    --plan $AppServicePlan `
    --name $WebAppName `
    --runtime "DOTNET|8.0"
if ($LASTEXITCODE -ne 0) { exit 1 }

# Configurar puerto 8080
Write-Host "⚙️ Configurando Web App..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings ASPNETCORE_URLS="http://+:8080" ASPNETCORE_ENVIRONMENT="Production"
if ($LASTEXITCODE -ne 0) { exit 1 }

# Solicitar contraseña SQL
Write-Host "🔐 Ingresa la contraseña para SQL Server admin:" -ForegroundColor Yellow
$securePassword = Read-Host -AsSecureString
$SqlPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
)

# Crear SQL Server
Write-Host "📦 Creando Azure SQL Server..." -ForegroundColor Yellow
az sql server create `
    --name $SqlServerName `
    --resource-group $ResourceGroup `
    --location $Location `
    --admin-user $SqlAdminUser `
    --admin-password $SqlPassword
if ($LASTEXITCODE -ne 0) { exit 1 }

# Crear Firewall Rule
Write-Host "🔥 Creando Firewall Rule..." -ForegroundColor Yellow
az sql server firewall-rule create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0
if ($LASTEXITCODE -ne 0) { exit 1 }

# Crear SQL Database
Write-Host "📦 Creando SQL Database..." -ForegroundColor Yellow
az sql db create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name $SqlDbName `
    --service-objective S0
if ($LASTEXITCODE -ne 0) { exit 1 }

# Crear Cosmos DB Account
Write-Host "📦 Creando Cosmos DB Account..." -ForegroundColor Yellow
az cosmosdb create `
    --name $CosmosAccount `
    --resource-group $ResourceGroup `
    --kind MongoDB
if ($LASTEXITCODE -ne 0) { exit 1 }

# Crear Cosmos DB Database
Write-Host "📦 Creando Cosmos DB Database..." -ForegroundColor Yellow
az cosmosdb mongodb database create `
    --account-name $CosmosAccount `
    --resource-group $ResourceGroup `
    --name $CosmosDb
if ($LASTEXITCODE -ne 0) { exit 1 }

# Crear Key Vault
Write-Host "📦 Creando Key Vault..." -ForegroundColor Yellow
az keyvault create `
    --name $KeyVaultName `
    --resource-group $ResourceGroup `
    --location $Location
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host ""
Write-Host "✅ Recursos creados exitosamente!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Resumen de recursos:" -ForegroundColor Cyan
Write-Host "  - Resource Group: $ResourceGroup"
Write-Host "  - Web App: $WebAppName"
Write-Host "  - SQL Server: $SqlServerName"
Write-Host "  - SQL Database: $SqlDbName"
Write-Host "  - Cosmos DB: $CosmosAccount"
Write-Host "  - Key Vault: $KeyVaultName"
Write-Host ""
Write-Host "📝 Próximos pasos:" -ForegroundColor Yellow
Write-Host "  1. Configurar connection strings en Key Vault"
Write-Host "  2. Configurar App Settings en Web App"
Write-Host "  3. Ejecutar migraciones de base de datos"
Write-Host "  4. Desplegar la aplicación"


