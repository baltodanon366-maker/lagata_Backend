# 📝 Scripts MongoDB: Crear Colecciones e Índices

## 🎯 Scripts para Ejecutar en MongoDB Compass o MongoDB Shell

---

## 1️⃣ Crear Base de Datos y Colecciones

### Opción A: Desde MongoDB Compass (UI)

1. Conectar a tu cluster
2. Click en **"Create Database"**
3. **Database Name**: `LicoreriaMongoDB`
4. **Collection Name**: `notificaciones`
5. Click en **"Create"**
6. Repetir para cada colección

### Opción B: Desde MongoDB Shell (MongoDB Compass)

Abre MongoDB Compass → Click en **"MongoSH"** → Ejecuta:

```javascript
// Seleccionar base de datos (se crea automáticamente)
use LicoreriaMongoDB

// Crear colecciones explícitamente (opcional, se crean al insertar)
db.createCollection("notificaciones")
db.createCollection("logs")
db.createCollection("auditoria")
db.createCollection("configuraciones")
db.createCollection("cache")
db.createCollection("documentos")
```

---

## 2️⃣ Crear Índices

Ejecuta en MongoDB Shell (MongoSH):

```javascript
use LicoreriaMongoDB

// =============================================
// ÍNDICES PARA: notificaciones
// =============================================

// Índice para consultar notificaciones por usuario (ordenadas por fecha)
db.notificaciones.createIndex(
  { "usuarioId": 1, "fechaCreacion": -1 },
  { name: "idx_usuario_fecha" }
)

// Índice para no leídas por usuario
db.notificaciones.createIndex(
  { "usuarioId": 1, "leida": 1, "fechaCreacion": -1 },
  { name: "idx_usuario_leida_fecha" }
)

// Índice para expiración automática (opcional - TTL)
db.notificaciones.createIndex(
  { "fechaCreacion": 1 },
  { name: "idx_fecha_ttl", expireAfterSeconds: 2592000 } // 30 días
)

// =============================================
// ÍNDICES PARA: logs
// =============================================

// Índice para consultar logs recientes
db.logs.createIndex(
  { "fecha": -1 },
  { name: "idx_fecha" }
)

// Índice para filtrar por nivel y fecha
db.logs.createIndex(
  { "nivel": 1, "fecha": -1 },
  { name: "idx_nivel_fecha" }
)

// Índice para buscar por usuario
db.logs.createIndex(
  { "usuarioId": 1, "fecha": -1 },
  { name: "idx_usuario_fecha" }
)

// Índice para expiración automática (logs más antiguos de 90 días)
db.logs.createIndex(
  { "fecha": 1 },
  { name: "idx_fecha_ttl", expireAfterSeconds: 7776000 } // 90 días
)

// =============================================
// ÍNDICES PARA: auditoria
// =============================================

// Índice para buscar por entidad
db.auditoria.createIndex(
  { "entidad": 1, "entidadId": 1, "fecha": -1 },
  { name: "idx_entidad_id_fecha" }
)

// Índice para buscar por usuario
db.auditoria.createIndex(
  { "usuarioId": 1, "fecha": -1 },
  { name: "idx_usuario_fecha" }
)

// Índice para buscar por acción
db.auditoria.createIndex(
  { "accion": 1, "fecha": -1 },
  { name: "idx_accion_fecha" }
)

// =============================================
// ÍNDICES PARA: configuraciones
// =============================================

// Índice único para clave (una configuración por clave)
db.configuraciones.createIndex(
  { "clave": 1 },
  { name: "idx_clave_unique", unique: true }
)

// =============================================
// ÍNDICES PARA: cache
// =============================================

// Índice para expiración automática (TTL)
db.cache.createIndex(
  { "fechaExpiracion": 1 },
  { name: "idx_expiracion_ttl", expireAfterSeconds: 0 }
)

// Índice para buscar por clave
db.cache.createIndex(
  { "clave": 1 },
  { name: "idx_clave" }
)

// =============================================
// ÍNDICES PARA: documentos
// =============================================

// Índice para buscar por tipo y entidad
db.documentos.createIndex(
  { "tipoDocumento": 1, "entidadId": 1 },
  { name: "idx_tipo_entidad" }
)

// Índice para buscar por fecha
db.documentos.createIndex(
  { "fechaCreacion": -1 },
  { name: "idx_fecha" }
)
```

