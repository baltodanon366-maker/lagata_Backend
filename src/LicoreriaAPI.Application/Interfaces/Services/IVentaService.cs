using LicoreriaAPI.DTOs.Transacciones;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Ventas (SQL Server)
/// </summary>
public interface IVentaService
{
    Task<(int VentaId, string? ErrorMessage)> CrearAsync(CrearVentaDto crearDto, int usuarioId);
    Task<VentaDto?> ObtenerPorIdAsync(int ventaId);
    Task<List<VentaDto>> MostrarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, int top = 100);
}

