using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Proveedores (SQL Server)
/// </summary>
public interface IProveedorService
{
    Task<ProveedorDto?> CrearAsync(CrearProveedorDto crearDto);
    Task<bool> EditarAsync(int id, EditarProveedorDto editarDto);
    Task<bool> ActivarAsync(int id);
    Task<bool> DesactivarAsync(int id);
    Task<List<ProveedorDto>> MostrarActivosAsync(int top = 100);
    Task<ProveedorDto?> MostrarActivosPorIdAsync(int id);
    Task<List<ProveedorDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100);
    Task<List<ProveedorDto>> MostrarInactivosAsync(int top = 100);
    Task<List<ProveedorDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100);
    Task<ProveedorDto?> MostrarInactivosPorIdAsync(int id);
}

