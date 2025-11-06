using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using LicoreriaAPI.Infrastructure.Configuration;
using LicoreriaAPI.Infrastructure.Data.SqlServer;
using LicoreriaAPI.Infrastructure.Data.MongoDB;
using LicoreriaAPI.Infrastructure.Data.DataWarehouse;

namespace LicoreriaAPI.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureSqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServerConnection");
        services.AddDbContext<LicoreriaDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly("LicoreriaAPI")));
    }

    public static void ConfigureMongoDB(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoSettings = configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
        var connectionString = configuration.GetConnectionString("MongoDBConnection");

        services.AddSingleton<IMongoClient>(sp =>
        {
            return new MongoClient(connectionString);
        });

        services.AddScoped<MongoDbContext>(sp =>
        {
            var mongoClient = sp.GetRequiredService<IMongoClient>();
            return new MongoDbContext(mongoClient, mongoSettings!);
        });
    }

    public static void ConfigureDataWarehouse(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DataWarehouseConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<DataWarehouseContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("LicoreriaAPI")));
        }
    }

    public static void ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        var key = Encoding.ASCII.GetBytes(jwtSettings!.SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    }

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Licoreria API - Sistema de Facturación",
                Version = "v1.0",
                Description = @"
# 🍾 API de Sistema de Facturación para Licorería

Esta API proporciona un sistema completo de gestión transaccional para una licorería, con soporte para operaciones de venta, compra, inventario, análisis y más.

## 📊 Arquitectura de Bases de Datos

La API utiliza **tres bases de datos** especializadas según el tipo de operación:

### 1. 🔷 SQL Server (Operacional)
- **Uso**: Operaciones transaccionales críticas
- **Endpoints**: Autenticación, seguridad, catálogos, compras, ventas, devoluciones
- **Características**: 
  - Transacciones ACID garantizadas
  - Stored Procedures para lógica de negocio
  - Actualización automática de stock
  - Validaciones de integridad

### 2. 📊 Data Warehouse (Analytics)
- **Uso**: Consultas analíticas y reportes para dashboard móvil
- **Endpoints**: Métricas, ventas por período, compras por proveedor, inventario, KPIs
- **Características**:
  - Datos agregados optimizados para consultas
  - Solo lectura (alimentado por ETL)
  - Respuestas rápidas para gráficos
  - Agrupaciones flexibles (día, semana, mes, año)

### 3. 🍃 MongoDB (Funcionalidades)
- **Uso**: Funcionalidades flexibles y de alto volumen
- **Endpoints**: Notificaciones, logs de auditoría, metadatos de documentos
- **Características**:
  - Esquema flexible
  - Escalabilidad horizontal
  - Datos no estructurados
  - Alto rendimiento para escritura

## 🔐 Autenticación

Todos los endpoints (excepto `/api/auth/login` y `/api/auth/registro`) requieren autenticación JWT.

**Cómo usar:**
1. Obtén un token usando `/api/auth/login`
2. Incluye el token en el header: `Authorization: Bearer {token}`
3. El token expira después de 60 minutos (configurable)

## 📋 Organización de Endpoints

Los endpoints están organizados por **tags** que indican:
- 🔐 **Autenticación**: Login, registro, gestión de usuarios
- 📦 **Catálogos**: Productos, categorías, marcas, modelos, clientes, proveedores, empleados
- 🛒 **Transacciones**: Compras, ventas, devoluciones
- 📊 **Analytics**: Métricas, reportes, dashboard
- 🍃 **Funcionalidades**: Notificaciones, logs, documentos

## ⚠️ Códigos de Respuesta Comunes

- **200 OK**: Operación exitosa
- **201 Created**: Recurso creado exitosamente
- **400 Bad Request**: Datos inválidos o error de validación
- **401 Unauthorized**: No autenticado o token inválido
- **404 Not Found**: Recurso no encontrado
- **500 Internal Server Error**: Error interno del servidor

## 📝 Notas Importantes

- **Stock**: Se actualiza automáticamente al crear compras, ventas o devoluciones
- **Folios**: Se generan automáticamente con formato único
- **Fechas**: Se usan en formato UTC cuando no se especifica
- **Paginación**: Los endpoints de listado tienen límite por defecto (consultar cada endpoint)
- **Validaciones**: Todos los campos requeridos deben cumplir con las restricciones especificadas

## 🚀 Inicio Rápido

1. **Autenticarse**: `POST /api/auth/login`
2. **Explorar catálogos**: `GET /api/categorias`, `GET /api/productos`, etc.
3. **Crear una venta**: `POST /api/ventas`
4. **Ver métricas**: `GET /api/analytics/metricas/dashboard`
",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Soporte Licoreria API",
                    Email = "support@licoreria.com",
                    Url = new Uri("https://github.com/tu-repo/licoreria-api")
                },
                License = new Microsoft.OpenApi.Models.OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://licoreria.com/terms")
            });

            // Configurar tags para organizar por bases de datos
            // Los tags se obtienen del atributo [Tags] en cada controlador
            c.TagActionsBy(api =>
            {
                var controllerName = api.ActionDescriptor.RouteValues["controller"] ?? "";
                
                // Asignar tag según el nombre del controlador
                if (controllerName.Contains("Dashboard", StringComparison.OrdinalIgnoreCase) || 
                    controllerName.Contains("Analytics", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Metric", StringComparison.OrdinalIgnoreCase))
                {
                    return new[] { "Analytics (Data Warehouse)" };
                }

                if (controllerName.Contains("Notification", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Log", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Mongo", StringComparison.OrdinalIgnoreCase))
                {
                    return new[] { "Funcionalidades (MongoDB)" };
                }

                if (controllerName.Contains("Auth", StringComparison.OrdinalIgnoreCase))
                {
                    return new[] { "🔐 Autenticación - SQL Server" };
                }

                if (controllerName.Contains("Categoria", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Marca", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Modelo", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Producto", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Proveedor", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Cliente", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Empleado", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("DetalleProducto", StringComparison.OrdinalIgnoreCase))
                {
                    return new[] { "📦 Catálogos - SQL Server" };
                }

                if (controllerName.Contains("Compra", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Venta", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Contains("Devolucion", StringComparison.OrdinalIgnoreCase))
                {
                    return new[] { "🛒 Transacciones - SQL Server" };
                }

                return new[] { "Operacionales (SQL Server)" };
            });

            // Configuración de seguridad JWT en Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = @"
JWT Authorization usando el esquema Bearer.

**Pasos para usar:**
1. Primero, autentícate usando `/api/auth/login` con tus credenciales
2. Copia el token recibido en la respuesta
3. Haz clic en el botón 'Authorize' 🔓 (arriba a la derecha)
4. Ingresa el token en el formato: `Bearer {tu-token-aqui}`
5. Todas las solicitudes posteriores incluirán automáticamente el token

**Ejemplo de header:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Nota:** El token expira después de 60 minutos. Deberás autenticarte nuevamente para obtener un nuevo token.",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Agregar comentarios XML si existen
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Configurar ordenamiento de operaciones
            c.OrderActionsBy(apiDesc =>
            {
                // Ordenar por método HTTP (GET, POST, PUT, PATCH, DELETE)
                var methodOrder = apiDesc.HttpMethod switch
                {
                    "GET" => "1",
                    "POST" => "2",
                    "PUT" => "3",
                    "PATCH" => "4",
                    "DELETE" => "5",
                    _ => "6"
                };
                return $"{methodOrder}_{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}";
            });

            // Los comentarios XML se incluyen arriba si el archivo XML existe
        });
    }

    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }
}

