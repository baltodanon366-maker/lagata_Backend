-- =============================================
-- Script de Datos de Prueba - Licoreria API
-- Base de Datos: dbLicoreriaLaGata
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- =============================================
-- 1. ROLES Y PERMISOS
-- =============================================

-- Insertar Roles
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Nombre] = 'Administrador')
BEGIN
    INSERT INTO [Roles] ([Nombre], [Descripcion], [Activo])
    VALUES ('Administrador', 'Acceso completo al sistema. Puede gestionar usuarios, catálogos, transacciones y reportes.', 1);
    PRINT 'Rol Administrador creado';
END
GO

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Nombre] = 'Vendedor')
BEGIN
    INSERT INTO [Roles] ([Nombre], [Descripcion], [Activo])
    VALUES ('Vendedor', 'Acceso a ventas, compras y devoluciones. Solo lectura en catálogos. Reportes con filtros.', 1);
    PRINT 'Rol Vendedor creado';
END
GO

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Nombre] = 'Supervisor')
BEGIN
    INSERT INTO [Roles] ([Nombre], [Descripcion], [Activo])
    VALUES ('Supervisor', 'Puede gestionar ventas, compras, devoluciones y ver reportes completos. No puede gestionar usuarios.', 1);
    PRINT 'Rol Supervisor creado';
END
GO

-- Insertar Permisos
DECLARE @Permisos TABLE (
    Nombre NVARCHAR(100),
    Descripcion NVARCHAR(500),
    Modulo NVARCHAR(100)
);

INSERT INTO @Permisos VALUES
('Usuarios_Crear', 'Crear nuevos usuarios', 'Seguridad'),
('Usuarios_Editar', 'Editar usuarios existentes', 'Seguridad'),
('Usuarios_Eliminar', 'Eliminar usuarios', 'Seguridad'),
('Usuarios_Ver', 'Ver lista de usuarios', 'Seguridad'),
('Roles_Asignar', 'Asignar roles a usuarios', 'Seguridad'),
('Catálogos_Crear', 'Crear nuevos registros en catálogos', 'Catálogos'),
('Catálogos_Editar', 'Editar registros en catálogos', 'Catálogos'),
('Catálogos_Eliminar', 'Eliminar registros en catálogos', 'Catálogos'),
('Catálogos_Ver', 'Ver catálogos', 'Catálogos'),
('Ventas_Crear', 'Crear nuevas ventas', 'Transacciones'),
('Ventas_Editar', 'Editar ventas', 'Transacciones'),
('Ventas_Cancelar', 'Cancelar ventas', 'Transacciones'),
('Ventas_Ver', 'Ver ventas', 'Transacciones'),
('Compras_Crear', 'Crear nuevas compras', 'Transacciones'),
('Compras_Editar', 'Editar compras', 'Transacciones'),
('Compras_Cancelar', 'Cancelar compras', 'Transacciones'),
('Compras_Ver', 'Ver compras', 'Transacciones'),
('Devoluciones_Crear', 'Crear devoluciones', 'Transacciones'),
('Devoluciones_Ver', 'Ver devoluciones', 'Transacciones'),
('Reportes_Ver', 'Ver reportes', 'Reportes'),
('Reportes_Exportar', 'Exportar reportes', 'Reportes'),
('Inventario_Ver', 'Ver inventario', 'Inventario'),
('Inventario_Ajustar', 'Ajustar inventario', 'Inventario');

MERGE [Permisos] AS target
USING @Permisos AS source ON target.[Nombre] = source.[Nombre]
WHEN NOT MATCHED THEN
    INSERT ([Nombre], [Descripcion], [Modulo], [Activo])
    VALUES (source.[Nombre], source.[Descripcion], source.[Modulo], 1);
GO

-- Asignar Permisos a Roles
-- Administrador: Todos los permisos
INSERT INTO [RolesPermisos] ([RolId], [PermisoId])
SELECT r.[Id], p.[Id]
FROM [Roles] r
CROSS JOIN [Permisos] p
WHERE r.[Nombre] = 'Administrador'
AND NOT EXISTS (
    SELECT 1 FROM [RolesPermisos] rp 
    WHERE rp.[RolId] = r.[Id] AND rp.[PermisoId] = p.[Id]
);
GO

-- Vendedor: Solo lectura en catálogos, ventas/compras/devoluciones, reportes ver
INSERT INTO [RolesPermisos] ([RolId], [PermisoId])
SELECT r.[Id], p.[Id]
FROM [Roles] r
CROSS JOIN [Permisos] p
WHERE r.[Nombre] = 'Vendedor'
AND p.[Nombre] IN (
    'Catálogos_Ver',
    'Ventas_Crear', 'Ventas_Ver',
    'Compras_Crear', 'Compras_Ver',
    'Devoluciones_Crear', 'Devoluciones_Ver',
    'Reportes_Ver',
    'Inventario_Ver'
)
AND NOT EXISTS (
    SELECT 1 FROM [RolesPermisos] rp 
    WHERE rp.[RolId] = r.[Id] AND rp.[PermisoId] = p.[Id]
);
GO

-- Supervisor: Catálogos completos, transacciones, reportes, pero no usuarios
INSERT INTO [RolesPermisos] ([RolId], [PermisoId])
SELECT r.[Id], p.[Id]
FROM [Roles] r
CROSS JOIN [Permisos] p
WHERE r.[Nombre] = 'Supervisor'
AND p.[Nombre] NOT LIKE 'Usuarios_%'
AND p.[Nombre] NOT LIKE 'Roles_%'
AND NOT EXISTS (
    SELECT 1 FROM [RolesPermisos] rp 
    WHERE rp.[RolId] = r.[Id] AND rp.[PermisoId] = p.[Id]
);
GO

-- =============================================
-- 2. USUARIOS
-- =============================================

-- Usuario Administrador (password: Admin123!)
-- NOTA: Este hash es un placeholder. Debe ser reemplazado con un hash real cuando implementes BCrypt
INSERT INTO [Usuarios] ([NombreUsuario], [Email], [PasswordHash], [NombreCompleto], [Rol], [Activo])
VALUES ('admin', 'admin@licoreria.com', 'PLACEHOLDER_HASH_ADMIN123', 'Administrador del Sistema', 'Administrador', 1);

