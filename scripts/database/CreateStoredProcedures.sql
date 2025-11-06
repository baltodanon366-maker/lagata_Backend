-- =============================================
-- PROCEDIMIENTOS ALMACENADOS - Licoreria API
-- Base de Datos: dbLicoreriaLaGata
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- =============================================
-- 1. PROCEDIMIENTOS DE SEGURIDAD
-- =============================================

-- sp_Usuario_Login: Autenticar usuario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Usuario_Login]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Usuario_Login]
GO

CREATE PROCEDURE [dbo].[sp_Usuario_Login]
    @NombreUsuario NVARCHAR(100),
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.[Id],
        u.[NombreUsuario],
        u.[Email],
        u.[NombreCompleto],
        u.[Rol],
        u.[Activo],
        u.[UltimoAcceso]
    FROM [Usuarios] u
    WHERE u.[NombreUsuario] = @NombreUsuario
      AND u.[PasswordHash] = @PasswordHash
      AND u.[Activo] = 1;
    
    -- Actualizar último acceso
    UPDATE [Usuarios]
    SET [UltimoAcceso] = GETUTCDATE()
    WHERE [NombreUsuario] = @NombreUsuario;
END
GO

-- sp_Usuario_Registrar: Crear nuevo usuario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Usuario_Registrar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Usuario_Registrar]
GO

CREATE PROCEDURE [dbo].[sp_Usuario_Registrar]
    @NombreUsuario NVARCHAR(100),
    @Email NVARCHAR(200),
    @PasswordHash NVARCHAR(500),
    @NombreCompleto NVARCHAR(200),
    @Rol NVARCHAR(50) = 'Vendedor',
    @UsuarioId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar si el usuario ya existe
    IF EXISTS (SELECT 1 FROM [Usuarios] WHERE [NombreUsuario] = @NombreUsuario OR [Email] = @Email)
    BEGIN
        SET @UsuarioId = -1; -- Error: Usuario ya existe
        RETURN;
    END
    
    INSERT INTO [Usuarios] ([NombreUsuario], [Email], [PasswordHash], [NombreCompleto], [Rol], [Activo])
    VALUES (@NombreUsuario, @Email, @PasswordHash, @NombreCompleto, @Rol, 1);
    
    SET @UsuarioId = SCOPE_IDENTITY();
END
GO

-- sp_Usuario_ActualizarPassword: Actualizar contraseña
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Usuario_ActualizarPassword]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Usuario_ActualizarPassword]
GO

CREATE PROCEDURE [dbo].[sp_Usuario_ActualizarPassword]
    @UsuarioId INT,
    @PasswordHashAnterior NVARCHAR(500),
    @PasswordHashNuevo NVARCHAR(500),
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    -- Verificar contraseña anterior
    IF EXISTS (SELECT 1 FROM [Usuarios] WHERE [Id] = @UsuarioId AND [PasswordHash] = @PasswordHashAnterior)
    BEGIN
        UPDATE [Usuarios]
        SET [PasswordHash] = @PasswordHashNuevo,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @UsuarioId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Usuario_AsignarRol: Asignar rol a usuario
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
       AND EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = @RolId)
    BEGIN
        -- Eliminar roles anteriores
        DELETE FROM [UsuariosRoles] WHERE [UsuarioId] = @UsuarioId;
        
        -- Asignar nuevo rol
        INSERT INTO [UsuariosRoles] ([UsuarioId], [RolId])
        VALUES (@UsuarioId, @RolId);
        
        -- Actualizar rol en Usuarios
        UPDATE [Usuarios]
        SET [Rol] = (SELECT [Nombre] FROM [Roles] WHERE [Id] = @RolId),
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @UsuarioId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Usuario_ObtenerPermisos: Obtener permisos de un usuario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Usuario_ObtenerPermisos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Usuario_ObtenerPermisos]
GO

CREATE PROCEDURE [dbo].[sp_Usuario_ObtenerPermisos]
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        p.[Id],
        p.[Nombre],
        p.[Descripcion],
        p.[Modulo]
    FROM [Permisos] p
    INNER JOIN [RolesPermisos] rp ON p.[Id] = rp.[PermisoId]
    INNER JOIN [UsuariosRoles] ur ON rp.[RolId] = ur.[RolId]
    WHERE ur.[UsuarioId] = @UsuarioId
      AND p.[Activo] = 1;
END
GO

-- =============================================
-- 2. PROCEDIMIENTOS PARA CATÁLOGOS
-- =============================================

-- =============================================
-- 2.1. PROCEDIMIENTOS PARA PROVEEDORES
-- =============================================

