#!/bin/bash

# Script de deployment para Azure
# Uso: ./scripts/deploy-azure.sh [environment]

set -e

ENVIRONMENT=${1:-production}
AZURE_WEBAPP_NAME=${AZURE_WEBAPP_NAME:-licoreria-api}
RESOURCE_GROUP=${RESOURCE_GROUP:-licoreria-rg}

echo "🚀 Iniciando deployment a Azure..."
echo "Environment: $ENVIRONMENT"
echo "Web App: $AZURE_WEBAPP_NAME"
echo "Resource Group: $RESOURCE_GROUP"

# Verificar que Azure CLI esté instalado
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI no está instalado. Por favor instálalo primero."
    exit 1
fi

# Verificar login
echo "🔐 Verificando login en Azure..."
az account show > /dev/null 2>&1 || {
    echo "❌ No estás logueado en Azure. Ejecuta: az login"
    exit 1
}

# Build y publish
echo "📦 Compilando aplicación..."
dotnet restore LicoreriaAPI.sln
dotnet build LicoreriaAPI.sln --configuration Release --no-restore
dotnet publish src/LicoreriaAPI/LicoreriaAPI.csproj --configuration Release --output ./publish

# Deploy a Azure App Service
echo "☁️ Desplegando a Azure App Service..."
az webapp deploy \
    --resource-group $RESOURCE_GROUP \
    --name $AZURE_WEBAPP_NAME \
    --type zip \
    --src-path ./publish

echo "✅ Deployment completado exitosamente!"
echo "🌐 URL: https://$AZURE_WEBAPP_NAME.azurewebsites.net"


