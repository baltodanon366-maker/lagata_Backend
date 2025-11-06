-- =============================================
-- Script de Creación de Tablas - Licoreria API
-- Base de Datos: dbLicoreriaLaGata
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- =============================================
-- 1. TABLAS DE SEGURIDAD Y AUTENTICACIÓN
-- =============================================

-- Tabla: Roles
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Nombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Descripcion] NVARCHAR(500),
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2
    );
    PRINT 'Tabla Roles creada exitosamente';
END
GO

-- Tabla: Permisos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permisos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Permisos] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Nombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Descripcion] NVARCHAR(500),
        [Modulo] NVARCHAR(100), -- Ventas, Compras, Inventario, etc.
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Tabla Permisos creada exitosamente';
END
GO

-- Tabla: RolesPermisos (Relación)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolesPermisos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RolesPermisos] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [RolId] INT NOT NULL,
        [PermisoId] INT NOT NULL,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY ([RolId]) REFERENCES [Roles]([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([PermisoId]) REFERENCES [Permisos]([Id]) ON DELETE CASCADE,
        UNIQUE ([RolId], [PermisoId])
    );
    PRINT 'Tabla RolesPermisos creada exitosamente';
END
GO

-- Tabla: Usuarios
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Usuarios] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [NombreUsuario] NVARCHAR(100) NOT NULL UNIQUE,
        [Email] NVARCHAR(200) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [NombreCompleto] NVARCHAR(200),
        [Rol] NVARCHAR(50), -- Mantener por compatibilidad, pero usar RolesPermisos
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2,
        [UltimoAcceso] DATETIME2
    );
    PRINT 'Tabla Usuarios creada exitosamente';
END
GO

-- Tabla: UsuariosRoles (Relación)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UsuariosRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UsuariosRoles] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [UsuarioId] INT NOT NULL,
        [RolId] INT NOT NULL,
        [FechaAsignacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios]([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([RolId]) REFERENCES [Roles]([Id]) ON DELETE CASCADE,
        UNIQUE ([UsuarioId], [RolId])
    );
    PRINT 'Tabla UsuariosRoles creada exitosamente';
END
GO

-- Tabla: SesionesUsuario
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SesionesUsuario]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SesionesUsuario] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [UsuarioId] INT NOT NULL,
        [Token] NVARCHAR(MAX),
        [FechaInicio] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaExpiracion] DATETIME2,
        [IpAddress] NVARCHAR(50),
        [UserAgent] NVARCHAR(500),
        [Activa] BIT NOT NULL DEFAULT 1,
        FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios]([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla SesionesUsuario creada exitosamente';
END
GO

-- =============================================
-- 2. TABLAS DE CATÁLOGOS
-- =============================================

-- Tabla: Categorias
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categorias]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Categorias] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Nombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Descripcion] NVARCHAR(500),
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2
    );
    PRINT 'Tabla Categorias creada exitosamente';
END
GO

-- Tabla: Marcas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Marcas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Marcas] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Nombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Descripcion] NVARCHAR(500),
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2
    );
    PRINT 'Tabla Marcas creada exitosamente';
END
GO

-- Tabla: Modelos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Modelos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Modelos] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Nombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Descripcion] NVARCHAR(500),
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2
    );
    PRINT 'Tabla Modelos creada exitosamente';
END
GO

-- Tabla: Productos (Sin relaciones - solo información básica)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Productos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Productos] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Nombre] NVARCHAR(200) NOT NULL,
        [Descripcion] NVARCHAR(1000),
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2
    );
    PRINT 'Tabla Productos creada exitosamente';
END
GO

-- Tabla: DetalleProducto (Con todas las relaciones y campos de negocio)
-- ROL: Estado actual del producto para consultas rápidas (Stock cacheado)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DetalleProducto]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DetalleProducto] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ProductoId] INT NOT NULL,
        [CategoriaId] INT NOT NULL,
        [MarcaId] INT NOT NULL,
        [ModeloId] INT NOT NULL,
        [Codigo] NVARCHAR(50) NOT NULL UNIQUE,
        [SKU] NVARCHAR(100),
        [Observaciones] NVARCHAR(500),
        [PrecioCompra] DECIMAL(18,2) NOT NULL,
        [PrecioVenta] DECIMAL(18,2) NOT NULL,
        [Stock] INT NOT NULL DEFAULT 0, -- Cacheado, actualizado automáticamente por trigger desde MovimientosStock
        [StockMinimo] INT NOT NULL DEFAULT 0, -- Valor de referencia para alertas
        [UnidadMedida] NVARCHAR(50), -- Botella, Lata, Caja, etc.
        [FechaUltimoMovimiento] DATETIME2 NULL, -- Timestamp de última actualización de stock
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2,
        FOREIGN KEY ([ProductoId]) REFERENCES [Productos]([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias]([Id]),
        FOREIGN KEY ([MarcaId]) REFERENCES [Marcas]([Id]),
        FOREIGN KEY ([ModeloId]) REFERENCES [Modelos]([Id])
    );
    PRINT 'Tabla DetalleProducto creada exitosamente';