-- sp_Proveedor_Crear
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_Crear]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_Crear]
    @CodigoProveedor NVARCHAR(50),
    @Nombre NVARCHAR(200),
    @RazonSocial NVARCHAR(300) = NULL,
    @RFC NVARCHAR(50) = NULL,
    @Direccion NVARCHAR(500) = NULL,
    @Telefono NVARCHAR(50) = NULL,
    @Email NVARCHAR(200) = NULL,
    @ProveedorId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar si el código ya existe
    IF EXISTS (SELECT 1 FROM [Proveedores] WHERE [CodigoProveedor] = @CodigoProveedor)
    BEGIN
        SET @ProveedorId = -1; -- Error: Código ya existe
        RETURN;
    END
    
    INSERT INTO [Proveedores] ([CodigoProveedor], [Nombre], [RazonSocial], [RFC], [Direccion], [Telefono], [Email], [Activo])
    VALUES (@CodigoProveedor, @Nombre, @RazonSocial, @RFC, @Direccion, @Telefono, @Email, 1);
    
    SET @ProveedorId = SCOPE_IDENTITY();
END
GO

-- sp_Proveedor_Editar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_Editar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_Editar]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_Editar]
    @ProveedorId INT,
    @Nombre NVARCHAR(200),
    @RazonSocial NVARCHAR(300) = NULL,
    @RFC NVARCHAR(50) = NULL,
    @Direccion NVARCHAR(500) = NULL,
    @Telefono NVARCHAR(50) = NULL,
    @Email NVARCHAR(200) = NULL,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Proveedores] WHERE [Id] = @ProveedorId)
    BEGIN
        UPDATE [Proveedores]
        SET [Nombre] = @Nombre,
            [RazonSocial] = @RazonSocial,
            [RFC] = @RFC,
            [Direccion] = @Direccion,
            [Telefono] = @Telefono,
            [Email] = @Email,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ProveedorId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Proveedor_Activar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_Activar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_Activar]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_Activar]
    @ProveedorId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Proveedores] WHERE [Id] = @ProveedorId)
    BEGIN
        UPDATE [Proveedores]
        SET [Activo] = 1,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ProveedorId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Proveedor_Desactivar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_Desactivar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_Desactivar]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_Desactivar]
    @ProveedorId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Proveedores] WHERE [Id] = @ProveedorId)
    BEGIN
        UPDATE [Proveedores]
        SET [Activo] = 0,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ProveedorId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Proveedor_MostrarActivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_MostrarActivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_MostrarActivos]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_MostrarActivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [CodigoProveedor],
        [Nombre],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Proveedores]
    WHERE [Activo] = 1
    ORDER BY [FechaCreacion] DESC;
END
GO

-- sp_Proveedor_MostrarActivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_MostrarActivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_MostrarActivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_MostrarActivosPorId]
    @ProveedorId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [CodigoProveedor],
        [Nombre],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Proveedores]
    WHERE [Id] = @ProveedorId
      AND [Activo] = 1;
END
GO

-- sp_Proveedor_MostrarActivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_MostrarActivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_MostrarActivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_MostrarActivosPorNombre]
    @Nombre NVARCHAR(200),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [CodigoProveedor],
        [Nombre],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Proveedores]
    WHERE [Activo] = 1
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Proveedor_MostrarInactivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_MostrarInactivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_MostrarInactivos]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_MostrarInactivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [CodigoProveedor],
        [Nombre],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Proveedores]
    WHERE [Activo] = 0
    ORDER BY [FechaCreacion] DESC;
END
GO

-- sp_Proveedor_MostrarInactivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_MostrarInactivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_MostrarInactivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_MostrarInactivosPorNombre]
    @Nombre NVARCHAR(200),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [CodigoProveedor],
        [Nombre],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Proveedores]
    WHERE [Activo] = 0
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Proveedor_MostrarInactivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Proveedor_MostrarInactivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Proveedor_MostrarInactivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Proveedor_MostrarInactivosPorId]
    @ProveedorId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [CodigoProveedor],
        [Nombre],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Proveedores]
    WHERE [Id] = @ProveedorId
      AND [Activo] = 0;
END
GO

-- =============================================
-- 2.2. PROCEDIMIENTOS PARA CLIENTES
-- =============================================

