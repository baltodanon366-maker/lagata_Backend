# 🚀 Guía: Deployment a Producción

## ✅ ¿Funcionará el Login en Producción?

**Sí, funcionará**, PERO necesitas asegurarte de que:

1. ✅ **Las Connection Strings estén configuradas en Azure App Service**
2. ✅ **Los hashes de contraseñas estén actualizados en la base de datos** (ya lo hiciste)
3. ✅ **Los JWT Settings estén configurados** (para generar tokens)

---

## 📋 Verificación Pre-Deployment

### 1. Hashes de Contraseñas ✅

Ya ejecutaste `UpdatePasswordHashes.sql`, así que los usuarios tienen hashes BCrypt reales. **Esto funciona igual en producción** porque usas la misma base de datos.

### 2. Connection Strings ⚠️

**Necesitas configurarlas en Azure App Service** porque:
- `appsettings.Production.json` tiene las connection strings vacías (intencionalmente)
- ASP.NET Core en Azure lee las connection strings desde **App Settings** o **Connection Strings** de Azure

---

## 🔧 Configurar Azure App Service

### Opción 1: Usar el Script (Recomendado)

Ejecuta el script que ya tienes configurado:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\configurar-appsettings-fix.ps1
```

Este script configura:
- ✅ `ConnectionStrings__SqlServerConnection`
- ✅ `ConnectionStrings__DataWarehouseConnection`
- ✅ `JwtSettings__SecretKey`
- ✅ `JwtSettings__Issuer`
- ✅ `JwtSettings__Audience`
- ✅ `JwtSettings__ExpirationMinutes`
- ✅ `MongoDBSettings__DatabaseName`
- ✅ `ASPNETCORE_URLS` (puerto 8080)
- ✅ `ASPNETCORE_ENVIRONMENT` (Production)

### Opción 2: Manual desde Azure Portal

1. Ve a **Azure Portal** → Tu App Service (`api-lagata`)
2. **Configuration** → **Application settings**
3. Agrega estas **App Settings**:

   | Name | Value |
   |------|-------|
   | `ConnectionStrings__SqlServerConnection` | `Server=tcp:sqlserverjuan123.database.windows.net,1433;Database=dbLicoreriaLaGata;User ID=adminjuan;Password=LicoreriaLaGata2025!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` |
   | `ConnectionStrings__DataWarehouseConnection` | `Server=tcp:sqlserverjuan123.database.windows.net,1433;Database=dbLicoreriaDW;User ID=adminjuan;Password=LicoreriaLaGata2025!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` |
   | `JwtSettings__SecretKey` | `LicoreriaLaGata2025SuperSecretKeyForJWTTokenGenerationMin32Chars` |
   | `JwtSettings__Issuer` | `LicoreriaAPI` |
   | `JwtSettings__Audience` | `LicoreriaAPIUsers` |
   | `JwtSettings__ExpirationMinutes` | `60` |
   | `MongoDBSettings__DatabaseName` | `LicoreriaMongoDB` |
   | `ASPNETCORE_URLS` | `http://+:8080` |
   | `ASPNETCORE_ENVIRONMENT` | `Production` |

4. Click en **Save**
5. Azure reiniciará automáticamente la aplicación

---

## 🔍 Verificar que Funciona

### 1. Verificar App Settings en Azure

```powershell
az webapp config appsettings list `
    --resource-group "RG Licoreria" `
    --name "api-lagata" `
    --query "[?contains(name, 'Connection') || contains(name, 'Jwt')]" `
    --output table
```

### 2. Probar Login en Producción

```bash
curl -X 'POST' \
  'https://api-lagata-f2afdpf8cqcngrbm.canadacentral-01.azurewebsites.net/api/Auth/login' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "nombreUsuario": "admin",
  "password": "Admin123!"
}'
```

### 3. Verificar Swagger

Abre en tu navegador:
```
https://api-lagata-f2afdpf8cqcngrbm.canadacentral-01.azurewebsites.net/swagger
```

---

## ⚠️ Importante

### Los Hashes de Contraseñas

Los hashes que actualizaste en la base de datos **funcionan tanto en local como en producción** porque:
- ✅ Usas la **misma base de datos** (`dbLicoreriaLaGata`)
- ✅ Los usuarios están en la misma tabla
- ✅ BCrypt verifica igual en cualquier entorno

### Connection Strings

**SÍ necesitas configurarlas en Azure** porque:
- `appsettings.Production.json` tiene strings vacías (por seguridad)
- Azure App Service lee desde **App Settings** o **Connection Strings**
- Si no están configuradas, la API no podrá conectarse a la base de datos

---

## 📝 Resumen

**Para que funcione en producción:**

1. ✅ Hashes de contraseñas → **Ya están actualizados** (misma BD)
2. ⚠️ Connection Strings → **Ejecuta el script de configuración**
3. ⚠️ JWT Settings → **Ejecuta el script de configuración**
4. ✅ Deploy → **Haz el deploy manual o automático**

**Después del deploy y configuración, el login funcionará igual que en local.**

---

## 🚀 Orden Recomendado

1. **Ejecutar script de configuración** (antes o después del deploy)
2. **Hacer deploy manual** usando `deploy-to-existing-webapp.ps1`
3. **Verificar** que el login funciona
4. **Configurar secrets de GitHub** para futuros deploys automáticos

