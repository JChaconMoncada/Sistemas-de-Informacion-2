namespace Sistema_contable.Models
{
    public class ConfiguracionSistema
    {
        public int Id { get; set; }
        public decimal PorcentajeIva { get; set; } = 16;
        public decimal PorcentajeIslr { get; set; } = 34;
        public string RegimenFiscal { get; set; } = "Ordinario";
        public string MonedaBase { get; set; } = "Bolívares (Bs.)";
        public string EjercicioFiscal { get; set; } = "Enero - Diciembre";
        public bool AutoguardadoHabilitado { get; set; } = true;
        public bool BackupAutomaticoHabilitado { get; set; } = true;
        public bool MostrarAlertasVencimientos { get; set; } = true;
        public bool ConfirmarAntesDeEliminar { get; set; } = true;
    }
}