-- Usuario Vendedor (password: Vendedor123!)
-- NOTA: Este hash es un placeholder. Debe ser reemplazado con un hash real cuando implementes BCrypt
INSERT INTO [Usuarios] ([NombreUsuario], [Email], [PasswordHash], [NombreCompleto], [Rol], [Activo])
VALUES ('vendedor1', 'vendedor1@licoreria.com', 'PLACEHOLDER_HASH_VENDEDOR123', 'Juan Pérez', 'Vendedor', 1);

-- Usuario Supervisor (password: Supervisor123!)
-- NOTA: Este hash es un placeholder. Debe ser reemplazado con un hash real cuando implementes BCrypt
INSERT INTO [Usuarios] ([NombreUsuario], [Email], [PasswordHash], [NombreCompleto], [Rol], [Activo])
VALUES ('supervisor1', 'supervisor1@licoreria.com', 'PLACEHOLDER_HASH_SUPERVISOR123', 'María González', 'Supervisor', 1);

-- Asignar Roles a Usuarios
INSERT INTO [UsuariosRoles] ([UsuarioId], [RolId])
SELECT u.[Id], r.[Id]
FROM [Usuarios] u
CROSS JOIN [Roles] r
WHERE (u.[NombreUsuario] = 'admin' AND r.[Nombre] = 'Administrador')
   OR (u.[NombreUsuario] = 'vendedor1' AND r.[Nombre] = 'Vendedor')
   OR (u.[NombreUsuario] = 'supervisor1' AND r.[Nombre] = 'Supervisor');
GO

PRINT 'Usuarios creados exitosamente';
GO

-- =============================================
-- 3. CATÁLOGOS: CATEGORÍAS, MARCAS, MODELOS
-- =============================================

-- Categorías (20 categorías realistas para licorería)
DECLARE @Categorias TABLE (Nombre NVARCHAR(100), Descripcion NVARCHAR(500));
INSERT INTO @Categorias VALUES
('Ron', 'Bebidas alcohólicas destiladas de caña de azúcar'),
('Vodka', 'Bebida alcohólica destilada sin sabor'),
('Whisky', 'Bebida alcohólica destilada de cereales'),
('Tequila', 'Bebida alcohólica destilada del agave'),
('Ginebra', 'Bebida alcohólica destilada con enebro'),
('Brandy', 'Bebida alcohólica destilada del vino'),
('Coñac', 'Brandy producido en la región de Coñac'),
('Champagne', 'Vino espumoso de la región de Champagne'),
('Vino Tinto', 'Vino elaborado con uvas tintas'),
('Vino Blanco', 'Vino elaborado con uvas blancas'),
('Vino Rosado', 'Vino con color rosado'),
('Cerveza', 'Bebida alcohólica fermentada'),
('Cerveza Artesanal', 'Cerveza producida en pequeñas cantidades'),
('Licor', 'Bebida alcohólica dulce y aromática'),
('Anís', 'Bebida alcohólica con sabor a anís'),
('Aguardiente', 'Bebida alcohólica destilada fuerte'),
('Cocktail', 'Mezcla de bebidas alcohólicas'),
('Sake', 'Bebida alcohólica japonesa de arroz'),
('Soju', 'Bebida alcohólica coreana'),
('Aperitivo', 'Bebida alcohólica para antes de las comidas');

MERGE [Categorias] AS target
USING @Categorias AS source ON target.[Nombre] = source.[Nombre]
WHEN NOT MATCHED THEN
    INSERT ([Nombre], [Descripcion], [Activo])
    VALUES (source.[Nombre], source.[Descripcion], 1);
GO

-- Marcas (30 marcas realistas)
DECLARE @Marcas TABLE (Nombre NVARCHAR(100), Descripcion NVARCHAR(500));
INSERT INTO @Marcas VALUES
('Bacardi', 'Marca líder de ron'),
('Havana Club', 'Ron cubano premium'),
('Flor de Caña', 'Ron nicaragüense'),
('Captain Morgan', 'Ron especiado'),
('Smirnoff', 'Vodka ruso'),
('Absolut', 'Vodka sueco'),
('Grey Goose', 'Vodka premium francés'),
('Johnnie Walker', 'Whisky escocés'),
('Jack Daniels', 'Whisky americano'),
('Chivas Regal', 'Whisky escocés premium'),
('Jose Cuervo', 'Tequila mexicano'),
('Patrón', 'Tequila premium'),
('Don Julio', 'Tequila ultra premium'),
('Bombay Sapphire', 'Ginebra premium'),
('Hennessy', 'Coñac francés'),
('Rémy Martin', 'Coñac francés'),
('Moët & Chandon', 'Champagne francés'),
('Dom Pérignon', 'Champagne premium'),
('Concha y Toro', 'Vino chileno'),
('Santa Rita', 'Vino chileno'),
('Corona', 'Cerveza mexicana'),
('Heineken', 'Cerveza holandesa'),
('Stella Artois', 'Cerveza belga'),
('Baileys', 'Licor irlandés'),
('Kahlúa', 'Licor de café mexicano'),
('Amarula', 'Licor sudafricano'),
('Jägermeister', 'Licor alemán'),
('Grand Marnier', 'Licor francés'),
('Cointreau', 'Licor francés'),
('Fernet Branca', 'Aperitivo italiano');

MERGE [Marcas] AS target
USING @Marcas AS source ON target.[Nombre] = source.[Nombre]
WHEN NOT MATCHED THEN
    INSERT ([Nombre], [Descripcion], [Activo])
    VALUES (source.[Nombre], source.[Descripcion], 1);
GO

-- Modelos (25 modelos variados)
DECLARE @Modelos TABLE (Nombre NVARCHAR(100), Descripcion NVARCHAR(500));
INSERT INTO @Modelos VALUES
('750ml', 'Botella estándar de 750ml'),
('1L', 'Botella de 1 litro'),
('375ml', 'Media botella'),
('1.75L', 'Botella grande de 1.75 litros'),
('200ml', 'Botella pequeña'),
('500ml', 'Botella mediana'),
('6 Pack', 'Pack de 6 unidades'),
('12 Pack', 'Pack de 12 unidades'),
('24 Pack', 'Pack de 24 unidades'),
('Caja', 'Presentación en caja'),
('Lata 355ml', 'Lata estándar'),
('Lata 473ml', 'Lata grande'),
('Botella 330ml', 'Botella de cerveza'),
('Botella 500ml', 'Botella de cerveza grande'),
('Botella 750ml Premium', 'Botella premium'),
('Botella 1L Premium', 'Botella grande premium'),
('Magnum 1.5L', 'Botella magnum'),
('Jeroboam 3L', 'Botella jeroboam'),
('Pack Promocional', 'Pack promocional'),
('Edición Limitada', 'Edición limitada'),
('Añejo', 'Versión añeja'),
('Reserva', 'Versión reserva'),
('Premium', 'Versión premium'),
('Ultra Premium', 'Versión ultra premium'),
('Standard', 'Versión estándar');

