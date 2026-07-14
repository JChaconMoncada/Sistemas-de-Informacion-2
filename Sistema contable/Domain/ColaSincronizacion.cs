using System;
namespace SistemaContableZulay.UI.Domain;

public class ColaSincronizacion
{
    public int Id { get; set; }
    public string TipoEntidad { get; set; } = string.Empty; // "FacturaCobranza", "ComprobanteContable"
    public string PayloadJson { get; set; } = string.Empty;
    public int Intentos { get; set; } = 0;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}