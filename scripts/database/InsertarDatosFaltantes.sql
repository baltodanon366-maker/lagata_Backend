-- =============================================
-- Script para insertar datos faltantes en:
-- - Ventas
-- - VentasDetalle
-- - DevolucionesVenta
-- - DevolucionesVentaDetalle
-- - SesionesUsuario
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- =============================================
-- 1. VERIFICAR DEPENDENCIAS
-- =============================================

PRINT 'Verificando dependencias...';

IF NOT EXISTS (SELECT 1 FROM [Usuarios])
BEGIN
    PRINT 'ERROR: No hay usuarios. Ejecuta primero InsertTestData.sql';
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM [DetalleProducto])
BEGIN
    PRINT 'ERROR: No hay productos. Ejecuta primero InsertTestData.sql';
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM [Empleados])
BEGIN
    PRINT 'ADVERTENCIA: No hay empleados. Las ventas se crearán sin EmpleadoId.';
END

IF NOT EXISTS (SELECT 1 FROM [Clientes])
BEGIN
    PRINT 'ADVERTENCIA: No hay clientes. Las ventas se crearán sin ClienteId.';
END

PRINT 'Dependencias verificadas.';
GO

-- =============================================
-- 2. INSERTAR VENTAS Y DETALLES
-- =============================================

PRINT 'Creando ventas...';

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
DECLARE @DetalleProductoVentaId INT;
DECLARE @CantidadVenta DECIMAL(18,2);
DECLARE @PrecioUnitarioVenta DECIMAL(18,2);
DECLARE @Descuento DECIMAL(18,2);
DECLARE @SubtotalVenta DECIMAL(18,2);
DECLARE @SubtotalVentaTotal DECIMAL(18,2);
DECLARE @ImpuestosVenta DECIMAL(18,2);
DECLARE @TotalVenta DECIMAL(18,2);

