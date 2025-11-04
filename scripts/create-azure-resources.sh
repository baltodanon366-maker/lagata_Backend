#!/bin/bash

# Script para crear todos los recursos de Azure necesarios
# Uso: ./scripts/create-azure-resources.sh

set -e

# Variables de configuración
RESOURCE_GROUP="licoreria-rg"
LOCATION="eastus"
APP_SERVICE_PLAN="licoreria-plan"
WEB_APP_NAME="licoreria-api"
SQL_SERVER_NAME="licoreria-sql-server"
SQL_ADMIN_USER="sqladmin"
SQL_DB_NAME="LicoreriaDB"
COSMOS_ACCOUNT="licoreria-cosmos"
COSMOS_DB="LicoreriaMongoDB"
KEY_VAULT_NAME="licoreria-keyvault"

echo "🚀 Creando recursos de Azure para Licoreria API..."
echo "Resource Group: $RESOURCE_GROUP"
echo "Location: $LOCATION"

# Verificar login
echo "🔐 Verificando login en Azure..."
az account show > /dev/null 2>&1 || {
    echo "❌ No estás logueado en Azure. Ejecuta: az login"
    exit 1
}

# Crear Resource Group
echo "📦 Creando Resource Group..."
az group create \
    --name $RESOURCE_GROUP \
    --location $LOCATION

# Crear App Service Plan
echo "📦 Creando App Service Plan..."
az appservice plan create \
    --name $APP_SERVICE_PLAN \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku B1 \
    --is-linux

# Crear Web App
echo "📦 Creando Web App..."
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $WEB_APP_NAME \
    --runtime "DOTNET|8.0"

# Configurar puerto 8080
echo "⚙️ Configurando Web App..."
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --settings ASPNETCORE_URLS="http://+:8080" \
    ASPNETCORE_ENVIRONMENT="Production"

# Crear SQL Server (se solicitará contraseña)
echo "📦 Creando Azure SQL Server..."
read -sp "Ingresa la contraseña para SQL Server admin: " SQL_PASSWORD
echo ""
az sql server create \
    --name $SQL_SERVER_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $SQL_ADMIN_USER \
    --admin-password $SQL_PASSWORD

# Crear Firewall Rule para Azure Services
echo " firewall rule..."
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

# Crear SQL Database
echo "📦 Creando SQL Database..."
az sql db create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name $SQL_DB_NAME \
    --service-objective S0

# Crear Cosmos DB Account
echo "📦 Creando Cosmos DB Account..."
az cosmosdb create \
    --name $COSMOS_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --kind MongoDB

# Crear Cosmos DB Database
echo "📦 Creando Cosmos DB Database..."
az cosmosdb mongodb database create \
    --account-name $COSMOS_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --name $COSMOS_DB

# Crear Key Vault
echo "📦 Creando Key Vault..."
az keyvault create \
    --name $KEY_VAULT_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION

echo ""
echo "✅ Recursos creados exitosamente!"
echo ""
echo "📋 Resumen de recursos:"
echo "  - Resource Group: $RESOURCE_GROUP"
echo "  - Web App: $WEB_APP_NAME"
echo "  - SQL Server: $SQL_SERVER_NAME"
echo "  - SQL Database: $SQL_DB_NAME"
echo "  - Cosmos DB: $COSMOS_ACCOUNT"
echo "  - Key Vault: $KEY_VAULT_NAME"
echo ""
echo "📝 Próximos pasos:"
echo "  1. Configurar connection strings en Key Vault"
echo "  2. Configurar App Settings en Web App"
echo "  3. Ejecutar migraciones de base de datos"
echo "  4. Desplegar la aplicación"


