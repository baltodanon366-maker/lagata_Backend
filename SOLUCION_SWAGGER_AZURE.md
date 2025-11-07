# 🔧 Solución: Error "Failed to load API definition" en Swagger

## ❌ Error Actual

```
Failed to load API definition
Fetch error
Site Disabled /swagger/v1/swagger.json
```

## 🔍 Diagnóstico

Este error indica que:
1. ⚠️ La aplicación **no está iniciando correctamente** en Azure
2. ⚠️ O hay un problema con la **configuración de Swagger**
3. ⚠️ O la aplicación está **deshabilitada** en Azure

## ✅ Solución Implementada

He actualizado `Program.cs` para:
1. ✅ **Habilitar Swagger siempre** (también en producción)
2. ✅ **Simplificar la configuración** de Swagger UI
3. ✅ **Asegurar que Swagger esté disponible** en la raíz (`/`)

---

## 📋 Pasos para Resolver

### Paso 1: Verificar que el Código Esté Actualizado

El código ya está actualizado. Asegúrate de hacer commit y deploy:

```powershell
# Commit y push
git add .
git commit -m "Corregir configuración de Swagger para Azure"
git push

# Deploy manual
powershell -ExecutionPolicy Bypass -File .\scripts\deploy-to-existing-webapp.ps1
```

### Paso 2: Verificar Estado de la Aplicación

```powershell
# Verificar estado
powershell -ExecutionPolicy Bypass -File .\scripts\verificar-estado-app.ps1
```

Este script probará:
- ✅ Si la aplicación responde en la raíz
- ✅ Si Swagger JSON está disponible
- ✅ Si el endpoint de login funciona

### Paso 3: Revisar Logs de Azure

```powershell
# Ver logs en tiempo real
az webapp log tail --resource-group "RG_Licoreria" --name "api-lagata"
```

**Busca errores como:**
- `Application startup exception`
- `Failed to start application`
- `Connection string not found`
- `JWT settings not configured`

### Paso 4: Verificar Connection Strings

Aunque ya están configuradas, verifica que estén correctas:

```powershell
az webapp config appsettings list `
    --resource-group "RG_Licoreria" `
    --name "api-lagata" `
    --query "[?contains(name, 'Connection')]" `
    --output table
```

---

## 🔍 Posibles Causas Adicionales

### 1. ⚠️ Aplicación No Inicia por Error de Configuración

**Síntoma:** La aplicación no responde en ningún endpoint.

**Solución:** Revisa los logs para ver el error específico de inicio.

### 2. ⚠️ Problema con HTTPS Redirection

**Síntoma:** Errores relacionados con HTTPS.

**Solución:** Ya está configurado `UseHttpsRedirection()`, pero Azure puede requerir configuración adicional.

### 3. ⚠️ Problema con el Puerto

**Síntoma:** La aplicación no escucha en el puerto correcto.

**Solución:** Azure App Service usa el puerto `8080` por defecto. Ya está configurado en `appsettings.json`:
```json
"ASPNETCORE_URLS": "http://+:8080"
```

---

## 🧪 Pruebas Rápidas

### Probar Raíz (Swagger UI)
```
https://api-lagata-f2afdpf8cqcngrbm.canadacentral-01.azurewebsites.net/
```

### Probar Swagger JSON Directamente
```
https://api-lagata-f2afdpf8cqcngrbm.canadacentral-01.azurewebsites.net/swagger/v1/swagger.json
```

### Probar Endpoint de Login
```bash
curl -X POST \
  'https://api-lagata-f2afdpf8cqcngrbm.canadacentral-01.azurewebsites.net/api/Auth/login' \
  -H 'Content-Type: application/json' \
  -d '{"nombreUsuario": "admin", "password": "Admin123!"}'
```

---

## 💡 Nota Importante

**Si la aplicación no inicia:**
- ❌ Swagger no funcionará
- ❌ Ningún endpoint funcionará
- ✅ Los logs te dirán exactamente qué está fallando

**Los logs son tu mejor amigo aquí.** Revisa los logs inmediatamente después del deploy para ver si hay errores de inicio.

---

## 📞 Siguiente Paso

1. **Haz deploy** del código actualizado
2. **Ejecuta** `verificar-estado-app.ps1` para probar los endpoints
3. **Revisa los logs** si algo falla
4. **Comparte los logs** si necesitas más ayuda

El código ahora está configurado para que Swagger funcione correctamente en Azure.

