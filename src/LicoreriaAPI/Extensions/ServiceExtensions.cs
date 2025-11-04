using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using LicoreriaAPI.Infrastructure.Configuration;
using LicoreriaAPI.Infrastructure.Data.SqlServer;
using LicoreriaAPI.Infrastructure.Data.MongoDB;

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
                Title = "Licoreria API",
                Version = "v1",
                Description = "API transaccional de facturación para una licorería. " +
                             "Esta API utiliza dos bases de datos: SQL Server para operaciones transaccionales " +
                             "(login, seguridad, catálogos, compras, ventas, devoluciones) y MongoDB para datos adicionales.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Licoreria API",
                    Email = "support@licoreria.com"
                }
            });

            // Configuración de seguridad JWT en Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
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