END
GO

-- Tabla: Clientes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Clientes] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [CodigoCliente] NVARCHAR(50) NOT NULL UNIQUE,
        [NombreCompleto] NVARCHAR(200) NOT NULL,
        [RazonSocial] NVARCHAR(200),
        [RFC] NVARCHAR(20),
        [Direccion] NVARCHAR(500),
        [Telefono] NVARCHAR(20),
        [Email] NVARCHAR(200),
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2
    );
    PRINT 'Tabla Clientes creada exitosamente';
END
GO

-- Tabla: Proveedores
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Proveedores]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Proveedores] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [CodigoProveedor] NVARCHAR(50) NOT NULL UNIQUE,
        [Nombre] NVARCHAR(200) NOT NULL,
        [RazonSocial] NVARCHAR(200),
        [RFC] NVARCHAR(20),
        [Direccion] NVARCHAR(500),
        [Telefono] NVARCHAR(20),
        [Email] NVARCHAR(200),
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2
    );
    PRINT 'Tabla Proveedores creada exitosamente';
END
GO

-- Tabla: Empleados
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Empleados]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Empleados] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [UsuarioId] INT NULL, -- Relación opcional con Usuarios
        [CodigoEmpleado] NVARCHAR(20) NOT NULL UNIQUE,
        [NombreCompleto] NVARCHAR(200) NOT NULL,
        [Telefono] NVARCHAR(20),
        [Email] NVARCHAR(200),
        [Direccion] NVARCHAR(500),
        [FechaNacimiento] DATE,
        [FechaIngreso] DATE NOT NULL,
        [Salario] DECIMAL(18,2),
        [Departamento] NVARCHAR(100),
        [Puesto] NVARCHAR(100),
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2,
        FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios]([Id])
    );
    PRINT 'Tabla Empleados creada exitosamente';
END
GO

-- =============================================
-- 3. TABLAS DE TRANSACCIONES
-- =============================================

-- Tabla: Compras
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Compras]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Compras] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Folio] NVARCHAR(50) NOT NULL UNIQUE,
        [ProveedorId] INT NOT NULL,
        [UsuarioId] INT NOT NULL,
        [FechaCompra] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [Subtotal] DECIMAL(18,2) NOT NULL,
        [Impuestos] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Total] DECIMAL(18,2) NOT NULL,
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Pendiente', -- Pendiente, Completada, Cancelada
        [Observaciones] NVARCHAR(1000),
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2,
        FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores]([Id]),
        FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios]([Id])
    );
    PRINT 'Tabla Compras creada exitosamente';
END
GO

-- Tabla: ComprasDetalle
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ComprasDetalle]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ComprasDetalle] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [CompraId] INT NOT NULL,
        [DetalleProductoId] INT NOT NULL,
        [Cantidad] DECIMAL(18,2) NOT NULL,
        [PrecioUnitario] DECIMAL(18,2) NOT NULL,
        [Subtotal] DECIMAL(18,2) NOT NULL,
        FOREIGN KEY ([CompraId]) REFERENCES [Compras]([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([DetalleProductoId]) REFERENCES [DetalleProducto]([Id])
    );
    PRINT 'Tabla ComprasDetalle creada exitosamente';
END
GO

-- Tabla: Ventas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ventas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Ventas] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Folio] NVARCHAR(50) NOT NULL UNIQUE,
        [ClienteId] INT NULL,
        [UsuarioId] INT NOT NULL,
        [EmpleadoId] INT NULL,
        [FechaVenta] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [Subtotal] DECIMAL(18,2) NOT NULL,
        [Impuestos] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Descuento] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Total] DECIMAL(18,2) NOT NULL,
        [MetodoPago] NVARCHAR(50), -- Efectivo, Tarjeta, Transferencia
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Completada', -- Completada, Cancelada, Pendiente
        [Observaciones] NVARCHAR(1000),
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2,
        FOREIGN KEY ([ClienteId]) REFERENCES [Clientes]([Id]),
        FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios]([Id]),
        FOREIGN KEY ([EmpleadoId]) REFERENCES [Empleados]([Id])
    );
    PRINT 'Tabla Ventas creada exitosamente';
