namespace Sistema_contable.Models
{
    public class CuentaContable
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public int Nivel { get; set; }
        public int? CuentaPadreId { get; set; }
        public bool AceptaMovimiento { get; set; }
        public bool Activo { get; set; }
    }
}
