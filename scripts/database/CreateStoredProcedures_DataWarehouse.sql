-- =============================================
-- PROCEDIMIENTOS ALMACENADOS - DATA WAREHOUSE
-- Base de Datos: dbLicoreriaDW
-- Para exponer endpoints de Analytics desde la API
-- =============================================

USE [dbLicoreriaDW]
GO

-- =============================================
-- 1. PROCEDIMIENTOS PARA HECHO VENTAS
-- =============================================

-- sp_DW_Ventas_PorRangoFechas: Ventas agregadas por rango de fechas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Ventas_PorRangoFechas]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Ventas_PorRangoFechas]
GO

CREATE PROCEDURE [dbo].[sp_DW_Ventas_PorRangoFechas]
    @FechaInicio DATE,
    @FechaFin DATE,
    @AgruparPor NVARCHAR(50) = 'Dia' -- Dia, Semana, Mes, Año
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @AgruparPor = 'Dia'
    BEGIN
        SELECT 
            t.[Fecha] AS Fecha,
            SUM(hv.[CantidadVendida]) AS TotalCantidad,
            SUM(hv.[TotalVentas]) AS TotalVentas,
            SUM(hv.[DescuentoTotal]) AS TotalDescuento,
            SUM(hv.[ImpuestosTotal]) AS TotalImpuestos,
            SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
            AVG(hv.[PromedioTicket]) AS PromedioTicket
        FROM [HechoVenta] hv
        INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
        WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
        GROUP BY t.[Fecha]
        ORDER BY t.[Fecha];
    END
    ELSE IF @AgruparPor = 'Semana'
    BEGIN
        SELECT 
            t.[Anio],
            t.[Semana],
            SUM(hv.[CantidadVendida]) AS TotalCantidad,
            SUM(hv.[TotalVentas]) AS TotalVentas,
            SUM(hv.[DescuentoTotal]) AS TotalDescuento,
            SUM(hv.[ImpuestosTotal]) AS TotalImpuestos,
            SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
            AVG(hv.[PromedioTicket]) AS PromedioTicket
        FROM [HechoVenta] hv
        INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
        WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
        GROUP BY t.[Anio], t.[Semana]
        ORDER BY t.[Anio], t.[Semana];
    END
    ELSE IF @AgruparPor = 'Mes'
    BEGIN
        SELECT 
            t.[Anio],
            t.[Mes],
            t.[NombreMes],
            SUM(hv.[CantidadVendida]) AS TotalCantidad,
            SUM(hv.[TotalVentas]) AS TotalVentas,
            SUM(hv.[DescuentoTotal]) AS TotalDescuento,
            SUM(hv.[ImpuestosTotal]) AS TotalImpuestos,
            SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
            AVG(hv.[PromedioTicket]) AS PromedioTicket
        FROM [HechoVenta] hv
        INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
        WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
        GROUP BY t.[Anio], t.[Mes], t.[NombreMes]
        ORDER BY t.[Anio], t.[Mes];
    END
    ELSE IF @AgruparPor = 'Año'
    BEGIN
        SELECT 
            t.[Anio],
            SUM(hv.[CantidadVendida]) AS TotalCantidad,
            SUM(hv.[TotalVentas]) AS TotalVentas,
            SUM(hv.[DescuentoTotal]) AS TotalDescuento,
            SUM(hv.[ImpuestosTotal]) AS TotalImpuestos,
            SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
            AVG(hv.[PromedioTicket]) AS PromedioTicket
        FROM [HechoVenta] hv
        INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
        WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
        GROUP BY t.[Anio]
        ORDER BY t.[Anio];
    END
END
GO

-- sp_DW_Ventas_PorProducto: Ventas agregadas por producto
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Ventas_PorProducto]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Ventas_PorProducto]
GO

