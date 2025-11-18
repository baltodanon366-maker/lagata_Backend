using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.DTOs.Transacciones;

/// <summary>
/// DTO para respuesta de Venta (encabezado)
/// </summary>
public class VentaDto
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Folio { get; set; } = string.Empty;
    
    public int? ClienteId { get; set; }
    
    public string? ClienteNombre { get; set; }
    
    public int UsuarioId { get; set; }
    
    public string? UsuarioNombre { get; set; }
    
    public int? EmpleadoId { get; set; }
    
    public string? EmpleadoNombre { get; set; }
    
    public DateTime FechaVenta { get; set; }
    
    public decimal Subtotal { get; set; }
    
    public decimal Impuestos { get; set; }
    
    public decimal Descuento { get; set; }
    
    public decimal Total { get; set; }
    
    [MaxLength(50)]
    public string? MetodoPago { get; set; }
    
    [MaxLength(50)]
    public string Estado { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Observaciones { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaModificacion { get; set; }
    
    [NotMapped]
    public List<VentaDetalleDto> Detalles { get; set; } = new List<VentaDetalleDto>();
}

/// <summary>
/// DTO para respuesta de Detalle de Venta
/// </summary>
public class VentaDetalleDto
{
    public int Id { get; set; }
    
    public int VentaId { get; set; }
    
    public int DetalleProductoId { get; set; }
    
    public string? ProductoCodigo { get; set; }
    
    public string? ProductoNombre { get; set; }
    
    public string? MarcaNombre { get; set; }
    
    public string? CategoriaNombre { get; set; }
    
    public decimal Cantidad { get; set; }
    
    public decimal PrecioUnitario { get; set; }
    
    public decimal Descuento { get; set; }
    
    public decimal Subtotal { get; set; }
}

/// <summary>
/// DTO para crear una nueva Venta
/// </summary>
/// <remarks>
/// Estructura para crear una venta con múltiples productos.
/// 
/// **Nota importante:** El folio se genera automáticamente si no se proporciona.
/// El usuario se obtiene del token JWT, no es necesario enviarlo.
/// </remarks>
public class CrearVentaDto
{
    /// <summary>
    /// Folio único de la venta. Si no se proporciona, se genera automáticamente con formato VTA-YYYYMMDDHHMMSS-XXXX
    /// </summary>
    /// <example>VTA-20250115-0001</example>
    [Required(ErrorMessage = "El folio es requerido")]
    [MaxLength(50, ErrorMessage = "El folio no puede exceder 50 caracteres")]
    public string Folio { get; set; } = string.Empty;
    
    /// <summary>
    /// ID del cliente (opcional, puede ser venta sin cliente registrado)
    /// </summary>
    /// <example>1</example>
    public int? ClienteId { get; set; }
    
    /// <summary>
    /// ID del empleado que realizó la venta (opcional)
    /// </summary>
    /// <example>2</example>
    public int? EmpleadoId { get; set; }
    
    /// <summary>
    /// Fecha de la venta (opcional, por defecto: fecha actual UTC)
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime? FechaVenta { get; set; }
    
    /// <summary>
    /// Método de pago utilizado (Efectivo, Tarjeta, Transferencia, etc.)
    /// </summary>
    /// <example>Efectivo</example>
    [MaxLength(50, ErrorMessage = "El método de pago no puede exceder 50 caracteres")]
    public string? MetodoPago { get; set; }
    
    /// <summary>
    /// Observaciones adicionales sobre la venta
    /// </summary>
    /// <example>Venta realizada en mostrador, cliente frecuente</example>
    [MaxLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
    public string? Observaciones { get; set; }
    
    /// <summary>
    /// Lista de productos vendidos. Debe contener al menos un producto.
    /// </summary>
    [Required(ErrorMessage = "Los detalles de la venta son requeridos")]
    [MinLength(1, ErrorMessage = "Debe haber al menos un detalle")]
    public List<CrearVentaDetalleDto> Detalles { get; set; } = new List<CrearVentaDetalleDto>();
}

/// <summary>
/// DTO para crear un detalle de venta
/// </summary>
/// <remarks>
/// Representa un producto individual dentro de una venta.
/// 
/// **Importante:**
/// - El `DetalleProductoId` debe existir en el sistema
/// - La cantidad debe ser mayor a 0
/// - El precio unitario debe ser positivo
/// - El sistema validará que haya stock suficiente antes de crear la venta
/// </remarks>
public class CrearVentaDetalleDto
{
    /// <summary>
    /// ID del detalle de producto a vender (obtenido de /api/detalle-productos)
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "El ID del detalle producto es requerido")]
    public int DetalleProductoId { get; set; }
    
    /// <summary>
    /// Cantidad de unidades a vender (debe ser mayor a 0)
    /// </summary>
    /// <example>5</example>
    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; }
    
    /// <summary>
    /// Precio unitario del producto en el momento de la venta
    /// </summary>
    /// <example>150.00</example>
    [Required(ErrorMessage = "El precio unitario es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser un valor positivo")]
    public decimal PrecioUnitario { get; set; }
    
    /// <summary>
    /// Descuento aplicado a este producto (opcional, por defecto: 0)
    /// </summary>
    /// <example>10.00</example>
    [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser un valor positivo")]
    public decimal Descuento { get; set; } = 0;
}