-- sp_Cliente_Crear
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_Crear]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_Crear]
    @CodigoCliente NVARCHAR(50),
    @NombreCompleto NVARCHAR(200),
    @RazonSocial NVARCHAR(300) = NULL,
    @RFC NVARCHAR(50) = NULL,
    @Direccion NVARCHAR(500) = NULL,
    @Telefono NVARCHAR(50) = NULL,
    @Email NVARCHAR(200) = NULL,
    @ClienteId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [Clientes] WHERE [CodigoCliente] = @CodigoCliente)
    BEGIN
        SET @ClienteId = -1;
        RETURN;
    END
    
    INSERT INTO [Clientes] ([CodigoCliente], [NombreCompleto], [RazonSocial], [RFC], [Direccion], [Telefono], [Email], [Activo])
    VALUES (@CodigoCliente, @NombreCompleto, @RazonSocial, @RFC, @Direccion, @Telefono, @Email, 1);
    
    SET @ClienteId = SCOPE_IDENTITY();
END
GO

-- sp_Cliente_Editar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_Editar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_Editar]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_Editar]
    @ClienteId INT,
    @NombreCompleto NVARCHAR(200),
    @RazonSocial NVARCHAR(300) = NULL,
    @RFC NVARCHAR(50) = NULL,
    @Direccion NVARCHAR(500) = NULL,
    @Telefono NVARCHAR(50) = NULL,
    @Email NVARCHAR(200) = NULL,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Clientes] WHERE [Id] = @ClienteId)
    BEGIN
        UPDATE [Clientes]
        SET [NombreCompleto] = @NombreCompleto,
            [RazonSocial] = @RazonSocial,
            [RFC] = @RFC,
            [Direccion] = @Direccion,
            [Telefono] = @Telefono,
            [Email] = @Email,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ClienteId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Cliente_Activar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_Activar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_Activar]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_Activar]
    @ClienteId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Clientes] WHERE [Id] = @ClienteId)
    BEGIN
        UPDATE [Clientes]
        SET [Activo] = 1,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ClienteId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Cliente_Desactivar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_Desactivar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_Desactivar]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_Desactivar]
    @ClienteId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Clientes] WHERE [Id] = @ClienteId)
    BEGIN
        UPDATE [Clientes]
        SET [Activo] = 0,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ClienteId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Cliente_MostrarActivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_MostrarActivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_MostrarActivos]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_MostrarActivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [CodigoCliente],
        [NombreCompleto],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Clientes]
    WHERE [Activo] = 1
    ORDER BY [FechaCreacion] DESC;
END
GO

-- sp_Cliente_MostrarActivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_MostrarActivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_MostrarActivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_MostrarActivosPorId]
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [CodigoCliente],
        [NombreCompleto],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Clientes]
    WHERE [Id] = @ClienteId
      AND [Activo] = 1;
END
GO

-- sp_Cliente_MostrarActivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_MostrarActivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_MostrarActivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_MostrarActivosPorNombre]
    @Nombre NVARCHAR(200),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [CodigoCliente],
        [NombreCompleto],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Clientes]
    WHERE [Activo] = 1
      AND [NombreCompleto] LIKE '%' + @Nombre + '%'
    ORDER BY [NombreCompleto];
END
GO

-- sp_Cliente_MostrarInactivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_MostrarInactivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_MostrarInactivos]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_MostrarInactivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [CodigoCliente],
        [NombreCompleto],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Clientes]
    WHERE [Activo] = 0
    ORDER BY [FechaCreacion] DESC;
END
GO

-- sp_Cliente_MostrarInactivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_MostrarInactivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_MostrarInactivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_MostrarInactivosPorNombre]
    @Nombre NVARCHAR(200),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [CodigoCliente],
        [NombreCompleto],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Clientes]
    WHERE [Activo] = 0
      AND [NombreCompleto] LIKE '%' + @Nombre + '%'
    ORDER BY [NombreCompleto];
END
GO

-- sp_Cliente_MostrarInactivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Cliente_MostrarInactivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Cliente_MostrarInactivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Cliente_MostrarInactivosPorId]
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [CodigoCliente],
        [NombreCompleto],
        [RazonSocial],
        [RFC],
        [Direccion],
        [Telefono],
        [Email],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Clientes]
    WHERE [Id] = @ClienteId
      AND [Activo] = 0;
END
GO

PRINT 'Procedimientos de Clientes creados exitosamente';
GO

-- =============================================
-- 2.3. PROCEDIMIENTOS PARA EMPLEADOS
-- =============================================

