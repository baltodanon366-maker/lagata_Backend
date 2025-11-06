-- =============================================
-- Script para Crear Solo Ventas y Devoluciones
-- Base de Datos: dbLicoreriaLaGata
-- Ejecutar si las ventas y devoluciones no se crearon
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- Variables para IDs
DECLARE @AdminUserId INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'admin');
DECLARE @VendedorUserId INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'vendedor1');
DECLARE @SupervisorUserId INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'supervisor1');

-- Obtener IDs necesarios
DECLARE @EmpleadoIds TABLE (EmpleadoId INT);
DECLARE @ClienteIds TABLE (ClienteId INT);
DECLARE @DetalleProductoIds TABLE (DetalleProductoId INT);

INSERT INTO @EmpleadoIds SELECT [Id] FROM [Empleados];
INSERT INTO @ClienteIds SELECT [Id] FROM [Clientes];
INSERT INTO @DetalleProductoIds SELECT [Id] FROM [DetalleProducto];

-- Crear 100 Ventas (últimos 6 meses)
DECLARE @VentasCount INT = 1;
DECLARE @FechaInicioVenta DATE = DATEADD(MONTH, -6, GETDATE());
DECLARE @FechaFinVenta DATE = GETDATE();
DECLARE @FechaVenta DATETIME2;
DECLARE @ClienteId INT;
DECLARE @EmpleadoId INT;
DECLARE @UsuarioVentaId INT;
DECLARE @VentaId INT;
DECLARE @FolioVenta NVARCHAR(50);
DECLARE @DetallesVentaCount INT;
DECLARE @SubtotalVentaTotal DECIMAL(18,2);
DECLARE @DetalleProductoVentaId INT;
DECLARE @CantidadVenta DECIMAL(18,2);
DECLARE @PrecioUnitarioVenta DECIMAL(18,2);
DECLARE @Descuento DECIMAL(18,2);
DECLARE @SubtotalVenta DECIMAL(18,2);
DECLARE @ImpuestosVenta DECIMAL(18,2);
DECLARE @TotalVenta DECIMAL(18,2);

SET NOCOUNT ON;

WHILE @VentasCount <= 100
BEGIN
    SET @FechaVenta = DATEADD(DAY, ABS(CHECKSUM(NEWID())) % DATEDIFF(DAY, @FechaInicioVenta, @FechaFinVenta), @FechaInicioVenta);
    SET @ClienteId = CASE WHEN @VentasCount % 4 = 0 THEN NULL ELSE (SELECT TOP 1 [ClienteId] FROM @ClienteIds ORDER BY NEWID()) END;
    SET @EmpleadoId = (SELECT TOP 1 [EmpleadoId] FROM @EmpleadoIds ORDER BY NEWID());
    SET @UsuarioVentaId = CASE WHEN @VentasCount % 3 = 0 THEN @SupervisorUserId ELSE @VendedorUserId END;
    SET @FolioVenta = 'VENT-' + RIGHT('0000' + CAST(@VentasCount AS VARCHAR), 4);
    
    INSERT INTO [Ventas] ([Folio], [ClienteId], [UsuarioId], [EmpleadoId], [FechaVenta], [Subtotal], [Impuestos], [Descuento], [Total], [MetodoPago], [Estado], [Observaciones])
    VALUES (@FolioVenta, @ClienteId, @UsuarioVentaId, @EmpleadoId, @FechaVenta, 0, 0, 0, 0, 
            CASE WHEN @VentasCount % 3 = 0 THEN 'Tarjeta' WHEN @VentasCount % 3 = 1 THEN 'Efectivo' ELSE 'Transferencia' END,
            'Completada', 'Venta de prueba');
    
    SET @VentaId = SCOPE_IDENTITY();
    SET @DetallesVentaCount = 1 + (ABS(CHECKSUM(NEWID())) % 4);
    SET @SubtotalVentaTotal = 0;
    
    WHILE @DetallesVentaCount > 0
    BEGIN
        SET @DetalleProductoVentaId = (SELECT TOP 1 [DetalleProductoId] FROM @DetalleProductoIds ORDER BY NEWID());
        SET @CantidadVenta = 1 + (ABS(CHECKSUM(NEWID())) % 10);
        SET @PrecioUnitarioVenta = (SELECT [PrecioVenta] FROM [DetalleProducto] WHERE [Id] = @DetalleProductoVentaId);
        SET @Descuento = CASE WHEN @VentasCount % 10 = 0 THEN @PrecioUnitarioVenta * @CantidadVenta * 0.1 ELSE 0 END;
        SET @SubtotalVenta = (@PrecioUnitarioVenta * @CantidadVenta) - @Descuento;
        
        INSERT INTO [VentasDetalle] ([VentaId], [DetalleProductoId], [Cantidad], [PrecioUnitario], [Descuento], [Subtotal])
        VALUES (@VentaId, @DetalleProductoVentaId, @CantidadVenta, @PrecioUnitarioVenta, @Descuento, @SubtotalVenta);
        
        SET @SubtotalVentaTotal = @SubtotalVentaTotal + @SubtotalVenta;
        SET @DetallesVentaCount = @DetallesVentaCount - 1;
    END
    
    SET @ImpuestosVenta = @SubtotalVentaTotal * 0.15;
    SET @TotalVenta = @SubtotalVentaTotal + @ImpuestosVenta;
    
    UPDATE [Ventas] SET [Subtotal] = @SubtotalVentaTotal, [Impuestos] = @ImpuestosVenta, [Total] = @TotalVenta WHERE [Id] = @VentaId;
    
    SET @VentasCount = @VentasCount + 1;
