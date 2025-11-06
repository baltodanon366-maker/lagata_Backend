# 🔐 Guía: Configurar Secrets de GitHub para Azure Deployment

## ❌ Problema

El deployment falla con el error:
```
Error: No credentials found. Add an Azure login action before this action.
```

## ✅ Solución

Necesitas configurar los **Secrets** en GitHub para que el workflow pueda autenticarse con Azure.

### Opción 1: Usar Publish Profile (Más Simple) ⭐ Recomendado

Si ya tienes el **Publish Profile** de Azure:

1. **Obtén el Publish Profile:**
   - Ve a Azure Portal → Tu App Service → **"Get publish profile"**
   - Descarga el archivo `.PublishSettings`

2. **Configura el Secret en GitHub:**
   - Ve a tu repositorio en GitHub
   - Settings → **Secrets and variables** → **Actions**
   - Click en **"New repository secret"**
   - **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - **Value**: Copia TODO el contenido del archivo `.PublishSettings`
   - Click **"Add secret"**

3. **Verifica que el workflow está correcto:**
   - El workflow ya está configurado para usar `publish-profile`
   - Si el secret no existe, el workflow usará login con credenciales (Opción 2)

---

### Opción 2: Usar Service Principal (Más Seguro)

Si prefieres usar un Service Principal de Azure:

1. **Crea un Service Principal en Azure:**
   ```bash
   az ad sp create-for-rbac --name "LicoreriaAPI-GitHubActions" \
     --role contributor \
     --scopes /subscriptions/{SUBSCRIPTION_ID}/resourceGroups/{RESOURCE_GROUP} \
     --sdk-auth
   ```

2. **Copia la salida JSON** (se verá algo así):
   ```json
   {
     "clientId": "...",
     "clientSecret": "...",
     "subscriptionId": "...",
     "tenantId": "..."
   }
   ```

3. **Configura el Secret en GitHub:**
   - Ve a tu repositorio → Settings → Secrets → Actions
   - Click en **"New repository secret"**
   - **Name**: `AZURE_CREDENTIALS`
   - **Value**: Pega TODO el JSON completo (sin formato, una sola línea)
   - Click **"Add secret"**

4. **Actualiza el workflow** (ya está hecho, pero verifica):
   - El workflow ahora incluye el paso `Azure Login` que usa `AZURE_CREDENTIALS`

---

## 📋 Secrets Requeridos

### Para Publish Profile (Opción 1):
- ✅ `AZURE_WEBAPP_PUBLISH_PROFILE` - Contenido del archivo .PublishSettings

### Para Service Principal (Opción 2):
- ✅ `AZURE_CREDENTIALS` - JSON con credenciales del service principal

### Ambos métodos también necesitan:
- ✅ `AZURE_WEBAPP_NAME` - Nombre de tu App Service (ya configurado en el workflow como `licoreria-api`)

---

## 🔍 Verificar Configuración

1. **Ve a tu repositorio en GitHub:**
   - Settings → Secrets and variables → Actions
   - Deberías ver al menos uno de estos secrets configurados

2. **Verifica el nombre del App Service:**
   - El workflow usa: `AZURE_WEBAPP_NAME: licoreria-api`
   - Si tu App Service tiene otro nombre, actualiza el workflow o agrega el secret

---

## 🚀 Después de Configurar

1. Haz un nuevo commit o push a `main`/`master`
2. El workflow debería ejecutarse automáticamente
3. Ve a Actions → Deberías ver el deployment ejecutándose

---

## 📝 Notas

- **Publish Profile** es más simple pero menos flexible
- **Service Principal** es más seguro y permite más control
- El workflow está configurado para usar ambos métodos (prioriza credentials si existe, sino usa publish-profile)

---

**¿Necesitas ayuda?** Verifica que:
1. El secret está configurado correctamente
2. El nombre del App Service coincide
3. El App Service existe en Azure
4. Tienes permisos para desplegar en ese App Service

