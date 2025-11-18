-- =============================================
-- PROCEDIMIENTOS ALMACENADOS - TRANSACCIONES
-- Base de Datos: dbLicoreriaLaGata
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- =============================================
-- 3. PROCEDIMIENTOS PARA COMPRAS
-- =============================================

-- sp_Compra_Crear: Crear nueva compra con detalles y movimiento de stock
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Compra_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Compra_Crear]
GO

CREATE PROCEDURE [dbo].[sp_Compra_Crear]
    @Folio NVARCHAR(50),
    @ProveedorId INT,
    @UsuarioId INT,
    @FechaCompra DATETIME2 = NULL,
    @Observaciones NVARCHAR(1000) = NULL,
    @DetallesCompra NVARCHAR(MAX), -- JSON con array de detalles: [{"DetalleProductoId":1,"Cantidad":10,"PrecioUnitario":100.00}]
    @CompraId INT OUTPUT,
    @MensajeError NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @CompraId = -1;
    SET @MensajeError = '';
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar que el folio no exista
        IF EXISTS (SELECT 1 FROM [Compras] WHERE [Folio] = @Folio)
        BEGIN
            SET @MensajeError = 'El folio de compra ya existe';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Establecer fecha si no se proporciona
        IF @FechaCompra IS NULL
            SET @FechaCompra = GETUTCDATE();
        
        -- Crear encabezado de compra
        INSERT INTO [Compras] ([Folio], [ProveedorId], [UsuarioId], [FechaCompra], [Subtotal], [Impuestos], [Total], [Estado], [Observaciones])
        VALUES (@Folio, @ProveedorId, @UsuarioId, @FechaCompra, 0, 0, 0, 'Completada', @Observaciones);
        
        SET @CompraId = SCOPE_IDENTITY();
        
        -- Parsear JSON y crear detalles
        DECLARE @SubtotalTotal DECIMAL(18,2) = 0;
        DECLARE @DetalleProductoId INT;
        DECLARE @Cantidad DECIMAL(18,2);
        DECLARE @PrecioUnitario DECIMAL(18,2);
        DECLARE @Subtotal DECIMAL(18,2);
        
        -- Crear tabla temporal para detalles
        DECLARE @DetallesTemp TABLE (
            DetalleProductoId INT,
            Cantidad DECIMAL(18,2),
            PrecioUnitario DECIMAL(18,2)
        );
        
        -- Insertar detalles desde JSON (requiere SQL Server 2016+)
        INSERT INTO @DetallesTemp (DetalleProductoId, Cantidad, PrecioUnitario)
        SELECT 
            CAST(JSON_VALUE(value, '$.DetalleProductoId') AS INT),
            CAST(JSON_VALUE(value, '$.Cantidad') AS DECIMAL(18,2)),
            CAST(JSON_VALUE(value, '$.PrecioUnitario') AS DECIMAL(18,2))
        FROM OPENJSON(@DetallesCompra);
        
        -- Insertar detalles y crear movimientos de stock
        DECLARE detalle_cursor CURSOR FOR
        SELECT DetalleProductoId, Cantidad, PrecioUnitario
        FROM @DetallesTemp;
        
        OPEN detalle_cursor;
        FETCH NEXT FROM detalle_cursor INTO @DetalleProductoId, @Cantidad, @PrecioUnitario;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @Subtotal = @Cantidad * @PrecioUnitario;
            SET @SubtotalTotal = @SubtotalTotal + @Subtotal;
            
            -- Insertar detalle
            INSERT INTO [ComprasDetalle] ([CompraId], [DetalleProductoId], [Cantidad], [PrecioUnitario], [Subtotal])
            VALUES (@CompraId, @DetalleProductoId, @Cantidad, @PrecioUnitario, @Subtotal);
            
            -- Obtener stock actual antes del movimiento
            DECLARE @StockAnterior DECIMAL(18,2) = (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = @DetalleProductoId);
            DECLARE @StockNuevo DECIMAL(18,2) = @StockAnterior + @Cantidad;
            
            -- Crear movimiento de stock (Entrada)
            INSERT INTO [MovimientosStock] ([DetalleProductoId], [TipoMovimiento], [Cantidad], [StockAnterior], [StockNuevo], [ReferenciaId], [ReferenciaTipo], [UsuarioId], [Motivo], [FechaMovimiento])
            VALUES (@DetalleProductoId, 'Entrada', @Cantidad, @StockAnterior, @StockNuevo, @CompraId, 'Compra', @UsuarioId, 'Compra de productos', @FechaCompra);
            
            -- El trigger actualizará automáticamente DetalleProducto.Stock
            
            FETCH NEXT FROM detalle_cursor INTO @DetalleProductoId, @Cantidad, @PrecioUnitario;
        END
        
        CLOSE detalle_cursor;
        DEALLOCATE detalle_cursor;
        
        -- Calcular impuestos y total
        DECLARE @Impuestos DECIMAL(18,2) = @SubtotalTotal * 0.15;
        DECLARE @Total DECIMAL(18,2) = @SubtotalTotal + @Impuestos;
        
        -- Actualizar totales de la compra
        UPDATE [Compras]
        SET [Subtotal] = @SubtotalTotal,
            [Impuestos] = @Impuestos,
            [Total] = @Total
        WHERE [Id] = @CompraId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SET @CompraId = -1;
        SET @MensajeError = ERROR_MESSAGE();
    END CATCH