CREATE PROCEDURE [dbo].[sp_DW_Ventas_PorProducto]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL,
    @Top INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT TOP (@Top)
        dp.[Id] AS ProductoId,
        dp.[ProductoNombre] AS ProductoNombre,
        dp.[ProductoCodigo] AS ProductoCodigo,
        cat.[CategoriaNombre] AS CategoriaNombre,
        m.[MarcaNombre] AS MarcaNombre,
        SUM(hv.[CantidadVendida]) AS TotalCantidadVendida,
        SUM(hv.[TotalVentas]) AS TotalVentas,
        AVG(hv.[PromedioTicket]) AS PromedioTicket,
        SUM(hv.[CantidadTransacciones]) AS NumeroVentas
    FROM [HechoVenta] hv
    INNER JOIN [DimProducto] dp ON hv.[ProductoId] = dp.[Id]
    INNER JOIN [DimCategoria] cat ON dp.[CategoriaId] = cat.[Id]
    INNER JOIN [DimMarca] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
    GROUP BY dp.[Id], dp.[ProductoNombre], dp.[ProductoCodigo], cat.[CategoriaNombre], m.[MarcaNombre]
    ORDER BY SUM(hv.[TotalVentas]) DESC;
END
GO

-- sp_DW_Ventas_PorCategoria: Ventas agregadas por categoría
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Ventas_PorCategoria]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Ventas_PorCategoria]
GO

CREATE PROCEDURE [dbo].[sp_DW_Ventas_PorCategoria]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT 
        cat.[Id] AS CategoriaId,
        cat.[CategoriaNombre],
        SUM(hv.[CantidadVendida]) AS TotalCantidad,
        SUM(hv.[TotalVentas]) AS TotalVentas,
        COUNT(DISTINCT hv.[ProductoId]) AS NumeroProductos,
        SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
        (SUM(hv.[TotalVentas]) * 100.0 / (SELECT SUM([TotalVentas]) FROM [HechoVenta] hv2 INNER JOIN [DimTiempo] t2 ON hv2.[FechaId] = t2.[Id] WHERE t2.[Fecha] BETWEEN @FechaInicio AND @FechaFin)) AS PorcentajeTotal
    FROM [HechoVenta] hv
    INNER JOIN [DimProducto] dp ON hv.[ProductoId] = dp.[Id]
    INNER JOIN [DimCategoria] cat ON dp.[CategoriaId] = cat.[Id]
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
    GROUP BY cat.[Id], cat.[CategoriaNombre]
    ORDER BY SUM(hv.[TotalVentas]) DESC;
END
GO

-- sp_DW_Ventas_PorCliente: Ventas agregadas por cliente
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Ventas_PorCliente]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Ventas_PorCliente]
GO

CREATE PROCEDURE [dbo].[sp_DW_Ventas_PorCliente]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL,
    @Top INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT TOP (@Top)
        c.[Id] AS ClienteId,
        c.[ClienteNombre],
        c.[ClienteCodigo],
        SUM(hv.[CantidadVendida]) AS TotalCantidadComprada,
        SUM(hv.[TotalVentas]) AS TotalVentas,
        SUM(hv.[CantidadTransacciones]) AS NumeroCompras,
        AVG(hv.[PromedioTicket]) AS PromedioCompra
    FROM [HechoVenta] hv
    INNER JOIN [DimCliente] c ON hv.[ClienteId] = c.[Id]
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
      AND c.[Id] IS NOT NULL -- Solo clientes registrados
    GROUP BY c.[Id], c.[ClienteNombre], c.[ClienteCodigo]
    ORDER BY SUM(hv.[TotalVentas]) DESC;
END
GO

-- sp_DW_Ventas_PorEmpleado: Ventas agregadas por empleado
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Ventas_PorEmpleado]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Ventas_PorEmpleado]
GO

CREATE PROCEDURE [dbo].[sp_DW_Ventas_PorEmpleado]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT 
        e.[Id] AS EmpleadoId,
        e.[EmpleadoNombre],
        e.[EmpleadoCodigo],
        e.[Departamento],
        e.[Puesto],
        SUM(hv.[CantidadVendida]) AS TotalCantidadVendida,
        SUM(hv.[TotalVentas]) AS TotalVentas,
        SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
        AVG(hv.[PromedioTicket]) AS PromedioVenta
    FROM [HechoVenta] hv
    INNER JOIN [DimEmpleado] e ON hv.[EmpleadoId] = e.[Id]
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
      AND e.[Id] IS NOT NULL
    GROUP BY e.[Id], e.[EmpleadoNombre], e.[EmpleadoCodigo], e.[Departamento], e.[Puesto]
    ORDER BY SUM(hv.[TotalVentas]) DESC;
