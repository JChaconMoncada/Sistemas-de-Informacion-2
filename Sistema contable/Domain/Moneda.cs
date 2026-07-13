namespace SistemaContableZulay.UI.Domain;

public class Moneda
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal TasaCambio { get; set; }
}
