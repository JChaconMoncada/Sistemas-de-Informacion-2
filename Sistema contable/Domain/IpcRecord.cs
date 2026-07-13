using System;

namespace SistemaContableZulay.UI.Domain
{
    public class IpcRecord
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Valor { get; set; }
    }
}