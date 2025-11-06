using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Modelos (SQL Server)
/// </summary>
public interface IModeloService
{
    Task<ModeloDto?> CrearAsync(CrearModeloDto crearDto);
    Task<bool> EditarAsync(int id, EditarModeloDto editarDto);
    Task<bool> ActivarAsync(int id);
    Task<bool> DesactivarAsync(int id);
    Task<List<ModeloDto>> MostrarActivosAsync(int top = 100);
    Task<ModeloDto?> MostrarActivosPorIdAsync(int id);
    Task<List<ModeloDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100);
    Task<List<ModeloDto>> MostrarInactivosAsync(int top = 100);
    Task<List<ModeloDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100);
    Task<ModeloDto?> MostrarInactivosPorIdAsync(int id);
}

