using System.Collections.Generic;

namespace SistemaContableZulay.UI.Domain;

public class LineaDetalleResultado
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}

public class DetalleEstadoResultados
{
    public List<LineaDetalleResultado> DetalleIngresos { get; set; } = new();
    public List<LineaDetalleResultado> DetalleEgresos { get; set; } = new();
}