-- sp_Empleado_Crear
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_Crear]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_Crear]
    @UsuarioId INT = NULL,
    @CodigoEmpleado NVARCHAR(20),
    @NombreCompleto NVARCHAR(200),
    @Telefono NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @Direccion NVARCHAR(500) = NULL,
    @FechaNacimiento DATE = NULL,
    @FechaIngreso DATE,
    @Salario DECIMAL(18,2) = NULL,
    @Departamento NVARCHAR(100) = NULL,
    @Puesto NVARCHAR(100) = NULL,
    @EmpleadoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [Empleados] WHERE [CodigoEmpleado] = @CodigoEmpleado)
    BEGIN
        SET @EmpleadoId = -1;
        RETURN;
    END
    
    INSERT INTO [Empleados] ([UsuarioId], [CodigoEmpleado], [NombreCompleto], [Telefono], [Email], [Direccion], [FechaNacimiento], [FechaIngreso], [Salario], [Departamento], [Puesto], [Activo])
    VALUES (@UsuarioId, @CodigoEmpleado, @NombreCompleto, @Telefono, @Email, @Direccion, @FechaNacimiento, @FechaIngreso, @Salario, @Departamento, @Puesto, 1);
    
    SET @EmpleadoId = SCOPE_IDENTITY();
END
GO

-- sp_Empleado_Editar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_Editar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_Editar]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_Editar]
    @EmpleadoId INT,
    @NombreCompleto NVARCHAR(200),
    @Telefono NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @Direccion NVARCHAR(500) = NULL,
    @FechaNacimiento DATE = NULL,
    @Salario DECIMAL(18,2) = NULL,
    @Departamento NVARCHAR(100) = NULL,
    @Puesto NVARCHAR(100) = NULL,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Empleados] WHERE [Id] = @EmpleadoId)
    BEGIN
        UPDATE [Empleados]
        SET [NombreCompleto] = @NombreCompleto,
            [Telefono] = @Telefono,
            [Email] = @Email,
            [Direccion] = @Direccion,
            [FechaNacimiento] = @FechaNacimiento,
            [Salario] = @Salario,
            [Departamento] = @Departamento,
            [Puesto] = @Puesto,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @EmpleadoId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Empleado_Activar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_Activar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_Activar]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_Activar]
    @EmpleadoId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Empleados] WHERE [Id] = @EmpleadoId)
    BEGIN
        UPDATE [Empleados]
        SET [Activo] = 1,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @EmpleadoId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Empleado_Desactivar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_Desactivar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_Desactivar]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_Desactivar]
    @EmpleadoId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Empleados] WHERE [Id] = @EmpleadoId)
    BEGIN
        UPDATE [Empleados]
        SET [Activo] = 0,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @EmpleadoId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Empleado_MostrarActivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_MostrarActivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_MostrarActivos]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_MostrarActivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [UsuarioId],
        [CodigoEmpleado],
        [NombreCompleto],
        [Telefono],
        [Email],
        [Direccion],
        [FechaNacimiento],
        [FechaIngreso],
        [Salario],
        [Departamento],
        [Puesto],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Empleados]
    WHERE [Activo] = 1
    ORDER BY [FechaCreacion] DESC;
END
GO

-- sp_Empleado_MostrarActivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_MostrarActivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_MostrarActivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_MostrarActivosPorId]
    @EmpleadoId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [UsuarioId],
        [CodigoEmpleado],
        [NombreCompleto],
        [Telefono],
        [Email],
        [Direccion],
        [FechaNacimiento],
        [FechaIngreso],
        [Salario],
        [Departamento],
        [Puesto],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Empleados]
    WHERE [Id] = @EmpleadoId
      AND [Activo] = 1;
END
GO

-- sp_Empleado_MostrarActivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_MostrarActivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_MostrarActivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_MostrarActivosPorNombre]
    @Nombre NVARCHAR(200),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [UsuarioId],
        [CodigoEmpleado],
        [NombreCompleto],
        [Telefono],
        [Email],
        [Direccion],
        [FechaNacimiento],
        [FechaIngreso],
        [Salario],
        [Departamento],
        [Puesto],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Empleados]
    WHERE [Activo] = 1
      AND [NombreCompleto] LIKE '%' + @Nombre + '%'
    ORDER BY [NombreCompleto];
END
GO

-- sp_Empleado_MostrarInactivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_MostrarInactivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_MostrarInactivos]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_MostrarInactivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [UsuarioId],
        [CodigoEmpleado],
        [NombreCompleto],
        [Telefono],
        [Email],
        [Direccion],
        [FechaNacimiento],
        [FechaIngreso],
        [Salario],
        [Departamento],
        [Puesto],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Empleados]
    WHERE [Activo] = 0
    ORDER BY [FechaCreacion] DESC;
END
GO

-- sp_Empleado_MostrarInactivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_MostrarInactivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_MostrarInactivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_MostrarInactivosPorNombre]
    @Nombre NVARCHAR(200),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        [Id],
        [UsuarioId],
        [CodigoEmpleado],
        [NombreCompleto],
        [Telefono],
        [Email],
        [Direccion],
        [FechaNacimiento],
        [FechaIngreso],
        [Salario],
        [Departamento],
        [Puesto],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Empleados]
    WHERE [Activo] = 0
      AND [NombreCompleto] LIKE '%' + @Nombre + '%'
    ORDER BY [NombreCompleto];
