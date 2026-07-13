namespace SistemaContableZulay.UI.Domain;

public class AsientoLinea
{
    public int Id { get; set; }
    public int ComprobanteContableId { get; set; }
    public string CodigoCuenta { get; set; } = string.Empty;
    public string DescripcionCuenta { get; set; } = string.Empty;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }

}