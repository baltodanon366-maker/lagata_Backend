-- =============================================
-- PROCEDIMIENTOS ALMACENADOS - PARTE 2
-- Continuación de CreateStoredProcedures.sql
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- =============================================
-- 2.5. PROCEDIMIENTOS PARA MARCAS
-- =============================================

-- sp_Marca_Crear
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_Crear]
GO

CREATE PROCEDURE [dbo].[sp_Marca_Crear]
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(500) = NULL,
    @MarcaId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [Marcas] WHERE [Nombre] = @Nombre)
    BEGIN
        SET @MarcaId = -1;
        RETURN;
    END
    
    INSERT INTO [Marcas] ([Nombre], [Descripcion], [Activo])
    VALUES (@Nombre, @Descripcion, 1);
    
    SET @MarcaId = SCOPE_IDENTITY();
END
GO

-- sp_Marca_Editar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_Editar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_Editar]
GO

CREATE PROCEDURE [dbo].[sp_Marca_Editar]
    @MarcaId INT,
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(500) = NULL,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Marcas] WHERE [Id] = @MarcaId)
    BEGIN
        UPDATE [Marcas]
        SET [Nombre] = @Nombre,
            [Descripcion] = @Descripcion,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @MarcaId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Marca_Activar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_Activar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_Activar]
GO

CREATE PROCEDURE [dbo].[sp_Marca_Activar]
    @MarcaId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Marcas] WHERE [Id] = @MarcaId)
    BEGIN
        UPDATE [Marcas]
        SET [Activo] = 1,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @MarcaId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Marca_Desactivar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_Desactivar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_Desactivar]
GO

CREATE PROCEDURE [dbo].[sp_Marca_Desactivar]
    @MarcaId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Marcas] WHERE [Id] = @MarcaId)
    BEGIN
        UPDATE [Marcas]
        SET [Activo] = 0,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @MarcaId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Marca_MostrarActivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_MostrarActivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_MostrarActivos]
GO

CREATE PROCEDURE [dbo].[sp_Marca_MostrarActivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Marcas]
    WHERE [Activo] = 1
    ORDER BY [Nombre];
END
GO

-- sp_Marca_MostrarActivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_MostrarActivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_MostrarActivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Marca_MostrarActivosPorId]
    @MarcaId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Marcas]
    WHERE [Id] = @MarcaId
      AND [Activo] = 1;
END
GO

-- sp_Marca_MostrarActivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_MostrarActivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_MostrarActivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Marca_MostrarActivosPorNombre]
    @Nombre NVARCHAR(100),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Marcas]
    WHERE [Activo] = 1
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Marca_MostrarInactivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_MostrarInactivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_MostrarInactivos]
GO

CREATE PROCEDURE [dbo].[sp_Marca_MostrarInactivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Marcas]
    WHERE [Activo] = 0
    ORDER BY [Nombre];
END
GO

-- sp_Marca_MostrarInactivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_MostrarInactivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_MostrarInactivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Marca_MostrarInactivosPorNombre]
    @Nombre NVARCHAR(100),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Marcas]
    WHERE [Activo] = 0
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Marca_MostrarInactivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Marca_MostrarInactivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Marca_MostrarInactivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Marca_MostrarInactivosPorId]
    @MarcaId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Marcas]
    WHERE [Id] = @MarcaId
      AND [Activo] = 0;
END
GO

PRINT 'Procedimientos de Marcas creados exitosamente';
GO

-- =============================================
-- 2.6. PROCEDIMIENTOS PARA MODELOS
-- =============================================

-- sp_Modelo_Crear
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_Crear]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_Crear]
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(500) = NULL,
    @ModeloId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [Modelos] WHERE [Nombre] = @Nombre)
    BEGIN
        SET @ModeloId = -1;
        RETURN;
    END
    
    INSERT INTO [Modelos] ([Nombre], [Descripcion], [Activo])
    VALUES (@Nombre, @Descripcion, 1);
    
    SET @ModeloId = SCOPE_IDENTITY();
END
GO

-- sp_Modelo_Editar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_Editar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_Editar]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_Editar]
    @ModeloId INT,
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(500) = NULL,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Modelos] WHERE [Id] = @ModeloId)
    BEGIN
        UPDATE [Modelos]
        SET [Nombre] = @Nombre,
            [Descripcion] = @Descripcion,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ModeloId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Modelo_Activar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_Activar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_Activar]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_Activar]
    @ModeloId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Modelos] WHERE [Id] = @ModeloId)
    BEGIN
        UPDATE [Modelos]
        SET [Activo] = 1,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ModeloId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Modelo_Desactivar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_Desactivar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_Desactivar]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_Desactivar]
    @ModeloId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Modelos] WHERE [Id] = @ModeloId)
    BEGIN
        UPDATE [Modelos]
        SET [Activo] = 0,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ModeloId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Modelo_MostrarActivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_MostrarActivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_MostrarActivos]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_MostrarActivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Modelos]
    WHERE [Activo] = 1
    ORDER BY [Nombre];
END
GO

-- sp_Modelo_MostrarActivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_MostrarActivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_MostrarActivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_MostrarActivosPorId]
    @ModeloId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Modelos]
    WHERE [Id] = @ModeloId
      AND [Activo] = 1;
END
GO

-- sp_Modelo_MostrarActivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_MostrarActivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_MostrarActivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_MostrarActivosPorNombre]
    @Nombre NVARCHAR(100),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Modelos]
    WHERE [Activo] = 1
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Modelo_MostrarInactivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_MostrarInactivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_MostrarInactivos]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_MostrarInactivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Modelos]
    WHERE [Activo] = 0
    ORDER BY [Nombre];
END
GO

-- sp_Modelo_MostrarInactivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_MostrarInactivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_MostrarInactivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_MostrarInactivosPorNombre]
    @Nombre NVARCHAR(100),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Modelos]
    WHERE [Activo] = 0
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Modelo_MostrarInactivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Modelo_MostrarInactivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Modelo_MostrarInactivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Modelo_MostrarInactivosPorId]
    @ModeloId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Nombre],
        [Descripcion],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Modelos]
    WHERE [Id] = @ModeloId
      AND [Activo] = 0;
END
GO

PRINT 'Procedimientos de Modelos creados exitosamente';
GO

