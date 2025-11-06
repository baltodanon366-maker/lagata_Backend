using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Clientes (SQL Server)
/// </summary>
public interface IClienteService
{
    Task<ClienteDto?> CrearAsync(CrearClienteDto crearDto);
    Task<bool> EditarAsync(int id, EditarClienteDto editarDto);
    Task<bool> ActivarAsync(int id);
    Task<bool> DesactivarAsync(int id);
    Task<List<ClienteDto>> MostrarActivosAsync(int top = 100);
    Task<ClienteDto?> MostrarActivosPorIdAsync(int id);
    Task<List<ClienteDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100);
    Task<List<ClienteDto>> MostrarInactivosAsync(int top = 100);
    Task<List<ClienteDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100);
    Task<ClienteDto?> MostrarInactivosPorIdAsync(int id);
}

