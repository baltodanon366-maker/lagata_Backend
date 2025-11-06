-- =============================================
-- Script de Creación de Data Warehouse
-- Base de Datos: dbLicoreriaDW (Data Warehouse)
-- Esquema: Estrella (Star Schema)
-- =============================================

-- NOTA: Crear esta base de datos en un servidor SQL Server separado
-- o en la misma instancia pero diferente base de datos

USE [dbLicoreriaDW]
GO

-- =============================================
-- 1. TABLAS DE DIMENSIONES
-- =============================================

-- Tabla: DimTiempo (Dimensión de Tiempo)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimTiempo]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DimTiempo] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Fecha] DATE NOT NULL UNIQUE,
        [Anio] INT NOT NULL,
        [Trimestre] INT NOT NULL, -- 1, 2, 3, 4
        [Mes] INT NOT NULL, -- 1-12
        [Semana] INT NOT NULL, -- 1-52
        [Dia] INT NOT NULL, -- 1-31
        [DiaSemana] INT NOT NULL, -- 1=Lunes, 7=Domingo
        [NombreMes] NVARCHAR(20), -- Enero, Febrero, etc.
        [NombreDiaSemana] NVARCHAR(20), -- Lunes, Martes, etc.
        [EsFinDeSemana] BIT NOT NULL,
        [EsFestivo] BIT NOT NULL DEFAULT 0
    );
    PRINT 'Tabla DimTiempo creada exitosamente';
END
GO

-- Tabla: DimCategoria
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimCategoria]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DimCategoria] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [CategoriaNombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Descripcion] NVARCHAR(500),
        [Activo] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla DimCategoria creada exitosamente';
END
GO

-- Tabla: DimMarca
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimMarca]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DimMarca] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [MarcaNombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Descripcion] NVARCHAR(500),
        [Activo] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla DimMarca creada exitosamente';
END
GO

-- Tabla: DimModelo
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimModelo]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DimModelo] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ModeloNombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Descripcion] NVARCHAR(500),
        [Activo] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla DimModelo creada exitosamente';
END
GO

-- Tabla: DimProducto
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimProducto]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DimProducto] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ProductoCodigo] NVARCHAR(50) NOT NULL UNIQUE,
        [ProductoNombre] NVARCHAR(200) NOT NULL,
        [CategoriaId] INT NULL, -- FK → DimCategoria
        [MarcaId] INT NULL, -- FK → DimMarca
        [ModeloId] INT NULL, -- FK → DimModelo
        [PrecioCompraPromedio] DECIMAL(18,2),
        [PrecioVentaPromedio] DECIMAL(18,2),
        [UnidadMedida] NVARCHAR(50),
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaInicio] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [FechaFin] DATETIME2 NULL, -- Para SCD Type 2 (Slowly Changing Dimension)
        FOREIGN KEY ([CategoriaId]) REFERENCES [DimCategoria]([Id]),
        FOREIGN KEY ([MarcaId]) REFERENCES [DimMarca]([Id]),
        FOREIGN KEY ([ModeloId]) REFERENCES [DimModelo]([Id])
    );
    PRINT 'Tabla DimProducto creada exitosamente';
END
GO

-- Tabla: DimCliente
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimCliente]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DimCliente] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ClienteCodigo] NVARCHAR(50) NOT NULL UNIQUE,
        [ClienteNombre] NVARCHAR(200) NOT NULL,
        [RFC] NVARCHAR(20),
        [TipoCliente] NVARCHAR(50), -- Mayorista, Minorista, etc.
        [Activo] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla DimCliente creada exitosamente';
END
GO

-- Tabla: DimProveedor
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimProveedor]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DimProveedor] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ProveedorCodigo] NVARCHAR(50) NOT NULL UNIQUE,
        [ProveedorNombre] NVARCHAR(200) NOT NULL,
        [RFC] NVARCHAR(20),
        [Activo] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla DimProveedor creada exitosamente';
END
GO

-- Tabla: DimEmpleado
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimEmpleado]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DimEmpleado] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [EmpleadoCodigo] NVARCHAR(20) NOT NULL UNIQUE,
        [EmpleadoNombre] NVARCHAR(200) NOT NULL,
        [Departamento] NVARCHAR(100),
        [Puesto] NVARCHAR(100),
        [Activo] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Tabla DimEmpleado creada exitosamente';
END
GO

-- =============================================
-- 2. TABLAS DE HECHOS (FACT TABLES)
-- =============================================

-- Tabla: HechoVenta
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HechoVenta]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HechoVenta] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        -- Claves Foráneas (Dimensiones)
        [FechaId] INT NOT NULL, -- FK → DimTiempo
        [ProductoId] INT NOT NULL, -- FK → DimProducto
        [ClienteId] INT NULL, -- FK → DimCliente
        [EmpleadoId] INT NULL, -- FK → DimEmpleado
        [CategoriaId] INT NULL, -- FK → DimCategoria
        -- Medidas (Métricas)
        [TotalVentas] DECIMAL(18,2) NOT NULL,
        [CantidadVendida] DECIMAL(18,2) NOT NULL,
        [CantidadTransacciones] INT NOT NULL,
        [PromedioTicket] DECIMAL(18,2),
        [DescuentoTotal] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ImpuestosTotal] DECIMAL(18,2) NOT NULL DEFAULT 0,
        -- Metadatos
        [FechaProcesamiento] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY ([FechaId]) REFERENCES [DimTiempo]([Id]),
        FOREIGN KEY ([ProductoId]) REFERENCES [DimProducto]([Id]),
        FOREIGN KEY ([ClienteId]) REFERENCES [DimCliente]([Id]),
        FOREIGN KEY ([EmpleadoId]) REFERENCES [DimEmpleado]([Id]),
        FOREIGN KEY ([CategoriaId]) REFERENCES [DimCategoria]([Id])
    );
    PRINT 'Tabla HechoVenta creada exitosamente';