MERGE [Modelos] AS target
USING @Modelos AS source ON target.[Nombre] = source.[Nombre]
WHEN NOT MATCHED THEN
    INSERT ([Nombre], [Descripcion], [Activo])
    VALUES (source.[Nombre], source.[Descripcion], 1);
GO

PRINT 'Catálogos base creados exitosamente';
GO

-- =============================================
-- 4. PRODUCTOS Y DETALLEPRODUCTO
-- =============================================

-- Productos (30 productos variados)
DECLARE @Productos TABLE (Nombre NVARCHAR(200), Descripcion NVARCHAR(1000));
INSERT INTO @Productos VALUES
('Ron Bacardi Superior', 'Ron blanco suave y ligero'),
('Ron Havana Club 7 Años', 'Ron añejo cubano'),
('Ron Flor de Caña 12 Años', 'Ron premium nicaragüense'),
('Vodka Smirnoff', 'Vodka ruso suave'),
('Vodka Absolut', 'Vodka sueco premium'),
('Whisky Johnnie Walker Black', 'Whisky escocés 12 años'),
('Whisky Jack Daniels', 'Whisky americano'),
('Tequila Jose Cuervo Tradicional', 'Tequila blanco mexicano'),
('Tequila Patrón Silver', 'Tequila premium'),
('Ginebra Bombay Sapphire', 'Ginebra premium inglesa'),
('Coñac Hennessy VS', 'Coñac francés'),
('Champagne Moët & Chandon', 'Champagne francés'),
('Vino Tinto Concha y Toro', 'Vino tinto chileno'),
('Vino Blanco Santa Rita', 'Vino blanco chileno'),
('Cerveza Corona Extra', 'Cerveza mexicana'),
('Cerveza Heineken', 'Cerveza holandesa'),
('Licor Baileys', 'Licor irlandés cremoso'),
('Licor Kahlúa', 'Licor de café'),
('Ron Captain Morgan', 'Ron especiado'),
('Whisky Chivas Regal 12', 'Whisky escocés premium'),
('Tequila Don Julio 1942', 'Tequila ultra premium'),
('Vodka Grey Goose', 'Vodka premium francés'),
('Coñac Rémy Martin VSOP', 'Coñac francés VSOP'),
('Champagne Dom Pérignon', 'Champagne ultra premium'),
('Vino Rosado Concha y Toro', 'Vino rosado chileno'),
('Cerveza Stella Artois', 'Cerveza belga'),
('Licor Amarula', 'Licor sudafricano'),
('Licor Jägermeister', 'Licor alemán'),
('Aperitivo Fernet Branca', 'Aperitivo italiano'),
('Licor Grand Marnier', 'Licor francés de naranja');

DECLARE @ProductoIds TABLE (ProductoId INT, Nombre NVARCHAR(200));
INSERT INTO @ProductoIds (ProductoId, Nombre)
SELECT [Id], [Nombre]
FROM [Productos]
WHERE [Nombre] IN (SELECT [Nombre] FROM @Productos);

-- Insertar Productos si no existen
INSERT INTO [Productos] ([Nombre], [Descripcion], [Activo])
SELECT p.[Nombre], p.[Descripcion], 1
FROM @Productos p
WHERE NOT EXISTS (SELECT 1 FROM [Productos] WHERE [Nombre] = p.[Nombre]);

-- Actualizar @ProductoIds
DELETE FROM @ProductoIds;
INSERT INTO @ProductoIds (ProductoId, Nombre)
SELECT [Id], [Nombre]
FROM [Productos];

-- DetalleProducto (50 ejemplos variados)
-- Crear combinaciones específicas de productos con categorías, marcas y modelos
DECLARE @DetalleProductoData TABLE (
    ProductoNombre NVARCHAR(200),
    CategoriaNombre NVARCHAR(100),
    MarcaNombre NVARCHAR(100),
    ModeloNombre NVARCHAR(100),
    Codigo NVARCHAR(50),
    PrecioCompra DECIMAL(18,2),
    PrecioVenta DECIMAL(18,2),
    Stock INT,
    StockMinimo INT
);