END

SET NOCOUNT OFF;
PRINT '100 Ventas creadas exitosamente';
GO

-- Crear 12 Devoluciones (últimos 3 meses)
DECLARE @DevolucionesCount INT = 1;
DECLARE @FechaInicioDevolucion DATE = DATEADD(MONTH, -3, GETDATE());
DECLARE @FechaFinDevolucion DATE = GETDATE();
DECLARE @FechaDevolucion DATETIME2;
DECLARE @VentaDevolucionId INT;
DECLARE @UsuarioDevolucionId INT;
DECLARE @DevolucionVentaId INT;
DECLARE @FolioDevolucion NVARCHAR(50);
DECLARE @VentaDetalleId INT;
DECLARE @DetalleProductoDevolucionId INT;
DECLARE @CantidadDevolver DECIMAL(18,2);
DECLARE @CantidadOriginal DECIMAL(18,2);
DECLARE @SubtotalOriginal DECIMAL(18,2);
DECLARE @SubtotalDevolucion DECIMAL(18,2);

DECLARE @VentaIds TABLE (VentaId INT);
INSERT INTO @VentaIds SELECT TOP 50 [Id] FROM [Ventas] ORDER BY NEWID();

SET NOCOUNT ON;

WHILE @DevolucionesCount <= 12
BEGIN
    SET @FechaDevolucion = DATEADD(DAY, ABS(CHECKSUM(NEWID())) % DATEDIFF(DAY, @FechaInicioDevolucion, @FechaFinDevolucion), @FechaInicioDevolucion);
    SET @VentaDevolucionId = (SELECT TOP 1 [VentaId] FROM @VentaIds ORDER BY NEWID());
    SET @UsuarioDevolucionId = CASE WHEN @DevolucionesCount % 2 = 0 THEN @SupervisorUserId ELSE @VendedorUserId END;
    SET @FolioDevolucion = 'DEV-' + RIGHT('0000' + CAST(@DevolucionesCount AS VARCHAR), 4);
    
    -- Obtener un detalle de la venta
    SET @VentaDetalleId = (SELECT TOP 1 [Id] FROM [VentasDetalle] WHERE [VentaId] = @VentaDevolucionId ORDER BY NEWID());
    SET @DetalleProductoDevolucionId = (SELECT [DetalleProductoId] FROM [VentasDetalle] WHERE [Id] = @VentaDetalleId);
    SET @CantidadOriginal = (SELECT [Cantidad] FROM [VentasDetalle] WHERE [Id] = @VentaDetalleId);
    SET @SubtotalOriginal = (SELECT [Subtotal] FROM [VentasDetalle] WHERE [Id] = @VentaDetalleId);
    SET @CantidadDevolver = 1 + (ABS(CHECKSUM(NEWID())) % CASE WHEN @CantidadOriginal > 3 THEN 3 ELSE CAST(@CantidadOriginal AS INT) END);
    SET @SubtotalDevolucion = (@SubtotalOriginal / @CantidadOriginal) * @CantidadDevolver;
    
    INSERT INTO [DevolucionesVenta] ([Folio], [VentaId], [UsuarioId], [FechaDevolucion], [Motivo], [TotalDevolucion], [Estado], [Observaciones])
    VALUES (@FolioDevolucion, @VentaDevolucionId, @UsuarioDevolucionId, @FechaDevolucion, 'Devolución de prueba', @SubtotalDevolucion, 'Completada', 'Devolución de prueba');
    
    SET @DevolucionVentaId = SCOPE_IDENTITY();
    
    INSERT INTO [DevolucionesVentaDetalle] ([DevolucionVentaId], [VentaDetalleId], [DetalleProductoId], [CantidadDevolver], [Motivo], [Subtotal])
    VALUES (@DevolucionVentaId, @VentaDetalleId, @DetalleProductoDevolucionId, @CantidadDevolver, 'Producto defectuoso', @SubtotalDevolucion);
    
    SET @DevolucionesCount = @DevolucionesCount + 1;
