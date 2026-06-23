namespace SistemaContableZulay.UI.Domain;

public class ComprobanteContable
{
    public int IdComprobante { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string Descripcion { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty;
    public int IdEmpresa { get; set; }
    public string Estado { get; set; } = "Pendiente de Validación";
    public List<AsientoLinea> Lineas { get; set; } = new();

    public void ValidarComprobante()
    {
        if (Estado != "Pendiente de Validación")
            throw new InvalidOperationException("Solo se puede validar un comprobante pendiente.");

        Estado = "Validado";
    }

    public void RegistrarComprobante()
    {
        if (Estado != "Validado")
            throw new InvalidOperationException("Solo se puede registrar un comprobante validado.");

        Estado = "Registrado";
    }

    public void ActualizarComprobante() { }

    public void EliminarComprobante() { }

    public void SeleccionarComprobante() { }

    public void RegistrarAsientoMulticuenta() { }

    public void ReversarComprobante() { }
}