-- Insertar 50 combinaciones variadas
INSERT INTO @DetalleProductoData VALUES
('Ron Bacardi Superior', 'Ron', 'Bacardi', '750ml', 'DP0001', 120.00, 180.00, 45, 10),
('Ron Havana Club 7 Años', 'Ron', 'Havana Club', '750ml', 'DP0002', 180.00, 270.00, 30, 10),
('Ron Flor de Caña 12 Años', 'Ron', 'Flor de Caña', '750ml Premium', 'DP0003', 250.00, 380.00, 25, 10),
('Vodka Smirnoff', 'Vodka', 'Smirnoff', '750ml', 'DP0004', 100.00, 150.00, 60, 10),
('Vodka Absolut', 'Vodka', 'Absolut', '750ml', 'DP0005', 130.00, 195.00, 50, 10),
('Whisky Johnnie Walker Black', 'Whisky', 'Johnnie Walker', '750ml', 'DP0006', 350.00, 525.00, 20, 5),
('Whisky Jack Daniels', 'Whisky', 'Jack Daniels', '750ml', 'DP0007', 280.00, 420.00, 35, 10),
('Tequila Jose Cuervo Tradicional', 'Tequila', 'Jose Cuervo', '750ml', 'DP0008', 150.00, 225.00, 40, 10),
('Tequila Patrón Silver', 'Tequila', 'Patrón', '750ml Premium', 'DP0009', 320.00, 480.00, 15, 5),
('Ginebra Bombay Sapphire', 'Ginebra', 'Bombay Sapphire', '750ml', 'DP0010', 200.00, 300.00, 28, 10),
('Coñac Hennessy VS', 'Coñac', 'Hennessy', '750ml', 'DP0011', 400.00, 600.00, 18, 5),
('Champagne Moët & Chandon', 'Champagne', 'Moët & Chandon', '750ml', 'DP0012', 450.00, 675.00, 12, 5),
('Vino Tinto Concha y Toro', 'Vino Tinto', 'Concha y Toro', '750ml', 'DP0013', 80.00, 120.00, 70, 15),
('Vino Blanco Santa Rita', 'Vino Blanco', 'Santa Rita', '750ml', 'DP0014', 75.00, 112.50, 65, 15),
('Cerveza Corona Extra', 'Cerveza', 'Corona', 'Lata 355ml', 'DP0015', 15.00, 25.00, 200, 50),
('Cerveza Heineken', 'Cerveza', 'Heineken', 'Lata 355ml', 'DP0016', 18.00, 30.00, 180, 50),
('Licor Baileys', 'Licor', 'Baileys', '750ml', 'DP0017', 220.00, 330.00, 22, 10),
('Licor Kahlúa', 'Licor', 'Kahlúa', '750ml', 'DP0018', 190.00, 285.00, 26, 10),
('Ron Captain Morgan', 'Ron', 'Captain Morgan', '750ml', 'DP0019', 140.00, 210.00, 38, 10),
('Whisky Chivas Regal 12', 'Whisky', 'Chivas Regal', '750ml Premium', 'DP0020', 380.00, 570.00, 16, 5),
('Tequila Don Julio 1942', 'Tequila', 'Don Julio', '750ml Ultra Premium', 'DP0021', 600.00, 900.00, 8, 3),
('Vodka Grey Goose', 'Vodka', 'Grey Goose', '750ml Premium', 'DP0022', 280.00, 420.00, 19, 5),
('Coñac Rémy Martin VSOP', 'Coñac', 'Rémy Martin', '750ml Premium', 'DP0023', 450.00, 675.00, 14, 5),
('Champagne Dom Pérignon', 'Champagne', 'Dom Pérignon', '750ml Ultra Premium', 'DP0024', 1200.00, 1800.00, 5, 2),
('Vino Rosado Concha y Toro', 'Vino Rosado', 'Concha y Toro', '750ml', 'DP0025', 85.00, 127.50, 55, 15),
('Cerveza Stella Artois', 'Cerveza', 'Stella Artois', 'Lata 355ml', 'DP0026', 20.00, 35.00, 150, 50),
('Licor Amarula', 'Licor', 'Amarula', '750ml', 'DP0027', 210.00, 315.00, 24, 10),
('Licor Jägermeister', 'Licor', 'Jägermeister', '750ml', 'DP0028', 170.00, 255.00, 32, 10),
('Aperitivo Fernet Branca', 'Aperitivo', 'Fernet Branca', '750ml', 'DP0029', 160.00, 240.00, 30, 10),
('Licor Grand Marnier', 'Licor', 'Grand Marnier', '750ml', 'DP0030', 240.00, 360.00, 20, 10),
('Ron Bacardi Superior', 'Ron', 'Bacardi', '1L', 'DP0031', 150.00, 225.00, 35, 10),
('Ron Havana Club 7 Años', 'Ron', 'Havana Club', '1L', 'DP0032', 220.00, 330.00, 25, 10),
('Vodka Smirnoff', 'Vodka', 'Smirnoff', '1L', 'DP0033', 125.00, 187.50, 45, 10),
('Whisky Johnnie Walker Black', 'Whisky', 'Johnnie Walker', '1L', 'DP0034', 420.00, 630.00, 15, 5),
('Tequila Jose Cuervo Tradicional', 'Tequila', 'Jose Cuervo', '1L', 'DP0035', 180.00, 270.00, 30, 10),
('Cerveza Corona Extra', 'Cerveza', 'Corona', '6 Pack', 'DP0036', 80.00, 130.00, 100, 30),
('Cerveza Heineken', 'Cerveza', 'Heineken', '6 Pack', 'DP0037', 95.00, 160.00, 90, 30),
('Vino Tinto Concha y Toro', 'Vino Tinto', 'Concha y Toro', '1L', 'DP0038', 100.00, 150.00, 50, 15),
('Ron Flor de Caña 12 Años', 'Ron', 'Flor de Caña', '1L Premium', 'DP0039', 300.00, 450.00, 18, 5),
('Vodka Absolut', 'Vodka', 'Absolut', '1L', 'DP0040', 160.00, 240.00, 40, 10),
('Whisky Jack Daniels', 'Whisky', 'Jack Daniels', '1L', 'DP0041', 340.00, 510.00, 22, 5),
('Tequila Patrón Silver', 'Tequila', 'Patrón', '1L Premium', 'DP0042', 380.00, 570.00, 12, 5),
('Ginebra Bombay Sapphire', 'Ginebra', 'Bombay Sapphire', '1L', 'DP0043', 240.00, 360.00, 20, 10),
('Coñac Hennessy VS', 'Coñac', 'Hennessy', '1L', 'DP0044', 480.00, 720.00, 10, 5),
('Champagne Moët & Chandon', 'Champagne', 'Moët & Chandon', '1L', 'DP0045', 540.00, 810.00, 8, 3),
('Cerveza Corona Extra', 'Cerveza', 'Corona', '12 Pack', 'DP0046', 150.00, 240.00, 80, 25),
('Cerveza Heineken', 'Cerveza', 'Heineken', '12 Pack', 'DP0047', 180.00, 300.00, 70, 25),
('Licor Baileys', 'Licor', 'Baileys', '1L', 'DP0048', 270.00, 405.00, 16, 5),
('Licor Kahlúa', 'Licor', 'Kahlúa', '1L', 'DP0049', 230.00, 345.00, 20, 10),
('Ron Captain Morgan', 'Ron', 'Captain Morgan', '1L', 'DP0050', 170.00, 255.00, 28, 10);

-- Insertar DetalleProducto
INSERT INTO [DetalleProducto] ([ProductoId], [CategoriaId], [MarcaId], [ModeloId], [Codigo], [SKU], [PrecioCompra], [PrecioVenta], [Stock], [StockMinimo], [UnidadMedida], [Activo])
SELECT 
    p.[Id],
    c.[Id],
    m.[Id],
    mod.[Id],
    dpd.[Codigo],
    'SKU-' + dpd.[Codigo],
    dpd.[PrecioCompra],
    dpd.[PrecioVenta],
    dpd.[Stock],
    dpd.[StockMinimo],
    'Botella',
    1
