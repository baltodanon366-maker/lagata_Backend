// Script para generar hashes BCrypt - Ejecutar en .NET Interactive o crear un proyecto de consola
// Usa: dotnet script generar-hashes-csharp.cs
// O crea un proyecto de consola y copia este código

using BCrypt.Net;
using BCrypt.Net;

Console.WriteLine("==========================================");
Console.WriteLine("  Generador de Hashes BCrypt");
Console.WriteLine("==========================================");
Console.WriteLine();

// Contraseñas estándar
string adminPassword = "Admin123!";
string vendedorPassword = "Vendedor123!";
string supervisorPassword = "Supervisor123!";

// Generar hashes
string adminHash = BCrypt.HashPassword(adminPassword, BCrypt.GenerateSalt(12));
string vendedorHash = BCrypt.HashPassword(vendedorPassword, BCrypt.GenerateSalt(12));
string supervisorHash = BCrypt.HashPassword(supervisorPassword, BCrypt.GenerateSalt(12));

Console.WriteLine("Usuario: admin");
Console.WriteLine($"Contraseña: {adminPassword}");
Console.WriteLine($"Hash: {adminHash}");
Console.WriteLine();

Console.WriteLine("Usuario: vendedor1");
Console.WriteLine($"Contraseña: {vendedorPassword}");
Console.WriteLine($"Hash: {vendedorHash}");
Console.WriteLine();

Console.WriteLine("Usuario: supervisor1");
Console.WriteLine($"Contraseña: {supervisorPassword}");
Console.WriteLine($"Hash: {supervisorHash}");
Console.WriteLine();

Console.WriteLine("==========================================");
Console.WriteLine("Copia estos hashes al script SQL:");
Console.WriteLine("==========================================");
Console.WriteLine();
Console.WriteLine($"UPDATE [Usuarios] SET [PasswordHash] = '{adminHash}' WHERE [NombreUsuario] = 'admin';");
Console.WriteLine($"UPDATE [Usuarios] SET [PasswordHash] = '{vendedorHash}' WHERE [NombreUsuario] = 'vendedor1';");
Console.WriteLine($"UPDATE [Usuarios] SET [PasswordHash] = '{supervisorHash}' WHERE [NombreUsuario] = 'supervisor1';");

