using Sistema_contable.Models;
using System.Collections.ObjectModel;

namespace Sistema_contable.ViewModels
{
    public class CobranzaViewModel : ViewModelBase
    {
        private int _facturasPendientes;
        private int _facturasVencidas;
        private int _facturasCobradasMes;
        private decimal _montoPendiente;
        private decimal _montoVencido;
        private decimal _montoCobradoMes;

        public ObservableCollection<FacturaInterna> Facturas { get; set; }

        public int FacturasPendientes
        {
            get => _facturasPendientes;
            set => SetProperty(ref _facturasPendientes, value);
        }

        public int FacturasVencidas
        {
            get => _facturasVencidas;
            set => SetProperty(ref _facturasVencidas, value);
        }

        public int FacturasCobradasMes
        {
            get => _facturasCobradasMes;
            set => SetProperty(ref _facturasCobradasMes, value);
        }

        public decimal MontoPendiente
        {
            get => _montoPendiente;
            set => SetProperty(ref _montoPendiente, value);
        }

        public decimal MontoVencido
        {
            get => _montoVencido;
            set => SetProperty(ref _montoVencido, value);
        }

        public decimal MontoCobradoMes
        {
            get => _montoCobradoMes;
            set => SetProperty(ref _montoCobradoMes, value);
        }

        public CobranzaViewModel()
        {
            Facturas = new ObservableCollection<FacturaInterna>();
            FacturasPendientes = 12;
            FacturasVencidas = 3;
            FacturasCobradasMes = 8;
            MontoPendiente = 45200.00m;
            MontoVencido = 12500.00m;
            MontoCobradoMes = 28750.00m;
        }
    }
}