END
GO

-- sp_Compra_ObtenerPorId: Obtener compra con detalles
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Compra_ObtenerPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Compra_ObtenerPorId]
GO

CREATE PROCEDURE [dbo].[sp_Compra_ObtenerPorId]
    @CompraId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Encabezado
    SELECT 
        c.[Id],
        c.[Folio],
        c.[ProveedorId],
        p.[Nombre] AS ProveedorNombre,
        c.[UsuarioId],
        u.[NombreUsuario] AS UsuarioNombre,
        c.[FechaCompra],
        c.[Subtotal],
        c.[Impuestos],
        c.[Total],
        c.[Estado],
        c.[Observaciones],
        c.[FechaCreacion],
        c.[FechaModificacion]
    FROM [Compras] c
    INNER JOIN [Proveedores] p ON c.[ProveedorId] = p.[Id]
    INNER JOIN [Usuarios] u ON c.[UsuarioId] = u.[Id]
    WHERE c.[Id] = @CompraId;
    
    -- Detalles
    SELECT 
        cd.[Id],
        cd.[CompraId],
        cd.[DetalleProductoId],
        dp.[Codigo] AS ProductoCodigo,
        p.[Nombre] AS ProductoNombre,
        m.[Nombre] AS MarcaNombre,
        c.[Nombre] AS CategoriaNombre,
        cd.[Cantidad],
        cd.[PrecioUnitario],
        cd.[Subtotal]
    FROM [ComprasDetalle] cd
    INNER JOIN [DetalleProducto] dp ON cd.[DetalleProductoId] = dp.[Id]
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Marcas] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [Categorias] c ON dp.[CategoriaId] = c.[Id]
    WHERE cd.[CompraId] = @CompraId;
END
GO

-- sp_Compra_MostrarPorRangoFechas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Compra_MostrarPorRangoFechas]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Compra_MostrarPorRangoFechas]
GO

CREATE PROCEDURE [dbo].[sp_Compra_MostrarPorRangoFechas]
    @FechaInicio DATETIME2,
    @FechaFin DATETIME2,
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        c.[Id],
        c.[Folio],
        c.[ProveedorId],
        p.[Nombre] AS ProveedorNombre,
        c.[UsuarioId],
        u.[NombreUsuario],
        c.[FechaCompra],
        c.[Subtotal],
        c.[Impuestos],
        c.[Total],
        c.[Estado],
        c.[Observaciones]
    FROM [Compras] c
    INNER JOIN [Proveedores] p ON c.[ProveedorId] = p.[Id]
    INNER JOIN [Usuarios] u ON c.[UsuarioId] = u.[Id]
    WHERE c.[FechaCompra] BETWEEN @FechaInicio AND @FechaFin
    ORDER BY c.[FechaCompra] DESC;
