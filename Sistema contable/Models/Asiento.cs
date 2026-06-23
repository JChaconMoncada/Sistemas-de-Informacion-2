using System;
using System.Collections.Generic;

namespace Sistema_contable.Models
{
    public class Asiento
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public DateTime Fecha { get; set; }
        public int Numero { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public List<DetalleAsiento> Detalles { get; set; }
    }

    public class DetalleAsiento
    {
        public int Id { get; set; }
        public int AsientoId { get; set; }
        public int CuentaId { get; set; }
        public string CodigoCuenta { get; set; }
        public string NombreCuenta { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public string Descripcion { get; set; }
    }
}
