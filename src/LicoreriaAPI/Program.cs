using LicoreriaAPI.Extensions;
using LicoreriaAPI.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios de configuración
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

// Configurar bases de datos
builder.Services.ConfigureSqlServer(builder.Configuration);
builder.Services.ConfigureMongoDB(builder.Configuration);
builder.Services.ConfigureDataWarehouse(builder.Configuration);

// Configurar repositorios de métricas
builder.Services.ConfigureMetricsRepositories();

// HttpContextAccessor para AuthService
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IUsuarioService, LicoreriaAPI.Application.Services.UsuarioService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.ICategoriaService, LicoreriaAPI.Application.Services.CategoriaService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IMarcaService, LicoreriaAPI.Application.Services.MarcaService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IModeloService, LicoreriaAPI.Application.Services.ModeloService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IProductoService, LicoreriaAPI.Application.Services.ProductoService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IProveedorService, LicoreriaAPI.Application.Services.ProveedorService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IClienteService, LicoreriaAPI.Application.Services.ClienteService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IEmpleadoService, LicoreriaAPI.Application.Services.EmpleadoService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IDetalleProductoService, LicoreriaAPI.Application.Services.DetalleProductoService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.ICompraService, LicoreriaAPI.Application.Services.CompraService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IVentaService, LicoreriaAPI.Application.Services.VentaService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IDevolucionVentaService, LicoreriaAPI.Application.Services.DevolucionVentaService>();
builder.Services.AddScoped<LicoreriaAPI.Application.Interfaces.Services.IAnalyticsService, LicoreriaAPI.Application.Services.AnalyticsService>();
var app = builder.Build();
// Configurar el pipeline HTTP
// Swagger siempre habilitado (también en producción para facilitar pruebas)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Licoreria API V1");
    c.RoutePrefix = string.Empty; // Swagger UI en la raíz (/) en lugar de /swagger
    c.DisplayRequestDuration();
});
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Middlewares de métricas (antes de autenticación para capturar todo)
app.UseMiddleware<LicoreriaAPI.Middleware.NetworkUsageMiddleware>();
app.UseMiddleware<LicoreriaAPI.Middleware.ActiveUserMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
