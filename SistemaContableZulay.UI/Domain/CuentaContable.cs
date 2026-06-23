namespace SistemaContableZulay.UI.Domain;

public class CuentaContable
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public string DisplayName => $"{Codigo} - {Nombre}";
}
