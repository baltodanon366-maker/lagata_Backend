# 🚀 Guía de Deployment Inicial - Licoreria API

## 📋 ¿Cuándo hacer el deployment?

**✅ RECOMENDACIÓN: Subir el proyecto AHORA**

### Ventajas de subirlo ahora:
1. ✅ **Probar la infraestructura**: Verificar que Azure, SQL y MongoDB funcionan correctamente
2. ✅ **Detectar problemas temprano**: Identificar issues de configuración antes de tener código complejo
3. ✅ **CI/CD funcionando**: Cada cambio futuro se desplegará automáticamente
4. ✅ **Base sólida**: Tener la estructura base funcionando en producción da confianza
5. ✅ **Testing continuo**: Puedes probar cada feature conforme la vas desarrollando

### Desventajas de esperar:
- ❌ Más difícil debuggear problemas de deployment con código complejo
- ❌ Pueden surgir problemas de configuración que retrasen el desarrollo
- ❌ No tendrás un ambiente de producción para probar desde el inicio

## 🔄 ¿Cómo actualizar el proyecto después?

Una vez configurado el CI/CD, **los cambios se actualizan automáticamente**:

### Opción 1: GitHub Actions (Recomendado - Más fácil)
Cada vez que hagas `git push` a la rama `main`, se despliega automáticamente:

```bash
# 1. Hacer cambios en tu código
# 2. Commit y push
git add .
git commit -m "Nueva funcionalidad: módulo de ventas"
git push origin main

# 3. ¡Listo! GitHub Actions desplegará automáticamente
# Puedes ver el progreso en: https://github.com/tu-usuario/tu-repo/actions
```

### Opción 2: Azure DevOps Pipeline
Similar, pero usando Azure DevOps:

```bash
git push origin main
# El pipeline se ejecuta automáticamente
```

### Opción 3: Deployment Manual (Solo para pruebas)
Si necesitas hacer un deployment manual sin CI/CD:

```bash
# Windows
.\scripts\deploy-azure.ps1

# Linux/Mac
./scripts/deploy-azure.sh
```

## 📝 Pasos para Deployment Inicial

### Paso 1: Preparar el Repositorio

```bash
# Inicializar Git (si no lo has hecho)
git init

# Agregar todos los archivos
git add .

# Commit inicial
git commit -m "Initial commit: Estructura base de Licoreria API"

# Crear repositorio en GitHub (o usar uno existente)
# Luego:
git remote add origin https://github.com/tu-usuario/licoreria-api.git
git branch -M main
git push -u origin main
```

### Paso 2: Crear Recursos en Azure

#### Opción A: Script Automático (Recomendado)

**Windows:**
```powershell
# Ejecutar script de creación de recursos
.\scripts\create-azure-resources.ps1
```

**Linux/Mac:**
```bash
chmod +x scripts/create-azure-resources.sh
./scripts/create-azure-resources.sh
```

#### Opción B: Manual (Paso a paso)

```bash
# 1. Login en Azure
az login

# 2. Crear Resource Group
az group create --name licoreria-rg --location eastus

# 3. Crear App Service Plan
az appservice plan create \
    --name licoreria-plan \
    --resource-group licoreria-rg \
    --sku B1 \
    --is-linux

# 4. Crear Web App
az webapp create \
    --resource-group licoreria-rg \
    --plan licoreria-plan \
    --name licoreria-api \
    --runtime "DOTNET|8.0"

# 5. Configurar puerto
az webapp config appsettings set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --settings ASPNETCORE_URLS="http://+:8080"
```

### Paso 3: Configurar Bases de Datos

#### Azure SQL Database:
```bash
# Crear SQL Server (te pedirá contraseña)
az sql server create \
    --name licoreria-sql-server \
    --resource-group licoreria-rg \
    --location eastus \
    --admin-user sqladmin \
    --admin-password "TuPasswordSeguro123!"

# Firewall rule para Azure
az sql server firewall-rule create \
    --resource-group licoreria-rg \
    --server licoreria-sql-server \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

# Crear Database
az sql db create \
    --resource-group licoreria-rg \
    --server licoreria-sql-server \
    --name LicoreriaDB \
    --service-objective S0
```

#### Azure Cosmos DB (MongoDB):
```bash
# Crear Cosmos DB
az cosmosdb create \
    --name licoreria-cosmos \
    --resource-group licoreria-rg \
    --kind MongoDB

# Crear Database
az cosmosdb mongodb database create \
    --account-name licoreria-cosmos \
    --resource-group licoreria-rg \
    --name LicoreriaMongoDB
```

### Paso 4: Configurar Variables de Entorno

Obtener connection strings y configurarlas:

```bash
# Obtener SQL Connection String
SQL_CONNECTION=$(az sql db show-connection-string \
    --server licoreria-sql-server \
    --name LicoreriaDB \
    --client ado.net -o tsv)

# Obtener MongoDB Connection String
MONGO_CONNECTION=$(az cosmosdb keys list \
    --name licoreria-cosmos \
    --resource-group licoreria-rg \
    --type connection-strings \
    --query "connectionStrings[0].connectionString" -o tsv)

# Configurar en Web App
az webapp config connection-string set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --connection-string-type SQLServer \
    --settings SqlServerConnection="$SQL_CONNECTION"

az webapp config appsettings set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --settings \
        ConnectionStrings__MongoDBConnection="$MONGO_CONNECTION" \
        JwtSettings__SecretKey="TuClaveSecretaSuperLargaDeAlMenos32Caracteres" \
        JwtSettings__Issuer="LicoreriaAPI" \
        JwtSettings__Audience="LicoreriaAPIUsers" \
        JwtSettings__ExpirationMinutes="60" \
        MongoDBSettings__DatabaseName="LicoreriaMongoDB"
```

### Paso 5: Configurar GitHub Actions (Para CI/CD Automático)

1. **Obtener Publish Profile de Azure:**
   ```bash
   az webapp deployment list-publishing-profiles \
       --name licoreria-api \
       --resource-group licoreria-rg \
       --xml > publish-profile.xml
   ```

2. **Agregar Secret en GitHub:**
   - Ve a tu repositorio en GitHub
   - Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Copia el contenido de `publish-profile.xml`

3. **El pipeline ya está configurado** en `.github/workflows/azure-deploy.yml`

### Paso 6: Deployment Inicial

#### Opción A: Con GitHub Actions (Recomendado)
```bash
# Simplemente haz push
git push origin main

# GitHub Actions hará el deployment automáticamente
# Ver progreso en: https://github.com/tu-usuario/tu-repo/actions
```

#### Opción B: Manual
```bash
# Compilar
dotnet publish src/LicoreriaAPI/LicoreriaAPI.csproj -c Release -o ./publish

# Desplegar
az webapp deploy \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --type zip \
    --src-path ./publish
```

### Paso 7: Verificar Deployment

```bash
# Ver logs en tiempo real
az webapp log tail \
    --resource-group licoreria-rg \
    --name licoreria-api

# Abrir en navegador
# https://licoreria-api.azurewebsites.net
```

## 🔄 Workflow de Desarrollo Futuro

Una vez configurado, tu flujo de trabajo será:

```bash
# 1. Desarrollar localmente
# ... hacer cambios en el código ...

# 2. Probar localmente
dotnet run --project src/LicoreriaAPI/LicoreriaAPI.csproj

# 3. Commit y push
git add .
git commit -m "Descripción de los cambios"
git push origin main

# 4. ¡Automático! GitHub Actions despliega
# Ver en: https://github.com/tu-usuario/tu-repo/actions

# 5. Verificar en producción
# https://licoreria-api.azurewebsites.net
```

## 🎯 Resumen de Comandos Rápidos

### Para deployment inicial:
```bash
# 1. Crear recursos
.\scripts\create-azure-resources.ps1  # Windows
# o
./scripts/create-azure-resources.sh   # Linux/Mac

# 2. Configurar variables (ver Paso 4 arriba)

# 3. Desplegar
git push origin main  # Si tienes CI/CD
# o
.\scripts\deploy-azure.ps1  # Manual
```

### Para actualizaciones futuras:
```bash
# Solo necesitas hacer:
git add .
git commit -m "Tu cambio"
git push origin main

# ¡El resto es automático!
```

## 📚 Recursos Adicionales

- Ver logs: `az webapp log tail --resource-group licoreria-rg --name licoreria-api`
- Reiniciar app: `az webapp restart --resource-group licoreria-rg --name licoreria-api`
- Ver configuración: `az webapp config show --resource-group licoreria-rg --name licoreria-api`

## ❓ Preguntas Frecuentes

**P: ¿Puedo desplegar sin tener todas las funcionalidades?**
R: ¡Sí! Es mejor tener la estructura base funcionando desde el inicio.

**P: ¿Cómo revierto un deployment que falló?**
R: Puedes hacer rollback desde Azure Portal o desplegar una versión anterior con Git.

**P: ¿Los cambios se aplican inmediatamente?**
R: Con CI/CD, el deployment toma 2-5 minutos después del push.

**P: ¿Puedo tener múltiples ambientes (dev, staging, prod)?**
R: Sí, puedes crear múltiples Web Apps y configurar diferentes branches para cada uno.

