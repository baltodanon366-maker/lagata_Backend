using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Catalogos;

/// <summary>
/// DTO para respuesta de DetalleProducto
/// </summary>
public class DetalleProductoDto
{
    public int Id { get; set; }
    
    public int ProductoId { get; set; }
    
    public string ProductoNombre { get; set; } = string.Empty;
    
    public int CategoriaId { get; set; }
    
    public string CategoriaNombre { get; set; } = string.Empty;
    
    public int MarcaId { get; set; }
    
    public string MarcaNombre { get; set; } = string.Empty;
    
    public int ModeloId { get; set; }
    
    public string ModeloNombre { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? SKU { get; set; }
    
    [MaxLength(500)]
    public string? Observaciones { get; set; }
    
    public decimal PrecioCompra { get; set; }
    
    public decimal PrecioVenta { get; set; }
    
    public int Stock { get; set; }
    
    public int StockMinimo { get; set; }
    
    [MaxLength(50)]
    public string? UnidadMedida { get; set; }
    
    public DateTime? FechaUltimoMovimiento { get; set; }
    
    public bool Activo { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// DTO para crear un nuevo DetalleProducto
/// </summary>
public class CrearDetalleProductoDto
{
    [Required(ErrorMessage = "El ID del producto es requerido")]
    public int ProductoId { get; set; }
    
    [Required(ErrorMessage = "El ID de la categoría es requerido")]
    public int CategoriaId { get; set; }
    
    [Required(ErrorMessage = "El ID de la marca es requerido")]
    public int MarcaId { get; set; }
    
    [Required(ErrorMessage = "El ID del modelo es requerido")]
    public int ModeloId { get; set; }
    
    [Required(ErrorMessage = "El código es requerido")]
    [MaxLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
    public string Codigo { get; set; } = string.Empty;
    
    [MaxLength(100, ErrorMessage = "El SKU no puede exceder 100 caracteres")]
    public string? SKU { get; set; }
    
    [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    [Required(ErrorMessage = "El precio de compra es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio de compra debe ser un valor positivo")]
    public decimal PrecioCompra { get; set; }
    
    [Required(ErrorMessage = "El precio de venta es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio de venta debe ser un valor positivo")]
    public decimal PrecioVenta { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser un valor positivo")]
    public int StockMinimo { get; set; } = 0;
    
    [MaxLength(50, ErrorMessage = "La unidad de medida no puede exceder 50 caracteres")]
    public string? UnidadMedida { get; set; }
}

/// <summary>
/// DTO para editar un DetalleProducto existente
/// </summary>
public class EditarDetalleProductoDto
{
    [Required(ErrorMessage = "El ID de la categoría es requerido")]
    public int CategoriaId { get; set; }
    
    [Required(ErrorMessage = "El ID de la marca es requerido")]
    public int MarcaId { get; set; }
    
    [Required(ErrorMessage = "El ID del modelo es requerido")]
    public int ModeloId { get; set; }
    
    [MaxLength(100, ErrorMessage = "El SKU no puede exceder 100 caracteres")]
    public string? SKU { get; set; }
    
    [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    [Required(ErrorMessage = "El precio de compra es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio de compra debe ser un valor positivo")]
    public decimal PrecioCompra { get; set; }
    
    [Required(ErrorMessage = "El precio de venta es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio de venta debe ser un valor positivo")]
    public decimal PrecioVenta { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser un valor positivo")]
    public int StockMinimo { get; set; }
    
    [MaxLength(50, ErrorMessage = "La unidad de medida no puede exceder 50 caracteres")]
    public string? UnidadMedida { get; set; }
}
