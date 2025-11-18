-- =============================================
-- PROCEDIMIENTOS ALMACENADOS PARA GESTIÓN DE USUARIOS
-- Base de Datos: dbLicoreriaLaGata
-- Para listar usuarios y asignar roles (solo admin)
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- =============================================
-- sp_Usuario_MostrarTodos
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Usuario_MostrarTodos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Usuario_MostrarTodos]
GO

CREATE PROCEDURE [dbo].[sp_Usuario_MostrarTodos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        u.[Id],
        u.[NombreUsuario],
        u.[Email],
        u.[NombreCompleto],
        u.[Rol] AS RolNombre,
        u.[Activo],
        u.[FechaCreacion],
        u.[FechaModificacion],
        u.[UltimoAcceso],
        -- Roles asignados (concatenados)
        ISNULL((
            SELECT STRING_AGG(r.[Nombre], ', ')
            FROM [UsuariosRoles] ur
            INNER JOIN [Roles] r ON ur.[RolId] = r.[Id]
            WHERE ur.[UsuarioId] = u.[Id]
        ), '') AS RolesAsignados
    FROM [Usuarios] u
    ORDER BY u.[FechaCreacion] DESC;
END
GO

-- =============================================
-- sp_Usuario_AsignarRol (ya existe, pero lo verificamos)
-- =============================================
-- Este procedimiento ya debería existir, pero lo incluimos por si acaso
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Usuario_AsignarRol]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Usuario_AsignarRol]
GO

CREATE PROCEDURE [dbo].[sp_Usuario_AsignarRol]
    @UsuarioId INT,
    @RolId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    -- Verificar que el usuario y rol existen
    IF EXISTS (SELECT 1 FROM [Usuarios] WHERE [Id] = @UsuarioId)
       AND EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = @RolId AND [Activo] = 1)
    BEGIN
        -- Eliminar roles anteriores del usuario
        DELETE FROM [UsuariosRoles] WHERE [UsuarioId] = @UsuarioId;
        
        -- Asignar nuevo rol
        IF NOT EXISTS (SELECT 1 FROM [UsuariosRoles] WHERE [UsuarioId] = @UsuarioId AND [RolId] = @RolId)
        BEGIN
            INSERT INTO [UsuariosRoles] ([UsuarioId], [RolId])
            VALUES (@UsuarioId, @RolId);
        END
        
        -- Actualizar rol en Usuarios (por compatibilidad)
        UPDATE [Usuarios]
        SET [Rol] = (SELECT [Nombre] FROM [Roles] WHERE [Id] = @RolId),
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @UsuarioId;
        
        SET @Resultado = 1;
    END
END
GO

-- =============================================
-- sp_Rol_MostrarTodos (para obtener lista de roles disponibles)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Rol_MostrarTodos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Rol_MostrarTodos]
GO

CREATE PROCEDURE [dbo].[sp_Rol_MostrarTodos]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.[Id],
        r.[Nombre],
        r.[Descripcion],
        r.[Activo],
        r.[FechaCreacion],
        r.[FechaModificacion]
    FROM [Roles] r
    WHERE r.[Activo] = 1
    ORDER BY r.[Nombre];
END
GO

PRINT '✅ Procedimientos almacenados creados exitosamente:'
PRINT '   - sp_Usuario_MostrarTodos'
PRINT '   - sp_Usuario_AsignarRol'
PRINT '   - sp_Rol_MostrarTodos'
GO

