using System.Collections.ObjectModel;
using SistemaContableZulay.UI.Services;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;

namespace Sistema_contable.ViewModels
{
    public class PlanCuentasViewModel : ViewModelBase
    {
        private CuentaContable _cuentaSeleccionada;
        private readonly ContabilidadService _contabilidadService;

        public ObservableCollection<CuentaContable> Cuentas { get; set; }

        public CuentaContable CuentaSeleccionada
        {
            get => _cuentaSeleccionada;
            set => SetProperty(ref _cuentaSeleccionada, value);
        }

        public ICommand NuevaCuentaCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand CancelarCommand { get; }

        public PlanCuentasViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Cuentas = new ObservableCollection<CuentaContable>();

            NuevaCuentaCommand = new RelayCommand(() => NuevaCuenta());
            GuardarCommand = new RelayCommand(() => GuardarCuenta(), () => CuentaSeleccionada != null);
            EliminarCommand = new RelayCommand(() => EliminarCuenta(), () => CuentaSeleccionada != null);
            CancelarCommand = new RelayCommand(() => { CuentaSeleccionada = null; CargarCuentas(); });

            CargarCuentas();
        }

        private void CargarCuentas()
        {
            var cuentasServicio = _contabilidadService.ObtenerCuentasContables();
            Cuentas.Clear();
            foreach (var c in cuentasServicio)
            {
                Cuentas.Add(c);
            }
        }

        private void NuevaCuenta()
        {
            CuentaSeleccionada = new CuentaContable { Codigo = "Nueva", Nombre = "Nueva Cuenta" };
            Cuentas.Add(CuentaSeleccionada);
        }

        private void GuardarCuenta()
        {
            if (CuentaSeleccionada == null) return;
            
            _contabilidadService.GuardarCuenta(CuentaSeleccionada);
            CargarCuentas(); 
        }

        private void EliminarCuenta()
        {
            if (CuentaSeleccionada != null && !string.IsNullOrEmpty(CuentaSeleccionada.Codigo))
            {
                _contabilidadService.EliminarCuenta(CuentaSeleccionada.Codigo);
                CargarCuentas();
                CuentaSeleccionada = null;
            }
        }
    }
}
