# Script para crear App Service y configurar todo con recursos SQL existentes
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\configurar-todo-con-recursos-existentes.ps1

param(
    [string]$ResourceGroup = "la_gata_2",
    [string]$SqlServerName = "lagatabianca",
    [string]$SqlAdminUser = "adminlicoreria",
    [string]$SqlPassword = "Bi4nc41006",
    [string]$Location = "eastus2",
    [string]$AppServicePlanName = "plan-lagata",
    [string]$WebAppName = "api-lagata"
)

Write-Host ""
Write-Host "CONFIGURANDO APP SERVICE Y CONNECTION STRINGS" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

# Verificar login
Write-Host "Verificando login en Azure..." -ForegroundColor Yellow
$account = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: No estas logueado en Azure. Ejecuta: az login" -ForegroundColor Red
    exit 1
}
Write-Host "OK: Logueado correctamente" -ForegroundColor Green
Write-Host ""

Write-Host "Configuracion:" -ForegroundColor Cyan
Write-Host "  Resource Group: $ResourceGroup" -ForegroundColor White
Write-Host "  SQL Server: $SqlServerName.database.windows.net" -ForegroundColor White
Write-Host "  SQL Admin: $SqlAdminUser" -ForegroundColor White
Write-Host "  Location: $Location" -ForegroundColor White
Write-Host "  Web App: $WebAppName" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Continuar? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "Operacion cancelada" -ForegroundColor Red
    exit 0
}

# 1. Verificar/Crear App Service Plan
Write-Host ""
Write-Host "[1/4] Verificando App Service Plan..." -ForegroundColor Yellow
$planExists = az appservice plan show --name $AppServicePlanName --resource-group $ResourceGroup 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "  Creando App Service Plan..." -ForegroundColor Gray
    az appservice plan create --name $AppServicePlanName --resource-group $ResourceGroup --location $Location --sku "B1" --is-linux | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: App Service Plan creado" -ForegroundColor Green
    } else {
        Write-Host "ERROR: No se pudo crear App Service Plan" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "OK: App Service Plan ya existe" -ForegroundColor Green
}

# 2. Verificar/Crear Web App
Write-Host ""
Write-Host "[2/4] Verificando Web App..." -ForegroundColor Yellow
$webAppExists = az webapp show --name $WebAppName --resource-group $ResourceGroup 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "  Creando Web App..." -ForegroundColor Gray
    $runtime = "DOTNET|8.0"
    az webapp create --resource-group $ResourceGroup --plan $AppServicePlanName --name $WebAppName --runtime $runtime | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: Web App creada" -ForegroundColor Green
    } else {
        Write-Host "ERROR: No se pudo crear Web App" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "OK: Web App ya existe" -ForegroundColor Green
}

# 3. Construir connection strings
Write-Host ""
Write-Host "[3/4] Configurando Connection Strings y App Settings..." -ForegroundColor Yellow

$sqlConnectionString = "Server=tcp:" + $SqlServerName + ".database.windows.net,1433;Database=dbLicoreriaLaGata;User ID=" + $SqlAdminUser + ";Password=" + $SqlPassword + ";Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$dataWarehouseConnectionString = "Server=tcp:" + $SqlServerName + ".database.windows.net,1433;Database=dbLicoreriaDW;User ID=" + $SqlAdminUser + ";Password=" + $SqlPassword + ";Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configurar puerto y entorno
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ASPNETCORE_URLS="http://+:8080" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ASPNETCORE_ENVIRONMENT="Production" | Out-Null

# Configurar JWT Settings
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings JwtSettings__SecretKey="LicoreriaLaGata2025SuperSecretKeyForJWTTokenGenerationMin32Chars" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings JwtSettings__Issuer="LicoreriaAPI" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings JwtSettings__Audience="LicoreriaAPIUsers" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings JwtSettings__ExpirationMinutes="60" | Out-Null

# Configurar MongoDB Settings
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings MongoDBSettings__DatabaseName="LicoreriaMongoDB" | Out-Null

# Configurar Connection Strings
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ConnectionStrings__SqlServerConnection="$sqlConnectionString" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ConnectionStrings__DataWarehouseConnection="$dataWarehouseConnectionString" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ConnectionStrings__MongoDBConnection="mongodb://localhost:27017" | Out-Null

# Configurar Connection String como tipo SQLServer
az webapp config connection-string set --resource-group $ResourceGroup --name $WebAppName --connection-string-type SQLServer --settings SqlServerConnection="$sqlConnectionString" | Out-Null

Write-Host "OK: App Settings y Connection Strings configurados" -ForegroundColor Green

# 4. Actualizar appsettings.Development.json
Write-Host ""
Write-Host "[4/4] Actualizando appsettings.Development.json..." -ForegroundColor Yellow
$appsettingsPath = "src\LicoreriaAPI\appsettings.Development.json"

if (Test-Path $appsettingsPath) {
    $json = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    $json.ConnectionStrings.SqlServerConnection = $sqlConnectionString
    $json.ConnectionStrings.DataWarehouseConnection = $dataWarehouseConnectionString
    $json | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath -Encoding UTF8
    Write-Host "OK: appsettings.Development.json actualizado" -ForegroundColor Green
} else {
    Write-Host "ADVERTENCIA: No se encontro appsettings.Development.json" -ForegroundColor Yellow
}

# Resumen final
Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host "CONFIGURACION COMPLETADA" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Recursos configurados:" -ForegroundColor Cyan
Write-Host "  Resource Group: $ResourceGroup" -ForegroundColor White
Write-Host "  SQL Server: $SqlServerName.database.windows.net" -ForegroundColor White
Write-Host "  Base de Datos Principal: dbLicoreriaLaGata" -ForegroundColor White
Write-Host "  Data Warehouse: dbLicoreriaDW" -ForegroundColor White
Write-Host "  Web App: https://$WebAppName.azurewebsites.net" -ForegroundColor White
Write-Host ""
Write-Host "URLs importantes:" -ForegroundColor Cyan
Write-Host "  API: https://$WebAppName.azurewebsites.net" -ForegroundColor Yellow
Write-Host "  Swagger: https://$WebAppName.azurewebsites.net/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "Proximos pasos:" -ForegroundColor Cyan
Write-Host "  1. Ejecutar scripts SQL para crear las tablas" -ForegroundColor White
Write-Host "  2. Hacer deploy de la API" -ForegroundColor White
Write-Host ""

