namespace LicoreriaAPI.DTOs.Analytics;

/// <summary>
/// DTO para compras agregadas por rango de fechas
/// </summary>
public class ComprasPorRangoFechasDto
{
    public DateTime? Fecha { get; set; }
    public int? Anio { get; set; }
    public int? Mes { get; set; }
    public string? NombreMes { get; set; }
    public decimal TotalCantidad { get; set; }
    public decimal TotalCompras { get; set; }
    public decimal TotalImpuestos { get; set; }
    public int NumeroCompras { get; set; }
    public decimal PromedioCompra { get; set; }
}

/// <summary>
/// DTO para compras por proveedor
/// </summary>
public class ComprasPorProveedorDto
{
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public string ProveedorCodigo { get; set; } = string.Empty;
    public decimal TotalCantidad { get; set; }
    public decimal TotalCompras { get; set; }
    public int NumeroCompras { get; set; }
    public decimal PromedioCompra { get; set; }
}

/// <summary>
/// DTO para compras por producto
/// </summary>
public class ComprasPorProductoDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string MarcaNombre { get; set; } = string.Empty;
    public decimal TotalCantidad { get; set; }
    public decimal TotalCompras { get; set; }
    public decimal PromedioPrecioCompra { get; set; }
}

