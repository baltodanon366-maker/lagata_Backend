using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Productos (SQL Server)
/// </summary>
public interface IProductoService
{
    Task<ProductoDto?> CrearAsync(CrearProductoDto crearDto);
    Task<bool> EditarAsync(int id, EditarProductoDto editarDto);
    Task<bool> ActivarAsync(int id);
    Task<bool> DesactivarAsync(int id);
    Task<List<ProductoDto>> MostrarActivosAsync(int top = 100);
    Task<ProductoDto?> MostrarActivosPorIdAsync(int id);
    Task<List<ProductoDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100);
    Task<List<ProductoDto>> MostrarInactivosAsync(int top = 100);
    Task<List<ProductoDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100);
    Task<ProductoDto?> MostrarInactivosPorIdAsync(int id);
}

