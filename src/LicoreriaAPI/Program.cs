using LicoreriaAPI.Extensions;
using LicoreriaAPI.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios de configuración
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

// Configurar bases de datos
builder.Services.ConfigureSqlServer(builder.Configuration);
builder.Services.ConfigureMongoDB(builder.Configuration);

// Configurar autenticación JWT
builder.Services.ConfigureJwtAuthentication(builder.Configuration);

// Agregar controladores
builder.Services.AddControllers();

// Configurar Swagger
builder.Services.ConfigureSwagger();

// Configurar CORS
builder.Services.ConfigureCors();

// Agregar servicios de la aplicación
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IAuthService, LicoreriaAPI.Application.Services.AuthService>();

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Licoreria API V1");
        c.RoutePrefix = string.Empty; // Swagger UI en la raíz
        // En producción, puedes deshabilitar Swagger UI comentando las líneas anteriores
        // o configurando un acceso restringido
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