END
GO

-- Tabla: VentasDetalle
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VentasDetalle]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[VentasDetalle] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [VentaId] INT NOT NULL,
        [DetalleProductoId] INT NOT NULL,
        [Cantidad] DECIMAL(18,2) NOT NULL,
        [PrecioUnitario] DECIMAL(18,2) NOT NULL,
        [Descuento] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Subtotal] DECIMAL(18,2) NOT NULL,
        FOREIGN KEY ([VentaId]) REFERENCES [Ventas]([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([DetalleProductoId]) REFERENCES [DetalleProducto]([Id])
    );
    PRINT 'Tabla VentasDetalle creada exitosamente';
END
GO

-- Tabla: DevolucionesVenta
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DevolucionesVenta]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DevolucionesVenta] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Folio] NVARCHAR(50) NOT NULL UNIQUE,
        [VentaId] INT NOT NULL,
        [UsuarioId] INT NOT NULL,
        [FechaDevolucion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [Motivo] NVARCHAR(500),
        [TotalDevolucion] DECIMAL(18,2) NOT NULL,
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Pendiente', -- Pendiente, Completada, Rechazada
        [Observaciones] NVARCHAR(1000),
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2,
        FOREIGN KEY ([VentaId]) REFERENCES [Ventas]([Id]),
        FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios]([Id])
    );
    PRINT 'Tabla DevolucionesVenta creada exitosamente';
END
GO

-- Tabla: DevolucionesVentaDetalle
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DevolucionesVentaDetalle]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DevolucionesVentaDetalle] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [DevolucionVentaId] INT NOT NULL,
        [VentaDetalleId] INT NOT NULL,
        [DetalleProductoId] INT NOT NULL,
        [CantidadDevolver] DECIMAL(18,2) NOT NULL,
        [Motivo] NVARCHAR(500),
        [Subtotal] DECIMAL(18,2) NOT NULL,
        FOREIGN KEY ([DevolucionVentaId]) REFERENCES [DevolucionesVenta]([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([VentaDetalleId]) REFERENCES [VentasDetalle]([Id]),
        FOREIGN KEY ([DetalleProductoId]) REFERENCES [DetalleProducto]([Id])
    );
    PRINT 'Tabla DevolucionesVentaDetalle creada exitosamente';
END
GO

-- =============================================
-- 4. TABLAS DE INVENTARIO
-- =============================================

-- Tabla: MovimientosStock
-- ROL: Source of Truth para historial y auditoría de movimientos de stock
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MovimientosStock]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MovimientosStock] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [DetalleProductoId] INT NOT NULL,
        [TipoMovimiento] NVARCHAR(50) NOT NULL, -- Entrada, Salida, Ajuste
        [Cantidad] DECIMAL(18,2) NOT NULL,
        [StockAnterior] DECIMAL(18,2) NOT NULL, -- Stock antes del movimiento (para auditoría)
        [StockNuevo] DECIMAL(18,2) NOT NULL, -- Stock después del movimiento (Source of Truth)
        [ReferenciaId] INT NULL, -- ID de Compra, Venta, etc.
        [ReferenciaTipo] NVARCHAR(50) NULL, -- Compra, Venta, Devolucion, Ajuste
        [UsuarioId] INT NOT NULL,
        [Motivo] NVARCHAR(500),
        [FechaMovimiento] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY ([DetalleProductoId]) REFERENCES [DetalleProducto]([Id]),
        FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios]([Id])
    );
    PRINT 'Tabla MovimientosStock creada exitosamente';
END
GO

-- =============================================
-- 5. TABLAS ADICIONALES
-- =============================================

-- Tabla: Precios
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Precios]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Precios] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [DetalleProductoId] INT NOT NULL,
        [PrecioCompra] DECIMAL(18,2) NOT NULL,
        [PrecioVenta] DECIMAL(18,2) NOT NULL,
        [PrecioVentaMinimo] DECIMAL(18,2),
        [FechaInicio] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaFin] DATETIME2 NULL,
        [Activo] BIT NOT NULL DEFAULT 1,
        FOREIGN KEY ([DetalleProductoId]) REFERENCES [DetalleProducto]([Id])
    );
    PRINT 'Tabla Precios creada exitosamente';
END
GO

