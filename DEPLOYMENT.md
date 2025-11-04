# Guía de Deployment en Azure

Esta guía te ayudará a desplegar la API Licoreria en Azure App Service.

## 📋 Prerrequisitos

1. **Azure CLI** instalado y configurado
   ```bash
   # Instalar Azure CLI
   # Windows: https://aka.ms/installazurecliwindows
   # macOS: brew install azure-cli
   # Linux: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
   
   # Login
   az login
   ```

2. **.NET 8 SDK** instalado
3. **Cuenta de Azure** con suscripción activa
4. **Docker** (opcional, para containerización)

## 🏗️ Recursos de Azure Necesarios

### 1. Azure App Service (Web App)
- Plan de App Service (Linux o Windows)
- Web App con .NET 8 runtime

### 2. Azure SQL Database
- Base de datos SQL para datos transaccionales
- **O** SQL Server en VM (si prefieres IaaS)

### 3. Azure Cosmos DB (MongoDB API)
- Cosmos DB con API de MongoDB
- **O** MongoDB en Azure VM o MongoDB Atlas

### 4. Azure Key Vault (Recomendado)
- Para almacenar secretos y cadenas de conexión de forma segura

## 🚀 Deployment Rápido

### Opción 1: Deployment Manual con Scripts

#### Windows (PowerShell):
```powershell
# Configurar variables de entorno
$env:AZURE_WEBAPP_NAME = "licoreria-api"
$env:RESOURCE_GROUP = "licoreria-rg"

# Ejecutar script de deployment
.\scripts\deploy-azure.ps1
```

#### Linux/macOS:
```bash
# Configurar variables de entorno
export AZURE_WEBAPP_NAME="licoreria-api"
export RESOURCE_GROUP="licoreria-rg"

# Dar permisos de ejecución
chmod +x scripts/deploy-azure.sh

# Ejecutar script
./scripts/deploy-azure.sh
```

### Opción 2: Deployment con Azure CLI

```bash
# 1. Crear Resource Group
az group create --name licoreria-rg --location eastus

# 2. Crear App Service Plan
az appservice plan create \
    --name licoreria-plan \
    --resource-group licoreria-rg \
    --sku B1 \
    --is-linux

# 3. Crear Web App
az webapp create \
    --resource-group licoreria-rg \
    --plan licoreria-plan \
    --name licoreria-api \
    --runtime "DOTNET|8.0"

# 4. Compilar y publicar
dotnet publish src/LicoreriaAPI/LicoreriaAPI.csproj -c Release -o ./publish

# 5. Desplegar
az webapp deploy \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --type zip \
    --src-path ./publish
```

### Opción 3: Deployment con Docker

```bash
# 1. Build de la imagen
docker build -t licoreria-api:latest .

# 2. Tag para Azure Container Registry
docker tag licoreria-api:latest <registry>.azurecr.io/licoreria-api:latest

# 3. Login a ACR
az acr login --name <registry>

# 4. Push de la imagen
docker push <registry>.azurecr.io/licoreria-api:latest

# 5. Configurar Web App para usar Docker
az webapp config container set \
    --name licoreria-api \
    --resource-group licoreria-rg \
    --docker-custom-image-name <registry>.azurecr.io/licoreria-api:latest
```

## ⚙️ Configuración de Variables de Entorno en Azure

### Configurar Connection Strings

```bash
# SQL Server Connection String
az webapp config connection-string set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --connection-string-type SQLServer \
    --settings SqlServerConnection="Server=tcp:<server>.database.windows.net,1433;Initial Catalog=LicoreriaDB;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# MongoDB Connection String
az webapp config appsettings set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --settings ConnectionStrings__MongoDBConnection="mongodb://<connection-string>"
```

### Configurar App Settings

```bash
az webapp config appsettings set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --settings \
        JwtSettings__SecretKey="<your-secret-key-min-32-chars>" \
        JwtSettings__Issuer="LicoreriaAPI" \
        JwtSettings__Audience="LicoreriaAPIUsers" \
        JwtSettings__ExpirationMinutes="60" \
        MongoDBSettings__DatabaseName="LicoreriaMongoDB" \
        ASPNETCORE_ENVIRONMENT="Production"
```

### Usar Azure Key Vault (Recomendado)

