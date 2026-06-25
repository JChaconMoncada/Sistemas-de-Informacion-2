using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Sistema_contable.Models;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class LineaLibroDiario
    {
        public DateTime Fecha { get; set; }
        public int Numero { get; set; }
        public string CodigoCuenta { get; set; }
        public string NombreCuenta { get; set; }
        public string Descripcion { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
    }

    public class LibroDiarioViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;
        private ObservableCollection<LineaLibroDiario> _lineas;
        private decimal _totalDebe;
        private decimal _totalHaber;

        public ObservableCollection<LineaLibroDiario> Lineas
        {
            get => _lineas;
            set => SetProperty(ref _lineas, value);
        }

        public decimal TotalDebe
        {
            get => _totalDebe;
            set => SetProperty(ref _totalDebe, value);
        }

        public decimal TotalHaber
        {
            get => _totalHaber;
            set => SetProperty(ref _totalHaber, value);
        }

        private LineaLibroDiario _selectedLinea;
        public LineaLibroDiario SelectedLinea
        {
            get => _selectedLinea;
            set => SetProperty(ref _selectedLinea, value);
        }

        public ICommand ExportarPdfCommand { get; }
        public ICommand ExportarExcelCommand { get; }
        public ICommand ImprimirCommand { get; }
        public ICommand VerDetalleCommand { get; }

        public LibroDiarioViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Lineas = new ObservableCollection<LineaLibroDiario>();
            
            _contabilidadService.OnEmpresaCambiada += CargarLibroDiario;
            CargarLibroDiario();

            ExportarPdfCommand = new RelayCommand(() => System.Windows.MessageBox.Show("Exportando Libro Diario a PDF...", "Exportación PDF", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information));
            ExportarExcelCommand = new RelayCommand(() => System.Windows.MessageBox.Show("Exportando Libro Diario a Excel...", "Exportación Excel", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information));
            ImprimirCommand = new RelayCommand(() => System.Windows.MessageBox.Show("Enviando Libro Diario a la impresora...", "Imprimir", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information));
            VerDetalleCommand = new RelayCommand(() => {
                if (SelectedLinea != null)
                {
                    System.Windows.MessageBox.Show($"Asiento #{SelectedLinea.Numero}\nFecha: {SelectedLinea.Fecha:dd/MM/yyyy}\nCuenta: {SelectedLinea.CodigoCuenta} - {SelectedLinea.NombreCuenta}\nDescripción: {SelectedLinea.Descripcion}\nDebe: {SelectedLinea.Debe:N2}\nHaber: {SelectedLinea.Haber:N2}", "Detalle de Asiento", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("Por favor, seleccione una línea del libro diario para ver su detalle.", "Información", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            });
        }

        private void CargarLibroDiario()
        {
            var comprobantes = _contabilidadService.ObtenerComprobantesGuardados();
            var cuentas = _contabilidadService.ObtenerCuentasContables();

            var lineasTemp = new ObservableCollection<LineaLibroDiario>();
            decimal totalD = 0;
            decimal totalH = 0;

            var comprobantesFiltrados = comprobantes.OrderBy(c => c.Fecha);

            foreach (var comp in comprobantesFiltrados)
            {
                foreach (var linea in comp.Lineas)
                {
                    var cuenta = cuentas.FirstOrDefault(c => c.Codigo == linea.CodigoCuenta);
                    var item = new LineaLibroDiario
                    {
                        Fecha = comp.Fecha,
                        Numero = comp.IdComprobante,
                        CodigoCuenta = linea.CodigoCuenta,
                        NombreCuenta = cuenta?.Nombre ?? "Cuenta no encontrada",
                        Descripcion = linea.DescripcionCuenta ?? comp.Descripcion,
                        Debe = linea.Debe,
                        Haber = linea.Haber
                    };

                    lineasTemp.Add(item);
                    totalD += linea.Debe;
                    totalH += linea.Haber;
                }
            }

            Lineas = lineasTemp;
            TotalDebe = totalD;
            TotalHaber = totalH;
        }
    }
}
