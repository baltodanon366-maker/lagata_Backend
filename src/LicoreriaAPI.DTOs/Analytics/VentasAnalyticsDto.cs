namespace LicoreriaAPI.DTOs.Analytics;

/// <summary>
/// DTO para ventas agregadas por rango de fechas
/// </summary>
public class VentasPorRangoFechasDto
{
    public DateTime? Fecha { get; set; }
    public int? Anio { get; set; }
    public int? Semana { get; set; }
    public int? Mes { get; set; }
    public string? NombreMes { get; set; }
    public decimal TotalCantidad { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalDescuento { get; set; }
    public decimal TotalImpuestos { get; set; }
    public int NumeroVentas { get; set; }
    public decimal PromedioTicket { get; set; }
}

/// <summary>
/// DTO para ventas por producto
/// </summary>
public class VentasPorProductoDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string MarcaNombre { get; set; } = string.Empty;
    public decimal TotalCantidadVendida { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal PromedioTicket { get; set; }
    public int NumeroVentas { get; set; }
}

/// <summary>
/// DTO para ventas por categoría
/// </summary>
public class VentasPorCategoriaDto
{
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public decimal TotalCantidad { get; set; }
    public decimal TotalVentas { get; set; }
    public int NumeroProductos { get; set; }
    public int NumeroVentas { get; set; }
    public decimal PorcentajeTotal { get; set; }
}

/// <summary>
/// DTO para ventas por cliente
/// </summary>
public class VentasPorClienteDto
{
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteCodigo { get; set; } = string.Empty;
    public decimal TotalCantidadComprada { get; set; }
    public decimal TotalVentas { get; set; }
    public int NumeroCompras { get; set; }
    public decimal PromedioCompra { get; set; }
    public DateTime? UltimaCompra { get; set; }
}

/// <summary>
/// DTO para ventas por empleado
/// </summary>
public class VentasPorEmpleadoDto
{
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string EmpleadoCodigo { get; set; } = string.Empty;
    public decimal TotalVentas { get; set; }
    public int NumeroVentas { get; set; }
    public decimal PromedioTicket { get; set; }
}

/// <summary>
/// DTO para ventas por método de pago
/// </summary>
public class VentasPorMetodoPagoDto
{
    public string MetodoPago { get; set; } = string.Empty;
    public decimal TotalVentas { get; set; }
    public int NumeroVentas { get; set; }
    public decimal PorcentajeTotal { get; set; }
}

