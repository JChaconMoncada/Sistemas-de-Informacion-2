namespace SistemaContableZulay.UI.Domain;

public class EmpresaCliente
{
    public string NombreEmpresa { get; set; } = string.Empty;
    public string Rif { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public void RegistrarEmpresa() { }

    public void SeleccionarEmpresa() { }
}
