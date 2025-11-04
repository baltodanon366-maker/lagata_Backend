using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador de ejemplo para operaciones con MongoDB
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("🍃 MongoDB - Operaciones")]
[Authorize]
public class MongoDBController : ControllerBase
{
    private readonly ILogger<MongoDBController> _logger;

    public MongoDBController(ILogger<MongoDBController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Endpoint de ejemplo para MongoDB
    /// </summary>
    /// <returns>Mensaje de ejemplo</returns>
    /// <response code="200">Operación exitosa</response>
    [HttpGet("ejemplo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ejemplo()
    {
        return Ok(new { 
            message = "Este es un endpoint de ejemplo que utilizará MongoDB",
            database = "MongoDB",
            note = "Los endpoints de MongoDB se implementarán aquí"
        });
    }
}


