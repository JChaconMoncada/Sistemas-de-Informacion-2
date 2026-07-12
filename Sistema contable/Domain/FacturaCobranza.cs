using System;
using System.Xml.Serialization;

namespace SistemaContableZulay.UI.Domain
{
    public class FacturaCobranza
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public int IdEmpresa { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public DateTime FechaVencimiento { get; set; } = DateTime.Now.AddDays(30);
        public decimal Monto { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string TipoPago { get; set; } = "Mensualidad";
        public DateTime? FechaPago { get; set; }
        public int IdComprobanteEmision { get; set; }
        public int IdComprobantePago { get; set; }

        [XmlIgnore]
        public int DiasVencido =>
            Estado == "Vencida" ? Math.Max(0, (DateTime.Now.Date - FechaVencimiento.Date).Days) : 0;

        [XmlIgnore]
        public int DiasRestantes =>
            Estado == "Pendiente" ? Math.Max(0, (FechaVencimiento.Date - DateTime.Now.Date).Days) : 0;
    }
}
