# Script para generar hashes de BCrypt para las contraseñas
# Uso: powershell -ExecutionPolicy Bypass -File .\scripts\generar-hashes-password.ps1

Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Generador de Hashes BCrypt" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

# Instalar BCrypt.Net si no está instalado
$moduleInstalled = Get-Module -ListAvailable -Name BCrypt.Net
if (-not $moduleInstalled) {
    Write-Host "Instalando BCrypt.Net..." -ForegroundColor Yellow
    Install-Package BCrypt.Net-Next -Force -SkipDependencies
}

# Importar BCrypt
Add-Type -Path "C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.0\ref\net8.0\System.Security.Cryptography.dll" -ErrorAction SilentlyContinue
$bcryptPath = (Get-ChildItem -Path . -Recurse -Filter "BCrypt.Net*.dll" | Select-Object -First 1).FullName
if ($bcryptPath) {
    Add-Type -Path $bcryptPath
}

Write-Host "Generando hashes BCrypt..." -ForegroundColor Yellow
Write-Host ""

# Contraseñas de prueba (estándar)
$passwords = @{
    "admin" = "Admin123!"
    "vendedor1" = "Vendedor123!"
    "supervisor1" = "Supervisor123!"
}

Write-Host "Contraseñas y sus hashes:" -ForegroundColor Cyan
Write-Host ""

foreach ($user in $passwords.Keys) {
    $password = $passwords[$user]
    # Generar hash usando .NET directamente
    $hash = [BCrypt.Net.BCrypt]::HashPassword($password, [BCrypt.Net.BCrypt]::GenerateSalt(12))
    Write-Host "Usuario: $user" -ForegroundColor Yellow
    Write-Host "Contraseña: $password" -ForegroundColor White
    Write-Host "Hash: $hash" -ForegroundColor Green
    Write-Host ""
}

Write-Host "==========================================" -ForegroundColor Green
Write-Host "Hashes generados. Copia estos hashes para el script SQL." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Green

