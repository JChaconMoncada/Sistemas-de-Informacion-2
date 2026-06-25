using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Sistema_contable.Models;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class FacturaCobranza : ViewModelBase
    {
        private string _numeroFactura = string.Empty;
        private string _nombreCliente = string.Empty;
        private DateTime _fechaEmision;
        private DateTime _fechaVencimiento;
        private decimal _monto;
        private string _estado = string.Empty;

        public string NumeroFactura { get => _numeroFactura; set => SetProperty(ref _numeroFactura, value); }
        public string NombreCliente { get => _nombreCliente; set => SetProperty(ref _nombreCliente, value); }
        public DateTime FechaEmision { get => _fechaEmision; set => SetProperty(ref _fechaEmision, value); }
        public DateTime FechaVencimiento { get => _fechaVencimiento; set => SetProperty(ref _fechaVencimiento, value); }
        public decimal Monto { get => _monto; set => SetProperty(ref _monto, value); }
        public string Estado { get => _estado; set => SetProperty(ref _estado, value); }
        public int DiasVencido => (DateTime.Now - FechaVencimiento).Days > 0 ? (DateTime.Now - FechaVencimiento).Days : 0;
    }

    public class CobranzaViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;
        private ObservableCollection<FacturaCobranza> _facturas;
        private FacturaCobranza _selectedFactura;
        
        private int _pendientes;
        private int _vencidas;
        private int _cobradas;
        private decimal _montoPendientes;
        private decimal _montoVencidas;
        private decimal _montoCobradas;

        public ObservableCollection<FacturaCobranza> Facturas
        {
            get => _facturas;
            set => SetProperty(ref _facturas, value);
        }

        public FacturaCobranza SelectedFactura
        {
            get => _selectedFactura;
            set => SetProperty(ref _selectedFactura, value);
        }

        public int Pendientes
        {
            get => _pendientes;
            set => SetProperty(ref _pendientes, value);
        }

        public int Vencidas
        {
            get => _vencidas;
            set => SetProperty(ref _vencidas, value);
        }

        public int Cobradas
        {
            get => _cobradas;
            set => SetProperty(ref _cobradas, value);
        }

        public decimal MontoPendientes
        {
            get => _montoPendientes;
            set => SetProperty(ref _montoPendientes, value);
        }

        public decimal MontoVencidas
        {
            get => _montoVencidas;
            set => SetProperty(ref _montoVencidas, value);
        }

        public decimal MontoCobradas
        {
            get => _montoCobradas;
            set => SetProperty(ref _montoCobradas, value);
        }

        public ICommand NuevaFacturaCommand { get; }
        public ICommand MarcarPagadaCommand { get; }
        public ICommand EnviarRecordatorioCommand { get; }
        public ICommand ExportarCommand { get; }
        public ICommand VerDetalleCommand { get; }

        public CobranzaViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Facturas = new ObservableCollection<FacturaCobranza>();
            
            NuevaFacturaCommand = new RelayCommand(() => NuevaFactura());
            MarcarPagadaCommand = new RelayCommand<object>((p) => MarcarPagada(p), (p) => SelectedFactura != null || p != null);
            EnviarRecordatorioCommand = new RelayCommand<object>((p) => EnviarRecordatorio(p), (p) => SelectedFactura != null || p != null);
            ExportarCommand = new RelayCommand(() => System.Windows.MessageBox.Show("Exportando cobranza...", "Exportar", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information));
            VerDetalleCommand = new RelayCommand<object>((p) => VerDetalle(p), (p) => SelectedFactura != null || p != null);

            _contabilidadService.OnEmpresaCambiada += CargarDatos;
            CargarDatos();
        }

        private void CargarDatos()
        {
            Facturas.Clear();
            
            // Add some mock invoices to make the view beautiful and alive
            Facturas.Add(new FacturaCobranza
            {
                NumeroFactura = "F-0001",
                NombreCliente = "Distribuidora Polar C.A.",
                FechaEmision = DateTime.Now.AddDays(-30),
                FechaVencimiento = DateTime.Now.AddDays(-5),
                Monto = 1500.00m,
                Estado = "Vencida"
            });

            Facturas.Add(new FacturaCobranza
            {
                NumeroFactura = "F-0002",
                NombreCliente = "Corporación Venezolana de Comercio",
                FechaEmision = DateTime.Now.AddDays(-15),
                FechaVencimiento = DateTime.Now.AddDays(15),
                Monto = 2450.50m,
                Estado = "Pendiente"
            });

            Facturas.Add(new FacturaCobranza
            {
                NumeroFactura = "F-0003",
                NombreCliente = "Comercializadora del Sur",
                FechaEmision = DateTime.Now.AddDays(-10),
                FechaVencimiento = DateTime.Now.AddDays(20),
                Monto = 3100.00m,
                Estado = "Pendiente"
            });

            Facturas.Add(new FacturaCobranza
            {
                NumeroFactura = "F-0004",
                NombreCliente = "Inversiones Zulay S.A.",
                FechaEmision = DateTime.Now.AddDays(-40),
                FechaVencimiento = DateTime.Now.AddDays(-10),
                Monto = 980.00m,
                Estado = "Pagada"
            });

            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            Pendientes = Facturas.Count(f => f.Estado == "Pendiente");
            Vencidas = Facturas.Count(f => f.Estado == "Vencida");
            Cobradas = Facturas.Count(f => f.Estado == "Pagada");

            MontoPendientes = Facturas.Where(f => f.Estado == "Pendiente").Sum(f => f.Monto);
            MontoVencidas = Facturas.Where(f => f.Estado == "Vencida").Sum(f => f.Monto);
            MontoCobradas = Facturas.Where(f => f.Estado == "Pagada").Sum(f => f.Monto);
        }

        private void NuevaFactura()
        {
            var nueva = new FacturaCobranza
            {
                NumeroFactura = $"F-000{Facturas.Count + 1}",
                NombreCliente = "Nuevo Cliente S.A.",
                FechaEmision = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddDays(30),
                Monto = 1000.00m,
                Estado = "Pendiente"
            };
            Facturas.Add(nueva);
            SelectedFactura = nueva;
            ActualizarTotales();
        }

        private void MarcarPagada(object? param = null)
        {
            var fact = (param as FacturaCobranza) ?? SelectedFactura;
            if (fact != null)
            {
                fact.Estado = "Pagada";
                ActualizarTotales();
                System.Windows.MessageBox.Show($"La factura {fact.NumeroFactura} ha sido marcada como Pagada.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void EnviarRecordatorio(object? param = null)
        {
            var fact = (param as FacturaCobranza) ?? SelectedFactura;
            if (fact != null)
            {
                System.Windows.MessageBox.Show($"Se ha enviado un correo electrónico de recordatorio de cobro al cliente '{fact.NombreCliente}' para la factura {fact.NumeroFactura}.", "Recordatorio Enviado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void VerDetalle(object? param = null)
        {
            var fact = (param as FacturaCobranza) ?? SelectedFactura;
            if (fact != null)
            {
                System.Windows.MessageBox.Show($"Detalle de Factura:\n\nNúmero: {fact.NumeroFactura}\nCliente: {fact.NombreCliente}\nEmisión: {fact.FechaEmision:dd/MM/yyyy}\nVencimiento: {fact.FechaVencimiento:dd/MM/yyyy}\nMonto: {fact.Monto:C}\nEstado: {fact.Estado}\nDías Vencido: {fact.DiasVencido}", "Información de Factura", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
    }
}