FROM @DetalleProductoData dpd
INNER JOIN [Productos] p ON p.[Nombre] = dpd.[ProductoNombre]
INNER JOIN [Categorias] c ON c.[Nombre] = dpd.[CategoriaNombre]
INNER JOIN [Marcas] m ON m.[Nombre] = dpd.[MarcaNombre]
INNER JOIN [Modelos] mod ON mod.[Nombre] = dpd.[ModeloNombre]
WHERE NOT EXISTS (SELECT 1 FROM [DetalleProducto] WHERE [Codigo] = dpd.[Codigo]);
GO

PRINT 'Productos y DetalleProducto creados exitosamente';
GO

-- =============================================
-- 5. EMPLEADOS, CLIENTES, PROVEEDORES
-- =============================================

-- Obtener IDs de usuarios para empleados
DECLARE @AdminUserIdForEmp INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'admin');
DECLARE @VendedorUserIdForEmp INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'vendedor1');
DECLARE @SupervisorUserIdForEmp INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'supervisor1');

-- Empleados (5 empleados) - Solo si no existen
IF NOT EXISTS (SELECT 1 FROM [Empleados] WHERE [CodigoEmpleado] = 'EMP001')
BEGIN
    INSERT INTO [Empleados] ([UsuarioId], [CodigoEmpleado], [NombreCompleto], [Telefono], [Email], [Direccion], [FechaNacimiento], [FechaIngreso], [Salario], [Departamento], [Puesto], [Activo])
    VALUES
    (@AdminUserIdForEmp, 'EMP001', 'Carlos Ramírez', '505-2234-5678', 'carlos.ramirez@licoreria.com', 'Managua, Nicaragua', '1985-05-15', '2020-01-15', 15000.00, 'Administración', 'Gerente', 1);
    PRINT 'Empleado EMP001 creado';
END
GO

IF NOT EXISTS (SELECT 1 FROM [Empleados] WHERE [CodigoEmpleado] = 'EMP002')
BEGIN
    INSERT INTO [Empleados] ([UsuarioId], [CodigoEmpleado], [NombreCompleto], [Telefono], [Email], [Direccion], [FechaNacimiento], [FechaIngreso], [Salario], [Departamento], [Puesto], [Activo])
    VALUES
    (@VendedorUserIdForEmp, 'EMP002', 'Ana Martínez', '505-2234-5679', 'ana.martinez@licoreria.com', 'Managua, Nicaragua', '1990-08-22', '2021-03-10', 12000.00, 'Ventas', 'Vendedora', 1);
    PRINT 'Empleado EMP002 creado';
END
GO

IF NOT EXISTS (SELECT 1 FROM [Empleados] WHERE [CodigoEmpleado] = 'EMP003')
BEGIN
    INSERT INTO [Empleados] ([UsuarioId], [CodigoEmpleado], [NombreCompleto], [Telefono], [Email], [Direccion], [FechaNacimiento], [FechaIngreso], [Salario], [Departamento], [Puesto], [Activo])
    VALUES
    (@SupervisorUserIdForEmp, 'EMP003', 'Luis Rodríguez', '505-2234-5680', 'luis.rodriguez@licoreria.com', 'Managua, Nicaragua', '1988-11-30', '2019-06-20', 11000.00, 'Ventas', 'Vendedor', 1);
    PRINT 'Empleado EMP003 creado';
END
GO

-- Crear 2 empleados adicionales sin usuario (para tener 5 empleados total)
DECLARE @EmpleadoId4 INT;
DECLARE @EmpleadoId5 INT;

IF NOT EXISTS (SELECT 1 FROM [Empleados] WHERE [CodigoEmpleado] = 'EMP004')
BEGIN
    INSERT INTO [Empleados] ([UsuarioId], [CodigoEmpleado], [NombreCompleto], [Telefono], [Email], [Direccion], [FechaNacimiento], [FechaIngreso], [Salario], [Departamento], [Puesto], [Activo])
    VALUES
    (NULL, 'EMP004', 'Sofía Hernández', '505-2234-5681', 'sofia.hernandez@licoreria.com', 'Managua, Nicaragua', '1992-02-14', '2022-01-05', 10000.00, 'Inventario', 'Almacenista', 1);
    SET @EmpleadoId4 = SCOPE_IDENTITY();
    PRINT 'Empleado EMP004 creado';
END
ELSE
BEGIN
    SET @EmpleadoId4 = (SELECT [Id] FROM [Empleados] WHERE [CodigoEmpleado] = 'EMP004');
END
GO

IF NOT EXISTS (SELECT 1 FROM [Empleados] WHERE [CodigoEmpleado] = 'EMP005')
BEGIN
    INSERT INTO [Empleados] ([UsuarioId], [CodigoEmpleado], [NombreCompleto], [Telefono], [Email], [Direccion], [FechaNacimiento], [FechaIngreso], [Salario], [Departamento], [Puesto], [Activo])
    VALUES
    (NULL, 'EMP005', 'Roberto López', '505-2234-5682', 'roberto.lopez@licoreria.com', 'Managua, Nicaragua', '1987-07-18', '2020-09-12', 11500.00, 'Ventas', 'Vendedor', 1);
    SET @EmpleadoId5 = SCOPE_IDENTITY();
    PRINT 'Empleado EMP005 creado';
END
ELSE
BEGIN
    SET @EmpleadoId5 = (SELECT [Id] FROM [Empleados] WHERE [CodigoEmpleado] = 'EMP005');
END
GO

-- Clientes (5 clientes) - Solo si no existen
IF NOT EXISTS (SELECT 1 FROM [Clientes] WHERE [CodigoCliente] = 'CLI001')
BEGIN
    INSERT INTO [Clientes] ([CodigoCliente], [NombreCompleto], [RazonSocial], [RFC], [Direccion], [Telefono], [Email], [Activo])
    VALUES
    ('CLI001', 'Restaurante El Buen Sabor', 'El Buen Sabor S.A.', 'RBS123456789', 'Managua, Zona 1', '505-2255-1234', 'contacto@buensabor.com', 1),
    ('CLI002', 'Bar La Rumba', 'La Rumba S.A.', 'LRB987654321', 'Managua, Zona 2', '505-2255-2345', 'info@larumba.com', 1),
    ('CLI003', 'Hotel Camino Real', 'Hotel Camino Real S.A.', 'HCR456789123', 'Managua, Zona 3', '505-2255-3456', 'compras@caminoreal.com', 1),
    ('CLI004', 'Supermercado La Colonia', 'La Colonia S.A.', 'LC789123456', 'Managua, Zona 4', '505-2255-4567', 'compras@lacolonia.com', 1),
    ('CLI005', 'Juan Pérez (Cliente Frecuente)', NULL, NULL, 'Managua, Zona 5', '505-8888-9999', 'juan.perez@email.com', 1);
    PRINT '5 Clientes creados';
