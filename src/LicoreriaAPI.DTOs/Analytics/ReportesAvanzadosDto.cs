namespace LicoreriaAPI.DTOs.Analytics;

/// <summary>
/// DTO para comparación ventas vs compras (ganancia bruta)
/// </summary>
public class VentasVsComprasDto
{
    public DateTime? Fecha { get; set; }
    public int? Anio { get; set; }
    public int? Mes { get; set; }
    public string? NombreMes { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalCompras { get; set; }
    public decimal GananciaBruta { get; set; }
    public decimal PorcentajeGanancia { get; set; }
}

/// <summary>
/// DTO para rotación de inventario
/// </summary>
public class RotacionInventarioDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string MarcaNombre { get; set; } = string.Empty;
    public decimal CantidadVendida { get; set; }
    public decimal StockPromedio { get; set; }
    public decimal Rotacion { get; set; }
    public int DiasInventario { get; set; }
}

