using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

/// <summary>
/// Dimensión de tiempo para Data Warehouse
/// </summary>
public class DimTiempo
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime Fecha { get; set; }

    public int Anio { get; set; }
    public int Trimestre { get; set; } // 1, 2, 3, 4
    public int Mes { get; set; } // 1-12
    public int Semana { get; set; } // 1-52
    public int Dia { get; set; } // 1-31
    public int DiaSemana { get; set; } // 1=Lunes, 7=Domingo

    [MaxLength(20)]
    public string? NombreMes { get; set; } // Enero, Febrero, etc.

    [MaxLength(20)]
    public string? NombreDiaSemana { get; set; } // Lunes, Martes, etc.

    public bool EsFinDeSemana { get; set; }
    public bool EsFestivo { get; set; } = false;
}