END
GO

-- sp_Empleado_MostrarInactivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Empleado_MostrarInactivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Empleado_MostrarInactivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Empleado_MostrarInactivosPorId]
    @EmpleadoId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [UsuarioId],
        [CodigoEmpleado],
        [NombreCompleto],
        [Telefono],
        [Email],
        [Direccion],
        [FechaNacimiento],
        [FechaIngreso],
        [Salario],
        [Departamento],
        [Puesto],
        [Activo],
        [FechaCreacion],
        [FechaModificacion]
    FROM [Empleados]
    WHERE [Id] = @EmpleadoId
      AND [Activo] = 0;
END
GO

PRINT 'Procedimientos de Empleados creados exitosamente';
GO

-- =============================================
-- 2.4. PROCEDIMIENTOS PARA CATEGORIAS
-- =============================================

-- sp_Categoria_Crear
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_Crear]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_Crear]
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(500) = NULL,
    @CategoriaId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [Categorias] WHERE [Nombre] = @Nombre)
    BEGIN
        SET @CategoriaId = -1;
        RETURN;
    END
    
    INSERT INTO [Categorias] ([Nombre], [Descripcion], [Activo])
    VALUES (@Nombre, @Descripcion, 1);
    
    SET @CategoriaId = SCOPE_IDENTITY();
END
GO

-- sp_Categoria_Editar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_Editar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_Editar]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_Editar]
    @CategoriaId INT,
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(500) = NULL,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Categorias] WHERE [Id] = @CategoriaId)
    BEGIN
        UPDATE [Categorias]
        SET [Nombre] = @Nombre,
            [Descripcion] = @Descripcion,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @CategoriaId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Categoria_Activar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_Activar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_Activar]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_Activar]
    @CategoriaId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Categorias] WHERE [Id] = @CategoriaId)
    BEGIN
        UPDATE [Categorias]
        SET [Activo] = 1,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @CategoriaId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Categoria_Desactivar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_Desactivar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_Desactivar]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_Desactivar]
    @CategoriaId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Categorias] WHERE [Id] = @CategoriaId)
    BEGIN
        UPDATE [Categorias]
        SET [Activo] = 0,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @CategoriaId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Categoria_MostrarActivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_MostrarActivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_MostrarActivos]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_MostrarActivos]
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
    FROM [Categorias]
    WHERE [Activo] = 1
    ORDER BY [Nombre];
END
GO

-- sp_Categoria_MostrarActivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_MostrarActivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_MostrarActivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_MostrarActivosPorId]
    @CategoriaId INT
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
    FROM [Categorias]
    WHERE [Id] = @CategoriaId
      AND [Activo] = 1;
END
GO

-- sp_Categoria_MostrarActivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_MostrarActivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_MostrarActivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_MostrarActivosPorNombre]
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
    FROM [Categorias]
    WHERE [Activo] = 1
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Categoria_MostrarInactivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_MostrarInactivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_MostrarInactivos]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_MostrarInactivos]
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
    FROM [Categorias]
    WHERE [Activo] = 0
    ORDER BY [Nombre];
END
GO

-- sp_Categoria_MostrarInactivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_MostrarInactivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_MostrarInactivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_MostrarInactivosPorNombre]
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
    FROM [Categorias]
    WHERE [Activo] = 0
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Categoria_MostrarInactivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Categoria_MostrarInactivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Categoria_MostrarInactivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Categoria_MostrarInactivosPorId]
    @CategoriaId INT
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
    FROM [Categorias]
    WHERE [Id] = @CategoriaId
      AND [Activo] = 0;
END
GO

PRINT 'Procedimientos de Categorias creados exitosamente';
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

-- =============================================
-- 2.7. PROCEDIMIENTOS PARA PRODUCTOS
-- =============================================

-- sp_Producto_Crear
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_Crear]
GO

CREATE PROCEDURE [dbo].[sp_Producto_Crear]
    @Nombre NVARCHAR(200),
    @Descripcion NVARCHAR(1000) = NULL,
    @ProductoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Productos] ([Nombre], [Descripcion], [Activo])
    VALUES (@Nombre, @Descripcion, 1);
    
    SET @ProductoId = SCOPE_IDENTITY();
END
GO

-- sp_Producto_Editar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_Editar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_Editar]
GO

