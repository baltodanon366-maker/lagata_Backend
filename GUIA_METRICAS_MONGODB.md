# Guía de Configuración: Métricas MongoDB Atlas

## 📋 Resumen

Este documento describe cómo configurar MongoDB Atlas para almacenar las 5 métricas implementadas en el sistema:

1. **Uso de red** (bytes enviados/recibidos)
2. **Intentos fallidos de inicio de sesión**
3. **Consultas lentas** (>100ms)
4. **Usuarios activos**
5. **Transacciones por tipo**

## 🚀 Configuración de MongoDB Atlas

### Paso 1: Crear Cluster en MongoDB Atlas

1. Ve a [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)
2. Inicia sesión o crea una cuenta
3. Crea un nuevo cluster (puedes usar el tier gratuito M0)
4. Selecciona la región más cercana a tu ubicación
5. Nombra tu cluster (ej: `LicoreriaCluster`)

### Paso 2: Configurar Acceso de Red

1. En el panel de Atlas, ve a **Network Access**
2. Haz clic en **Add IP Address**
3. Para desarrollo, puedes usar **Allow Access from Anywhere** (`0.0.0.0/0`)
4. Para producción, agrega solo las IPs de tus servidores

### Paso 3: Crear Usuario de Base de Datos

1. Ve a **Database Access**
2. Haz clic en **Add New Database User**
3. Configura:
   - **Username**: `licoreria_user` (o el que prefieras)
   - **Password**: Genera una contraseña segura
   - **Database User Privileges**: `Read and write to any database`
4. Guarda las credenciales de forma segura

### Paso 4: Obtener Connection String

1. Ve a **Database** → **Connect**
2. Selecciona **Connect your application**
3. Elige **.NET** como driver
4. Copia la connection string, será algo como:
   ```
   mongodb+srv://licoreria_user:<password>@licoreriacluster.xxxxx.mongodb.net/?retryWrites=true&w=majority
   ```
5. Reemplaza `<password>` con la contraseña que creaste

### Paso 5: Configurar en appsettings.json

Actualiza tu `appsettings.json` o `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "MongoDBConnection": "mongodb+srv://licoreria_user:TU_PASSWORD@licoreriacluster.xxxxx.mongodb.net/?retryWrites=true&w=majority"
  },
  "MongoDBSettings": {
    "DatabaseName": "LicoreriaMongoDB"
  }
}
```

## 📊 Colecciones que se Crearán Automáticamente

Las siguientes colecciones se crearán automáticamente cuando la aplicación ejecute las primeras operaciones:

### 1. `networkUsageMetrics`
Almacena métricas de uso de red.

**Índices creados automáticamente:**
- `timestamp` (descendente) - Para consultas por fecha

**Estructura:**
```json
{
  "_id": "ObjectId",
  "path": "/api/ventas",
  "method": "POST",
  "bytesSent": 1024,
  "bytesReceived": 512,
  "totalBytes": 1536,
  "clientIp": "192.168.1.1",
  "userAgent": "Mozilla/5.0...",
  "statusCode": 200,
  "timestamp": "2025-11-17T00:00:00Z",
  "durationMs": 150
}
```

### 2. `failedLoginAttempts`
Almacena intentos fallidos de inicio de sesión.

**Índices creados automáticamente:**
- `timestamp` (descendente)
- `ipAddress` (ascendente)
- `username` (ascendente)

**Estructura:**
```json
{
  "_id": "ObjectId",
  "username": "admin",
  "ipAddress": "192.168.1.1",
  "userAgent": "Mozilla/5.0...",
  "failureReason": "InvalidPassword",
  "timestamp": "2025-11-17T00:00:00Z",
  "isSuspicious": true,
  "attemptCount": 3
}
```

### 3. `slowQueries`
Almacena consultas SQL que tardan más de 100ms.

**Índices creados automáticamente:**
- `timestamp` (descendente)
- `durationMs` (descendente)
- `tableName` (ascendente)

**Estructura:**
```json
{
  "_id": "ObjectId",
  "queryType": "SELECT",
  "tableName": "Ventas",
  "queryText": "SELECT * FROM Ventas...",
  "durationMs": 250,
  "thresholdMs": 100,
  "rowsAffected": 50,
  "endpoint": "/api/ventas",
  "userId": 1,
  "timestamp": "2025-11-17T00:00:00Z"
}
```

### 4. `activeUsers`
Almacena información de usuarios activos.

**Índices creados automáticamente:**
- `userId` (ascendente, único)
- `lastActivity` (descendente)
- `isActive` (ascendente)

