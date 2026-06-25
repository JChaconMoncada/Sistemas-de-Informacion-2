using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class LineaLibroMayor
    {
        public DateTime Fecha { get; set; }
        public int NumeroAsiento { get; set; }
        public string CodigoCuenta { get; set; }
        public string NombreCuenta { get; set; }
        public string Descripcion { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public decimal Saldo { get; set; }
    }

    public class LibroMayorViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;
        private ObservableCollection<LineaLibroMayor> _lineas;
        private ObservableCollection<CuentaContable> _cuentasDisponibles;
        private CuentaContable _cuentaSeleccionada;
        private decimal _saldoFinal;

        public ObservableCollection<LineaLibroMayor> Lineas
        {
            get => _lineas;
            set => SetProperty(ref _lineas, value);
        }

        public ObservableCollection<CuentaContable> CuentasDisponibles
        {
            get => _cuentasDisponibles;
            set => SetProperty(ref _cuentasDisponibles, value);
        }

        public CuentaContable CuentaSeleccionada
        {
            get => _cuentaSeleccionada;
            set
            {
                if (SetProperty(ref _cuentaSeleccionada, value))
                {
                    FiltrarMovimientos();
                }
            }
        }

        public decimal SaldoFinal
        {
            get => _saldoFinal;
            set => SetProperty(ref _saldoFinal, value);
        }

        private LineaLibroMayor _selectedLinea;
        public LineaLibroMayor SelectedLinea
        {
            get => _selectedLinea;
            set => SetProperty(ref _selectedLinea, value);
        }

        public ICommand ExportarPdfCommand { get; }
        public ICommand ExportarExcelCommand { get; }
        public ICommand ImprimirCommand { get; }
        public ICommand VerMovimientosCommand { get; }

        public LibroMayorViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Lineas = new ObservableCollection<LineaLibroMayor>();
            CuentasDisponibles = new ObservableCollection<CuentaContable>();
            
            _contabilidadService.OnEmpresaCambiada += CargarDatos;
            CargarDatos();

            ExportarPdfCommand = new RelayCommand(() => System.Windows.MessageBox.Show("Exportando Libro Mayor a PDF...", "Exportación PDF", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information));
            ExportarExcelCommand = new RelayCommand(() => System.Windows.MessageBox.Show("Exportando Libro Mayor a Excel...", "Exportación Excel", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information));
            ImprimirCommand = new RelayCommand(() => System.Windows.MessageBox.Show("Enviando Libro Mayor a la impresora...", "Imprimir", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information));
            VerMovimientosCommand = new RelayCommand(() => {
                if (SelectedLinea != null)
                {
                    System.Windows.MessageBox.Show($"Asiento #{SelectedLinea.NumeroAsiento}\nFecha: {SelectedLinea.Fecha:dd/MM/yyyy}\nCuenta: {SelectedLinea.CodigoCuenta} - {SelectedLinea.NombreCuenta}\nDescripción: {SelectedLinea.Descripcion}\nDebe: {SelectedLinea.Debe:N2}\nHaber: {SelectedLinea.Haber:N2}\nSaldo Acumulado: {SelectedLinea.Saldo:N2}", "Detalle del Movimiento", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("Por favor, seleccione un movimiento del libro mayor para ver su detalle.", "Información", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            });
        }

        private void CargarDatos()
        {
            var cuentas = _contabilidadService.ObtenerCuentasContables();
            CuentasDisponibles.Clear();
            CuentasDisponibles.Add(new CuentaContable { Codigo = "", Nombre = "Todas las cuentas" });
            foreach (var c in cuentas)
            {
                CuentasDisponibles.Add(c);
            }

            CuentaSeleccionada = CuentasDisponibles.First();
            FiltrarMovimientos();
        }

        private void FiltrarMovimientos()
        {
            var comprobantes = _contabilidadService.ObtenerComprobantesGuardados();
            var cuentas = _contabilidadService.ObtenerCuentasContables();

            var lineasTemp = new ObservableCollection<LineaLibroMayor>();
            decimal saldoActual = 0;

            var comprobantesFiltrados = comprobantes.OrderBy(c => c.Fecha);

            foreach (var comp in comprobantesFiltrados)
            {
                foreach (var linea in comp.Lineas)
                {
                    // Si hay una cuenta específica seleccionada y no coincide, saltar
                    if (CuentaSeleccionada != null && !string.IsNullOrEmpty(CuentaSeleccionada.Codigo) && linea.CodigoCuenta != CuentaSeleccionada.Codigo)
                    {
                        continue;
                    }

                    saldoActual += linea.Debe - linea.Haber;

                    var cuenta = cuentas.FirstOrDefault(c => c.Codigo == linea.CodigoCuenta);
                    var item = new LineaLibroMayor
                    {
                        Fecha = comp.Fecha,
                        NumeroAsiento = comp.IdComprobante,
                        CodigoCuenta = linea.CodigoCuenta,
                        NombreCuenta = cuenta?.Nombre ?? "Cuenta no encontrada",
                        Descripcion = linea.DescripcionCuenta ?? comp.Descripcion,
                        Debe = linea.Debe,
                        Haber = linea.Haber,
                        Saldo = saldoActual
                    };

                    lineasTemp.Add(item);
                }
            }

            Lineas = lineasTemp;
            SaldoFinal = saldoActual;
        }
    }
}
