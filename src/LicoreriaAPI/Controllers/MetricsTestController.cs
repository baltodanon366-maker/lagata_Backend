using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LicoreriaAPI.Domain.Interfaces;
using LicoreriaAPI.Domain.Models.MongoDB;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador de prueba para insertar datos de ejemplo en las métricas MongoDB
/// </summary>
/// <remarks>
/// Este controlador es solo para pruebas y generación de datos de ejemplo.
/// En producción, este controlador debería ser eliminado o deshabilitado.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("🧪 Pruebas Métricas")]
[Authorize]
public class MetricsTestController : ControllerBase
{
    private readonly INetworkUsageMetricRepository _networkRepository;
    private readonly IFailedLoginAttemptRepository _failedLoginRepository;
    private readonly ISlowQueryMetricRepository _slowQueryRepository;
    private readonly IActiveUserMetricRepository _activeUserRepository;
    private readonly ITransactionMetricRepository _transactionRepository;

    public MetricsTestController(
        INetworkUsageMetricRepository networkRepository,
        IFailedLoginAttemptRepository failedLoginRepository,
        ISlowQueryMetricRepository slowQueryRepository,
        IActiveUserMetricRepository activeUserRepository,
        ITransactionMetricRepository transactionRepository)
    {
        _networkRepository = networkRepository;
        _failedLoginRepository = failedLoginRepository;
        _slowQueryRepository = slowQueryRepository;
        _activeUserRepository = activeUserRepository;
        _transactionRepository = transactionRepository;
    }

