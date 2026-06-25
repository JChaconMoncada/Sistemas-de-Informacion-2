using System;

namespace Sistema_contable.Models
{
    public class Documento
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string NombreEmpresa { get; set; }
        public string TipoDocumento { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
    }
}
