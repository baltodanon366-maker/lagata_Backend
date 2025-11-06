using LicoreriaAPI.DTOs.Transacciones;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Devoluciones de Venta (SQL Server)
/// </summary>
public interface IDevolucionVentaService
{
    Task<(int DevolucionVentaId, string? ErrorMessage)> CrearAsync(CrearDevolucionVentaDto crearDto, int usuarioId);
    Task<DevolucionVentaDto?> ObtenerPorIdAsync(int devolucionVentaId);
    Task<List<DevolucionVentaDto>> MostrarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, int top = 100);
}