**Estructura:**
```json
{
  "_id": "ObjectId",
  "userId": 1,
  "username": "admin",
  "role": "Administrador",
  "sessionStart": "2025-11-17T00:00:00Z",
  "lastActivity": "2025-11-17T01:00:00Z",
  "ipAddress": "192.168.1.1",
  "requestCount": 45,
  "isActive": true,
  "timestamp": "2025-11-17T00:00:00Z"
}
```

### 5. `transactionMetrics`
Almacena métricas de transacciones por tipo.

**Índices creados automáticamente:**
- `timestamp` (descendente)
- `transactionType` (ascendente)
- `status` (ascendente)

**Estructura:**
```json
{
  "_id": "ObjectId",
  "transactionType": "Venta",
  "transactionId": 123,
  "amount": 1500.50,
  "userId": 1,
  "clientId": 5,
  "status": "Completed",
  "itemCount": 3,
  "paymentMethod": "Efectivo",
  "timestamp": "2025-11-17T00:00:00Z",
  "durationMs": 200,
  "ipAddress": "192.168.1.1"
}
```

## 🔧 Verificación de Colecciones

### Desde MongoDB Atlas

1. Ve a **Database** → **Browse Collections**
2. Selecciona tu base de datos `LicoreriaMongoDB`
3. Deberías ver las 5 colecciones listadas arriba

### Desde MongoDB Compass

1. Descarga [MongoDB Compass](https://www.mongodb.com/products/compass)
2. Conéctate usando tu connection string
3. Navega a `LicoreriaMongoDB`
4. Verifica que las colecciones existan

### Desde la API

Una vez que la aplicación esté ejecutándose, las colecciones se crearán automáticamente cuando:
- Se haga una petición HTTP (crea `networkUsageMetrics`)
- Se intente un login fallido (crea `failedLoginAttempts`)
- Se ejecute una consulta SQL lenta (crea `slowQueries`)
- Un usuario autenticado haga una petición (crea `activeUsers`)
- Se cree una transacción (crea `transactionMetrics`)

## 📡 Endpoints de Métricas

Una vez configurado, puedes consultar las métricas usando:

### Uso de Red
- `GET /api/metrics/network` - Lista métricas de red
- `GET /api/metrics/network/total?startDate=...&endDate=...` - Total de bytes por período

### Intentos Fallidos de Login
- `GET /api/metrics/failed-logins` - Lista intentos fallidos
- `GET /api/metrics/failed-logins/suspicious` - Intentos sospechosos

### Consultas Lentas
- `GET /api/metrics/slow-queries` - Lista consultas lentas
- `GET /api/metrics/slow-queries/slowest?limit=10` - Las 10 más lentas

### Usuarios Activos
- `GET /api/metrics/active-users` - Lista usuarios activos
- `GET /api/metrics/active-users/count` - Conteo de usuarios activos

### Transacciones
- `GET /api/metrics/transactions` - Lista transacciones
- `GET /api/metrics/transactions/count-by-type` - Conteo por tipo
- `GET /api/metrics/transactions/total-by-type?transactionType=Venta` - Total por tipo

## 🔒 Seguridad

**Importante para Producción:**

1. **No uses `0.0.0.0/0` en Network Access** - Restringe a IPs específicas
2. **Usa contraseñas seguras** - Genera contraseñas complejas
3. **Habilita autenticación** - Todos los endpoints de métricas requieren JWT
4. **Revisa logs regularmente** - Monitorea intentos sospechosos
5. **Configura backups** - MongoDB Atlas ofrece backups automáticos

## 🧪 Pruebas

Para probar que todo funciona:

1. **Inicia la aplicación**
2. **Haz algunas peticiones** a la API
3. **Intenta un login fallido** (usuario incorrecto)
4. **Consulta las métricas** usando los endpoints arriba
5. **Verifica en MongoDB Atlas** que los datos se estén guardando

## 📝 Notas Adicionales

- Los índices se crean automáticamente la primera vez que se inserta un documento
- Las métricas se registran de forma asíncrona para no afectar el rendimiento
- El umbral de consultas lentas es configurable (actualmente 100ms)
- Los usuarios inactivos se marcan automáticamente después de un período sin actividad

## 🆘 Solución de Problemas

### No se crean las colecciones
- Verifica la connection string
- Asegúrate de que el usuario tenga permisos de escritura
- Revisa los logs de la aplicación

### Errores de conexión
- Verifica que tu IP esté en la whitelist de MongoDB Atlas
- Confirma que la connection string sea correcta
- Revisa que el cluster esté activo

### Métricas no se registran
- Verifica que los middlewares estén registrados en `Program.cs`
- Revisa los logs de la aplicación para errores
- Confirma que MongoDB esté accesible desde tu servidor

