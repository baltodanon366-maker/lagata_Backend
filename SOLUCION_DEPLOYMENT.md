# 🔧 Solución: Deployment desde GitHub Actions a Azure

## 🔍 Diagnóstico del Problema

**Situación actual:**
- ✅ El commit y push en GitHub funcionaron correctamente
- ❌ GitHub Actions está intentando hacer deploy pero **falla** por falta de credenciales
- ⚠️ Azure muestra "CI/CD no está configurado" porque GitHub Actions no puede autenticarse
- 📅 El último deploy exitoso en Azure es del **4 de noviembre** (probablemente fue manual)

**El problema:**
El workflow de GitHub Actions necesita **credenciales** para conectarse a Azure, pero no están configuradas en los Secrets de GitHub.

---

## ✅ Solución: Configurar Secrets en GitHub

### Paso 1: Obtener el Publish Profile de Azure

1. Ve a **Azure Portal** → Tu App Service (`api-lagata`)
2. En el menú lateral, busca **"Get publish profile"** o **"Obtener perfil de publicación"**
3. **Descarga** el archivo `.PublishSettings`
4. **Abre** el archivo con un editor de texto (Notepad, VS Code, etc.)
5. **Copia TODO** el contenido del archivo

### Paso 2: Agregar el Secret en GitHub

1. Ve a tu **repositorio en GitHub**
2. Click en **Settings** (Configuración)
3. En el menú lateral, click en **"Secrets and variables"** → **"Actions"**
4. Click en **"New repository secret"** (Nuevo secreto del repositorio)
5. **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE`
6. **Value**: Pega **TODO** el contenido del archivo `.PublishSettings` que copiaste
7. Click en **"Add secret"**

### Paso 3: Verificar el Nombre del App Service

El workflow ya está actualizado para usar el nombre correcto: **`api-lagata`**

Si tu App Service tiene otro nombre, actualiza la línea 14 del archivo `.github/workflows/azure-deploy.yml`:

```yaml
env:
  AZURE_WEBAPP_NAME: TU_NOMBRE_APP_SERVICE_AQUI
```

---

## 🚀 Después de Configurar

1. **Haz un nuevo commit** (o simplemente haz un push vacío):
   ```bash
   git commit --allow-empty -m "Trigger deployment"
   git push origin main
   ```

2. **Ve a GitHub Actions:**
   - En tu repositorio → Pestaña **"Actions"**
   - Deberías ver el workflow ejecutándose
   - Espera a que termine el build y luego el deploy

3. **Verifica en Azure:**
   - Ve a Azure Portal → Tu App Service → **"Deployment Center"** o **"Centro de implementación"**
   - Deberías ver el nuevo deployment

---

## 🔍 Verificar que Funciona

### En GitHub Actions:
- ✅ El job `build` debería completarse exitosamente
- ✅ El job `deploy` debería completarse exitosamente
- ❌ Si falla, revisa los logs para ver el error específico

### En Azure Portal:
- Ve a tu App Service → **"Deployment Center"** o **"Implementación"**
- Deberías ver el nuevo deployment con fecha/hora actual
- El status debería ser **"Success"** o **"Correcto"**

---

## 📝 Notas Importantes

1. **El deploy de ayer probablemente fue manual:**
   - Tal vez usaste Visual Studio, VS Code, o algún script local
   - Por eso funcionó sin necesidad de secrets en GitHub

2. **GitHub Actions necesita autenticación:**
   - A diferencia de un deploy manual, GitHub Actions necesita credenciales
   - El Publish Profile es la forma más simple de proporcionarlas

3. **El nombre del App Service:**
   - Ya está corregido en el workflow: `api-lagata`
   - Si tu App Service tiene otro nombre, actualízalo

---

## ❓ Problemas Comunes

### Error: "No credentials found"
**Solución:** Asegúrate de que el secret `AZURE_WEBAPP_PUBLISH_PROFILE` está configurado correctamente

### Error: "App not found"
**Solución:** Verifica que el nombre del App Service en el workflow coincide con el nombre real en Azure

### El workflow no se ejecuta
**Solución:** Verifica que estás haciendo push a la rama `main` o `master`

### El deploy falla sin error claro
**Solución:** Revisa los logs completos en GitHub Actions para ver el error específico

---

**¿Necesitas más ayuda?** Revisa los logs de GitHub Actions para ver el error específico que está ocurriendo.

