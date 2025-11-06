# 🔍 Diagnóstico: Login funciona en Local pero NO en Azure

## ✅ Lo que SABEMOS que está bien:

1. ✅ **Hashes actualizados en Azure SQL** - Ya ejecutaste los UPDATE
2. ✅ **Connection Strings configuradas** - Verificado en Azure
3. ✅ **JWT Settings configuradas** - Verificado en Azure
4. ✅ **Login funciona en local** - Misma BD, mismo código
5. ✅ **App Service reiniciado** - Cambios aplicados

## 🔴 El Problema:

El login funciona en **local** pero NO en **producción (Azure)**, usando:
- ✅ La **misma base de datos** (Azure SQL)
- ✅ Las **mismas credenciales** (`admin` / `Admin123!`)
- ✅ El **mismo código** (debería ser)

## 🔍 Posibles Causas:

### 1. ⚠️ Código Desplegado es Diferente (Más Probable)

**Problema:** El código desplegado en Azure puede ser una versión anterior sin los cambios recientes.

**Solución:**
```powershell
# Asegúrate de hacer deploy del código más reciente
powershell -ExecutionPolicy Bypass -File .\scripts\deploy-to-existing-webapp.ps1
```

### 2. ⚠️ Error Silencioso en BCrypt

**Problema:** BCrypt puede estar fallando silenciosamente en producción.

**Solución:** He mejorado el logging para detectar esto. Los logs ahora mostrarán:
- Si el hash es un placeholder
- Si BCrypt.Verify falla
- El error específico

### 3. ⚠️ Problema de Conexión a la BD

**Problema:** Aunque la connection string está configurada, puede haber un problema de red o firewall.

**Verificación:**
```powershell
# Ver logs de conexión
az webapp log tail --resource-group "RG_Licoreria" --name "api-lagata"
```

Busca errores como:
- `SqlException`
- `Connection timeout`
- `Cannot open database`

### 4. ⚠️ Entorno Production vs Development

**Problema:** El código puede comportarse diferente en `Production` vs `Development`.

**Verificación:**
```powershell
az webapp config appsettings list `
    --resource-group "RG_Licoreria" `
    --name "api-lagata" `
    --query "[?name=='ASPNETCORE_ENVIRONMENT']" `
    --output table
```

---

## 🔧 Solución Implementada:

He mejorado el **logging** en `AuthService.cs` para que ahora registre:

1. ✅ **Cada paso del proceso de login**
2. ✅ **Si el usuario no se encuentra**
3. ✅ **Si la contraseña es inválida**
4. ✅ **Si el hash es un placeholder**
5. ✅ **Errores específicos de BCrypt**
6. ✅ **Stack traces completos**

---

## 📋 Pasos para Resolver:

### Paso 1: Hacer Deploy del Código Mejorado

```powershell
# 1. Commit los cambios
git add .
git commit -m "Mejorar logging en AuthService para diagnóstico de login"
git push

# 2. Deploy manual (mientras configuras GitHub Actions)
powershell -ExecutionPolicy Bypass -File .\scripts\deploy-to-existing-webapp.ps1
```

### Paso 2: Revisar Logs de Azure

```powershell
# Ver logs en tiempo real
az webapp log tail --resource-group "RG_Licoreria" --name "api-lagata"

# O descargar logs
az webapp log download --resource-group "RG_Licoreria" --name "api-lagata" --log-file logs.zip
```

**Busca en los logs:**
- `Intento de login para usuario: admin`
- `Usuario encontrado. Verificando contraseña...`
- `Contraseña inválida` o `BCrypt.Verify retornó false`
- `PasswordHash es un placeholder`
- `Error al verificar contraseña con BCrypt`

### Paso 3: Probar Login de Nuevo

```bash
curl -X 'POST' \
  'https://api-lagata-f2afdpf8cqcngrbm.canadacentral-01.azurewebsites.net/api/Auth/login' \
  -H 'Content-Type: application/json' \
  -d '{"nombreUsuario": "admin", "password": "Admin123!"}'
```

### Paso 4: Analizar los Logs

Los logs ahora te dirán **exactamente** qué está fallando:
- ❌ Si el usuario no se encuentra → Problema de conexión a BD
- ❌ Si el hash es placeholder → Hashes no actualizados (aunque ejecutaste el script)
- ❌ Si BCrypt falla → Problema con la librería BCrypt
- ❌ Si hay excepción → Ver el stack trace completo

---

## 🧪 Verificación Adicional:

### Verificar que los Hashes Están Actualizados:

```sql
-- Ejecuta en Azure SQL
USE [dbLicoreriaLaGata]
GO

SELECT 
    [NombreUsuario],
    LEFT([PasswordHash], 30) AS HashPreview,
    CASE 
        WHEN [PasswordHash] LIKE 'PLACEHOLDER%' THEN '❌ Placeholder'
        WHEN [PasswordHash] LIKE '$2a$12$%' THEN '✅ BCrypt válido'
        ELSE '⚠️ Desconocido'
    END AS Estado,
    LEN([PasswordHash]) AS Longitud
FROM [Usuarios]
WHERE [NombreUsuario] = 'admin';
```

**Debe mostrar:**
- HashPreview: `$2a$12$cVT0CpbTFVrzhIjwGARLT...`
- Estado: `✅ BCrypt válido`
- Longitud: `60` (típico para BCrypt)

### Verificar Connection String en Azure:

```powershell
az webapp config appsettings list `
    --resource-group "RG_Licoreria" `
    --name "api-lagata" `
    --query "[?name=='ConnectionStrings__SqlServerConnection']" `
    --output table
```

**Debe mostrar:**
```
Server=tcp:sqlserverjuan123.database.windows.net,1433;Database=dbLicoreriaLaGata;...
```

---

## 💡 Nota Importante:

**Si el login funciona en local pero NO en producción:**
- ✅ Es la **misma base de datos** → Los hashes están bien
- ✅ Es el **mismo código** → Debería funcionar igual
- ❌ **Algo diferente en el entorno** → Logs lo revelarán

**Los logs mejorados te dirán exactamente qué está pasando.**

---

## 📞 Siguiente Paso:

1. **Haz commit y push** de los cambios de logging
2. **Haz deploy manual**
3. **Intenta login en producción**
4. **Revisa los logs inmediatamente** para ver el error específico
5. **Comparte los logs** si necesitas más ayuda

Los logs ahora son mucho más detallados y te dirán exactamente dónde está fallando el proceso de login.