END

SET NOCOUNT OFF;
PRINT '12 Devoluciones creadas exitosamente';
GO

-- Crear movimientos de stock desde ventas (salidas)
WITH VentasOrdenadas AS (
    SELECT 
        vd.[Id] AS VentaDetalleId,
        vd.[DetalleProductoId],
        vd.[Cantidad],
        v.[Id] AS VentaId,
        v.[UsuarioId],
        v.[FechaVenta],
        ROW_NUMBER() OVER (PARTITION BY vd.[DetalleProductoId] ORDER BY v.[FechaVenta], v.[Id]) AS Orden
    FROM [VentasDetalle] vd
    INNER JOIN [Ventas] v ON vd.[VentaId] = v.[Id]
)
INSERT INTO [MovimientosStock] ([DetalleProductoId], [TipoMovimiento], [Cantidad], [StockAnterior], [StockNuevo], [ReferenciaId], [ReferenciaTipo], [UsuarioId], [Motivo], [FechaMovimiento])
SELECT 
    vo.[DetalleProductoId],
    'Salida',
    vo.[Cantidad],
    ISNULL((
        SELECT TOP 1 [StockNuevo] 
        FROM [MovimientosStock] 
        WHERE [DetalleProductoId] = vo.[DetalleProductoId] 
          AND [FechaMovimiento] < vo.[FechaVenta]
        ORDER BY [FechaMovimiento] DESC
    ), (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = vo.[DetalleProductoId])) AS StockAnterior,
    ISNULL((
        SELECT TOP 1 [StockNuevo] 
        FROM [MovimientosStock] 
        WHERE [DetalleProductoId] = vo.[DetalleProductoId] 
          AND [FechaMovimiento] < vo.[FechaVenta]
        ORDER BY [FechaMovimiento] DESC
    ), (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = vo.[DetalleProductoId])) - vo.[Cantidad] AS StockNuevo,
    vo.[VentaId] AS ReferenciaId,
    'Venta' AS ReferenciaTipo,
    vo.[UsuarioId],
    'Venta de productos',
    vo.[FechaVenta]
FROM VentasOrdenadas vo
ORDER BY vo.[FechaVenta], vo.[VentaId];
GO

-- Crear movimientos de stock desde devoluciones (entradas)
INSERT INTO [MovimientosStock] ([DetalleProductoId], [TipoMovimiento], [Cantidad], [StockAnterior], [StockNuevo], [ReferenciaId], [ReferenciaTipo], [UsuarioId], [Motivo], [FechaMovimiento])
SELECT 
    dvd.[DetalleProductoId],
    'Entrada',
    dvd.[CantidadDevolver],
    ISNULL((
        SELECT TOP 1 [StockNuevo] 
        FROM [MovimientosStock] 
        WHERE [DetalleProductoId] = dvd.[DetalleProductoId] 
          AND [FechaMovimiento] < dv.[FechaDevolucion]
        ORDER BY [FechaMovimiento] DESC
    ), (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = dvd.[DetalleProductoId])) AS StockAnterior,
    ISNULL((
        SELECT TOP 1 [StockNuevo] 
        FROM [MovimientosStock] 
        WHERE [DetalleProductoId] = dvd.[DetalleProductoId] 
          AND [FechaMovimiento] < dv.[FechaDevolucion]
        ORDER BY [FechaMovimiento] DESC
    ), (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = dvd.[DetalleProductoId])) + dvd.[CantidadDevolver] AS StockNuevo,
    dv.[Id] AS ReferenciaId,
    'Devolucion' AS ReferenciaTipo,
    dv.[UsuarioId],
    'Devolución de productos',
    dv.[FechaDevolucion]
FROM [DevolucionesVentaDetalle] dvd
INNER JOIN [DevolucionesVenta] dv ON dvd.[DevolucionVentaId] = dv.[Id]
ORDER BY dv.[FechaDevolucion], dv.[Id];
GO

PRINT 'MovimientosStock de ventas y devoluciones creados exitosamente';
GO