END
GO

-- sp_DW_Ventas_MetodoPago: Ventas por método de pago
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Ventas_MetodoPago]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Ventas_MetodoPago]
GO

CREATE PROCEDURE [dbo].[sp_DW_Ventas_MetodoPago]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    -- Nota: MetodoPago no está en HechoVenta, se puede agregar como campo calculado o en una dimensión separada
    -- Por ahora retornamos datos básicos
    SELECT 
        'Todos' AS MetodoPago,
        SUM(hv.[CantidadVendida]) AS TotalCantidad,
        SUM(hv.[TotalVentas]) AS TotalVentas,
        SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
        (SUM(hv.[TotalVentas]) * 100.0 / (SELECT SUM([TotalVentas]) FROM [HechoVenta] hv2 INNER JOIN [DimTiempo] t2 ON hv2.[FechaId] = t2.[Id] WHERE t2.[Fecha] BETWEEN @FechaInicio AND @FechaFin)) AS PorcentajeTotal
    FROM [HechoVenta] hv
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
    ORDER BY SUM(hv.[TotalVentas]) DESC;
END
GO

-- =============================================
-- 2. PROCEDIMIENTOS PARA HECHO COMPRAS
-- =============================================

-- sp_DW_Compras_PorRangoFechas: Compras agregadas por rango de fechas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Compras_PorRangoFechas]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Compras_PorRangoFechas]
GO

CREATE PROCEDURE [dbo].[sp_DW_Compras_PorRangoFechas]
    @FechaInicio DATE,
    @FechaFin DATE,
    @AgruparPor NVARCHAR(50) = 'Dia'
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @AgruparPor = 'Dia'
    BEGIN
        SELECT 
            t.[Fecha] AS Fecha,
            SUM(hc.[CantidadComprada]) AS TotalCantidad,
            SUM(hc.[TotalCompras]) AS TotalCompras,
            SUM(hc.[ImpuestosTotal]) AS TotalImpuestos,
            SUM(hc.[CantidadTransacciones]) AS NumeroCompras,
            AVG(hc.[PromedioCompra]) AS PromedioCompra
        FROM [HechoCompra] hc
        INNER JOIN [DimTiempo] t ON hc.[FechaId] = t.[Id]
        WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
        GROUP BY t.[Fecha]
        ORDER BY t.[Fecha];
    END
    ELSE IF @AgruparPor = 'Mes'
    BEGIN
        SELECT 
            t.[Anio],
            t.[Mes],
            t.[NombreMes],
            SUM(hc.[CantidadComprada]) AS TotalCantidad,
            SUM(hc.[TotalCompras]) AS TotalCompras,
            SUM(hc.[ImpuestosTotal]) AS TotalImpuestos,
            SUM(hc.[CantidadTransacciones]) AS NumeroCompras,
            AVG(hc.[PromedioCompra]) AS PromedioCompra
        FROM [HechoCompra] hc
        INNER JOIN [DimTiempo] t ON hc.[FechaId] = t.[Id]
        WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
        GROUP BY t.[Anio], t.[Mes], t.[NombreMes]
        ORDER BY t.[Anio], t.[Mes];
    END
END
GO

-- sp_DW_Compras_PorProveedor: Compras agregadas por proveedor
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Compras_PorProveedor]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Compras_PorProveedor]
GO

CREATE PROCEDURE [dbo].[sp_DW_Compras_PorProveedor]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL,
    @Top INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT TOP (@Top)
        prov.[Id] AS ProveedorId,
        prov.[ProveedorNombre],
        prov.[ProveedorCodigo],
        SUM(hc.[CantidadComprada]) AS TotalCantidadComprada,
        SUM(hc.[TotalCompras]) AS TotalCompras,
        SUM(hc.[CantidadTransacciones]) AS NumeroCompras,
        AVG(hc.[PromedioCompra]) AS PromedioCompra
    FROM [HechoCompra] hc
    INNER JOIN [DimProveedor] prov ON hc.[ProveedorId] = prov.[Id]
    INNER JOIN [DimTiempo] t ON hc.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
    GROUP BY prov.[Id], prov.[ProveedorNombre], prov.[ProveedorCodigo]
    ORDER BY SUM(hc.[TotalCompras]) DESC;
