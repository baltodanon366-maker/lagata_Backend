namespace LicoreriaAPI.DTOs.Analytics;

/// <summary>
/// DTO para stock actual de productos
/// </summary>
public class StockActualDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string MarcaNombre { get; set; } = string.Empty;
    public string? ModeloNombre { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public bool StockBajo { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal ValorInventario { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

/// <summary>
/// DTO para productos con stock bajo
/// </summary>
public class ProductoStockBajoDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string MarcaNombre { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public int CantidadFaltante { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

/// <summary>
/// DTO para valor del inventario
/// </summary>
public class ValorInventarioDto
{
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public decimal ValorTotal { get; set; }
    public int CantidadProductos { get; set; }
    public decimal ValorPromedio { get; set; }
}

