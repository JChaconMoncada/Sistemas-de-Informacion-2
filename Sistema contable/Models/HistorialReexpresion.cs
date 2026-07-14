using System;

namespace Sistema_contable.Models
{
    public class HistorialReexpresion
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public DateTime FechaCalculo { get; set; }
        public string CodigoCuenta { get; set; }
        public string NombreCuenta { get; set; }
        public int IdMovimientoOriginal { get; set; }
        public decimal ValorOriginal { get; set; }
        public decimal MontoAjuste { get; set; }
        public decimal ValorAjustado { get; set; }
        public decimal FactorAplicado { get; set; }
        public DateTime FechaOrigen { get; set; }
        public DateTime FechaDestino { get; set; }
        
        /// <summary>
        /// ID del comprobante contable que se generó para este ajuste.
        /// Útil para poder anularlo o sobrescribirlo.
        /// </summary>
        public int IdComprobanteAsociado { get; set; }
        
        public bool Anulado { get; set; }
    }
}
