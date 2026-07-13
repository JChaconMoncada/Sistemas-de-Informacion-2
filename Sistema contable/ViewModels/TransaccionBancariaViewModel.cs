using System;
using SistemaContableZulay.UI.Domain;

namespace Sistema_contable.ViewModels
{
    public class TransaccionBancariaViewModel : ViewModelBase
    {
        private bool _importar = true;
        private DateTime? _fecha;
        private string _referencia = string.Empty;
        private string _descripcion = string.Empty;
        private decimal _debito;
        private decimal _credito;
        private bool _isError;
        private string _rawLine = string.Empty;
        private CuentaContable? _cuentaSeleccionada;

        public bool Importar
        {
            get => _importar;
            set => SetProperty(ref _importar, value);
        }

        public DateTime? Fecha
        {
            get => _fecha;
            set => SetProperty(ref _fecha, value);
        }

        public string Referencia
        {
            get => _referencia;
            set => SetProperty(ref _referencia, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        public decimal Debito
        {
            get => _debito;
            set => SetProperty(ref _debito, value);
        }

        public decimal Credito
        {
            get => _credito;
            set => SetProperty(ref _credito, value);
        }

        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        public string RawLine
        {
            get => _rawLine;
            set => SetProperty(ref _rawLine, value);
        }

        public CuentaContable? CuentaSeleccionada
        {
            get => _cuentaSeleccionada;
            set => SetProperty(ref _cuentaSeleccionada, value);
        }
    }
}
