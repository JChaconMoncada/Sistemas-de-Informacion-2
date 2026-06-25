using System;

namespace Sistema_contable.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Rif { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string ActividadEconomica { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