CREATE PROCEDURE [dbo].[sp_Producto_Editar]
    @ProductoId INT,
    @Nombre NVARCHAR(200),
    @Descripcion NVARCHAR(1000) = NULL,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Productos] WHERE [Id] = @ProductoId)
    BEGIN
        UPDATE [Productos]
        SET [Nombre] = @Nombre,
            [Descripcion] = @Descripcion,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ProductoId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Producto_Activar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_Activar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_Activar]
GO

CREATE PROCEDURE [dbo].[sp_Producto_Activar]
    @ProductoId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Productos] WHERE [Id] = @ProductoId)
    BEGIN
        UPDATE [Productos]
        SET [Activo] = 1,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ProductoId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Producto_Desactivar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_Desactivar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_Desactivar]
GO

CREATE PROCEDURE [dbo].[sp_Producto_Desactivar]
    @ProductoId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [Productos] WHERE [Id] = @ProductoId)
    BEGIN
        UPDATE [Productos]
        SET [Activo] = 0,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @ProductoId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_Producto_MostrarActivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_MostrarActivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_MostrarActivos]
GO

CREATE PROCEDURE [dbo].[sp_Producto_MostrarActivos]
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
    FROM [Productos]
    WHERE [Activo] = 1
    ORDER BY [Nombre];
END
GO

-- sp_Producto_MostrarActivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_MostrarActivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_MostrarActivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Producto_MostrarActivosPorId]
    @ProductoId INT
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
    FROM [Productos]
    WHERE [Id] = @ProductoId
      AND [Activo] = 1;
END
GO

-- sp_Producto_MostrarActivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_MostrarActivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_MostrarActivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Producto_MostrarActivosPorNombre]
    @Nombre NVARCHAR(200),
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
    FROM [Productos]
    WHERE [Activo] = 1
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Producto_MostrarInactivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_MostrarInactivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_MostrarInactivos]
GO

CREATE PROCEDURE [dbo].[sp_Producto_MostrarInactivos]
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
    FROM [Productos]
    WHERE [Activo] = 0
    ORDER BY [Nombre];
END
GO

-- sp_Producto_MostrarInactivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_MostrarInactivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_MostrarInactivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_Producto_MostrarInactivosPorNombre]
    @Nombre NVARCHAR(200),
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
    FROM [Productos]
    WHERE [Activo] = 0
      AND [Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY [Nombre];
END
GO

-- sp_Producto_MostrarInactivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Producto_MostrarInactivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_Producto_MostrarInactivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_Producto_MostrarInactivosPorId]
    @ProductoId INT
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
    FROM [Productos]
    WHERE [Id] = @ProductoId
      AND [Activo] = 0;
END
GO

PRINT 'Procedimientos de Productos creados exitosamente';
GO

-- =============================================
-- 2.8. PROCEDIMIENTOS PARA DETALLEPRODUCTO
-- =============================================

-- sp_DetalleProducto_Crear
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_Crear]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_Crear]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_Crear]
    @ProductoId INT,
    @CategoriaId INT,
    @MarcaId INT,
    @ModeloId INT,
    @Codigo NVARCHAR(50),
    @SKU NVARCHAR(100) = NULL,
    @Observaciones NVARCHAR(500) = NULL,
    @PrecioCompra DECIMAL(18,2),
    @PrecioVenta DECIMAL(18,2),
    @StockMinimo INT = 0,
    @UnidadMedida NVARCHAR(50) = NULL,
    @DetalleProductoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [DetalleProducto] WHERE [Codigo] = @Codigo)
    BEGIN
        SET @DetalleProductoId = -1;
        RETURN;
    END
    
    INSERT INTO [DetalleProducto] ([ProductoId], [CategoriaId], [MarcaId], [ModeloId], [Codigo], [SKU], [Observaciones], [PrecioCompra], [PrecioVenta], [Stock], [StockMinimo], [UnidadMedida], [Activo])
    VALUES (@ProductoId, @CategoriaId, @MarcaId, @ModeloId, @Codigo, @SKU, @Observaciones, @PrecioCompra, @PrecioVenta, 0, @StockMinimo, @UnidadMedida, 1);
    
    SET @DetalleProductoId = SCOPE_IDENTITY();
END
GO