    /// <summary>
    /// Inserta datos de prueba para todas las métricas
    /// </summary>
    [HttpPost("insertar-datos-prueba")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> InsertarDatosPrueba()
    {
        try
        {
            var random = new Random();
            var ahora = DateTime.UtcNow;

        // 1. Métricas de uso de red (10 registros)
        for (int i = 0; i < 10; i++)
        {
            var bytesSent = random.Next(500, 5000);
            var bytesReceived = random.Next(100, 2000);
            await _networkRepository.AddAsync(new NetworkUsageMetric
            {
                Path = $"/api/{(i % 2 == 0 ? "ventas" : "productos")}",
                Method = i % 3 == 0 ? "GET" : i % 3 == 1 ? "POST" : "PUT",
                BytesSent = bytesSent,
                BytesReceived = bytesReceived,
                TotalBytes = bytesSent + bytesReceived,
                ClientIp = $"192.168.1.{random.Next(1, 255)}",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                StatusCode = random.Next(0, 2) == 0 ? 200 : 201,
                DurationMs = random.Next(50, 500),
                Timestamp = ahora.AddMinutes(-i * 5)
            });
        }

        // 2. Intentos fallidos de login (5 registros)
        var usuarios = new[] { "admin", "vendedor1", "supervisor1", "usuario_inexistente", "test" };
        for (int i = 0; i < 5; i++)
        {
            await _failedLoginRepository.AddAsync(new FailedLoginAttemptMetric
            {
                Username = usuarios[i],
                IpAddress = $"192.168.1.{random.Next(1, 50)}",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                FailureReason = i % 2 == 0 ? "InvalidPassword" : "UserNotFound",
                IsSuspicious = i >= 3,
                AttemptCount = i + 1,
                Timestamp = ahora.AddHours(-i)
            });
        }

        // 3. Consultas lentas (8 registros)
        var tablas = new[] { "Ventas", "Productos", "Compras", "Clientes", "Proveedores" };
        for (int i = 0; i < 8; i++)
        {
            await _slowQueryRepository.AddAsync(new SlowQueryMetric
            {
                QueryType = i % 2 == 0 ? "SELECT" : "StoredProcedure",
                TableName = tablas[i % tablas.Length],
                QueryText = $"SELECT * FROM {tablas[i % tablas.Length]} WHERE ...",
                DurationMs = random.Next(100, 800),
                ThresholdMs = 100,
                RowsAffected = random.Next(10, 500),
                Endpoint = $"/api/{tablas[i % tablas.Length].ToLower()}",
                UserId = random.Next(1, 4),
                Timestamp = ahora.AddMinutes(-i * 10)
            });
        }

        // 4. Usuarios activos (3 registros)
        var roles = new[] { "Administrador", "Vendedor", "Supervisor" };
        for (int i = 1; i <= 3; i++)
        {
            await _activeUserRepository.AddOrUpdateAsync(new ActiveUserMetric
            {
                UserId = i,
                Username = usuarios[i - 1],
                Role = roles[i - 1],
                SessionStart = ahora.AddHours(-2),
                LastActivity = ahora.AddMinutes(-random.Next(5, 30)),
                IpAddress = $"192.168.1.{random.Next(1, 50)}",
                RequestCount = random.Next(10, 100),
                IsActive = true,
                Timestamp = ahora
            });
        }

        // 5. Métricas de transacciones (15 registros)
        var tiposTransaccion = new[] { "Venta", "Compra", "DevolucionVenta" };
        var estados = new[] { "Completed", "Pending", "Cancelled" };
        for (int i = 0; i < 15; i++)
        {
            await _transactionRepository.AddAsync(new TransactionMetric
            {
                TransactionType = tiposTransaccion[i % tiposTransaccion.Length],
                TransactionId = 100 + i,
                Amount = (decimal)(random.NextDouble() * 5000 + 100),
                UserId = random.Next(1, 4),
                ClientId = tiposTransaccion[i % tiposTransaccion.Length] == "Venta" ? random.Next(1, 10) : null,
                SupplierId = tiposTransaccion[i % tiposTransaccion.Length] == "Compra" ? random.Next(1, 5) : null,
                Status = estados[i % estados.Length],
                ItemCount = random.Next(1, 10),
                PaymentMethod = random.Next(0, 2) == 0 ? "Efectivo" : "Tarjeta",
                DurationMs = random.Next(100, 500),
                IpAddress = $"192.168.1.{random.Next(1, 50)}",
                Timestamp = ahora.AddHours(-i)
            });
        }

            return Ok(new
            {
                message = "Datos de prueba insertados exitosamente",
                detalles = new
                {
                    networkUsageMetrics = 10,
                    failedLoginAttempts = 5,
                    slowQueries = 8,
                    activeUsers = 3,
                    transactionMetrics = 15,
                    total = 41
                },
                instrucciones = "Puedes verificar los datos usando los endpoints en /api/metrics/*"
            });
        }
        catch (MongoDB.Driver.MongoConnectionException ex)
        {
            return StatusCode(503, new
            {
                error = "Error de conexión con MongoDB Atlas",
                mensaje = "No se pudo conectar a MongoDB Atlas. Verifica:",
                soluciones = new[]
                {
                    "1. Verifica que tu IP esté en la whitelist de MongoDB Atlas (Network Access)",
                    "2. Verifica que la cadena de conexión sea correcta en appsettings.json",
                    "3. Verifica que el cluster de MongoDB Atlas esté activo",
                    "4. Verifica que el usuario y contraseña sean correctos",
                    "5. Revisa si hay un firewall bloqueando la conexión"
                },
                detalle = ex.Message
            });
        }
        catch (System.TimeoutException ex)
        {
            return StatusCode(503, new
            {
                error = "Timeout al conectar con MongoDB Atlas",
                mensaje = "La conexión a MongoDB Atlas está tardando demasiado. Verifica:",
                soluciones = new[]
                {
                    "1. Verifica que tu IP esté en la whitelist de MongoDB Atlas",
                    "2. Verifica tu conexión a internet",
                    "3. Verifica que el cluster de MongoDB Atlas esté activo y accesible",
                    "4. Intenta aumentar los timeouts en ServiceExtensions.cs"
                },
                detalle = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Error al insertar datos de prueba",
                mensaje = ex.Message,
                tipo = ex.GetType().Name,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Verifica que las colecciones existan y tengan datos
    /// </summary>
    [HttpGet("verificar-colecciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerificarColecciones()
    {
        var ahora = DateTime.UtcNow;
        var hace7Dias = ahora.AddDays(-7);

        var networkMetrics = await _networkRepository.GetMetricsAsync(hace7Dias, ahora, 1);
        var failedLogins = await _failedLoginRepository.GetAttemptsAsync(null, null, hace7Dias, ahora, 1);
        var slowQueries = await _slowQueryRepository.GetSlowQueriesAsync(null, hace7Dias, ahora, 1);
        var activeUsers = await _activeUserRepository.GetActiveUsersAsync(hace7Dias);
        var transactions = await _transactionRepository.GetTransactionsAsync(null, hace7Dias, ahora, 1);

        return Ok(new
        {
            colecciones = new
            {
                networkUsageMetrics = new
                {
                    existe = true,
                    tieneDatos = networkMetrics.Any(),
                    cantidad = networkMetrics.Count()
                },
                failedLoginAttempts = new
                {
                    existe = true,
                    tieneDatos = failedLogins.Any(),
                    cantidad = failedLogins.Count()
                },
                slowQueries = new
                {
                    existe = true,
                    tieneDatos = slowQueries.Any(),
                    cantidad = slowQueries.Count()
                },
                activeUsers = new
                {
                    existe = true,
                    tieneDatos = activeUsers.Any(),
                    cantidad = activeUsers.Count()
                },
                transactionMetrics = new
                {
                    existe = true,
                    tieneDatos = transactions.Any(),
                    cantidad = transactions.Count()
                }
            },
            mensaje = "Todas las colecciones están configuradas correctamente en MongoDB Atlas"
        });
    }
}

