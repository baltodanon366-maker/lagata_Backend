-- =============================================
-- FIX URGENTE: Actualizar hashes de contraseñas
-- EJECUTAR ESTE SCRIPT EN AZURE SQL SERVER
-- =============================================

USE [dbLicoreriaLaGata]
GO

PRINT '=============================================';
PRINT '  ACTUALIZANDO HASHES DE CONTRASEÑAS';
PRINT '=============================================';
PRINT '';

-- Verificar estado actual
PRINT 'Estado ANTES de actualizar:';
SELECT 
    [NombreUsuario],
    CASE 
        WHEN [PasswordHash] LIKE 'PLACEHOLDER%' THEN '❌ Placeholder'
        WHEN [PasswordHash] LIKE '$2a$12$%' THEN '✅ BCrypt válido'
        ELSE '⚠️ Desconocido'
    END AS [Estado],
    LEFT([PasswordHash], 50) + '...' AS [HashPreview]
FROM [Usuarios]
WHERE [NombreUsuario] IN ('admin', 'vendedor1', 'supervisor1');
GO

-- Actualizar admin
PRINT '';
PRINT 'Actualizando admin...';
UPDATE [Usuarios]
SET [PasswordHash] = '$2a$12$cVT0CpbTFVrzhIjwGARLT.OhWDlwuaN1QE0gb/cs17Nqk.P75MX3K',
    [FechaModificacion] = GETUTCDATE()
WHERE [NombreUsuario] = 'admin';

IF @@ROWCOUNT > 0
    PRINT '✅ admin actualizado (Contraseña: Admin123!)';
ELSE
    PRINT '❌ No se encontró el usuario admin';
GO

-- Actualizar vendedor1
PRINT '';
PRINT 'Actualizando vendedor1...';
UPDATE [Usuarios]
SET [PasswordHash] = '$2a$12$bIkYA.iBd0nOQVxy1vKKceNULcnNIWt4wqxV83FbXqnGowIzemXV6',
    [FechaModificacion] = GETUTCDATE()
WHERE [NombreUsuario] = 'vendedor1';

IF @@ROWCOUNT > 0
    PRINT '✅ vendedor1 actualizado (Contraseña: Vendedor123!)';
ELSE
    PRINT '❌ No se encontró el usuario vendedor1';
GO

-- Actualizar supervisor1
PRINT '';
PRINT 'Actualizando supervisor1...';
UPDATE [Usuarios]
SET [PasswordHash] = '$2a$12$NdKZ8iT/xc/lGQH6idRTROAh4LphaU76uCRIpcBSHm0c/d6guJw6.',
    [FechaModificacion] = GETUTCDATE()
WHERE [NombreUsuario] = 'supervisor1';

IF @@ROWCOUNT > 0
    PRINT '✅ supervisor1 actualizado (Contraseña: Supervisor123!)';
ELSE
    PRINT '❌ No se encontró el usuario supervisor1';
GO

-- Verificar estado después
PRINT '';
PRINT '=============================================';
PRINT 'Estado DESPUÉS de actualizar:';
PRINT '=============================================';
SELECT 
    [NombreUsuario],
    CASE 
        WHEN [PasswordHash] LIKE 'PLACEHOLDER%' THEN '❌ Placeholder'
        WHEN [PasswordHash] LIKE '$2a$12$%' THEN '✅ BCrypt válido'
        ELSE '⚠️ Desconocido'
    END AS [Estado],
    [Activo],
    [Rol]
FROM [Usuarios]
WHERE [NombreUsuario] IN ('admin', 'vendedor1', 'supervisor1');
GO

PRINT '';
PRINT '=============================================';
PRINT '✅ ACTUALIZACIÓN COMPLETADA';
PRINT '=============================================';
PRINT '';
PRINT 'Credenciales de prueba:';
PRINT '  admin / Admin123!';
PRINT '  vendedor1 / Vendedor123!';
PRINT '  supervisor1 / Supervisor123!';
PRINT '';
PRINT '⚠️ IMPORTANTE: Reinicia la App Service en Azure';
PRINT '   az webapp restart --resource-group "RG Licoreria" --name "api-lagata"';
PRINT '=============================================';