-- Tabla: Descuentos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Descuentos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Descuentos] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Nombre] NVARCHAR(100) NOT NULL,
        [Tipo] NVARCHAR(50) NOT NULL, -- Porcentaje, Fijo
        [Valor] DECIMAL(18,2) NOT NULL,
        [DetalleProductoId] INT NULL, -- Si es NULL, aplica a todos
        [CategoriaId] INT NULL,
        [FechaInicio] DATETIME2 NOT NULL,
        [FechaFin] DATETIME2 NOT NULL,
        [Activo] BIT NOT NULL DEFAULT 1,
        FOREIGN KEY ([DetalleProductoId]) REFERENCES [DetalleProducto]([Id]),
        FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias]([Id])
    );
    PRINT 'Tabla Descuentos creada exitosamente';
END
GO

-- Tabla: ConfiguracionSistema
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracionSistema]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ConfiguracionSistema] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Clave] NVARCHAR(100) NOT NULL UNIQUE,
        [Valor] NVARCHAR(MAX),
        [Tipo] NVARCHAR(50), -- String, Int, Decimal, Boolean, JSON
        [Descripcion] NVARCHAR(500),
        [FechaModificacion] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Tabla ConfiguracionSistema creada exitosamente';
END
GO

-- =============================================
-- 6. ÍNDICES PARA MEJOR PERFORMANCE
-- =============================================

-- Índices en DetalleProducto
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DetalleProducto_Codigo')
    CREATE INDEX IX_DetalleProducto_Codigo ON [DetalleProducto]([Codigo]);
GO

-- Índices en DetalleProducto
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DetalleProducto_ProductoId')
    CREATE INDEX IX_DetalleProducto_ProductoId ON [DetalleProducto]([ProductoId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DetalleProducto_CategoriaId')
    CREATE INDEX IX_DetalleProducto_CategoriaId ON [DetalleProducto]([CategoriaId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DetalleProducto_MarcaId')
    CREATE INDEX IX_DetalleProducto_MarcaId ON [DetalleProducto]([MarcaId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DetalleProducto_ModeloId')
    CREATE INDEX IX_DetalleProducto_ModeloId ON [DetalleProducto]([ModeloId]);
GO

-- Índices en Ventas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ventas_UsuarioId')
    CREATE INDEX IX_Ventas_UsuarioId ON [Ventas]([UsuarioId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ventas_FechaVenta')
    CREATE INDEX IX_Ventas_FechaVenta ON [Ventas]([FechaVenta]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ventas_ClienteId')
    CREATE INDEX IX_Ventas_ClienteId ON [Ventas]([ClienteId]);
GO

-- Índices en Compras
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Compras_ProveedorId')
    CREATE INDEX IX_Compras_ProveedorId ON [Compras]([ProveedorId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Compras_FechaCompra')
    CREATE INDEX IX_Compras_FechaCompra ON [Compras]([FechaCompra]);
GO

-- Índices en MovimientosStock
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MovimientosStock_DetalleProductoId')
    CREATE INDEX IX_MovimientosStock_DetalleProductoId ON [MovimientosStock]([DetalleProductoId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MovimientosStock_FechaMovimiento')
    CREATE INDEX IX_MovimientosStock_FechaMovimiento ON [MovimientosStock]([FechaMovimiento]);
GO

-- =============================================
-- 7. TRIGGERS PARA SINCRONIZACIÓN
-- =============================================

-- Trigger: Actualizar Stock en DetalleProducto cuando se inserta un MovimientoStock
-- Este trigger mantiene sincronizado DetalleProducto.Stock con el último movimiento
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TR_MovimientosStock_ActualizarStock]') AND type = 'TR')
    DROP TRIGGER [dbo].[TR_MovimientosStock_ActualizarStock]
GO

CREATE TRIGGER [dbo].[TR_MovimientosStock_ActualizarStock]
ON [dbo].[MovimientosStock]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Actualizar Stock y FechaUltimoMovimiento en DetalleProducto
    UPDATE dp
    SET 
        dp.[Stock] = CAST(i.[StockNuevo] AS INT),
        dp.[FechaUltimoMovimiento] = i.[FechaMovimiento],
        dp.[FechaModificacion] = GETUTCDATE()
    FROM [dbo].[DetalleProducto] dp
    INNER JOIN inserted i ON dp.[Id] = i.[DetalleProductoId];
END
GO

PRINT 'Trigger TR_MovimientosStock_ActualizarStock creado exitosamente';
GO

PRINT '=============================================';
PRINT 'Script de creación de tablas completado';
PRINT '=============================================';