END
GO

-- Proveedores (5 proveedores) - Solo si no existen
IF NOT EXISTS (SELECT 1 FROM [Proveedores] WHERE [CodigoProveedor] = 'PROV001')
BEGIN
    INSERT INTO [Proveedores] ([CodigoProveedor], [Nombre], [RazonSocial], [RFC], [Direccion], [Telefono], [Email], [Activo])
    VALUES
    ('PROV001', 'Distribuidora Internacional', 'Distribuidora Internacional S.A.', 'DIN123456789', 'Managua, Zona Industrial', '505-2266-1111', 'ventas@distribuidora.com', 1),
    ('PROV002', 'Importadora de Bebidas', 'Importadora de Bebidas S.A.', 'IBE987654321', 'Managua, Zona Industrial', '505-2266-2222', 'compras@importadora.com', 1),
    ('PROV003', 'Bacardi Nicaragua', 'Bacardi Nicaragua S.A.', 'BAC456789123', 'Managua, Zona Industrial', '505-2266-3333', 'ventas@bacardi.com', 1),
    ('PROV004', 'Heineken Centroamérica', 'Heineken Centroamérica S.A.', 'HEC789123456', 'Managua, Zona Industrial', '505-2266-4444', 'ventas@heineken.com', 1),
    ('PROV005', 'Distribuidora Nacional', 'Distribuidora Nacional S.A.', 'DNA321654987', 'Managua, Zona Industrial', '505-2266-5555', 'ventas@distribuidoranacional.com', 1);
    PRINT '5 Proveedores creados';
END
GO

PRINT 'Empleados, Clientes y Proveedores creados exitosamente';
GO

-- =============================================
-- 6. TRANSACCIONES: COMPRAS, VENTAS, DEVOLUCIONES
-- =============================================

-- Variables para IDs
DECLARE @AdminUserId INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'admin');
DECLARE @VendedorUserId INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'vendedor1');
DECLARE @SupervisorUserId INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'supervisor1');

-- Obtener IDs necesarios
DECLARE @EmpleadoIds TABLE (EmpleadoId INT);
DECLARE @ClienteIds TABLE (ClienteId INT);
DECLARE @ProveedorIds TABLE (ProveedorId INT);
DECLARE @DetalleProductoIds TABLE (DetalleProductoId INT);

INSERT INTO @EmpleadoIds SELECT [Id] FROM [Empleados];
INSERT INTO @ClienteIds SELECT [Id] FROM [Clientes];
INSERT INTO @ProveedorIds SELECT [Id] FROM [Proveedores];
INSERT INTO @DetalleProductoIds SELECT [Id] FROM [DetalleProducto];

-- Crear 100 Compras (últimos 6 meses)
DECLARE @ComprasCount INT = 1;
DECLARE @FechaInicio DATE = DATEADD(MONTH, -6, GETDATE());
DECLARE @FechaFin DATE = GETDATE();
DECLARE @FechaCompra DATETIME2;
DECLARE @ProveedorId INT;
DECLARE @UsuarioId INT;
DECLARE @CompraId INT;
DECLARE @FolioCompra NVARCHAR(50);
DECLARE @DetallesCount INT;
DECLARE @SubtotalTotal DECIMAL(18,2);
DECLARE @DetalleProductoId INT;
DECLARE @Cantidad DECIMAL(18,2);
DECLARE @PrecioUnitario DECIMAL(18,2);
DECLARE @Subtotal DECIMAL(18,2);
DECLARE @Impuestos DECIMAL(18,2);
DECLARE @Total DECIMAL(18,2);

SET NOCOUNT ON;

WHILE @ComprasCount <= 100
BEGIN
    SET @FechaCompra = DATEADD(DAY, ABS(CHECKSUM(NEWID())) % DATEDIFF(DAY, @FechaInicio, @FechaFin), @FechaInicio);
    SET @ProveedorId = (SELECT TOP 1 [ProveedorId] FROM @ProveedorIds ORDER BY NEWID());
    SET @UsuarioId = CASE WHEN @ComprasCount % 3 = 0 THEN @AdminUserId ELSE @VendedorUserId END;
    SET @FolioCompra = 'COMP-' + RIGHT('0000' + CAST(@ComprasCount AS VARCHAR), 4);
    
    INSERT INTO [Compras] ([Folio], [ProveedorId], [UsuarioId], [FechaCompra], [Subtotal], [Impuestos], [Total], [Estado], [Observaciones])
    VALUES (@FolioCompra, @ProveedorId, @UsuarioId, @FechaCompra, 0, 0, 0, 'Completada', 'Compra de prueba');
    
    SET @CompraId = SCOPE_IDENTITY();
    SET @DetallesCount = 1 + (ABS(CHECKSUM(NEWID())) % 5);
    SET @SubtotalTotal = 0;
    
    WHILE @DetallesCount > 0
    BEGIN
        SET @DetalleProductoId = (SELECT TOP 1 [DetalleProductoId] FROM @DetalleProductoIds ORDER BY NEWID());
        SET @Cantidad = 10 + (ABS(CHECKSUM(NEWID())) % 40);
        SET @PrecioUnitario = (SELECT [PrecioCompra] FROM [DetalleProducto] WHERE [Id] = @DetalleProductoId);
        SET @Subtotal = @Cantidad * @PrecioUnitario;
        
        INSERT INTO [ComprasDetalle] ([CompraId], [DetalleProductoId], [Cantidad], [PrecioUnitario], [Subtotal])
        VALUES (@CompraId, @DetalleProductoId, @Cantidad, @PrecioUnitario, @Subtotal);
        
        SET @SubtotalTotal = @SubtotalTotal + @Subtotal;
        SET @DetallesCount = @DetallesCount - 1;
    END
    
    SET @Impuestos = @SubtotalTotal * 0.15;
    SET @Total = @SubtotalTotal + @Impuestos;
    
    UPDATE [Compras] SET [Subtotal] = @SubtotalTotal, [Impuestos] = @Impuestos, [Total] = @Total WHERE [Id] = @CompraId;
    
    SET @ComprasCount = @ComprasCount + 1;
