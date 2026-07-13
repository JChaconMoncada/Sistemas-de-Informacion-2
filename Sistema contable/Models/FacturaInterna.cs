using System;

namespace Sistema_contable.Models
{
    public class FacturaInterna
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string NombreCliente { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Monto { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; }
        public string Estado { get; set; }
        public string Descripcion { get; set; }
    }
}