```bash
# 1. Crear Key Vault
az keyvault create \
    --name licoreria-keyvault \
    --resource-group licoreria-rg \
    --location eastus

# 2. Agregar secretos
az keyvault secret set \
    --vault-name licoreria-keyvault \
    --name "SqlServerConnection" \
    --value "<connection-string>"

# 3. Configurar acceso desde Web App
az webapp identity assign \
    --name licoreria-api \
    --resource-group licoreria-rg

# 4. Dar permisos al Web App
az keyvault set-policy \
    --name licoreria-keyvault \
    --object-id <web-app-identity-id> \
    --secret-permissions get list

# 5. Referenciar en App Settings
az webapp config appsettings set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --settings \
        SqlServerConnection="@Microsoft.KeyVault(SecretUri=https://licoreria-keyvault.vault.azure.net/secrets/SqlServerConnection/)"
```

## 🔄 CI/CD con GitHub Actions

1. **Configurar Secret en GitHub:**
   - Ve a Settings → Secrets → Actions
   - Agrega `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Obtén el publish profile desde Azure Portal:
     ```bash
     az webapp deployment list-publishing-profiles \
         --name licoreria-api \
         --resource-group licoreria-rg \
         --xml
     ```

2. **El pipeline se ejecutará automáticamente** cuando hagas push a `main` o `master`

## 🔄 CI/CD con Azure DevOps

1. **Crear Service Connection:**
   - Azure DevOps → Project Settings → Service connections
   - Crear nueva conexión de Azure Resource Manager

2. **Configurar Pipeline:**
   - El archivo `azure-pipelines.yml` está listo
   - Ajusta las variables según tu configuración

## 🗄️ Configuración de Bases de Datos

### Azure SQL Database

```bash
# 1. Crear SQL Server
az sql server create \
    --name licoreria-sql-server \
    --resource-group licoreria-rg \
    --location eastus \
    --admin-user sqladmin \
    --admin-password <strong-password>

# 2. Crear Firewall Rule (para acceso desde Azure)
az sql server firewall-rule create \
    --resource-group licoreria-rg \
    --server licoreria-sql-server \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

# 3. Crear Database
az sql db create \
    --resource-group licoreria-rg \
    --server licoreria-sql-server \
    --name LicoreriaDB \
    --service-objective S0
```

### Azure Cosmos DB (MongoDB API)

```bash
# 1. Crear Cosmos DB Account
az cosmosdb create \
    --name licoreria-cosmos \
    --resource-group licoreria-rg \
    --kind MongoDB

# 2. Crear Database
az cosmosdb mongodb database create \
    --account-name licoreria-cosmos \
    --resource-group licoreria-rg \
    --name LicoreriaMongoDB
```

## 📊 Application Insights (Opcional)

```bash
# 1. Crear Application Insights
az monitor app-insights component create \
    --app licoreria-insights \
    --location eastus \
    --resource-group licoreria-rg

# 2. Obtener Instrumentation Key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
    --app licoreria-insights \
    --resource-group licoreria-rg \
    --query instrumentationKey -o tsv)

# 3. Configurar en Web App
az webapp config appsettings set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --settings \
        APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY
```

## 🔒 Seguridad

### Habilitar HTTPS

```bash
az webapp update \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --https-only true
```

### Configurar CORS

```bash
az webapp cors add \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --allowed-origins "https://your-frontend.azurewebsites.net"
```

## 📝 Checklist de Deployment

- [ ] Azure CLI instalado y configurado
- [ ] Resource Group creado
- [ ] App Service Plan creado
- [ ] Web App creada
- [ ] Azure SQL Database configurada
- [ ] Azure Cosmos DB (MongoDB) configurada
- [ ] Connection Strings configuradas
- [ ] App Settings configuradas
- [ ] Key Vault configurado (opcional pero recomendado)
- [ ] Application Insights configurado (opcional)
- [ ] CI/CD pipeline configurado
- [ ] HTTPS habilitado
- [ ] CORS configurado
- [ ] Migraciones de base de datos ejecutadas
- [ ] Pruebas de endpoints realizadas

## 🐛 Troubleshooting

### Ver logs de la aplicación

```bash
az webapp log tail \
    --resource-group licoreria-rg \
    --name licoreria-api
```

### Verificar configuración

```bash
az webapp config show \
    --resource-group licoreria-rg \
    --name licoreria-api
```

### Reiniciar aplicación

```bash
az webapp restart \
    --resource-group licoreria-rg \
    --name licoreria-api
```

## 📚 Referencias

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/sql-database/)
- [Azure Cosmos DB Documentation](https://docs.microsoft.com/azure/cosmos-db/)
- [.NET on Azure](https://dotnet.microsoft.com/apps/azure)