END

SET NOCOUNT OFF;
PRINT '100 Compras creadas exitosamente';
GO

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
-- Verificar que existan ventas antes de crear devoluciones
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

    -- Obtener IDs de usuarios
    DECLARE @AdminUserIdDev INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'admin');
    DECLARE @VendedorUserIdDev INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'vendedor1');
    DECLARE @SupervisorUserIdDev INT = (SELECT [Id] FROM [Usuarios] WHERE [NombreUsuario] = 'supervisor1');

    -- Obtener ventas disponibles (que tengan al menos un detalle)
    DECLARE @VentaIdsDisponibles TABLE (VentaId INT, RowNum INT);
    INSERT INTO @VentaIdsDisponibles (VentaId, RowNum)
    SELECT DISTINCT v.[Id], ROW_NUMBER() OVER (ORDER BY v.[Id])
    FROM [Ventas] v
    INNER JOIN [VentasDetalle] vd ON v.[Id] = vd.[VentaId]
    WHERE NOT EXISTS (
        SELECT 1 FROM [DevolucionesVenta] dv 
        WHERE dv.[VentaId] = v.[Id] AND dv.[Estado] = 'Completada'
    );

    SET NOCOUNT ON;

    WHILE @DevolucionesCount <= 12 AND (SELECT COUNT(*) FROM @VentaIdsDisponibles) > 0
    BEGIN
        -- Obtener una venta aleatoria que no haya sido devuelta
        SELECT TOP 1 @VentaDevolucionId = [VentaId]
        FROM @VentaIdsDisponibles
        WHERE [VentaId] NOT IN (SELECT [VentaId] FROM [DevolucionesVenta] WHERE [Estado] = 'Completada')
        ORDER BY NEWID();

        -- Si no hay ventas disponibles, salir del loop
        IF @VentaDevolucionId IS NULL
            BREAK;

        SET @FechaDevolucion = DATEADD(DAY, ABS(CHECKSUM(NEWID())) % DATEDIFF(DAY, @FechaInicioDevolucion, @FechaFinDevolucion), @FechaInicioDevolucion);
        SET @UsuarioDevolucionId = CASE WHEN @DevolucionesCount % 3 = 0 THEN @SupervisorUserIdDev 
                                        WHEN @DevolucionesCount % 2 = 0 THEN @VendedorUserIdDev 
                                        ELSE @AdminUserIdDev END;
        SET @FolioDevolucion = 'DEV-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + RIGHT('0000' + CAST(@DevolucionesCount AS VARCHAR), 4);
        
        -- Obtener un detalle de la venta que no haya sido devuelto
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

        -- Si no hay detalles disponibles, eliminar esta venta de la lista y continuar
        IF @VentaDetalleId IS NULL
        BEGIN
            DELETE FROM @VentaIdsDisponibles WHERE [VentaId] = @VentaDevolucionId;
            CONTINUE;
        END

        -- Calcular cantidad a devolver (máximo la cantidad original, mínimo 1)
        SET @CantidadDevolver = CASE 
            WHEN @CantidadOriginal <= 1 THEN 1
            WHEN @CantidadOriginal <= 3 THEN 1 + (ABS(CHECKSUM(NEWID())) % CAST(@CantidadOriginal AS INT))
            ELSE 1 + (ABS(CHECKSUM(NEWID())) % 3)
        END;

        -- Calcular subtotal de devolución
        SET @SubtotalDevolucion = (@SubtotalOriginal / @CantidadOriginal) * @CantidadDevolver;
        SET @TotalDevolucion = @SubtotalDevolucion;
        
        -- Insertar devolución
        INSERT INTO [DevolucionesVenta] ([Folio], [VentaId], [UsuarioId], [FechaDevolucion], [Motivo], [TotalDevolucion], [Estado], [Observaciones])
        VALUES (@FolioDevolucion, @VentaDevolucionId, @UsuarioDevolucionId, @FechaDevolucion, 'Devolución de prueba', @TotalDevolucion, 'Completada', 'Devolución de prueba para pruebas del sistema');
        
        SET @DevolucionVentaId = SCOPE_IDENTITY();
        
        -- Insertar detalle de devolución
        INSERT INTO [DevolucionesVentaDetalle] ([DevolucionVentaId], [VentaDetalleId], [DetalleProductoId], [CantidadDevolver], [Motivo], [Subtotal])
        VALUES (@DevolucionVentaId, @VentaDetalleId, @DetalleProductoDevolucionId, @CantidadDevolver, 'Producto defectuoso o devolución solicitada por cliente', @SubtotalDevolucion);
        
        -- Eliminar esta venta de la lista para evitar duplicados
        DELETE FROM @VentaIdsDisponibles WHERE [VentaId] = @VentaDevolucionId;
        
        SET @DevolucionesCount = @DevolucionesCount + 1;
        SET @VentaDevolucionId = NULL;
        SET @VentaDetalleId = NULL;
    END

    SET NOCOUNT OFF;
    PRINT 'Devoluciones creadas exitosamente: ' + CAST(@DevolucionesCount - 1 AS VARCHAR);
END
ELSE
BEGIN
    PRINT 'ADVERTENCIA: No se pueden crear devoluciones porque no hay ventas en el sistema.';
END
GO

-- =============================================
-- 7. MOVIMIENTOSSTOCK (Generados automáticamente desde compras y ventas)
-- =============================================

