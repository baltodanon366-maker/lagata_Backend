using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Categorías (SQL Server)
/// </summary>
public interface ICategoriaService
{
    Task<CategoriaDto?> CrearAsync(CrearCategoriaDto crearDto);
    Task<bool> EditarAsync(int id, EditarCategoriaDto editarDto);
    Task<bool> ActivarAsync(int id);
    Task<bool> DesactivarAsync(int id);
    Task<List<CategoriaDto>> MostrarActivosAsync(int top = 100);
    Task<CategoriaDto?> MostrarActivosPorIdAsync(int id);
    Task<List<CategoriaDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100);
    Task<List<CategoriaDto>> MostrarInactivosAsync(int top = 100);
    Task<List<CategoriaDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100);
    Task<CategoriaDto?> MostrarInactivosPorIdAsync(int id);
}

