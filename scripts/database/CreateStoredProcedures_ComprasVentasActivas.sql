-- =============================================
-- PROCEDIMIENTOS ALMACENADOS PARA COMPRAS Y VENTAS ACTIVAS
-- Base de Datos: dbLicoreriaLaGata
-- Para mostrar compras y ventas sin filtro de fecha
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- =============================================
-- sp_Compra_MostrarActivas
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Compra_MostrarActivas]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Compra_MostrarActivas]
GO

CREATE PROCEDURE [dbo].[sp_Compra_MostrarActivas]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Primero intentar obtener compras con estado 'Completada'
    -- Si no hay, mostrar todas las compras
    IF EXISTS (SELECT 1 FROM [Compras] WHERE [Estado] = 'Completada')
    BEGIN
        SELECT TOP (@Top)
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
        WHERE c.[Estado] = 'Completada'
        ORDER BY c.[FechaCompra] DESC;
    END
    ELSE
    BEGIN
        -- Si no hay compras con estado 'Completada', mostrar todas
        SELECT TOP (@Top)
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
        ORDER BY c.[FechaCompra] DESC;
    END
END
GO

-- =============================================
-- sp_Venta_MostrarActivas
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Venta_MostrarActivas]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Venta_MostrarActivas]
GO

CREATE PROCEDURE [dbo].[sp_Venta_MostrarActivas]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Primero intentar obtener ventas con estado 'Completada'
    -- Si no hay, mostrar todas las ventas
    IF EXISTS (SELECT 1 FROM [Ventas] WHERE [Estado] = 'Completada')
    BEGIN
        SELECT TOP (@Top)
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
        WHERE v.[Estado] = 'Completada'
        ORDER BY v.[FechaVenta] DESC;
    END
    ELSE
    BEGIN
        -- Si no hay ventas con estado 'Completada', mostrar todas
        SELECT TOP (@Top)
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
        ORDER BY v.[FechaVenta] DESC;
    END
END
GO

PRINT '✅ Procedimientos almacenados creados exitosamente:'
PRINT '   - sp_Compra_MostrarActivas'
PRINT '   - sp_Venta_MostrarActivas'
GO

