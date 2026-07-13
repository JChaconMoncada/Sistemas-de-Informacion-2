namespace SistemaContableZulay.UI.Domain;

public class CuentaContable
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = "Activo";
    public int Nivel { get; set; } = 1;
    public string CuentaPadre { get; set; } = string.Empty;
    public bool AceptaMovimiento { get; set; } = true;
    public bool Activo { get; set; } = true;

    public string DisplayName => $"{Codigo} - {Nombre}";
}