-- Obtener IDs de usuarios
DECLARE @AdminUserId INT = (SELECT TOP 1 [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'admin' OR [Rol] = 'Administrador');
DECLARE @VendedorUserId INT = (SELECT TOP 1 [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'vendedor1' OR [Rol] = 'Vendedor');
DECLARE @SupervisorUserId INT = (SELECT TOP 1 [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'supervisor1' OR [Rol] = 'Supervisor');

IF @AdminUserId IS NULL SET @AdminUserId = (SELECT TOP 1 [Id] FROM [Usuarios]);
IF @VendedorUserId IS NULL SET @VendedorUserId = @AdminUserId;
IF @SupervisorUserId IS NULL SET @SupervisorUserId = @AdminUserId;

-- Obtener IDs disponibles
DECLARE @ClienteIds TABLE (ClienteId INT);
INSERT INTO @ClienteIds SELECT [Id] FROM [Clientes];

DECLARE @EmpleadoIds TABLE (EmpleadoId INT);
INSERT INTO @EmpleadoIds SELECT [Id] FROM [Empleados];

DECLARE @DetalleProductoIds TABLE (DetalleProductoId INT);
INSERT INTO @DetalleProductoIds SELECT [Id] FROM [DetalleProducto] WHERE [Activo] = 1;

SET NOCOUNT ON;

-- Verificar si ya hay ventas
DECLARE @VentasExistentes INT = (SELECT COUNT(*) FROM [Ventas]);
IF @VentasExistentes > 0
BEGIN
    PRINT 'Ya existen ' + CAST(@VentasExistentes AS VARCHAR) + ' ventas. Saltando creación de ventas.';
    SET NOCOUNT OFF;
END
ELSE
BEGIN
    WHILE @VentasCount <= 50
    BEGIN
        SET @FechaVenta = DATEADD(DAY, ABS(CHECKSUM(NEWID())) % DATEDIFF(DAY, @FechaInicioVenta, @FechaFinVenta), @FechaInicioVenta);
        SET @ClienteId = CASE 
            WHEN (SELECT COUNT(*) FROM @ClienteIds) > 0 AND @VentasCount % 4 <> 0 
            THEN (SELECT TOP 1 [ClienteId] FROM @ClienteIds ORDER BY NEWID()) 
            ELSE NULL 
        END;
        SET @EmpleadoId = CASE 
            WHEN (SELECT COUNT(*) FROM @EmpleadoIds) > 0 
            THEN (SELECT TOP 1 [EmpleadoId] FROM @EmpleadoIds ORDER BY NEWID()) 
            ELSE NULL 
        END;
        SET @UsuarioVentaId = CASE 
            WHEN @VentasCount % 3 = 0 THEN @SupervisorUserId 
            WHEN @VentasCount % 2 = 0 THEN @VendedorUserId 
            ELSE @AdminUserId 
        END;
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
            IF @PrecioUnitarioVenta IS NULL SET @PrecioUnitarioVenta = 100.00;
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
    PRINT '50 Ventas creadas exitosamente';
END
GO

-- =============================================
-- 3. INSERTAR DEVOLUCIONES
-- =============================================

PRINT 'Creando devoluciones...';

IF EXISTS (SELECT TOP 1 [Id] FROM [Ventas])
BEGIN
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
    DECLARE @TotalDevolucion DECIMAL(18,2) = 0;

    DECLARE @AdminUserIdDev INT = (SELECT TOP 1 [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'admin' OR [Rol] = 'Administrador');
    DECLARE @VendedorUserIdDev INT = (SELECT TOP 1 [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'vendedor1' OR [Rol] = 'Vendedor');
    DECLARE @SupervisorUserIdDev INT = (SELECT TOP 1 [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'supervisor1' OR [Rol] = 'Supervisor');

    IF @AdminUserIdDev IS NULL SET @AdminUserIdDev = (SELECT TOP 1 [Id] FROM [Usuarios]);
    IF @VendedorUserIdDev IS NULL SET @VendedorUserIdDev = @AdminUserIdDev;
    IF @SupervisorUserIdDev IS NULL SET @SupervisorUserIdDev = @AdminUserIdDev;

    DECLARE @VentaIdsDisponibles TABLE (VentaId INT);
    INSERT INTO @VentaIdsDisponibles (VentaId)
    SELECT DISTINCT v.[Id]
    FROM [Ventas] v
    INNER JOIN [VentasDetalle] vd ON v.[Id] = vd.[VentaId]
    WHERE NOT EXISTS (
        SELECT 1 FROM [DevolucionesVenta] dv 
        WHERE dv.[VentaId] = v.[Id] AND dv.[Estado] = 'Completada'
    );

    DECLARE @TotalVentasDisponibles INT = (SELECT COUNT(*) FROM @VentaIdsDisponibles);
    PRINT 'Ventas disponibles para devolución: ' + CAST(@TotalVentasDisponibles AS VARCHAR);

    SET NOCOUNT ON;

    DECLARE @DevolucionesExistentes INT = (SELECT COUNT(*) FROM [DevolucionesVenta]);
    IF @DevolucionesExistentes > 0
    BEGIN
        PRINT 'Ya existen ' + CAST(@DevolucionesExistentes AS VARCHAR) + ' devoluciones. Saltando creación de devoluciones.';
        SET NOCOUNT OFF;
    END
    ELSE
    BEGIN
        DECLARE @VentasDisponiblesCount INT = (SELECT COUNT(*) FROM @VentaIdsDisponibles);
        WHILE @DevolucionesCount <= 10 AND @VentasDisponiblesCount > 0
        BEGIN
            SELECT TOP 1 @VentaDevolucionId = [VentaId]
            FROM @VentaIdsDisponibles
            ORDER BY NEWID();

            IF @VentaDevolucionId IS NULL
                BREAK;

            SET @FechaDevolucion = DATEADD(DAY, ABS(CHECKSUM(NEWID())) % DATEDIFF(DAY, @FechaInicioDevolucion, @FechaFinDevolucion), @FechaInicioDevolucion);
            SET @UsuarioDevolucionId = CASE 
                WHEN @DevolucionesCount % 3 = 0 THEN @SupervisorUserIdDev 
                WHEN @DevolucionesCount % 2 = 0 THEN @VendedorUserIdDev 
                ELSE @AdminUserIdDev 
            END;
            SET @FolioDevolucion = 'DEV-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + RIGHT('0000' + CAST(@DevolucionesCount AS VARCHAR), 4);
            
            SELECT TOP 1 
                @VentaDetalleId = vd.[Id],
                @DetalleProductoDevolucionId = vd.[DetalleProductoId],
                @CantidadOriginal = vd.[Cantidad],
                @SubtotalOriginal = vd.[Subtotal]
            FROM [VentasDetalle] vd
            WHERE vd.[VentaId] = @VentaDevolucionId
              AND NOT EXISTS (
                  SELECT 1 FROM [DevolucionesVentaDetalle] dvd 
                  WHERE dvd.[VentaDetalleId] = vd.[Id]
              )
            ORDER BY NEWID();

            IF @VentaDetalleId IS NULL
            BEGIN
                DELETE FROM @VentaIdsDisponibles WHERE [VentaId] = @VentaDevolucionId;
                CONTINUE;
            END

            SET @CantidadDevolver = CASE 
                WHEN @CantidadOriginal <= 1 THEN 1
                WHEN @CantidadOriginal <= 3 THEN 1 + (ABS(CHECKSUM(NEWID())) % CAST(@CantidadOriginal AS INT))
                ELSE 1 + (ABS(CHECKSUM(NEWID())) % 3)
            END;

            SET @SubtotalDevolucion = (@SubtotalOriginal / @CantidadOriginal) * @CantidadDevolver;
            SET @TotalDevolucion = @SubtotalDevolucion;
            
            INSERT INTO [DevolucionesVenta] ([Folio], [VentaId], [UsuarioId], [FechaDevolucion], [Motivo], [TotalDevolucion], [Estado], [Observaciones])
            VALUES (@FolioDevolucion, @VentaDevolucionId, @UsuarioDevolucionId, @FechaDevolucion, 'Devolución de prueba', @TotalDevolucion, 'Completada', 'Devolución de prueba para pruebas del sistema');
            
            SET @DevolucionVentaId = SCOPE_IDENTITY();
            
            INSERT INTO [DevolucionesVentaDetalle] ([DevolucionVentaId], [VentaDetalleId], [DetalleProductoId], [CantidadDevolver], [Motivo], [Subtotal])
            VALUES (@DevolucionVentaId, @VentaDetalleId, @DetalleProductoDevolucionId, @CantidadDevolver, 'Producto defectuoso o devolución solicitada por cliente', @SubtotalDevolucion);
            
            DELETE FROM @VentaIdsDisponibles WHERE [VentaId] = @VentaDevolucionId;
            
            SET @DevolucionesCount = @DevolucionesCount + 1;
            SET @VentaDevolucionId = NULL;
            SET @VentaDetalleId = NULL;
            SET @VentasDisponiblesCount = (SELECT COUNT(*) FROM @VentaIdsDisponibles);
        END
        
        SET NOCOUNT OFF;
        DECLARE @TotalDevoluciones INT = (SELECT COUNT(*) FROM [DevolucionesVenta]);
        PRINT CAST(@TotalDevoluciones AS VARCHAR) + ' Devoluciones creadas exitosamente';
    END
END
ELSE
BEGIN
    PRINT 'ADVERTENCIA: No hay ventas. No se pueden crear devoluciones.';
END
GO

-- =============================================
-- 4. INSERTAR SESIONES DE USUARIO
-- =============================================

PRINT 'Creando sesiones de usuario...';

DECLARE @SesionesExistentes INT = (SELECT COUNT(*) FROM [SesionesUsuario]);
IF @SesionesExistentes > 0
BEGIN
    PRINT 'Ya existen ' + CAST(@SesionesExistentes AS VARCHAR) + ' sesiones. Saltando creación de sesiones.';
END
ELSE
BEGIN
    INSERT INTO [SesionesUsuario] ([UsuarioId], [Token], [FechaInicio], [FechaExpiracion], [IpAddress], [UserAgent], [Activa])
    SELECT 
        [Id],
        'TOKEN_' + CAST([Id] AS VARCHAR) + '_' + FORMAT(GETDATE(), 'yyyyMMddHHmmss'),
        DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 30, GETDATE()),
        DATEADD(HOUR, 24, DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 30, GETDATE())),
        '192.168.1.' + CAST(100 + [Id] AS VARCHAR),
        'Mozilla/5.0 (Windows NT 10.0; Win64; x64)',
        CASE WHEN ABS(CHECKSUM(NEWID())) % 3 = 0 THEN 0 ELSE 1 END
    FROM [Usuarios]
    WHERE [Activo] = 1;

    DECLARE @TotalSesiones INT = (SELECT COUNT(*) FROM [SesionesUsuario]);
    PRINT CAST(@TotalSesiones AS VARCHAR) + ' Sesiones de usuario creadas exitosamente';
END
GO

-- =============================================
-- 5. CREAR MOVIMIENTOS DE STOCK PARA VENTAS (SALIDAS)
-- =============================================

PRINT 'Creando movimientos de stock para ventas (salidas)...';

-- Crear movimientos de stock desde ventas (salidas)
-- Ordenar por fecha para calcular stock correctamente
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
    WHERE NOT EXISTS (
        SELECT 1 FROM [MovimientosStock] ms
        WHERE ms.[ReferenciaId] = v.[Id] 
          AND ms.[ReferenciaTipo] = 'Venta'
          AND ms.[DetalleProductoId] = vd.[DetalleProductoId]
    )
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

DECLARE @TotalMovimientosSalida INT = (SELECT COUNT(*) FROM [MovimientosStock] WHERE [TipoMovimiento] = 'Salida');
PRINT CAST(@TotalMovimientosSalida AS VARCHAR) + ' Movimientos de stock (salidas) creados exitosamente';
GO

-- =============================================
-- 6. CREAR MOVIMIENTOS DE STOCK PARA DEVOLUCIONES (ENTRADAS)
-- =============================================

PRINT 'Creando movimientos de stock para devoluciones (entradas)...';

IF EXISTS (SELECT 1 FROM [DevolucionesVenta])
BEGIN
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
    WHERE NOT EXISTS (
        SELECT 1 FROM [MovimientosStock] ms
        WHERE ms.[ReferenciaId] = dv.[Id] 
          AND ms.[ReferenciaTipo] = 'Devolucion'
          AND ms.[DetalleProductoId] = dvd.[DetalleProductoId]
    )
    ORDER BY dv.[FechaDevolucion], dv.[Id];

    DECLARE @TotalMovimientosDevolucion INT = (SELECT COUNT(*) FROM [MovimientosStock] WHERE [TipoMovimiento] = 'Entrada' AND [ReferenciaTipo] = 'Devolucion');
    PRINT CAST(@TotalMovimientosDevolucion AS VARCHAR) + ' Movimientos de stock (devoluciones) creados exitosamente';
END
ELSE
BEGIN
    PRINT 'ADVERTENCIA: No hay devoluciones. No se pueden crear movimientos de stock para devoluciones.';
END
GO

-- =============================================
-- RESUMEN
-- =============================================

DECLARE @ResumenVentas INT = (SELECT COUNT(*) FROM [Ventas]);
DECLARE @ResumenVentasDetalle INT = (SELECT COUNT(*) FROM [VentasDetalle]);
DECLARE @ResumenDevoluciones INT = (SELECT COUNT(*) FROM [DevolucionesVenta]);
DECLARE @ResumenDevolucionesDetalle INT = (SELECT COUNT(*) FROM [DevolucionesVentaDetalle]);
DECLARE @ResumenSesiones INT = (SELECT COUNT(*) FROM [SesionesUsuario]);
DECLARE @ResumenMovimientos INT = (SELECT COUNT(*) FROM [MovimientosStock]);

PRINT '=============================================';
PRINT 'Script completado';
PRINT '=============================================';
PRINT 'Ventas: ' + CAST(@ResumenVentas AS VARCHAR);
PRINT 'VentasDetalle: ' + CAST(@ResumenVentasDetalle AS VARCHAR);
PRINT 'DevolucionesVenta: ' + CAST(@ResumenDevoluciones AS VARCHAR);
PRINT 'DevolucionesVentaDetalle: ' + CAST(@ResumenDevolucionesDetalle AS VARCHAR);
PRINT 'SesionesUsuario: ' + CAST(@ResumenSesiones AS VARCHAR);
PRINT 'MovimientosStock (Total): ' + CAST(@ResumenMovimientos AS VARCHAR);
PRINT '=============================================';

