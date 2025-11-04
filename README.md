# Licoreria API

API transaccional de facturación para una licorería desarrollada en .NET 8 con arquitectura escalable y mantenible.

## 📋 Descripción

Esta API utiliza una arquitectura de capas (Clean Architecture) para separar responsabilidades y facilitar el mantenimiento y escalabilidad del código. El sistema utiliza dos bases de datos:

- **SQL Server**: Para operaciones transaccionales críticas:
  - Autenticación y seguridad (Login, JWT)
  - Catálogos (Productos, Categorías, etc.)
  - Procesos de compras
  - Procesos de ventas
  - Devoluciones de ventas

- **MongoDB**: Para datos adicionales y flexibles (pendiente de definir casos de uso específicos)

## 🏗️ Arquitectura del Proyecto

El proyecto está organizado en las siguientes capas:

```
LicoreriaAPI/
├── src/
│   ├── LicoreriaAPI/                    # Capa de presentación (API)
│   │   ├── Controllers/                 # Controladores REST
│   │   ├── Extensions/                  # Extensiones de servicios
│   │   └── Program.cs                   # Configuración de la aplicación
│   │
│   ├── LicoreriaAPI.Domain/             # Capa de dominio
│   │   └── Models/                       # Entidades del dominio
│   │
│   ├── LicoreriaAPI.Application/        # Capa de aplicación
│   │   ├── Interfaces/
│   │   │   └── Services/                # Interfaces de servicios
│   │   └── Services/                     # Implementaciones de servicios
│   │
│   ├── LicoreriaAPI.Infrastructure/     # Capa de infraestructura
│   │   ├── Configuration/                # Clases de configuración
│   │   └── Data/
│   │       ├── SqlServer/                # Contexto EF Core (SQL Server)
│   │       └── MongoDB/                  # Contexto MongoDB
│   │
│   └── LicoreriaAPI.DTOs/                # Objetos de transferencia de datos
│       └── Auth/                         # DTOs por módulo
│
└── LicoreriaAPI.sln                      # Solución de Visual Studio
```

## 🚀 Tecnologías Utilizadas

- **.NET 8**
- **Entity Framework Core** (SQL Server)
- **MongoDB Driver** (MongoDB)
- **JWT Bearer Authentication**
- **Swagger/OpenAPI** (Documentación)
- **ASP.NET Core Web API**

## 📦 Configuración

### Requisitos Previos

1. .NET 8 SDK
2. SQL Server (local o remoto)
3. MongoDB (local o remoto)
4. Visual Studio 2022 o VS Code

### Configuración de Base de Datos

1. Edita el archivo `appsettings.json` o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=LicoreriaDB;User Id=sa;Password=YourPassword123;TrustServerCertificate=true;",
    "MongoDBConnection": "mongodb://localhost:27017"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong",
    "Issuer": "LicoreriaAPI",
    "Audience": "LicoreriaAPIUsers",
    "ExpirationMinutes": 60
  },
  "MongoDBSettings": {
    "DatabaseName": "LicoreriaMongoDB"
  }
}
```

2. Ajusta las cadenas de conexión según tu entorno.

## 🔧 Instalación y Ejecución

1. Clona el repositorio
2. Restaura los paquetes NuGet:
   ```bash
   dotnet restore
   ```
3. Compila el proyecto:
   ```bash
   dotnet build
   ```
4. Ejecuta la aplicación:
   ```bash
   dotnet run --project src/LicoreriaAPI/LicoreriaAPI.csproj
   ```
5. Accede a Swagger UI en: `https://localhost:5001` o `http://localhost:5000`

## 📚 Documentación de Swagger

La API está documentada con Swagger/OpenAPI. Los endpoints están organizados por tags que indican la base de datos utilizada:

- **🔐 Autenticación - SQL Server**: Endpoints de login y seguridad
- **🍃 MongoDB - Operaciones**: Endpoints que utilizan MongoDB

### Autenticación JWT

Para usar endpoints protegidos:

1. Obtén un token llamando a `POST /api/auth/login`
2. Incluye el token en el header de las peticiones:
   ```
   Authorization: Bearer {tu_token}
   ```

## 📁 Estructura de Carpetas por Módulo

### SQL Server (Transaccional)
- **Autenticación**: `Controllers/AuthController.cs`
- **Catálogos**: (Por implementar)
- **Compras**: (Por implementar)
- **Ventas**: (Por implementar)
- **Devoluciones**: (Por implementar)

### MongoDB
- **Operaciones MongoDB**: `Controllers/MongoDBController.cs` (ejemplo)

## 🔐 Seguridad

- Autenticación basada en JWT
- Validación de tokens en endpoints protegidos
- Configuración de CORS
- Cifrado de contraseñas (pendiente de implementar BCrypt)

## 📝 Próximos Pasos

1. ✅ Estructura base del proyecto
2. ✅ Configuración de bases de datos
3. ✅ Configuración de JWT
4. ✅ Swagger documentado
5. ⏳ Crear tablas en SQL Server (Usuarios, Catálogos, etc.)
6. ⏳ Implementar autenticación completa
7. ⏳ Crear modelos y servicios para catálogos
8. ⏳ Implementar procesos de compras y ventas
9. ⏳ Implementar devoluciones
10. ⏳ Definir y implementar casos de uso para MongoDB

## 👥 Contribución

Este es un proyecto en desarrollo. Las contribuciones son bienvenidas.

## 📄 Licencia

Este proyecto es privado y confidencial.

---

**Nota**: Este proyecto está en desarrollo activo. La estructura base está lista para comenzar a construir los módulos funcionales.