END
GO

-- sp_DW_Compras_PorProducto: Compras agregadas por producto
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Compras_PorProducto]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Compras_PorProducto]
GO

CREATE PROCEDURE [dbo].[sp_DW_Compras_PorProducto]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL,
    @Top INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT TOP (@Top)
        dp.[Id] AS ProductoId,
        dp.[ProductoNombre],
        dp.[ProductoCodigo],
        cat.[CategoriaNombre],
        m.[MarcaNombre],
        SUM(hc.[CantidadComprada]) AS TotalCantidadComprada,
        SUM(hc.[TotalCompras]) AS TotalCompras,
        AVG(hc.[PromedioCompra]) AS PrecioPromedioCompra,
        SUM(hc.[CantidadTransacciones]) AS NumeroCompras
    FROM [HechoCompra] hc
    INNER JOIN [DimProducto] dp ON hc.[ProductoId] = dp.[Id]
    INNER JOIN [DimCategoria] cat ON dp.[CategoriaId] = cat.[Id]
    INNER JOIN [DimMarca] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [DimTiempo] t ON hc.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
    GROUP BY dp.[Id], dp.[ProductoNombre], dp.[ProductoCodigo], cat.[CategoriaNombre], m.[MarcaNombre]
    ORDER BY SUM(hc.[TotalCompras]) DESC;
END
GO

-- =============================================
-- 3. PROCEDIMIENTOS PARA HECHO INVENTARIO
-- =============================================

-- sp_DW_Inventario_StockActual: Stock actual de todos los productos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Inventario_StockActual]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Inventario_StockActual]
GO

CREATE PROCEDURE [dbo].[sp_DW_Inventario_StockActual]
    @IncluirInactivos BIT = 0,
    @Top INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Query NVARCHAR(MAX) = '
    SELECT 
        hi.[ProductoId],
        dp.[Nombre] AS ProductoNombre,
        dp.[Codigo] AS ProductoCodigo,
        cat.[Nombre] AS CategoriaNombre,
        m.[Nombre] AS MarcaNombre,
        mo.[Nombre] AS ModeloNombre,
        hi.[StockActual],
        hi.[StockMinimo],
        CASE 
            WHEN hi.[StockActual] <= hi.[StockMinimo] THEN 1 
            ELSE 0 
        END AS StockBajo,
        dp.[PrecioCompraPromedio] AS PrecioCompra,
        dp.[PrecioVentaPromedio] AS PrecioVenta,
        hi.[ValorInventario],
        hi.[FechaProcesamiento] AS FechaActualizacion
    FROM [HechoInventario] hi
    INNER JOIN [DimProducto] dp ON hi.[ProductoId] = dp.[Id]
    INNER JOIN [DimCategoria] cat ON dp.[CategoriaId] = cat.[Id]
    INNER JOIN [DimMarca] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [DimModelo] mo ON dp.[ModeloId] = mo.[Id]
    WHERE 1=1';
    
    IF @IncluirInactivos = 0
        SET @Query = @Query + ' AND dp.[Activo] = 1';
    
    SET @Query = @Query + ' ORDER BY hi.[StockActual] ASC';
    
    IF @Top IS NOT NULL
        SET @Query = 'SELECT TOP (' + CAST(@Top AS NVARCHAR) + ') * FROM (' + @Query + ') AS Result';
    
    EXEC sp_executesql @Query;
END
GO

-- sp_DW_Inventario_ProductosStockBajo: Productos con stock bajo
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Inventario_ProductosStockBajo]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Inventario_ProductosStockBajo]
GO

CREATE PROCEDURE [dbo].[sp_DW_Inventario_ProductosStockBajo]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        hi.[ProductoId],
        dp.[ProductoNombre],
        dp.[ProductoCodigo],
        cat.[CategoriaNombre],
        m.[MarcaNombre],
        hi.[StockActual],
        hi.[StockMinimo],
        (hi.[StockMinimo] - hi.[StockActual]) AS CantidadFaltante,
        dp.[PrecioCompraPromedio] AS PrecioCompra,
        dp.[PrecioVentaPromedio] AS PrecioVenta,
        hi.[FechaProcesamiento] AS FechaActualizacion
    FROM [HechoInventario] hi
    INNER JOIN [DimProducto] dp ON hi.[ProductoId] = dp.[Id]
    INNER JOIN [DimCategoria] cat ON dp.[CategoriaId] = cat.[Id]
    INNER JOIN [DimMarca] m ON dp.[MarcaId] = m.[Id]
    WHERE hi.[StockActual] <= hi.[StockMinimo]
      AND dp.[Activo] = 1
    ORDER BY (hi.[StockMinimo] - hi.[StockActual]) DESC;