END
GO

-- sp_Compra_MostrarActivas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Compra_MostrarActivas]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Compra_MostrarActivas]
GO

CREATE PROCEDURE [dbo].[sp_Compra_MostrarActivas]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        c.[Id],
        c.[Folio],
        c.[ProveedorId],
        p.[Nombre] AS ProveedorNombre,
        c.[UsuarioId],
        u.[NombreUsuario],
        c.[FechaCompra],
        c.[Subtotal],
        c.[Impuestos],
        c.[Total],
        c.[Estado],
        c.[Observaciones]
    FROM [Compras] c
    INNER JOIN [Proveedores] p ON c.[ProveedorId] = p.[Id]
    INNER JOIN [Usuarios] u ON c.[UsuarioId] = u.[Id]
    WHERE c.[Estado] = 'Completada'
    ORDER BY c.[FechaCompra] DESC;
END
GO

-- =============================================
-- 4. PROCEDIMIENTOS PARA VENTAS
-- =============================================

-- sp_Venta_Crear: Crear nueva venta con detalles y movimiento de stock (salida)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Venta_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Venta_Crear]
GO

CREATE PROCEDURE [dbo].[sp_Venta_Crear]
    @Folio NVARCHAR(50),
    @ClienteId INT = NULL,
    @UsuarioId INT,
    @EmpleadoId INT = NULL,
    @FechaVenta DATETIME2 = NULL,
    @MetodoPago NVARCHAR(50),
    @Observaciones NVARCHAR(1000) = NULL,
    @DetallesVenta NVARCHAR(MAX), -- JSON con array de detalles
    @VentaId INT OUTPUT,
    @MensajeError NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @VentaId = -1;
    SET @MensajeError = '';
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar que el folio no exista
        IF EXISTS (SELECT 1 FROM [Ventas] WHERE [Folio] = @Folio)
        BEGIN
            SET @MensajeError = 'El folio de venta ya existe';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Establecer fecha si no se proporciona
        IF @FechaVenta IS NULL
            SET @FechaVenta = GETUTCDATE();
        
        -- Crear encabezado de venta
        INSERT INTO [Ventas] ([Folio], [ClienteId], [UsuarioId], [EmpleadoId], [FechaVenta], [Subtotal], [Impuestos], [Descuento], [Total], [MetodoPago], [Estado], [Observaciones])
        VALUES (@Folio, @ClienteId, @UsuarioId, @EmpleadoId, @FechaVenta, 0, 0, 0, 0, @MetodoPago, 'Completada', @Observaciones);
        
        SET @VentaId = SCOPE_IDENTITY();
        
        -- Parsear JSON y crear detalles
        DECLARE @SubtotalTotal DECIMAL(18,2) = 0;
        DECLARE @DescuentoTotal DECIMAL(18,2) = 0;
        DECLARE @DetalleProductoId INT;
        DECLARE @Cantidad DECIMAL(18,2);
        DECLARE @PrecioUnitario DECIMAL(18,2);
        DECLARE @Descuento DECIMAL(18,2);
        DECLARE @Subtotal DECIMAL(18,2);
        
        -- Crear tabla temporal para detalles
        DECLARE @DetallesVentaTemp TABLE (
            DetalleProductoId INT,
            Cantidad DECIMAL(18,2),
            PrecioUnitario DECIMAL(18,2),
            Descuento DECIMAL(18,2)
        );
        
        -- Insertar detalles desde JSON
        INSERT INTO @DetallesVentaTemp (DetalleProductoId, Cantidad, PrecioUnitario, Descuento)
        SELECT 
            CAST(JSON_VALUE(value, '$.DetalleProductoId') AS INT),
            CAST(JSON_VALUE(value, '$.Cantidad') AS DECIMAL(18,2)),
            CAST(JSON_VALUE(value, '$.PrecioUnitario') AS DECIMAL(18,2)),
            ISNULL(CAST(JSON_VALUE(value, '$.Descuento') AS DECIMAL(18,2)), 0)
        FROM OPENJSON(@DetallesVenta);
        
        -- Insertar detalles y crear movimientos de stock
        DECLARE venta_detalle_cursor CURSOR FOR
        SELECT DetalleProductoId, Cantidad, PrecioUnitario, Descuento
        FROM @DetallesVentaTemp;
        
        OPEN venta_detalle_cursor;
        FETCH NEXT FROM venta_detalle_cursor INTO @DetalleProductoId, @Cantidad, @PrecioUnitario, @Descuento;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Validar stock disponible
            DECLARE @StockDisponible DECIMAL(18,2) = (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = @DetalleProductoId);
            
            IF @StockDisponible < @Cantidad
            BEGIN
                SET @MensajeError = 'Stock insuficiente para el producto con ID: ' + CAST(@DetalleProductoId AS NVARCHAR);
                CLOSE venta_detalle_cursor;
                DEALLOCATE venta_detalle_cursor;
                ROLLBACK TRANSACTION;
                RETURN;
            END
            
            SET @Subtotal = (@PrecioUnitario * @Cantidad) - @Descuento;
            SET @SubtotalTotal = @SubtotalTotal + @Subtotal;
            SET @DescuentoTotal = @DescuentoTotal + @Descuento;
            
            -- Insertar detalle
            INSERT INTO [VentasDetalle] ([VentaId], [DetalleProductoId], [Cantidad], [PrecioUnitario], [Descuento], [Subtotal])
            VALUES (@VentaId, @DetalleProductoId, @Cantidad, @PrecioUnitario, @Descuento, @Subtotal);
            
            -- Obtener stock actual antes del movimiento
            DECLARE @StockAnteriorVenta DECIMAL(18,2) = @StockDisponible;
            DECLARE @StockNuevoVenta DECIMAL(18,2) = @StockAnteriorVenta - @Cantidad;
            
            -- Crear movimiento de stock (Salida)
            INSERT INTO [MovimientosStock] ([DetalleProductoId], [TipoMovimiento], [Cantidad], [StockAnterior], [StockNuevo], [ReferenciaId], [ReferenciaTipo], [UsuarioId], [Motivo], [FechaMovimiento])
            VALUES (@DetalleProductoId, 'Salida', @Cantidad, @StockAnteriorVenta, @StockNuevoVenta, @VentaId, 'Venta', @UsuarioId, 'Venta de productos', @FechaVenta);
            
            -- El trigger actualizará automáticamente DetalleProducto.Stock
            
            FETCH NEXT FROM venta_detalle_cursor INTO @DetalleProductoId, @Cantidad, @PrecioUnitario, @Descuento;
        END
        
        CLOSE venta_detalle_cursor;
        DEALLOCATE venta_detalle_cursor;
        
        -- Calcular impuestos y total
        DECLARE @ImpuestosVenta DECIMAL(18,2) = @SubtotalTotal * 0.15;
        DECLARE @TotalVenta DECIMAL(18,2) = @SubtotalTotal + @ImpuestosVenta;
        
        -- Actualizar totales de la venta
        UPDATE [Ventas]
        SET [Subtotal] = @SubtotalTotal,
            [Impuestos] = @ImpuestosVenta,
            [Descuento] = @DescuentoTotal,
            [Total] = @TotalVenta
        WHERE [Id] = @VentaId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SET @VentaId = -1;
        SET @MensajeError = ERROR_MESSAGE();
    END CATCH
