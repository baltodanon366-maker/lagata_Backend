-- =============================================
-- Script para verificar usuarios y sus hashes
-- Ejecutar en Azure SQL Server
-- =============================================

USE [dbLicoreriaLaGata]
GO

-- Verificar usuarios existentes
SELECT 
    [Id],
    [NombreUsuario],
    [Email],
    LEFT([PasswordHash], 50) + '...' AS [PasswordHashPreview],
    LEN([PasswordHash]) AS [HashLength],
    [Activo],
    [Rol],
    [UltimoAcceso]
FROM [Usuarios]
ORDER BY [Id];
GO

-- Verificar específicamente el usuario admin
SELECT 
    [Id],
    [NombreUsuario],
    [Email],
    [PasswordHash],
    [Activo],
    [Rol],
    CASE 
        WHEN [PasswordHash] LIKE 'PLACEHOLDER%' THEN '❌ Hash placeholder (necesita actualización)'
        WHEN [PasswordHash] LIKE '$2a$12$%' THEN '✅ Hash BCrypt válido'
        ELSE '⚠️ Hash desconocido'
    END AS [EstadoHash]
FROM [Usuarios]
WHERE [NombreUsuario] = 'admin';
GO

-- Verificar si el hash está actualizado
DECLARE @AdminHash NVARCHAR(MAX);
SELECT @AdminHash = [PasswordHash] 
FROM [Usuarios] 
WHERE [NombreUsuario] = 'admin';

IF @AdminHash = 'PLACEHOLDER_HASH_ADMIN123'
BEGIN
    PRINT '❌ ADVERTENCIA: El usuario admin todavía tiene el hash placeholder';
    PRINT '   Ejecuta: scripts/database/UpdatePasswordHashes.sql';
END
ELSE IF @AdminHash LIKE '$2a$12$%'
BEGIN
    PRINT '✅ El usuario admin tiene un hash BCrypt válido';
END
ELSE
BEGIN
    PRINT '⚠️ El hash del usuario admin tiene un formato desconocido: ' + LEFT(@AdminHash, 30) + '...';
END
GO

