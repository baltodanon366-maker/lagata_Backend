using LicoreriaAPI.DTOs.Transacciones;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Compras (SQL Server)
/// </summary>
public interface ICompraService
{
    Task<(int CompraId, string? ErrorMessage)> CrearAsync(CrearCompraDto crearDto, int usuarioId);
    Task<CompraDto?> ObtenerPorIdAsync(int compraId);
    Task<List<CompraDto>> MostrarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, int top = 100);
}

