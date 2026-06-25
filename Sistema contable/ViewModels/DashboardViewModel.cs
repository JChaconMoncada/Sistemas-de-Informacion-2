namespace Sistema_contable.ViewModels
{
    using SistemaContableZulay.UI.Services;
    using System.Linq;

    public class DashboardViewModel : ViewModelBase
    {
        private decimal _saldoTotal;
        private decimal _ingresosMes;
        private decimal _gastosMes;
        private int _alertasPendientes;
        private int _totalEmpresas;

        private readonly ContabilidadService _contabilidadService;

        public decimal SaldoTotal
        {
            get => _saldoTotal;
            set => SetProperty(ref _saldoTotal, value);
        }

        public decimal IngresosMes
        {
            get => _ingresosMes;
            set => SetProperty(ref _ingresosMes, value);
        }

        public decimal GastosMes
        {
            get => _gastosMes;
            set => SetProperty(ref _gastosMes, value);
        }

        public int AlertasPendientes
        {
            get => _alertasPendientes;
            set => SetProperty(ref _alertasPendientes, value);
        }

        public int TotalEmpresas
        {
            get => _totalEmpresas;
            set => SetProperty(ref _totalEmpresas, value);
        }

        public DashboardViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            _contabilidadService.OnEmpresaCambiada += CargarDatos;
            CargarDatos();
        }

        private void CargarDatos()
        {
            var comprobantes = _contabilidadService.ObtenerComprobantesGuardados();
            
            // Lógica simple para simular valores reales
            IngresosMes = comprobantes.Where(c => c.TipoComprobante == "Ingreso").Count() * 1000m;
            GastosMes = comprobantes.Where(c => c.TipoComprobante == "Egreso").Count() * 500m;
            SaldoTotal = IngresosMes - GastosMes;

            AlertasPendientes = 0; // Podría calcular comprobantes descuadrados
            TotalEmpresas = _contabilidadService.ObtenerEmpresas().Count;
        }
    }
}
