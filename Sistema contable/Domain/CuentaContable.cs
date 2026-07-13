namespace SistemaContableZulay.UI.Domain;

public class CuentaContable
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = "Activo";
    public int Nivel { get; set; } = 1;
    public string CuentaPadre { get; set; } = string.Empty;
    public bool AceptaMovimiento { get; set; } = true;
    public bool Activo { get; set; } = true;
    public string Descripcion { get; set; } = string.Empty;

    public string DisplayName => $"{Codigo} - {Nombre}";
}