END
GO

-- sp_Venta_ObtenerPorId: Obtener venta con detalles
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Venta_ObtenerPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Venta_ObtenerPorId]
GO

CREATE PROCEDURE [dbo].[sp_Venta_ObtenerPorId]
    @VentaId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Encabezado
    SELECT 
        v.[Id],
        v.[Folio],
        v.[ClienteId],
        c.[NombreCompleto] AS ClienteNombre,
        v.[UsuarioId],
        u.[NombreUsuario] AS UsuarioNombre,
        v.[EmpleadoId],
        e.[NombreCompleto] AS EmpleadoNombre,
        v.[FechaVenta],
        v.[Subtotal],
        v.[Impuestos],
        v.[Descuento],
        v.[Total],
        v.[MetodoPago],
        v.[Estado],
        v.[Observaciones],
        v.[FechaCreacion],
        v.[FechaModificacion]
    FROM [Ventas] v
    LEFT JOIN [Clientes] c ON v.[ClienteId] = c.[Id]
    INNER JOIN [Usuarios] u ON v.[UsuarioId] = u.[Id]
    LEFT JOIN [Empleados] e ON v.[EmpleadoId] = e.[Id]
    WHERE v.[Id] = @VentaId;
    
    -- Detalles
    SELECT 
        vd.[Id],
        vd.[VentaId],
        vd.[DetalleProductoId],
        dp.[Codigo] AS ProductoCodigo,
        p.[Nombre] AS ProductoNombre,
        m.[Nombre] AS MarcaNombre,
        cat.[Nombre] AS CategoriaNombre,
        vd.[Cantidad],
        vd.[PrecioUnitario],
        vd.[Descuento],
        vd.[Subtotal]
    FROM [VentasDetalle] vd
    INNER JOIN [DetalleProducto] dp ON vd.[DetalleProductoId] = dp.[Id]
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Marcas] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [Categorias] cat ON dp.[CategoriaId] = cat.[Id]
    WHERE vd.[VentaId] = @VentaId;
