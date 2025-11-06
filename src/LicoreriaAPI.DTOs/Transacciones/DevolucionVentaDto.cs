using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Transacciones;

/// <summary>
/// DTO para respuesta de Devolución de Venta (encabezado)
/// </summary>
public class DevolucionVentaDto
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Folio { get; set; } = string.Empty;
    
    public int VentaId { get; set; }
    
    public string? VentaFolio { get; set; }
    
    public int UsuarioId { get; set; }
    
    public string? UsuarioNombre { get; set; }
    
    public DateTime FechaDevolucion { get; set; }
    
    [MaxLength(500)]
    public string? Motivo { get; set; }
    
    public decimal TotalDevolucion { get; set; }
    
    [MaxLength(50)]
    public string Estado { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Observaciones { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaModificacion { get; set; }
    
    public List<DevolucionVentaDetalleDto> Detalles { get; set; } = new List<DevolucionVentaDetalleDto>();
}

/// <summary>
/// DTO para respuesta de Detalle de Devolución de Venta
/// </summary>
public class DevolucionVentaDetalleDto
{
    public int Id { get; set; }
    
    public int DevolucionVentaId { get; set; }
    
    public int VentaDetalleId { get; set; }
    
    public int DetalleProductoId { get; set; }
    
    public string? ProductoCodigo { get; set; }
    
    public string? ProductoNombre { get; set; }
    
    public string? MarcaNombre { get; set; }
    
    public decimal CantidadDevolver { get; set; }
    
    [MaxLength(500)]
    public string? Motivo { get; set; }
    
    public decimal Subtotal { get; set; }
}

/// <summary>
/// DTO para crear una nueva Devolución de Venta
/// </summary>
public class CrearDevolucionVentaDto
{
    [Required(ErrorMessage = "El folio es requerido")]
    [MaxLength(50, ErrorMessage = "El folio no puede exceder 50 caracteres")]
    public string Folio { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El ID de la venta es requerido")]
    public int VentaId { get; set; }
    
    public DateTime? FechaDevolucion { get; set; }
    
    [Required(ErrorMessage = "El motivo es requerido")]
    [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
    public string Motivo { get; set; } = string.Empty;
    
    [MaxLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
    public string? Observaciones { get; set; }
    
    [Required(ErrorMessage = "Los detalles de la devolución son requeridos")]
    [MinLength(1, ErrorMessage = "Debe haber al menos un detalle")]
    public List<CrearDevolucionVentaDetalleDto> Detalles { get; set; } = new List<CrearDevolucionVentaDetalleDto>();
}

/// <summary>
/// DTO para crear un detalle de devolución de venta
/// </summary>
public class CrearDevolucionVentaDetalleDto
{
    [Required(ErrorMessage = "El ID del detalle de venta es requerido")]
    public int VentaDetalleId { get; set; }
    
    [Required(ErrorMessage = "El ID del detalle producto es requerido")]
    public int DetalleProductoId { get; set; }
    
    [Required(ErrorMessage = "La cantidad a devolver es requerida")]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad a devolver debe ser mayor a 0")]
    public decimal CantidadDevolver { get; set; }
    
    [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
    public string? Motivo { get; set; }
}