-- sp_DetalleProducto_Editar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_Editar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_Editar]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_Editar]
    @DetalleProductoId INT,
    @CategoriaId INT,
    @MarcaId INT,
    @ModeloId INT,
    @SKU NVARCHAR(100) = NULL,
    @Observaciones NVARCHAR(500) = NULL,
    @PrecioCompra DECIMAL(18,2),
    @PrecioVenta DECIMAL(18,2),
    @StockMinimo INT = 0,
    @UnidadMedida NVARCHAR(50) = NULL,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [DetalleProducto] WHERE [Id] = @DetalleProductoId)
    BEGIN
        UPDATE [DetalleProducto]
        SET [CategoriaId] = @CategoriaId,
            [MarcaId] = @MarcaId,
            [ModeloId] = @ModeloId,
            [SKU] = @SKU,
            [Observaciones] = @Observaciones,
            [PrecioCompra] = @PrecioCompra,
            [PrecioVenta] = @PrecioVenta,
            [StockMinimo] = @StockMinimo,
            [UnidadMedida] = @UnidadMedida,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @DetalleProductoId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_DetalleProducto_Activar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_Activar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_Activar]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_Activar]
    @DetalleProductoId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [DetalleProducto] WHERE [Id] = @DetalleProductoId)
    BEGIN
        UPDATE [DetalleProducto]
        SET [Activo] = 1,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @DetalleProductoId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_DetalleProducto_Desactivar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_Desactivar]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_Desactivar]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_Desactivar]
    @DetalleProductoId INT,
    @Resultado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    
    IF EXISTS (SELECT 1 FROM [DetalleProducto] WHERE [Id] = @DetalleProductoId)
    BEGIN
        UPDATE [DetalleProducto]
        SET [Activo] = 0,
            [FechaModificacion] = GETUTCDATE()
        WHERE [Id] = @DetalleProductoId;
        
        SET @Resultado = 1;
    END
END
GO

-- sp_DetalleProducto_MostrarActivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_MostrarActivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_MostrarActivos]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_MostrarActivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        dp.[Id],
        dp.[ProductoId],
        p.[Nombre] AS ProductoNombre,
        dp.[CategoriaId],
        c.[Nombre] AS CategoriaNombre,
        dp.[MarcaId],
        m.[Nombre] AS MarcaNombre,
        dp.[ModeloId],
        mo.[Nombre] AS ModeloNombre,
        dp.[Codigo],
        dp.[SKU],
        dp.[Observaciones],
        dp.[PrecioCompra],
        dp.[PrecioVenta],
        dp.[Stock],
        dp.[StockMinimo],
        dp.[UnidadMedida],
        dp.[FechaUltimoMovimiento],
        dp.[Activo],
        dp.[FechaCreacion],
        dp.[FechaModificacion]
    FROM [DetalleProducto] dp
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Categorias] c ON dp.[CategoriaId] = c.[Id]
    INNER JOIN [Marcas] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [Modelos] mo ON dp.[ModeloId] = mo.[Id]
    WHERE dp.[Activo] = 1
    ORDER BY dp.[FechaCreacion] DESC;
END
GO

-- sp_DetalleProducto_MostrarActivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_MostrarActivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_MostrarActivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_MostrarActivosPorId]
    @DetalleProductoId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        dp.[Id],
        dp.[ProductoId],
        p.[Nombre] AS ProductoNombre,
        dp.[CategoriaId],
        c.[Nombre] AS CategoriaNombre,
        dp.[MarcaId],
        m.[Nombre] AS MarcaNombre,
        dp.[ModeloId],
        mo.[Nombre] AS ModeloNombre,
        dp.[Codigo],
        dp.[SKU],
        dp.[Observaciones],
        dp.[PrecioCompra],
        dp.[PrecioVenta],
        dp.[Stock],
        dp.[StockMinimo],
        dp.[UnidadMedida],
        dp.[FechaUltimoMovimiento],
        dp.[Activo],
        dp.[FechaCreacion],
        dp.[FechaModificacion]
    FROM [DetalleProducto] dp
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Categorias] c ON dp.[CategoriaId] = c.[Id]
    INNER JOIN [Marcas] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [Modelos] mo ON dp.[ModeloId] = mo.[Id]
    WHERE dp.[Id] = @DetalleProductoId
      AND dp.[Activo] = 1;
END
GO