END
GO

-- sp_Venta_MostrarPorRangoFechas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Venta_MostrarPorRangoFechas]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Venta_MostrarPorRangoFechas]
GO

CREATE PROCEDURE [dbo].[sp_Venta_MostrarPorRangoFechas]
    @FechaInicio DATETIME2,
    @FechaFin DATETIME2,
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        v.[Id],
        v.[Folio],
        v.[ClienteId],
        c.[NombreCompleto] AS ClienteNombre,
        v.[UsuarioId],
        u.[NombreUsuario],
        v.[EmpleadoId],
        e.[NombreCompleto] AS EmpleadoNombre,
        v.[FechaVenta],
        v.[Subtotal],
        v.[Impuestos],
        v.[Descuento],
        v.[Total],
        v.[MetodoPago],
        v.[Estado]
    FROM [Ventas] v
    LEFT JOIN [Clientes] c ON v.[ClienteId] = c.[Id]
    INNER JOIN [Usuarios] u ON v.[UsuarioId] = u.[Id]
    LEFT JOIN [Empleados] e ON v.[EmpleadoId] = e.[Id]
    WHERE v.[FechaVenta] BETWEEN @FechaInicio AND @FechaFin
    ORDER BY v.[FechaVenta] DESC;
END
GO

-- sp_Venta_MostrarActivas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Venta_MostrarActivas]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Venta_MostrarActivas]
GO

CREATE PROCEDURE [dbo].[sp_Venta_MostrarActivas]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        v.[Id],
        v.[Folio],
        v.[ClienteId],
        c.[NombreCompleto] AS ClienteNombre,
        v.[UsuarioId],
        u.[NombreUsuario],
        v.[EmpleadoId],
        e.[NombreCompleto] AS EmpleadoNombre,
        v.[FechaVenta],
        v.[Subtotal],
        v.[Impuestos],
        v.[Descuento],
        v.[Total],
        v.[MetodoPago],
        v.[Estado]
    FROM [Ventas] v
    LEFT JOIN [Clientes] c ON v.[ClienteId] = c.[Id]
    INNER JOIN [Usuarios] u ON v.[UsuarioId] = u.[Id]
    LEFT JOIN [Empleados] e ON v.[EmpleadoId] = e.[Id]
    WHERE v.[Estado] = 'Completada'
    ORDER BY v.[FechaVenta] DESC;
END
GO

-- =============================================
-- 5. PROCEDIMIENTOS PARA DEVOLUCIONES
-- =============================================

-- sp_DevolucionVenta_Crear: Crear devolución con movimiento de stock (entrada)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DevolucionVenta_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DevolucionVenta_Crear]
GO

