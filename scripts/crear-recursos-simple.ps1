# Script simplificado para crear recursos de Azure
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\crear-recursos-simple.ps1

param(
    [string]$Location = "centralus",
    [string]$ResourceGroup = "rg-licoreria-lagata",
    [string]$SqlServerName = "sqlserver-lagata",
    [string]$SqlAdminUser = "adminlagata",
    [string]$SqlPassword = "",
    [string]$AppServicePlanName = "plan-lagata",
    [string]$WebAppName = "api-lagata"
)

Write-Host ""
Write-Host "CREANDO RECURSOS DE AZURE DESDE CERO" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
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

# Solicitar contraseña si no se proporciono
if ([string]::IsNullOrWhiteSpace($SqlPassword)) {
    Write-Host "Ingresa una contraseña para el SQL Server admin:" -ForegroundColor Yellow
    Write-Host "(Debe tener al menos 8 caracteres)" -ForegroundColor Gray
    $securePassword = Read-Host -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $SqlPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

if ($SqlPassword.Length -lt 8) {
    Write-Host "ERROR: La contraseña debe tener al menos 8 caracteres" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Configuracion:" -ForegroundColor Cyan
Write-Host "  Resource Group: $ResourceGroup" -ForegroundColor White
Write-Host "  Location: $Location" -ForegroundColor White
Write-Host "  SQL Server: $SqlServerName.database.windows.net" -ForegroundColor White
Write-Host "  SQL Admin: $SqlAdminUser" -ForegroundColor White
Write-Host "  Web App: $WebAppName" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Continuar con la creacion? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "Operacion cancelada" -ForegroundColor Red
    exit 0
}

# 1. Crear Resource Group
Write-Host ""
Write-Host "[1/8] Creando Resource Group..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location $Location | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Resource Group creado" -ForegroundColor Green
} else {
    Write-Host "ERROR: No se pudo crear Resource Group" -ForegroundColor Red
    exit 1
}

# 2. Crear SQL Server
Write-Host ""
Write-Host "[2/8] Creando Azure SQL Server..." -ForegroundColor Yellow
az sql server create --name $SqlServerName --resource-group $ResourceGroup --location $Location --admin-user $SqlAdminUser --admin-password $SqlPassword | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: SQL Server creado" -ForegroundColor Green
} else {
    Write-Host "ERROR: No se pudo crear SQL Server" -ForegroundColor Red
    exit 1
}

# 3. Configurar Firewall Rules
Write-Host ""
Write-Host "[3/8] Configurando Firewall Rules..." -ForegroundColor Yellow
az sql server firewall-rule create --resource-group $ResourceGroup --server $SqlServerName --name "AllowAzureServices" --start-ip-address "0.0.0.0" --end-ip-address "0.0.0.0" | Out-Null

$myIp = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content
az sql server firewall-rule create --resource-group $ResourceGroup --server $SqlServerName --name "AllowMyIP" --start-ip-address $myIp --end-ip-address $myIp | Out-Null
Write-Host "OK: Firewall Rules configuradas" -ForegroundColor Green

# 4. Crear Base de Datos Principal
Write-Host ""
Write-Host "[4/8] Creando Base de Datos Principal..." -ForegroundColor Yellow
az sql db create --resource-group $ResourceGroup --server $SqlServerName --name "dbLicoreriaLaGata" --service-objective "Basic" --backup-storage-redundancy "Local" | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Base de datos principal creada" -ForegroundColor Green
} else {
    Write-Host "ERROR: No se pudo crear base de datos principal" -ForegroundColor Red
    exit 1
}

# 5. Crear Data Warehouse
Write-Host ""
Write-Host "[5/8] Creando Data Warehouse..." -ForegroundColor Yellow
az sql db create --resource-group $ResourceGroup --server $SqlServerName --name "dbLicoreriaDW" --service-objective "Basic" --backup-storage-redundancy "Local" | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Data Warehouse creado" -ForegroundColor Green
} else {
    Write-Host "ERROR: No se pudo crear Data Warehouse" -ForegroundColor Red
    exit 1
}

# 6. Crear App Service Plan
Write-Host ""
Write-Host "[6/8] Creando App Service Plan..." -ForegroundColor Yellow
az appservice plan create --name $AppServicePlanName --resource-group $ResourceGroup --location $Location --sku "B1" --is-linux | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: App Service Plan creado" -ForegroundColor Green
} else {
    Write-Host "ERROR: No se pudo crear App Service Plan" -ForegroundColor Red
    exit 1
}

# 7. Crear Web App
Write-Host ""
Write-Host "[7/8] Creando Web App..." -ForegroundColor Yellow
az webapp create --resource-group $ResourceGroup --plan $AppServicePlanName --name $WebAppName --runtime "DOTNET|8.0" | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Web App creada" -ForegroundColor Green
} else {
    Write-Host "ERROR: No se pudo crear Web App" -ForegroundColor Red
    exit 1
}

# 8. Configurar App Settings
Write-Host ""
Write-Host "[8/8] Configurando App Settings..." -ForegroundColor Yellow

# Construir connection strings
$sqlConnStr = "Server=tcp:" + $SqlServerName + ".database.windows.net,1433;Database=dbLicoreriaLaGata;User ID=" + $SqlAdminUser + ";Password=" + $SqlPassword + ";Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$dwConnStr = "Server=tcp:" + $SqlServerName + ".database.windows.net,1433;Database=dbLicoreriaDW;User ID=" + $SqlAdminUser + ";Password=" + $SqlPassword + ";Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configurar settings uno por uno
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ASPNETCORE_URLS="http://+:8080" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ASPNETCORE_ENVIRONMENT="Production" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings JwtSettings__SecretKey="LicoreriaLaGata2025SuperSecretKeyForJWTTokenGenerationMin32Chars" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings JwtSettings__Issuer="LicoreriaAPI" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings JwtSettings__Audience="LicoreriaAPIUsers" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings JwtSettings__ExpirationMinutes="60" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings MongoDBSettings__DatabaseName="LicoreriaMongoDB" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ConnectionStrings__SqlServerConnection="$sqlConnStr" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ConnectionStrings__DataWarehouseConnection="$dwConnStr" | Out-Null
az webapp config appsettings set --resource-group $ResourceGroup --name $WebAppName --settings ConnectionStrings__MongoDBConnection="mongodb://localhost:27017" | Out-Null

# Connection String como tipo SQLServer
az webapp config connection-string set --resource-group $ResourceGroup --name $WebAppName --connection-string-type SQLServer --settings SqlServerConnection="$sqlConnStr" | Out-Null

Write-Host "OK: App Settings configurados" -ForegroundColor Green

# Resumen final
Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host "RECURSOS CREADOS EXITOSAMENTE" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Informacion de los recursos:" -ForegroundColor Cyan
Write-Host "  Resource Group: $ResourceGroup" -ForegroundColor White
Write-Host "  SQL Server: $SqlServerName.database.windows.net" -ForegroundColor White
Write-Host "  SQL Admin: $SqlAdminUser" -ForegroundColor White
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
Write-Host "  2. Actualizar appsettings.Development.json" -ForegroundColor White
Write-Host "  3. Hacer deploy de la API" -ForegroundColor White
Write-Host ""
Write-Host "IMPORTANTE: Guarda la contraseña del SQL Server de forma segura!" -ForegroundColor Red
Write-Host ""