-- Crear movimientos de stock desde compras (entradas)
-- Ordenar por fecha para calcular stock correctamente
WITH ComprasOrdenadas AS (
    SELECT 
        cd.[Id] AS CompraDetalleId,
        cd.[DetalleProductoId],
        cd.[Cantidad],
        c.[Id] AS CompraId,
        c.[UsuarioId],
        c.[FechaCompra],
        ROW_NUMBER() OVER (PARTITION BY cd.[DetalleProductoId] ORDER BY c.[FechaCompra], c.[Id]) AS Orden
    FROM [ComprasDetalle] cd
    INNER JOIN [Compras] c ON cd.[CompraId] = c.[Id]
)
INSERT INTO [MovimientosStock] ([DetalleProductoId], [TipoMovimiento], [Cantidad], [StockAnterior], [StockNuevo], [ReferenciaId], [ReferenciaTipo], [UsuarioId], [Motivo], [FechaMovimiento])
SELECT 
    co.[DetalleProductoId],
    'Entrada',
    co.[Cantidad],
    ISNULL((
        SELECT TOP 1 [StockNuevo] 
        FROM [MovimientosStock] 
        WHERE [DetalleProductoId] = co.[DetalleProductoId] 
          AND [FechaMovimiento] < co.[FechaCompra]
        ORDER BY [FechaMovimiento] DESC
    ), (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = co.[DetalleProductoId])) AS StockAnterior,
    ISNULL((
        SELECT TOP 1 [StockNuevo] 
        FROM [MovimientosStock] 
        WHERE [DetalleProductoId] = co.[DetalleProductoId] 
          AND [FechaMovimiento] < co.[FechaCompra]
        ORDER BY [FechaMovimiento] DESC
    ), (SELECT [Stock] FROM [DetalleProducto] WHERE [Id] = co.[DetalleProductoId])) + co.[Cantidad] AS StockNuevo,
    co.[CompraId] AS ReferenciaId,
    'Compra' AS ReferenciaTipo,
    co.[UsuarioId],
    'Compra de productos',
    co.[FechaCompra]
FROM ComprasOrdenadas co
ORDER BY co.[FechaCompra], co.[CompraId];
GO

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

PRINT 'MovimientosStock creados exitosamente desde transacciones';
GO

-- =============================================
-- 8. PRECIOS Y DESCUENTOS
-- =============================================

-- Insertar precios históricos para algunos productos
INSERT INTO [Precios] ([DetalleProductoId], [PrecioCompra], [PrecioVenta], [PrecioVentaMinimo], [FechaInicio], [FechaFin], [Activo])
SELECT TOP 20
    [Id],
    [PrecioCompra] * 0.9, -- Precio anterior más bajo
    [PrecioVenta] * 0.95,  -- Precio anterior más bajo
    [PrecioVenta] * 0.85, -- Precio mínimo
    DATEADD(MONTH, -3, GETDATE()),
    DATEADD(DAY, -1, GETDATE()),
    0 -- Inactivo (precio histórico)
FROM [DetalleProducto]
ORDER BY NEWID();
GO

-- Insertar descuentos activos
INSERT INTO [Descuentos] ([Nombre], [Tipo], [Valor], [DetalleProductoId], [CategoriaId], [FechaInicio], [FechaFin], [Activo])
VALUES
('Descuento 10% en Ron', 'Porcentaje', 10.00, NULL, (SELECT [Id] FROM [Categorias] WHERE [Nombre] = 'Ron'), DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, 30, GETDATE()), 1),
('Descuento 15% en Vodka', 'Porcentaje', 15.00, NULL, (SELECT [Id] FROM [Categorias] WHERE [Nombre] = 'Vodka'), DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, 15, GETDATE()), 1),
('Descuento Fijo Cerveza', 'Fijo', 50.00, NULL, (SELECT [Id] FROM [Categorias] WHERE [Nombre] = 'Cerveza'), DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, 20, GETDATE()), 1),
('Descuento Producto Específico', 'Porcentaje', 20.00, (SELECT TOP 1 [Id] FROM [DetalleProducto] ORDER BY NEWID()), NULL, DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, 25, GETDATE()), 1);
GO

-- =============================================
-- 9. CONFIGURACIÓN DEL SISTEMA
-- =============================================

INSERT INTO [ConfiguracionSistema] ([Clave], [Valor], [Tipo], [Descripcion])
VALUES
('IVA_Porcentaje', '15', 'Decimal', 'Porcentaje de IVA aplicable'),
('TasaCambio', '36.50', 'Decimal', 'Tasa de cambio USD a Córdoba'),
('DiasVencimientoStock', '30', 'Int', 'Días para considerar stock próximo a vencer'),
('EmailNotificaciones', 'admin@licoreria.com', 'String', 'Email para notificaciones del sistema'),
('StockMinimoGlobal', '10', 'Int', 'Stock mínimo global para alertas'),
('HabilitarDescuentos', 'true', 'Boolean', 'Habilitar o deshabilitar descuentos'),
('MetodosPago', 'Efectivo,Tarjeta,Transferencia', 'String', 'Métodos de pago disponibles');
GO

PRINT '=============================================';
PRINT 'Datos de prueba insertados exitosamente';
PRINT '=============================================';
PRINT 'Resumen:';
PRINT '- 3 Roles creados (Administrador, Vendedor, Supervisor)';
PRINT '- 3 Usuarios creados';
PRINT '- 20 Categorías creadas';
PRINT '- 30 Marcas creadas';
PRINT '- 25 Modelos creados';
PRINT '- 30 Productos creados';
PRINT '- 50 DetalleProducto creados';
PRINT '- 5 Empleados creados (3 vinculados a usuarios, 2 sin usuario)';
PRINT '- 5 Clientes creados';
PRINT '- 5 Proveedores creados';
PRINT '- 100 Compras creadas';
PRINT '- 100 Ventas creadas';
PRINT '- Devoluciones creadas (si hay ventas disponibles)';
PRINT '- MovimientosStock generados automáticamente';
PRINT '- Precios históricos creados';
PRINT '- Descuentos creados';
PRINT '- Configuración del sistema creada';
PRINT '=============================================';
PRINT '';
PRINT 'Verificación de datos insertados:';
PRINT 'Empleados: ' + CAST((SELECT COUNT(*) FROM [Empleados]) AS VARCHAR);
PRINT 'Devoluciones: ' + CAST((SELECT COUNT(*) FROM [DevolucionesVenta]) AS VARCHAR);
PRINT 'Ventas: ' + CAST((SELECT COUNT(*) FROM [Ventas]) AS VARCHAR);
PRINT 'Compras: ' + CAST((SELECT COUNT(*) FROM [Compras]) AS VARCHAR);
PRINT 'MovimientosStock: ' + CAST((SELECT COUNT(*) FROM [MovimientosStock]) AS VARCHAR);
PRINT '=============================================';