---

## 3️⃣ Verificar Índices Creados

```javascript
use LicoreriaMongoDB

// Ver todos los índices de una colección
db.notificaciones.getIndexes()
db.logs.getIndexes()
db.auditoria.getIndexes()
```

---

## 4️⃣ Insertar Datos de Prueba

```javascript
use LicoreriaMongoDB

// Insertar notificación de prueba
db.notificaciones.insertOne({
  "usuarioId": 1,
  "tipo": "Venta",
  "titulo": "Nueva venta realizada",
  "mensaje": "Se registró una venta por $150.00",
  "fechaCreacion": new Date(),
  "leida": false,
  "metadata": {
    "ventaId": 123,
    "total": 150.00
  }
})

// Insertar log de prueba
db.logs.insertOne({
  "nivel": "Information",
  "mensaje": "Venta procesada exitosamente",
  "fecha": new Date(),
  "usuarioId": 1,
  "endpoint": "/api/ventas",
  "metadata": {
    "ventaId": 123
  }
})

// Insertar auditoría de prueba
db.auditoria.insertOne({
  "accion": "CREATE",
  "entidad": "Venta",
  "entidadId": 123,
  "usuarioId": 1,
  "fecha": new Date(),
  "valoresAnteriores": {},
  "valoresNuevos": {
    "total": 150.00,
    "clienteId": 5
  }
})

// Insertar configuración
db.configuraciones.insertOne({
  "clave": "EmailTemplates",
  "valor": {
    "ventaExitosa": "Tu venta fue procesada exitosamente",
    "stockBajo": "El producto {0} tiene stock bajo"
  },
  "fechaModificacion": new Date()
})
```

---

## 5️⃣ Consultas de Prueba

```javascript
use LicoreriaMongoDB

// Obtener notificaciones no leídas de un usuario
db.notificaciones.find({
  "usuarioId": 1,
  "leida": false
}).sort({ "fechaCreacion": -1 }).limit(10)

// Obtener logs de error de las últimas 24 horas
db.logs.find({
  "nivel": "Error",
  "fecha": { $gte: new Date(Date.now() - 24 * 60 * 60 * 1000) }
}).sort({ "fecha": -1 })

// Obtener auditoría de una entidad específica
db.auditoria.find({
  "entidad": "Venta",
  "entidadId": 123
}).sort({ "fecha": -1 })

// Obtener una configuración
db.configuraciones.findOne({ "clave": "EmailTemplates" })

// Contar documentos en una colección
db.notificaciones.countDocuments()
db.logs.countDocuments({ "nivel": "Error" })
```

---

## 6️⃣ Scripts de Limpieza (Opcional)

```javascript
use LicoreriaMongoDB

// Eliminar notificaciones leídas mayores a 30 días
db.notificaciones.deleteMany({
  "leida": true,
  "fechaCreacion": { $lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
})

// Eliminar logs de información mayores a 90 días
db.logs.deleteMany({
  "nivel": "Information",
  "fecha": { $lt: new Date(Date.now() - 90 * 24 * 60 * 60 * 1000) }
})
```

---

## 7️⃣ Actualizar Connection String en appsettings.json

Después de crear todo, actualiza tu `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MongoDBConnection": "mongodb+srv://licoreria-user:TU_PASSWORD@licoreriacluster.xxxxx.mongodb.net/LicoreriaMongoDB?retryWrites=true&w=majority"
  },
  "MongoDBSettings": {
    "DatabaseName": "LicoreriaMongoDB"
  }
}
```

---

**¿Listo para ejecutar?** 🚀

