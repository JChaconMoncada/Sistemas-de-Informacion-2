using SistemaContableZulay.UI.Domain;

namespace SistemaContableZulay.UI.Services;

public class ContabilidadService
{
    private readonly List<ComprobanteContable> _comprobantesGuardados = new();

    public List<CuentaContable> ObtenerCuentasContables()
    {
        return new List<CuentaContable>
        {
            new() { Codigo = "1.1.01.01", Nombre = "Banco Banesco" },
            new() { Codigo = "1.1.02.01", Nombre = "Caja Chica" },
            new() { Codigo = "2.1.01.01", Nombre = "Cuentas por Pagar Proveedores" },
            new() { Codigo = "4.1.01.01", Nombre = "Ingresos por Servicios" }
        };
    }

    public List<EmpresaCliente> ObtenerEmpresas()
    {
        return new List<EmpresaCliente>
        {
            new() { Rif = "J-12345678-9", RazonSocial = "Inversiones Los Andes C.A.", NombreEmpresa = "Los Andes" },
            new() { Rif = "V-98765432-1", RazonSocial = "Servicios Tecnológicos Táchira", NombreEmpresa = "Servitech" }
        };
    }

    public List<string> ObtenerTiposComprobante()
    {
        return new List<string> { "Ingreso", "Egreso", "Diario" };
    }

    public void GuardarComprobante(ComprobanteContable comprobante)
    {
        comprobante.IdComprobante = _comprobantesGuardados.Count + 1;
        _comprobantesGuardados.Add(comprobante);
    }

    public IReadOnlyList<ComprobanteContable> ObtenerComprobantesGuardados() => _comprobantesGuardados.AsReadOnly();
}
