using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Empleados (SQL Server)
/// </summary>
public interface IEmpleadoService
{
    Task<EmpleadoDto?> CrearAsync(CrearEmpleadoDto crearDto);
    Task<bool> EditarAsync(int id, EditarEmpleadoDto editarDto);
    Task<bool> ActivarAsync(int id);
    Task<bool> DesactivarAsync(int id);
    Task<List<EmpleadoDto>> MostrarActivosAsync(int top = 100);
    Task<EmpleadoDto?> MostrarActivosPorIdAsync(int id);
    Task<List<EmpleadoDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100);
    Task<List<EmpleadoDto>> MostrarInactivosAsync(int top = 100);
    Task<List<EmpleadoDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100);
    Task<EmpleadoDto?> MostrarInactivosPorIdAsync(int id);
}

