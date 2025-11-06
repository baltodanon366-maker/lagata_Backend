# 🍾 Licoreria API - Sistema de Facturación

API transaccional de facturación para una licorería desarrollada en .NET 8 con arquitectura escalable y mantenible.

## 📋 Descripción

Esta API utiliza una arquitectura de capas (Clean Architecture) para separar responsabilidades y facilitar el mantenimiento y escalabilidad del código. El sistema utiliza **tres bases de datos** especializadas:

- **SQL Server (Operacional)**: Para operaciones transaccionales críticas:
  - Autenticación y seguridad (Login, JWT)
  - Catálogos (Productos, Categorías, Marcas, Modelos, Clientes, Proveedores, Empleados)
  - Procesos de compras, ventas y devoluciones
  - Gestión de inventario y stock

- **Data Warehouse (Analytics)**: Para consultas analíticas y reportes:
  - Métricas y KPIs para dashboard móvil
  - Reportes de ventas, compras e inventario
  - Análisis de tendencias y productos más vendidos

- **MongoDB**: Para funcionalidades flexibles:
  - Notificaciones en tiempo real
  - Logs de auditoría y sistema
  - Metadatos de documentos

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

La API está completamente documentada con Swagger/OpenAPI. Los endpoints están organizados por tags que indican la base de datos utilizada:

- **🔐 Autenticación - SQL Server**: Login, registro, gestión de usuarios
- **📦 Catálogos - SQL Server**: Productos, categorías, marcas, modelos, clientes, proveedores, empleados
- **🛒 Transacciones - SQL Server**: Compras, ventas, devoluciones
- **📊 Analytics (Data Warehouse)**: Métricas, reportes, dashboard
- **🍃 Funcionalidades (MongoDB)**: Notificaciones, logs, documentos

Accede a la documentación interactiva en: `https://tu-api-url/swagger` o `http://localhost:5000`

## 📊 Endpoints Disponibles

**Total: 122 endpoints implementados**

- ✅ **Seguridad**: 5 endpoints (login, registro, cambio de contraseña, permisos)
- ✅ **Catálogos**: 80 endpoints (8 tipos × 10 operaciones cada uno)
- ✅ **Transacciones**: 9 endpoints (compras, ventas, devoluciones)
- ✅ **Analytics**: 18 endpoints (métricas, reportes, dashboard)
- ✅ **MongoDB**: 10 endpoints (notificaciones, logs, documentos)

## 🔧 Scripts de Base de Datos

El proyecto incluye scripts SQL para:

- **Crear tablas operacionales**: `scripts/database/CreateTables.sql`
- **Crear Data Warehouse**: `scripts/database/CreateDataWarehouse.sql`
- **Crear Stored Procedures**: 
  - `scripts/database/CreateStoredProcedures.sql` (Catálogos y Seguridad)
  - `scripts/database/CreateStoredProcedures_Transacciones.sql` (Compras, Ventas, Devoluciones)
  - `scripts/database/CreateStoredProcedures_DataWarehouse.sql` (Analytics)
- **Datos de prueba**: `scripts/database/InsertTestData.sql`
- **MongoDB**: Ver `SCRIPTS_MONGODB_COLECCIONES.md` para scripts de colecciones

### Autenticación JWT

Para usar endpoints protegidos:

1. Obtén un token llamando a `POST /api/auth/login`
2. Incluye el token en el header de las peticiones:
   ```
   Authorization: Bearer {tu_token}
   ```

## 🔐 Seguridad

- ✅ Autenticación basada en JWT con tokens expirables
- ✅ Validación de tokens en endpoints protegidos
- ✅ Configuración de CORS habilitada
- ✅ Cifrado de contraseñas con BCrypt
- ✅ Sistema de roles y permisos (Administrador, Vendedor, Supervisor)

## 📝 Estado del Proyecto

✅ **Completado:**
- Estructura base del proyecto
- Configuración de 3 bases de datos (SQL Server, Data Warehouse, MongoDB)
- Configuración de JWT con BCrypt
- Swagger completamente documentado
- 122 endpoints implementados y funcionando
- Scripts SQL para crear tablas, stored procedures y datos de prueba
- Stored procedures para todas las operaciones
- Sistema de actualización automática de stock
- Integración con Data Warehouse para analytics

## 📖 Documentación Adicional

- **MongoDB**: Ver `SCRIPTS_MONGODB_COLECCIONES.md` para scripts de creación de colecciones e índices

## 👥 Contribución

Este es un proyecto en desarrollo. Las contribuciones son bienvenidas.

## 📄 Licencia

Este proyecto es privado y confidencial.

---

**Nota**: Este proyecto está en desarrollo activo. La estructura base está lista para comenzar a construir los módulos funcionales.


