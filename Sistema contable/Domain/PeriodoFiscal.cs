using System;

namespace SistemaContableZulay.UI.Domain;

public class PeriodoFiscal
{
    public int Id { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public bool Cerrado { get; set; }
    public DateTime? FechaCierre { get; set; }
}