CREATE PROCEDURE [dbo].[sp_DevolucionVenta_Crear]
    @Folio NVARCHAR(50),
    @VentaId INT,
    @UsuarioId INT,
    @FechaDevolucion DATETIME2 = NULL,
    @Motivo NVARCHAR(500),
    @Observaciones NVARCHAR(1000) = NULL,
    @DetallesDevolucion NVARCHAR(MAX), -- JSON: [{"VentaDetalleId":1,"DetalleProductoId":1,"CantidadDevolver":2,"Motivo":"Defectuoso"}]
    @DevolucionVentaId INT OUTPUT,
    @MensajeError NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @DevolucionVentaId = -1;
    SET @MensajeError = '';
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar que el folio no exista
        IF EXISTS (SELECT 1 FROM [DevolucionesVenta] WHERE [Folio] = @Folio)
        BEGIN
            SET @MensajeError = 'El folio de devolución ya existe';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validar que la venta exista
        IF NOT EXISTS (SELECT 1 FROM [Ventas] WHERE [Id] = @VentaId)
        BEGIN
            SET @MensajeError = 'La venta no existe';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Establecer fecha si no se proporciona
        IF @FechaDevolucion IS NULL
            SET @FechaDevolucion = GETUTCDATE();
        
        -- Crear encabezado de devolución
        INSERT INTO [DevolucionesVenta] ([Folio], [VentaId], [UsuarioId], [FechaDevolucion], [Motivo], [TotalDevolucion], [Estado], [Observaciones])
        VALUES (@Folio, @VentaId, @UsuarioId, @FechaDevolucion, @Motivo, 0, 'Completada', @Observaciones);
        
        SET @DevolucionVentaId = SCOPE_IDENTITY();
        
        -- Parsear JSON y crear detalles
        DECLARE @TotalDevolucion DECIMAL(18,2) = 0;
        DECLARE @VentaDetalleId INT;
        DECLARE @DetalleProductoId INT;
        DECLARE @CantidadDevolver DECIMAL(18,2);
        DECLARE @MotivoDetalle NVARCHAR(500);
        DECLARE @SubtotalDevolucion DECIMAL(18,2);
        
        -- Crear tabla temporal para detalles
        DECLARE @DetallesDevolucionTemp TABLE (
            VentaDetalleId INT,
            DetalleProductoId INT,
            CantidadDevolver DECIMAL(18,2),
            Motivo NVARCHAR(500)
        );
        
        -- Insertar detalles desde JSON
        INSERT INTO @DetallesDevolucionTemp (VentaDetalleId, DetalleProductoId, CantidadDevolver, Motivo)
        SELECT 
            CAST(JSON_VALUE(value, '$.VentaDetalleId') AS INT),
            CAST(JSON_VALUE(value, '$.DetalleProductoId') AS INT),
            CAST(JSON_VALUE(value, '$.CantidadDevolver') AS DECIMAL(18,2)),
            JSON_VALUE(value, '$.Motivo')
        FROM OPENJSON(@DetallesDevolucion);
        
        -- Insertar detalles y crear movimientos de stock
        DECLARE devolucion_detalle_cursor CURSOR FOR
        SELECT VentaDetalleId, DetalleProductoId, CantidadDevolver, Motivo
        FROM @DetallesDevolucionTemp;
        
        OPEN devolucion_detalle_cursor;
        FETCH NEXT FROM devolucion_detalle_cursor INTO @VentaDetalleId, @DetalleProductoId, @CantidadDevolver, @MotivoDetalle;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Validar que la cantidad a devolver no exceda la cantidad vendida
            DECLARE @CantidadVendida DECIMAL(18,2) = (SELECT [Cantidad] FROM [VentasDetalle] WHERE [Id] = @VentaDetalleId);
            
            IF @CantidadDevolver > @CantidadVendida
            BEGIN
                SET @MensajeError = 'La cantidad a devolver excede la cantidad vendida';
                CLOSE devolucion_detalle_cursor;
                DEALLOCATE devolucion_detalle_cursor;
                ROLLBACK TRANSACTION;
                RETURN;
            END
            
            -- Calcular subtotal de devolución (proporcional)
            DECLARE @SubtotalOriginal DECIMAL(18,2) = (SELECT [Subtotal] FROM [VentasDetalle] WHERE [Id] = @VentaDetalleId);
            SET @SubtotalDevolucion = (@SubtotalOriginal / @CantidadVendida) * @CantidadDevolver;
            SET @TotalDevolucion = @TotalDevolucion + @SubtotalDevolucion;
            
            -- Insertar detalle de devolución
            INSERT INTO [DevolucionesVentaDetalle] ([DevolucionVentaId], [VentaDetalleId], [DetalleProductoId], [CantidadDevolver], [Motivo], [Subtotal])
            VALUES (@DevolucionVentaId, @VentaDetalleId, @DetalleProductoId, @CantidadDevolver, @MotivoDetalle, @SubtotalDevolucion);
            
            -- Obtener stock actual antes del movimiento
            DECLARE @StockAnteriorDevolucion DECIMAL(18,2) = (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = @DetalleProductoId);
            DECLARE @StockNuevoDevolucion DECIMAL(18,2) = @StockAnteriorDevolucion + @CantidadDevolver;
            
            -- Crear movimiento de stock (Entrada)
            INSERT INTO [MovimientosStock] ([DetalleProductoId], [TipoMovimiento], [Cantidad], [StockAnterior], [StockNuevo], [ReferenciaId], [ReferenciaTipo], [UsuarioId], [Motivo], [FechaMovimiento])
            VALUES (@DetalleProductoId, 'Entrada', @CantidadDevolver, @StockAnteriorDevolucion, @StockNuevoDevolucion, @DevolucionVentaId, 'Devolucion', @UsuarioId, @MotivoDetalle, @FechaDevolucion);
            
            -- El trigger actualizará automáticamente DetalleProducto.Stock
            
            FETCH NEXT FROM devolucion_detalle_cursor INTO @VentaDetalleId, @DetalleProductoId, @CantidadDevolver, @MotivoDetalle;
        END
        
        CLOSE devolucion_detalle_cursor;
        DEALLOCATE devolucion_detalle_cursor;
        
        -- Actualizar total de la devolución
        UPDATE [DevolucionesVenta]
        SET [TotalDevolucion] = @TotalDevolucion
        WHERE [Id] = @DevolucionVentaId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SET @DevolucionVentaId = -1;
        SET @MensajeError = ERROR_MESSAGE();
    END CATCH
