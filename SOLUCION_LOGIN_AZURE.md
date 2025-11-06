# 🔧 Solución: Error 401 en Login en Azure

## ❌ Error Actual

```
401 Unauthorized
{
  "message": "Credenciales inválidas"
}
```

## 🔍 Diagnóstico

El error "Credenciales inválidas" puede deberse a varias causas:

### 1. ⚠️ Hashes de Contraseñas No Actualizados (Más Probable)

**Problema:** Los usuarios en la base de datos todavía tienen hashes placeholder (`PLACEHOLDER_HASH_ADMIN123`) en lugar de hashes BCrypt reales.

**Solución:**
1. Conéctate a Azure SQL Server
2. Ejecuta el script: `scripts/database/UpdatePasswordHashes.sql`
3. Verifica con: `scripts/database/VerificarUsuarios.sql`

**Verificación rápida:**
```sql
SELECT [NombreUsuario], LEFT([PasswordHash], 30) AS HashPreview
FROM [Usuarios]
WHERE [NombreUsuario] = 'admin';
```

Si ves `PLACEHOLDER_HASH_ADMIN123`, necesitas actualizar el hash.

---

### 2. ⚠️ Connection String No Configurada

**Problema:** La API no puede conectarse a la base de datos porque falta la connection string.

**Solución:**
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\configurar-appsettings-fix.ps1
```

**Verificación:**
```powershell
az webapp config appsettings list `
    --resource-group "RG Licoreria" `
    --name "api-lagata" `
    --query "[?name=='ConnectionStrings__SqlServerConnection']" `
    --output table
```

---

### 3. ⚠️ Usuario No Existe o Inactivo

**Problema:** El usuario no existe en la base de datos o está marcado como inactivo.

**Verificación:**
```sql
SELECT [Id], [NombreUsuario], [Activo], [Rol]
FROM [Usuarios]
WHERE [NombreUsuario] = 'admin';
```

**Solución:** Si no existe, ejecuta `InsertTestData.sql` y luego `UpdatePasswordHashes.sql`.

---

### 4. ⚠️ Error en BCrypt Verification

**Problema:** Hay un error al verificar el hash BCrypt (excepción capturada).

**Verificación:** Revisa los logs de Azure:
```powershell
az webapp log tail --resource-group "RG Licoreria" --name "api-lagata"
```

Busca errores relacionados con:
- `BCrypt`
- `VerifyPassword`
- `LoginAsync`
- `SqlException`

---

## 🔧 Pasos para Resolver

### Paso 1: Verificar Hashes en la Base de Datos

Ejecuta en Azure SQL Server:
```sql
-- Verificar estado de los hashes
SELECT 
    [NombreUsuario],
    CASE 
        WHEN [PasswordHash] LIKE 'PLACEHOLDER%' THEN '❌ Necesita actualización'
        WHEN [PasswordHash] LIKE '$2a$12$%' THEN '✅ Hash BCrypt válido'
        ELSE '⚠️ Hash desconocido'
    END AS [Estado],
    LEFT([PasswordHash], 50) + '...' AS [HashPreview]
FROM [Usuarios];
```

### Paso 2: Actualizar Hashes (Si es Necesario)

Si los hashes son placeholders, ejecuta:
```sql
-- Ejecuta: scripts/database/UpdatePasswordHashes.sql
UPDATE [Usuarios]
SET [PasswordHash] = '$2a$12$cVT0CpbTFVrzhIjwGARLT.OhWDlwuaN1QE0gb/cs17Nqk.P75MX3K'
WHERE [NombreUsuario] = 'admin';

UPDATE [Usuarios]
SET [PasswordHash] = '$2a$12$bIkYA.iBd0nOQVxy1vKKceNULcnNIWt4wqxV83FbXqnGowIzemXV6'
WHERE [NombreUsuario] = 'vendedor1';

UPDATE [Usuarios]
SET [PasswordHash] = '$2a$12$NdKZ8iT/xc/lGQH6idRTROAh4LphaU76uCRIpcBSHm0c/d6guJw6.'
WHERE [NombreUsuario] = 'supervisor1';
```

### Paso 3: Verificar Connection Strings

```powershell
# Verificar
az webapp config appsettings list `
    --resource-group "RG Licoreria" `
    --name "api-lagata" `
    --query "[?contains(name, 'Connection')]" `
    --output table

# Si falta, configurar
powershell -ExecutionPolicy Bypass -File .\scripts\configurar-appsettings-fix.ps1
```

### Paso 4: Revisar Logs de Azure

```powershell
# Ver logs en tiempo real
az webapp log tail --resource-group "RG Licoreria" --name "api-lagata"

# O descargar logs
az webapp log download --resource-group "RG Licoreria" --name "api-lagata" --log-file logs.zip
```

Busca errores específicos:
- `Error en LoginAsync`
- `BCrypt verification failed`
- `Connection failed`
- `SqlException`

### Paso 5: Reiniciar App Service

Después de actualizar hashes o connection strings:
```powershell
az webapp restart --resource-group "RG Licoreria" --name "api-lagata"
```

---

## 🧪 Probar Login

### Opción 1: cURL
```bash
curl -X 'POST' \
  'https://api-lagata-f2afdpf8cqcngrbm.canadacentral-01.azurewebsites.net/api/Auth/login' \
  -H 'Content-Type: application/json' \
  -d '{"nombreUsuario": "admin", "password": "Admin123!"}'
```

### Opción 2: Swagger
1. Abre: `https://api-lagata-f2afdpf8cqcngrbm.canadacentral-01.azurewebsites.net/swagger`
2. Busca `POST /api/Auth/login`
3. Prueba con:
   - `nombreUsuario`: `admin`
   - `password`: `Admin123!`

### Opción 3: Script PowerShell
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\debug-login-azure.ps1
```

---

## 📋 Checklist de Verificación

Antes de probar el login, verifica:

- [ ] ✅ Hashes actualizados en Azure SQL (ejecutar `UpdatePasswordHashes.sql`)
- [ ] ✅ Connection Strings configuradas en Azure App Service
- [ ] ✅ JWT Settings configurados en Azure App Service
- [ ] ✅ Usuario `admin` existe y está activo (`Activo = 1`)
- [ ] ✅ App Service reiniciado después de cambios
- [ ] ✅ Logs revisados para errores específicos

---

## 🔍 Scripts de Diagnóstico

1. **Verificar usuarios en BD:**
   ```sql
   -- scripts/database/VerificarUsuarios.sql
   ```

2. **Verificar configuración Azure:**
   ```powershell
   # scripts/verificar-login-azure.ps1
   ```

3. **Debug login:**
   ```powershell
   # scripts/debug-login-azure.ps1
   ```

---

## 💡 Nota Importante

**Los hashes que actualizaste en local funcionan en producción** porque:
- ✅ Es la **misma base de datos** (`dbLicoreriaLaGata`)
- ✅ Los usuarios están en la **misma tabla**
- ✅ BCrypt verifica igual en cualquier entorno

**Si el login funciona en local pero no en producción, el problema es:**
- ❌ Hashes no actualizados en la BD (más probable)
- ❌ Connection String mal configurada
- ❌ Error en los logs que no estamos viendo

---

**Siguiente paso:** Ejecuta `VerificarUsuarios.sql` en Azure SQL para confirmar si los hashes están actualizados.

