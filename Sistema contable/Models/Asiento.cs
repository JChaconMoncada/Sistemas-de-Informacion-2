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

    public class DetalleAsiento : System.ComponentModel.INotifyPropertyChanged
    {
        private int _id;
        private int _asientoId;
        private int _cuentaId;
        private string _codigoCuenta = string.Empty;
        private string _nombreCuenta = string.Empty;
        private decimal _debe;
        private decimal _haber;
        private string _descripcion = string.Empty;

        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }
        public int AsientoId { get => _asientoId; set { _asientoId = value; OnPropertyChanged(); } }
        public int CuentaId { get => _cuentaId; set { _cuentaId = value; OnPropertyChanged(); } }
        public string CodigoCuenta { get => _codigoCuenta; set { _codigoCuenta = value; OnPropertyChanged(); } }
        public string NombreCuenta { get => _nombreCuenta; set { _nombreCuenta = value; OnPropertyChanged(); } }
        public decimal Debe { get => _debe; set { _debe = value; OnPropertyChanged(); } }
        public decimal Haber { get => _haber; set { _haber = value; OnPropertyChanged(); } }
        public string Descripcion { get => _descripcion; set { _descripcion = value; OnPropertyChanged(); } }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