END
GO

-- sp_DevolucionVenta_ObtenerPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DevolucionVenta_ObtenerPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DevolucionVenta_ObtenerPorId]
GO

CREATE PROCEDURE [dbo].[sp_DevolucionVenta_ObtenerPorId]
    @DevolucionVentaId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Encabezado
    SELECT 
        dv.[Id],
        dv.[Folio],
        dv.[VentaId],
        v.[Folio] AS VentaFolio,
        dv.[UsuarioId],
        u.[NombreUsuario],
        dv.[FechaDevolucion],
        dv.[Motivo],
        dv.[TotalDevolucion],
        dv.[Estado],
        dv.[Observaciones],
        dv.[FechaCreacion],
        dv.[FechaModificacion]
    FROM [DevolucionesVenta] dv
    INNER JOIN [Ventas] v ON dv.[VentaId] = v.[Id]
    INNER JOIN [Usuarios] u ON dv.[UsuarioId] = u.[Id]
    WHERE dv.[Id] = @DevolucionVentaId;
    
    -- Detalles
    SELECT 
        dvd.[Id],
        dvd.[DevolucionVentaId],
        dvd.[VentaDetalleId],
        dvd.[DetalleProductoId],
        dp.[Codigo] AS ProductoCodigo,
        p.[Nombre] AS ProductoNombre,
        m.[Nombre] AS MarcaNombre,
        dvd.[CantidadDevolver],
        dvd.[Motivo],
        dvd.[Subtotal]
    FROM [DevolucionesVentaDetalle] dvd
    INNER JOIN [DetalleProducto] dp ON dvd.[DetalleProductoId] = dp.[Id]
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Marcas] m ON dp.[MarcaId] = m.[Id]
    WHERE dvd.[DevolucionVentaId] = @DevolucionVentaId;
