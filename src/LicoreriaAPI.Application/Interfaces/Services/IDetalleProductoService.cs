using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de DetalleProducto (SQL Server)
/// </summary>
public interface IDetalleProductoService
{
    Task<DetalleProductoDto?> CrearAsync(CrearDetalleProductoDto crearDto);
    Task<bool> EditarAsync(int id, EditarDetalleProductoDto editarDto);
    Task<bool> ActivarAsync(int id);
    Task<bool> DesactivarAsync(int id);
    Task<List<DetalleProductoDto>> MostrarActivosAsync(int top = 100);
    Task<DetalleProductoDto?> MostrarActivosPorIdAsync(int id);
    Task<List<DetalleProductoDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100);
    Task<List<DetalleProductoDto>> MostrarInactivosAsync(int top = 100);
    Task<List<DetalleProductoDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100);
    Task<DetalleProductoDto?> MostrarInactivosPorIdAsync(int id);
}

