namespace Sistema_contable.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private decimal _saldoTotal;
        private decimal _ingresosMes;
        private decimal _gastosMes;
        private int _alertasPendientes;

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

        public DashboardViewModel()
        {
            SaldoTotal = 125450.00m;
            IngresosMes = 85200.00m;
            GastosMes = 42350.00m;
            AlertasPendientes = 3;
        }
    }
}
