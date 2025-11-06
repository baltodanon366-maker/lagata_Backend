-- =============================================
-- Script para actualizar hashes de contraseñas con BCrypt
-- Ejecutar este script después de InsertTestData.sql
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- Hashes BCrypt generados para las contraseñas:
-- admin / Admin123!
-- vendedor1 / Vendedor123!
-- supervisor1 / Supervisor123!

-- =============================================
-- Actualizar hashes de contraseñas con BCrypt reales
-- Contraseñas:
--   admin / Admin123!
--   vendedor1 / Vendedor123!
--   supervisor1 / Supervisor123!
-- =============================================

-- Actualizar hash del usuario admin
UPDATE [Usuarios]
SET [PasswordHash] = '$2a$12$cVT0CpbTFVrzhIjwGARLT.OhWDlwuaN1QE0gb/cs17Nqk.P75MX3K'
WHERE [NombreUsuario] = 'admin';

IF @@ROWCOUNT > 0
    PRINT 'Hash de admin actualizado correctamente (Contraseña: Admin123!)';
ELSE
    PRINT 'ADVERTENCIA: No se encontró el usuario admin';
GO

-- Actualizar hash del usuario vendedor1
UPDATE [Usuarios]
SET [PasswordHash] = '$2a$12$bIkYA.iBd0nOQVxy1vKKceNULcnNIWt4wqxV83FbXqnGowIzemXV6'
WHERE [NombreUsuario] = 'vendedor1';

IF @@ROWCOUNT > 0
    PRINT 'Hash de vendedor1 actualizado correctamente (Contraseña: Vendedor123!)';
ELSE
    PRINT 'ADVERTENCIA: No se encontró el usuario vendedor1';
GO

-- Actualizar hash del usuario supervisor1
UPDATE [Usuarios]
SET [PasswordHash] = '$2a$12$NdKZ8iT/xc/lGQH6idRTROAh4LphaU76uCRIpcBSHm0c/d6guJw6.'
WHERE [NombreUsuario] = 'supervisor1';

IF @@ROWCOUNT > 0
    PRINT 'Hash de supervisor1 actualizado correctamente (Contraseña: Supervisor123!)';
ELSE
    PRINT 'ADVERTENCIA: No se encontró el usuario supervisor1';
GO

PRINT '=============================================';
PRINT 'Script completado. Hashes BCrypt actualizados.';
PRINT '=============================================';
PRINT '';
PRINT 'Credenciales de prueba:';
PRINT '  admin / Admin123!';
PRINT '  vendedor1 / Vendedor123!';
PRINT '  supervisor1 / Supervisor123!';
PRINT '=============================================';

