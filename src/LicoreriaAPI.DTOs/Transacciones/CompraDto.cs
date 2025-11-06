using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Transacciones;

/// <summary>
/// DTO para respuesta de Compra (encabezado)
/// </summary>
public class CompraDto
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Folio { get; set; } = string.Empty;
    
    public int ProveedorId { get; set; }
    
    public string? ProveedorNombre { get; set; }
    
    public int UsuarioId { get; set; }
    
    public string? UsuarioNombre { get; set; }
    
    public DateTime FechaCompra { get; set; }
    
    public decimal Subtotal { get; set; }
    
    public decimal Impuestos { get; set; }
    
    public decimal Total { get; set; }
    
    [MaxLength(50)]
    public string Estado { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Observaciones { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaModificacion { get; set; }
    
    public List<CompraDetalleDto> Detalles { get; set; } = new List<CompraDetalleDto>();
}

/// <summary>
/// DTO para respuesta de Detalle de Compra
/// </summary>
public class CompraDetalleDto
{
    public int Id { get; set; }
    
    public int CompraId { get; set; }
    
    public int DetalleProductoId { get; set; }
    
    public string? ProductoCodigo { get; set; }
    
    public string? ProductoNombre { get; set; }
    
    public string? MarcaNombre { get; set; }
    
    public string? CategoriaNombre { get; set; }
    
    public decimal Cantidad { get; set; }
    
    public decimal PrecioUnitario { get; set; }
    
    public decimal Subtotal { get; set; }
}

/// <summary>
/// DTO para crear una nueva Compra
/// </summary>
public class CrearCompraDto
{
    [Required(ErrorMessage = "El folio es requerido")]
    [MaxLength(50, ErrorMessage = "El folio no puede exceder 50 caracteres")]
    public string Folio { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El ID del proveedor es requerido")]
    public int ProveedorId { get; set; }
    
    public DateTime? FechaCompra { get; set; }
    
    [MaxLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
    public string? Observaciones { get; set; }
    
    [Required(ErrorMessage = "Los detalles de la compra son requeridos")]
    [MinLength(1, ErrorMessage = "Debe haber al menos un detalle")]
    public List<CrearCompraDetalleDto> Detalles { get; set; } = new List<CrearCompraDetalleDto>();
}

/// <summary>
/// DTO para crear un detalle de compra
/// </summary>
public class CrearCompraDetalleDto
{
    [Required(ErrorMessage = "El ID del detalle producto es requerido")]
    public int DetalleProductoId { get; set; }
    
    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; }
    
    [Required(ErrorMessage = "El precio unitario es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser un valor positivo")]
    public decimal PrecioUnitario { get; set; }
}

