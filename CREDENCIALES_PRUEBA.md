# 🔐 Credenciales de Prueba

## Usuarios del Sistema

### Administrador
- **Usuario**: `admin`
- **Contraseña**: `Admin123!`
- **Rol**: Administrador
- **Permisos**: Acceso completo

### Vendedor
- **Usuario**: `vendedor1`
- **Contraseña**: `Vendedor123!`
- **Rol**: Vendedor
- **Permisos**: Lectura de catálogos, ventas, compras, devoluciones

### Supervisor
- **Usuario**: `supervisor1`
- **Contraseña**: `Supervisor123!`
- **Rol**: Supervisor
- **Permisos**: Ventas, compras, devoluciones, reportes completos

---

## 📝 Nota Importante

**Estas contraseñas están hasheadas con BCrypt en la base de datos.**

Para actualizar los hashes en la base de datos, ejecuta:
```sql
-- Ver scripts/database/UpdatePasswordHashes.sql
```

---

## 🧪 Probar Login

### Con cURL:
```bash
curl -X 'POST' \
  'http://localhost:5000/api/Auth/login' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "nombreUsuario": "admin",
  "password": "Admin123!"
}'
```

### Con Swagger:
1. Ve a `http://localhost:5000/swagger`
2. Busca el endpoint `POST /api/Auth/login`
3. Prueba con:
   - `nombreUsuario`: `admin`
   - `password`: `Admin123!`

