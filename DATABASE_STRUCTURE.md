# Estructura de Base de Datos - Licoreria API

Este documento describe la estructura de las bases de datos que se utilizarán en el proyecto.

## 📊 SQL Server - Base de Datos Transaccional

### Tablas Principales

#### 1. Usuarios
Tabla para gestión de usuarios y autenticación.

```sql
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NombreUsuario NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    NombreCompleto NVARCHAR(100),
    Rol NVARCHAR(50),
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2,
    Activo BIT NOT NULL DEFAULT 1
);
```

#### 2. Categorias
Catálogo de categorías de productos.

```sql
CREATE TABLE Categorias (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2,
    Activo BIT NOT NULL DEFAULT 1
);
```

#### 3. Productos
Catálogo de productos de la licorería.

```sql
CREATE TABLE Productos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(1000),
    CategoriaId INT NOT NULL,
    PrecioVenta DECIMAL(18,2) NOT NULL,
    PrecioCompra DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    StockMinimo INT NOT NULL DEFAULT 0,
    UnidadMedida NVARCHAR(50),
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
);
```

#### 4. Proveedores
Catálogo de proveedores.

```sql
CREATE TABLE Proveedores (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(200) NOT NULL,
    RazonSocial NVARCHAR(200),
    RFC NVARCHAR(20),
    Direccion NVARCHAR(500),
    Telefono NVARCHAR(20),
    Email NVARCHAR(200),
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2,
    Activo BIT NOT NULL DEFAULT 1
);
```

#### 5. Compras
Cabecera de compras.

```sql
CREATE TABLE Compras (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Folio NVARCHAR(50) NOT NULL UNIQUE,
    ProveedorId INT NOT NULL,
    UsuarioId INT NOT NULL,
    FechaCompra DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Subtotal DECIMAL(18,2) NOT NULL,
    Impuestos DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    Observaciones NVARCHAR(1000),
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2,
    FOREIGN KEY (ProveedorId) REFERENCES Proveedores(Id),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 6. ComprasDetalle
Detalle de compras.

```sql
CREATE TABLE ComprasDetalle (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CompraId INT NOT NULL,
    ProductoId INT NOT NULL,
    Cantidad DECIMAL(18,2) NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (CompraId) REFERENCES Compras(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);
```

#### 7. Ventas
Cabecera de ventas.

```sql
CREATE TABLE Ventas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Folio NVARCHAR(50) NOT NULL UNIQUE,
    ClienteId INT,
    UsuarioId INT NOT NULL,
    FechaVenta DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Subtotal DECIMAL(18,2) NOT NULL,
    Impuestos DECIMAL(18,2) NOT NULL DEFAULT 0,
    Descuento DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    MetodoPago NVARCHAR(50),
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Completada',
    Observaciones NVARCHAR(1000),
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 8. VentasDetalle
Detalle de ventas.

```sql
CREATE TABLE VentasDetalle (
    Id INT PRIMARY KEY IDENTITY(1,1),
    VentaId INT NOT NULL,
    ProductoId INT NOT NULL,
    Cantidad DECIMAL(18,2) NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Descuento DECIMAL(18,2) NOT NULL DEFAULT 0,
    Subtotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (VentaId) REFERENCES Ventas(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);
```

#### 9. DevolucionesVenta
Cabecera de devoluciones de venta.

```sql
CREATE TABLE DevolucionesVenta (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Folio NVARCHAR(50) NOT NULL UNIQUE,
    VentaId INT NOT NULL,
    UsuarioId INT NOT NULL,
    FechaDevolucion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Motivo NVARCHAR(500),
    TotalDevolucion DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    Observaciones NVARCHAR(1000),
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2,
    FOREIGN KEY (VentaId) REFERENCES Ventas(Id),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 10. DevolucionesVentaDetalle
Detalle de devoluciones de venta.

```sql
CREATE TABLE DevolucionesVentaDetalle (
    Id INT PRIMARY KEY IDENTITY(1,1),
    DevolucionVentaId INT NOT NULL,
    VentaDetalleId INT NOT NULL,
    ProductoId INT NOT NULL,
    CantidadDevolver DECIMAL(18,2) NOT NULL,
    Motivo NVARCHAR(500),
    Subtotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (DevolucionVentaId) REFERENCES DevolucionesVenta(Id) ON DELETE CASCADE,
    FOREIGN KEY (VentaDetalleId) REFERENCES VentasDetalle(Id),
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);
```

### Índices Recomendados

```sql
-- Índices para mejorar el rendimiento
CREATE INDEX IX_Productos_CategoriaId ON Productos(CategoriaId);
CREATE INDEX IX_Compras_ProveedorId ON Compras(ProveedorId);
CREATE INDEX IX_Compras_UsuarioId ON Compras(UsuarioId);
CREATE INDEX IX_Compras_FechaCompra ON Compras(FechaCompra);
CREATE INDEX IX_Ventas_UsuarioId ON Ventas(UsuarioId);
CREATE INDEX IX_Ventas_FechaVenta ON Ventas(FechaVenta);
CREATE INDEX IX_VentasDetalle_VentaId ON VentasDetalle(VentaId);
CREATE INDEX IX_VentasDetalle_ProductoId ON VentasDetalle(ProductoId);
```

### Stored Procedures (Pendientes de Implementar)

1. `sp_CrearUsuario` - Crear nuevo usuario con hash de contraseña
2. `sp_ValidarUsuario` - Validar credenciales de usuario
3. `sp_RegistrarVenta` - Registrar una nueva venta con su detalle
4. `sp_RegistrarCompra` - Registrar una nueva compra con su detalle
5. `sp_RegistrarDevolucion` - Registrar una devolución de venta
6. `sp_ActualizarStock` - Actualizar stock después de compra/venta/devolución
7. `sp_ObtenerProductosBajoStock` - Obtener productos con stock bajo
8. `sp_ObtenerReporteVentas` - Reporte de ventas por rango de fechas

## 🍃 MongoDB - Base de Datos NoSQL

### Colecciones (Por Definir)

Las colecciones de MongoDB se utilizarán para:

- **Logs y Auditoría**: Registro de acciones de usuarios
- **Historial de Transacciones**: Historial detallado de operaciones
- **Documentos Adjuntos**: Imágenes, PDFs de facturas, etc.
- **Configuraciones Flexibles**: Configuraciones que pueden cambiar frecuentemente
- **Analíticas**: Datos de análisis y reportes

### Estructura de Ejemplo (Pendiente)

```javascript
// Ejemplo de colección de logs
{
  "_id": ObjectId("..."),
  "usuarioId": 1,
  "accion": "CrearVenta",
  "fecha": ISODate("2024-01-01T00:00:00Z"),
  "detalles": {
    "ventaId": 123,
    "total": 1500.00
  },
  "ip": "192.168.1.1"
}
```

## 📝 Notas de Implementación

1. Todas las tablas tienen campos de auditoría: `FechaCreacion`, `FechaModificacion`, `Activo`
2. Los precios se manejan con `DECIMAL(18,2)` para precisión
3. Las cantidades se manejan con `DECIMAL(18,2)` para permitir fracciones
4. Los folios se generan automáticamente con un formato específico
5. Se implementarán triggers para actualizar stock automáticamente
6. Se implementarán validaciones a nivel de base de datos

## 🔄 Próximos Pasos

1. Crear scripts de migración con Entity Framework Core
2. Implementar stored procedures
3. Crear índices adicionales según necesidades de rendimiento
4. Definir estructura completa de MongoDB
5. Implementar triggers y validaciones


