using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }
    public bool Activo { get; set; } = true;
}