END
GO

-- sp_DW_Inventario_ValorInventario: Valor total del inventario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Inventario_ValorInventario]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Inventario_ValorInventario]
GO

CREATE PROCEDURE [dbo].[sp_DW_Inventario_ValorInventario]
    @PorCategoria BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @PorCategoria = 0
    BEGIN
        -- Valor total del inventario
        SELECT 
            COUNT(*) AS NumeroProductos,
            SUM(hi.[StockActual]) AS TotalUnidades,
            SUM(hi.[ValorInventario]) AS ValorTotalInventario,
            AVG(hi.[ValorInventario]) AS ValorPromedioPorProducto
        FROM [HechoInventario] hi
        INNER JOIN [DimProducto] dp ON hi.[ProductoId] = dp.[Id]
        WHERE dp.[Activo] = 1;
    END
    ELSE
    BEGIN
        -- Valor del inventario por categoría
        SELECT 
            cat.[Id] AS CategoriaId,
            cat.[CategoriaNombre],
            COUNT(*) AS NumeroProductos,
            SUM(hi.[StockActual]) AS TotalUnidades,
            SUM(hi.[ValorInventario]) AS ValorTotalInventario,
            AVG(hi.[ValorInventario]) AS ValorPromedioPorProducto
        FROM [HechoInventario] hi
        INNER JOIN [DimProducto] dp ON hi.[ProductoId] = dp.[Id]
        INNER JOIN [DimCategoria] cat ON dp.[CategoriaId] = cat.[Id]
        WHERE dp.[Activo] = 1
        GROUP BY cat.[Id], cat.[CategoriaNombre]
        ORDER BY SUM(hi.[ValorInventario]) DESC;
    END
END
GO

-- =============================================
-- 4. PROCEDIMIENTOS PARA MÉTRICAS Y KPIs
-- =============================================

-- sp_DW_Metricas_Dashboard: Métricas principales para dashboard
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Metricas_Dashboard]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Metricas_Dashboard]
GO

CREATE PROCEDURE [dbo].[sp_DW_Metricas_Dashboard]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    -- Ventas del período
    SELECT 
        'Ventas' AS Metrica,
        SUM(hv.[CantidadTransacciones]) AS NumeroTransacciones,
        SUM(hv.[CantidadVendida]) AS TotalCantidad,
        SUM(hv.[TotalVentas]) AS TotalMonto,
        AVG(hv.[PromedioTicket]) AS PromedioTransaccion
    FROM [HechoVenta] hv
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
    
    UNION ALL
    
    -- Compras del período
    SELECT 
        'Compras' AS Metrica,
        SUM(hc.[CantidadTransacciones]) AS NumeroTransacciones,
        SUM(hc.[CantidadComprada]) AS TotalCantidad,
        SUM(hc.[TotalCompras]) AS TotalMonto,
        AVG(hc.[PromedioCompra]) AS PromedioTransaccion
    FROM [HechoCompra] hc
    INNER JOIN [DimTiempo] t ON hc.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
    
    UNION ALL
    
    -- Inventario actual
    SELECT 
        'Inventario' AS Metrica,
        COUNT(*) AS NumeroProductos,
        SUM(hi.[StockActual]) AS TotalUnidades,
        SUM(hi.[ValorInventario]) AS TotalMonto,
        AVG(hi.[ValorInventario]) AS PromedioPorProducto
    FROM [HechoInventario] hi
    INNER JOIN [DimProducto] dp ON hi.[ProductoId] = dp.[Id]
    WHERE dp.[Activo] = 1;
END
GO

-- sp_DW_Metricas_Tendencias: Tendencias de ventas (comparación períodos)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Metricas_Tendencias]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Metricas_Tendencias]
GO