END
GO

-- =============================================
-- 6. PROCEDIMIENTOS PARA MOVIMIENTOSSTOCK
-- =============================================

-- sp_MovimientoStock_Ajuste: Ajuste manual de stock
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_MovimientoStock_Ajuste]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_MovimientoStock_Ajuste]
GO

CREATE PROCEDURE [dbo].[sp_MovimientoStock_Ajuste]
    @DetalleProductoId INT,
    @Cantidad DECIMAL(18,2), -- Positivo para aumentar, negativo para disminuir
    @UsuarioId INT,
    @Motivo NVARCHAR(500),
    @MovimientoStockId INT OUTPUT,
    @MensajeError NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @MovimientoStockId = -1;
    SET @MensajeError = '';
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar que el producto exista
        IF NOT EXISTS (SELECT 1 FROM [DetalleProducto] WHERE [Id] = @DetalleProductoId)
        BEGIN
            SET @MensajeError = 'El producto no existe';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Obtener stock actual
        DECLARE @StockAnterior DECIMAL(18,2) = (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = @DetalleProductoId);
        DECLARE @StockNuevo DECIMAL(18,2) = @StockAnterior + @Cantidad;
        
        -- Validar que el stock no sea negativo
        IF @StockNuevo < 0
        BEGIN
            SET @MensajeError = 'El ajuste resultaría en stock negativo';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Determinar tipo de movimiento
        DECLARE @TipoMovimiento NVARCHAR(50);
        IF @Cantidad > 0
            SET @TipoMovimiento = 'Entrada';
        ELSE IF @Cantidad < 0
            SET @TipoMovimiento = 'Salida';
        ELSE
            SET @TipoMovimiento = 'Ajuste';
        
        -- Crear movimiento de stock
        INSERT INTO [MovimientosStock] ([DetalleProductoId], [TipoMovimiento], [Cantidad], [StockAnterior], [StockNuevo], [ReferenciaId], [ReferenciaTipo], [UsuarioId], [Motivo], [FechaMovimiento])
        VALUES (@DetalleProductoId, @TipoMovimiento, ABS(@Cantidad), @StockAnterior, @StockNuevo, NULL, 'Ajuste', @UsuarioId, @Motivo, GETUTCDATE());
        
        SET @MovimientoStockId = SCOPE_IDENTITY();
        
        -- El trigger actualizará automáticamente DetalleProducto.Stock
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SET @MovimientoStockId = -1;
        SET @MensajeError = ERROR_MESSAGE();
    END CATCH
END
GO

-- sp_MovimientoStock_MostrarPorProducto
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_MovimientoStock_MostrarPorProducto]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_MovimientoStock_MostrarPorProducto]
GO

CREATE PROCEDURE [dbo].[sp_MovimientoStock_MostrarPorProducto]
    @DetalleProductoId INT,
    @FechaInicio DATETIME2 = NULL,
    @FechaFin DATETIME2 = NULL,
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETUTCDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETUTCDATE();
    
    SELECT TOP (@Top)
        ms.[Id],
        ms.[DetalleProductoId],
        dp.[Codigo] AS ProductoCodigo,
        p.[Nombre] AS ProductoNombre,
        ms.[TipoMovimiento],
        ms.[Cantidad],
        ms.[StockAnterior],
        ms.[StockNuevo],
        ms.[ReferenciaId],
        ms.[ReferenciaTipo],
        ms.[UsuarioId],
        u.[NombreUsuario],
        ms.[Motivo],
        ms.[FechaMovimiento]
    FROM [MovimientosStock] ms
    INNER JOIN [DetalleProducto] dp ON ms.[DetalleProductoId] = dp.[Id]
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Usuarios] u ON ms.[UsuarioId] = u.[Id]
    WHERE ms.[DetalleProductoId] = @DetalleProductoId
      AND ms.[FechaMovimiento] BETWEEN @FechaInicio AND @FechaFin
    ORDER BY ms.[FechaMovimiento] DESC;
END
GO

PRINT 'Procedimientos de Transacciones creados exitosamente';
GO

