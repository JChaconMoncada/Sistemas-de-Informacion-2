namespace SistemaContableZulay.UI.Domain;

public class ResumenEjercicio
{
    public int Id { get; set; }
    public int Anio { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal Resultado { get; set; }
}