CREATE PROCEDURE [dbo].[sp_DW_Metricas_Tendencias]
    @PeriodoActualInicio DATE,
    @PeriodoActualFin DATE,
    @PeriodoAnteriorInicio DATE,
    @PeriodoAnteriorFin DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Período actual
    SELECT 
        'Actual' AS Periodo,
        SUM(hv.[TotalVentas]) AS TotalVentas,
        SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
        AVG(hv.[PromedioTicket]) AS PromedioVenta,
        SUM(hv.[CantidadVendida]) AS TotalCantidad
    FROM [HechoVenta] hv
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @PeriodoActualInicio AND @PeriodoActualFin
    
    UNION ALL
    
    -- Período anterior
    SELECT 
        'Anterior' AS Periodo,
        SUM(hv.[TotalVentas]) AS TotalVentas,
        SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
        AVG(hv.[PromedioTicket]) AS PromedioVenta,
        SUM(hv.[CantidadVendida]) AS TotalCantidad
    FROM [HechoVenta] hv
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @PeriodoAnteriorInicio AND @PeriodoAnteriorFin;
END
GO

-- sp_DW_Metricas_ProductosMasVendidos: Top productos más vendidos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Metricas_ProductosMasVendidos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Metricas_ProductosMasVendidos]
GO

CREATE PROCEDURE [dbo].[sp_DW_Metricas_ProductosMasVendidos]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL,
    @Top INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT TOP (@Top)
        dp.[Id] AS ProductoId,
        dp.[ProductoNombre],
        dp.[ProductoCodigo],
        cat.[CategoriaNombre],
        m.[MarcaNombre],
        SUM(hv.[CantidadVendida]) AS TotalVendido,
        SUM(hv.[TotalVentas]) AS TotalVentas,
        SUM(hv.[CantidadTransacciones]) AS NumeroVentas,
        RANK() OVER (ORDER BY SUM(hv.[CantidadVendida]) DESC) AS Ranking
    FROM [HechoVenta] hv
    INNER JOIN [DimProducto] dp ON hv.[ProductoId] = dp.[Id]
    INNER JOIN [DimCategoria] cat ON dp.[CategoriaId] = cat.[Id]
    INNER JOIN [DimMarca] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
    GROUP BY dp.[Id], dp.[ProductoNombre], dp.[ProductoCodigo], cat.[CategoriaNombre], m.[MarcaNombre]
    ORDER BY SUM(hv.[CantidadVendida]) DESC;
END
GO

-- sp_DW_Metricas_ClientesMasFrecuentes: Top clientes más frecuentes
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Metricas_ClientesMasFrecuentes]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Metricas_ClientesMasFrecuentes]
GO

CREATE PROCEDURE [dbo].[sp_DW_Metricas_ClientesMasFrecuentes]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL,
    @Top INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -1, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT TOP (@Top)
        c.[Id] AS ClienteId,
        c.[ClienteNombre],
        c.[ClienteCodigo],
        SUM(hv.[CantidadTransacciones]) AS NumeroCompras,
        SUM(hv.[TotalVentas]) AS TotalGastado,
        AVG(hv.[PromedioTicket]) AS PromedioCompra,
        MAX(t.[Fecha]) AS UltimaCompra,
        RANK() OVER (ORDER BY SUM(hv.[CantidadTransacciones]) DESC) AS Ranking
    FROM [HechoVenta] hv
    INNER JOIN [DimCliente] c ON hv.[ClienteId] = c.[Id]
    INNER JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id]
    WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
      AND c.[Id] IS NOT NULL
    GROUP BY c.[Id], c.[ClienteNombre], c.[ClienteCodigo]
        ORDER BY SUM(hv.[CantidadTransacciones]) DESC;
END
GO

-- =============================================
-- 5. PROCEDIMIENTOS PARA REPORTES AVANZADOS
-- =============================================

-- sp_DW_Reporte_VentasVsCompras: Comparación ventas vs compras
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Reporte_VentasVsCompras]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Reporte_VentasVsCompras]
GO

