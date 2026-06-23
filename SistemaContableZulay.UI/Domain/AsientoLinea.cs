namespace SistemaContableZulay.UI.Domain;

public class AsientoLinea
{
    public string CodigoCuenta { get; set; } = string.Empty;
    public string DescripcionCuenta { get; set; } = string.Empty;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}