END
GO

-- Tabla: HechoCompra
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HechoCompra]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HechoCompra] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        -- Claves Foráneas
        [FechaId] INT NOT NULL, -- FK → DimTiempo
        [ProductoId] INT NOT NULL, -- FK → DimProducto
        [ProveedorId] INT NOT NULL, -- FK → DimProveedor
        [CategoriaId] INT NULL, -- FK → DimCategoria
        -- Medidas
        [TotalCompras] DECIMAL(18,2) NOT NULL,
        [CantidadComprada] DECIMAL(18,2) NOT NULL,
        [CantidadTransacciones] INT NOT NULL,
        [PromedioCompra] DECIMAL(18,2),
        [ImpuestosTotal] DECIMAL(18,2) NOT NULL DEFAULT 0,
        -- Metadatos
        [FechaProcesamiento] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY ([FechaId]) REFERENCES [DimTiempo]([Id]),
        FOREIGN KEY ([ProductoId]) REFERENCES [DimProducto]([Id]),
        FOREIGN KEY ([ProveedorId]) REFERENCES [DimProveedor]([Id]),
        FOREIGN KEY ([CategoriaId]) REFERENCES [DimCategoria]([Id])
    );
    PRINT 'Tabla HechoCompra creada exitosamente';
END
GO

-- Tabla: HechoInventario
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HechoInventario]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HechoInventario] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        -- Claves Foráneas
        [FechaId] INT NOT NULL, -- FK → DimTiempo
        [ProductoId] INT NOT NULL, -- FK → DimProducto
        [CategoriaId] INT NULL, -- FK → DimCategoria
        -- Medidas
        [StockActual] INT NOT NULL,
        [StockMinimo] INT NOT NULL,
        [ValorInventario] DECIMAL(18,2) NOT NULL, -- Stock * PrecioCompra
        [ProductosConStockBajo] INT NOT NULL DEFAULT 0, -- 1 si Stock < StockMinimo, 0 si no
        -- Metadatos
        [FechaProcesamiento] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY ([FechaId]) REFERENCES [DimTiempo]([Id]),
        FOREIGN KEY ([ProductoId]) REFERENCES [DimProducto]([Id]),
        FOREIGN KEY ([CategoriaId]) REFERENCES [DimCategoria]([Id])
    );
    PRINT 'Tabla HechoInventario creada exitosamente';
END
GO

-- =============================================
-- 3. ÍNDICES PARA MEJOR PERFORMANCE
-- =============================================

-- Índices en HechoVenta
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HechoVenta_FechaId')
    CREATE INDEX IX_HechoVenta_FechaId ON [HechoVenta]([FechaId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HechoVenta_ProductoId')
    CREATE INDEX IX_HechoVenta_ProductoId ON [HechoVenta]([ProductoId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HechoVenta_CategoriaId')
    CREATE INDEX IX_HechoVenta_CategoriaId ON [HechoVenta]([CategoriaId]);
GO

-- Índices en HechoCompra
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HechoCompra_FechaId')
    CREATE INDEX IX_HechoCompra_FechaId ON [HechoCompra]([FechaId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HechoCompra_ProductoId')
    CREATE INDEX IX_HechoCompra_ProductoId ON [HechoCompra]([ProductoId]);
GO

-- Índices en HechoInventario
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HechoInventario_FechaId')
    CREATE INDEX IX_HechoInventario_FechaId ON [HechoInventario]([FechaId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HechoInventario_ProductoId')
    CREATE INDEX IX_HechoInventario_ProductoId ON [HechoInventario]([ProductoId]);
GO

-- =============================================
-- 4. POBLAR DIMENSIÓN DE TIEMPO
-- =============================================

-- Stored Procedure para poblar DimTiempo
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PoblarDimTiempo]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[sp_PoblarDimTiempo]
GO

CREATE PROCEDURE [dbo].[sp_PoblarDimTiempo]
    @FechaInicio DATE,
    @FechaFin DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Fecha DATE = @FechaInicio;
    
    WHILE @Fecha <= @FechaFin
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [DimTiempo] WHERE [Fecha] = @Fecha)
        BEGIN
            INSERT INTO [DimTiempo] (
                [Fecha], [Anio], [Trimestre], [Mes], [Semana], [Dia], [DiaSemana],
                [NombreMes], [NombreDiaSemana], [EsFinDeSemana], [EsFestivo]
            )
            VALUES (
                @Fecha,
                YEAR(@Fecha),
                (MONTH(@Fecha) - 1) / 3 + 1,
                MONTH(@Fecha),
                DATEPART(WEEK, @Fecha),
                DAY(@Fecha),
                DATEPART(WEEKDAY, @Fecha),
                DATENAME(MONTH, @Fecha),
                DATENAME(WEEKDAY, @Fecha),
                CASE WHEN DATEPART(WEEKDAY, @Fecha) IN (1, 7) THEN 1 ELSE 0 END,
                0 -- EsFestivo (se puede actualizar manualmente)
            );
        END
        
        SET @Fecha = DATEADD(DAY, 1, @Fecha);
    END
END
GO

-- Ejecutar para poblar últimos 5 años y próximos 2 años
EXEC [sp_PoblarDimTiempo] @FechaInicio = '2020-01-01', @FechaFin = '2027-12-31';
GO

PRINT '=============================================';
PRINT 'Script de creación de Data Warehouse completado';
PRINT '=============================================';