CREATE PROCEDURE [dbo].[sp_DW_Reporte_VentasVsCompras]
    @FechaInicio DATE,
    @FechaFin DATE,
    @AgruparPor NVARCHAR(50) = 'Mes'
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @AgruparPor = 'Mes'
    BEGIN
        SELECT 
            t.[Anio],
            t.[Mes],
            t.[NombreMes],
            -- Ventas
            ISNULL(SUM(hv.[TotalVentas]), 0) AS TotalVentas,
            ISNULL(SUM(hv.[CantidadVendida]), 0) AS CantidadVendida,
            ISNULL(SUM(hv.[CantidadTransacciones]), 0) AS NumeroVentas,
            -- Compras
            ISNULL(SUM(hc.[TotalCompras]), 0) AS TotalCompras,
            ISNULL(SUM(hc.[CantidadComprada]), 0) AS CantidadComprada,
            ISNULL(SUM(hc.[CantidadTransacciones]), 0) AS NumeroCompras,
            -- Ganancia (Ventas - Compras)
            ISNULL(SUM(hv.[TotalVentas]), 0) - ISNULL(SUM(hc.[TotalCompras]), 0) AS GananciaBruta
        FROM [DimTiempo] t
        LEFT JOIN [HechoVenta] hv ON t.[Id] = hv.[FechaId]
        LEFT JOIN [HechoCompra] hc ON t.[Id] = hc.[FechaId]
        WHERE t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
        GROUP BY t.[Anio], t.[Mes], t.[NombreMes]
        ORDER BY t.[Anio], t.[Mes];
    END
END
GO

-- sp_DW_Reporte_RotacionInventario: Rotación de inventario por producto
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DW_Reporte_RotacionInventario]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DW_Reporte_RotacionInventario]
GO

CREATE PROCEDURE [dbo].[sp_DW_Reporte_RotacionInventario]
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL,
    @Top INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEADD(MONTH, -3, GETDATE());
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT TOP (@Top)
        dp.[Id] AS ProductoId,
        dp.[ProductoNombre] AS ProductoNombre,
        dp.[ProductoCodigo] AS ProductoCodigo,
        cat.[CategoriaNombre] AS CategoriaNombre,
        -- Stock actual
        hi.[StockActual],
        hi.[StockMinimo],
        -- Ventas del período
        ISNULL(SUM(hv.[CantidadVendida]), 0) AS CantidadVendida,
        -- Rotación (Ventas / Stock Promedio)
        CASE 
            WHEN hi.[StockActual] > 0 
            THEN (ISNULL(SUM(hv.[CantidadVendida]), 0) * 1.0 / hi.[StockActual])
            ELSE 0 
        END AS RotacionInventario,
        -- Días de inventario
        CASE 
            WHEN ISNULL(SUM(hv.[CantidadVendida]), 0) > 0 
            THEN (hi.[StockActual] * DATEDIFF(DAY, @FechaInicio, @FechaFin) * 1.0 / SUM(hv.[CantidadVendida]))
            ELSE NULL 
        END AS DiasInventario
    FROM [HechoInventario] hi
    INNER JOIN [DimProducto] dp ON hi.[ProductoId] = dp.[Id]
    INNER JOIN [DimCategoria] cat ON dp.[CategoriaId] = cat.[Id]
    LEFT JOIN [HechoVenta] hv ON dp.[Id] = hv.[ProductoId]
    LEFT JOIN [DimTiempo] t ON hv.[FechaId] = t.[Id] AND t.[Fecha] BETWEEN @FechaInicio AND @FechaFin
    WHERE dp.[Activo] = 1
    GROUP BY dp.[Id], dp.[ProductoNombre], dp.[ProductoCodigo], cat.[CategoriaNombre], hi.[StockActual], hi.[StockMinimo]
    HAVING ISNULL(SUM(hv.[CantidadVendida]), 0) > 0
    ORDER BY RotacionInventario DESC;
END
GO

PRINT '=============================================';
PRINT 'Procedimientos almacenados del Data Warehouse creados exitosamente';
PRINT '=============================================';
PRINT 'Total de procedimientos: 15';
PRINT '- Ventas: 6 procedimientos';
PRINT '- Compras: 3 procedimientos';
PRINT '- Inventario: 3 procedimientos';
PRINT '- Métricas/KPIs: 3 procedimientos';
PRINT '=============================================';

