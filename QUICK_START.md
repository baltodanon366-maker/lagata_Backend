# ⚡ Quick Start - Deployment Rápido

## 🎯 Resumen Ejecutivo

**¿Cuándo desplegar?** → **AHORA** (es mejor tener la base funcionando desde el inicio)

**¿Cómo actualizar después?** → **Automáticamente con Git Push** (CI/CD configurado)

## 🚀 Deployment en 5 Pasos

### 1️⃣ Preparar Repositorio
```bash
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/TU-USUARIO/licoreria-api.git
git push -u origin main
```

### 2️⃣ Crear Recursos Azure
```powershell
# Windows
.\scripts\create-azure-resources.ps1

# Linux/Mac
chmod +x scripts/create-azure-resources.sh
./scripts/create-azure-resources.sh
```

### 3️⃣ Configurar Connection Strings
```bash
# Obtener y configurar SQL
SQL_CONNECTION="Server=tcp:licoreria-sql-server.database.windows.net,1433;Initial Catalog=LicoreriaDB;User ID=sqladmin;Password=TuPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config connection-string set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --connection-string-type SQLServer \
    --settings SqlServerConnection="$SQL_CONNECTION"

# MongoDB (obtener del portal de Azure)
az webapp config appsettings set \
    --resource-group licoreria-rg \
    --name licoreria-api \
    --settings \
        ConnectionStrings__MongoDBConnection="mongodb://..." \
        JwtSettings__SecretKey="TuClaveSecretaDeAlMenos32Caracteres" \
        MongoDBSettings__DatabaseName="LicoreriaMongoDB"
```

### 4️⃣ Configurar GitHub Actions
- GitHub → Settings → Secrets → Actions
- Agregar: `AZURE_WEBAPP_PUBLISH_PROFILE`
- Obtener valor: `az webapp deployment list-publishing-profiles --name licoreria-api --resource-group licoreria-rg --xml`

### 5️⃣ Desplegar
```bash
git push origin main
# ¡Listo! Se despliega automáticamente
```

## 🔄 Actualizaciones Futuras

```bash
# Solo esto:
git add .
git commit -m "Nueva funcionalidad"
git push origin main

# ¡El resto es automático!
```

## 📍 URLs Importantes

- **Tu API**: `https://licoreria-api.azurewebsites.net`
- **Swagger**: `https://licoreria-api.azurewebsites.net`
- **GitHub Actions**: `https://github.com/TU-USUARIO/licoreria-api/actions`
- **Azure Portal**: `https://portal.azure.com`

## ✅ Checklist

- [ ] Repositorio en GitHub creado
- [ ] Azure CLI instalado y logueado (`az login`)
- [ ] Recursos de Azure creados
- [ ] Connection strings configuradas
- [ ] GitHub Secrets configurados
- [ ] Primera deployment exitosa
- [ ] API respondiendo en producción

## 🆘 Troubleshooting

**Error de conexión SQL:**
```bash
# Verificar firewall
az sql server firewall-rule list --server licoreria-sql-server --resource-group licoreria-rg
```

**Ver logs:**
```bash
az webapp log tail --resource-group licoreria-rg --name licoreria-api
```

**Reiniciar app:**
```bash
az webapp restart --resource-group licoreria-rg --name licoreria-api
```

