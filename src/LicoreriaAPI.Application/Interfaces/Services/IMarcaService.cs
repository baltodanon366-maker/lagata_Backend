using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Marcas (SQL Server)
/// </summary>
public interface IMarcaService
{
    Task<MarcaDto?> CrearAsync(CrearMarcaDto crearDto);
    Task<bool> EditarAsync(int id, EditarMarcaDto editarDto);
    Task<bool> ActivarAsync(int id);
    Task<bool> DesactivarAsync(int id);
    Task<List<MarcaDto>> MostrarActivosAsync(int top = 100);
    Task<MarcaDto?> MostrarActivosPorIdAsync(int id);
    Task<List<MarcaDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100);
    Task<List<MarcaDto>> MostrarInactivosAsync(int top = 100);
    Task<List<MarcaDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100);
    Task<MarcaDto?> MostrarInactivosPorIdAsync(int id);
}

