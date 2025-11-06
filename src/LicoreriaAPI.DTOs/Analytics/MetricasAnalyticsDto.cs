namespace LicoreriaAPI.DTOs.Analytics;

/// <summary>
/// DTO para métricas del dashboard principal
/// </summary>
public class MetricasDashboardDto
{
    public string Tipo { get; set; } = string.Empty; // "Ventas", "Compras", "Inventario"
    public decimal Total { get; set; }
    public int NumeroTransacciones { get; set; }
    public decimal Promedio { get; set; }
    public decimal? Cantidad { get; set; }
}

/// <summary>
/// DTO para comparación de tendencias entre períodos
/// </summary>
public class TendenciasDto
{
    public string Periodo { get; set; } = string.Empty; // "Actual" o "Anterior"
    public decimal TotalVentas { get; set; }
    public int NumeroVentas { get; set; }
    public decimal PromedioVenta { get; set; }
    public decimal TotalCantidad { get; set; }
}

/// <summary>
/// DTO para productos más vendidos con ranking
/// </summary>
public class ProductosMasVendidosDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string MarcaNombre { get; set; } = string.Empty;
    public decimal TotalVendido { get; set; }
    public decimal TotalVentas { get; set; }
    public int NumeroVentas { get; set; }
    public int Ranking { get; set; }
}

/// <summary>
/// DTO para clientes más frecuentes con ranking
/// </summary>
public class ClientesMasFrecuentesDto
{
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteCodigo { get; set; } = string.Empty;
    public int NumeroCompras { get; set; }
    public decimal TotalGastado { get; set; }
    public decimal PromedioCompra { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public int Ranking { get; set; }
}