-- sp_DetalleProducto_MostrarActivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_MostrarActivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_MostrarActivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_MostrarActivosPorNombre]
    @Nombre NVARCHAR(200),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        dp.[Id],
        dp.[ProductoId],
        p.[Nombre] AS ProductoNombre,
        dp.[CategoriaId],
        c.[Nombre] AS CategoriaNombre,
        dp.[MarcaId],
        m.[Nombre] AS MarcaNombre,
        dp.[ModeloId],
        mo.[Nombre] AS ModeloNombre,
        dp.[Codigo],
        dp.[SKU],
        dp.[Observaciones],
        dp.[PrecioCompra],
        dp.[PrecioVenta],
        dp.[Stock],
        dp.[StockMinimo],
        dp.[UnidadMedida],
        dp.[FechaUltimoMovimiento],
        dp.[Activo],
        dp.[FechaCreacion],
        dp.[FechaModificacion]
    FROM [DetalleProducto] dp
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Categorias] c ON dp.[CategoriaId] = c.[Id]
    INNER JOIN [Marcas] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [Modelos] mo ON dp.[ModeloId] = mo.[Id]
    WHERE dp.[Activo] = 1
      AND p.[Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY p.[Nombre];
END
GO

-- sp_DetalleProducto_MostrarInactivos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_MostrarInactivos]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_MostrarInactivos]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_MostrarInactivos]
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        dp.[Id],
        dp.[ProductoId],
        p.[Nombre] AS ProductoNombre,
        dp.[CategoriaId],
        c.[Nombre] AS CategoriaNombre,
        dp.[MarcaId],
        m.[Nombre] AS MarcaNombre,
        dp.[ModeloId],
        mo.[Nombre] AS ModeloNombre,
        dp.[Codigo],
        dp.[SKU],
        dp.[Observaciones],
        dp.[PrecioCompra],
        dp.[PrecioVenta],
        dp.[Stock],
        dp.[StockMinimo],
        dp.[UnidadMedida],
        dp.[FechaUltimoMovimiento],
        dp.[Activo],
        dp.[FechaCreacion],
        dp.[FechaModificacion]
    FROM [DetalleProducto] dp
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Categorias] c ON dp.[CategoriaId] = c.[Id]
    INNER JOIN [Marcas] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [Modelos] mo ON dp.[ModeloId] = mo.[Id]
    WHERE dp.[Activo] = 0
    ORDER BY dp.[FechaCreacion] DESC;
END
GO

-- sp_DetalleProducto_MostrarInactivosPorNombre
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_MostrarInactivosPorNombre]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_MostrarInactivosPorNombre]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_MostrarInactivosPorNombre]
    @Nombre NVARCHAR(200),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Top)
        dp.[Id],
        dp.[ProductoId],
        p.[Nombre] AS ProductoNombre,
        dp.[CategoriaId],
        c.[Nombre] AS CategoriaNombre,
        dp.[MarcaId],
        m.[Nombre] AS MarcaNombre,
        dp.[ModeloId],
        mo.[Nombre] AS ModeloNombre,
        dp.[Codigo],
        dp.[SKU],
        dp.[Observaciones],
        dp.[PrecioCompra],
        dp.[PrecioVenta],
        dp.[Stock],
        dp.[StockMinimo],
        dp.[UnidadMedida],
        dp.[FechaUltimoMovimiento],
        dp.[Activo],
        dp.[FechaCreacion],
        dp.[FechaModificacion]
    FROM [DetalleProducto] dp
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Categorias] c ON dp.[CategoriaId] = c.[Id]
    INNER JOIN [Marcas] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [Modelos] mo ON dp.[ModeloId] = mo.[Id]
    WHERE dp.[Activo] = 0
      AND p.[Nombre] LIKE '%' + @Nombre + '%'
    ORDER BY p.[Nombre];
END
GO

-- sp_DetalleProducto_MostrarInactivosPorId
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DetalleProducto_MostrarInactivosPorId]') AND type = 'P')
    DROP PROCEDURE [dbo].[sp_DetalleProducto_MostrarInactivosPorId]
GO

CREATE PROCEDURE [dbo].[sp_DetalleProducto_MostrarInactivosPorId]
    @DetalleProductoId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        dp.[Id],
        dp.[ProductoId],
        p.[Nombre] AS ProductoNombre,
        dp.[CategoriaId],
        c.[Nombre] AS CategoriaNombre,
        dp.[MarcaId],
        m.[Nombre] AS MarcaNombre,
        dp.[ModeloId],
        mo.[Nombre] AS ModeloNombre,
        dp.[Codigo],
        dp.[SKU],
        dp.[Observaciones],
        dp.[PrecioCompra],
        dp.[PrecioVenta],
        dp.[Stock],
        dp.[StockMinimo],
        dp.[UnidadMedida],
        dp.[FechaUltimoMovimiento],
        dp.[Activo],
        dp.[FechaCreacion],
        dp.[FechaModificacion]
    FROM [DetalleProducto] dp
    INNER JOIN [Productos] p ON dp.[ProductoId] = p.[Id]
    INNER JOIN [Categorias] c ON dp.[CategoriaId] = c.[Id]
    INNER JOIN [Marcas] m ON dp.[MarcaId] = m.[Id]
    INNER JOIN [Modelos] mo ON dp.[ModeloId] = mo.[Id]
    WHERE dp.[Id] = @DetalleProductoId
      AND dp.[Activo] = 0;
END
GO

PRINT 'Procedimientos de DetalleProducto creados exitosamente';
GO

