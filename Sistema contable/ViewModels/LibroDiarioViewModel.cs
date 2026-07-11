using System;
using System.Collections.Generic;
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
        public string TipoComprobante { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
    }

    public class LibroDiarioViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;
        private ObservableCollection<LineaLibroDiario> _lineas;
        private decimal _totalDebe;
        private decimal _totalHaber;
        private DateTime? _fechaDesde;
        private DateTime? _fechaHasta;
        private string _tipoSeleccionado = "Todos";

        public ObservableCollection<LineaLibroDiario> Lineas
        {
            get => _lineas;
            set => SetProperty(ref _lineas, value);
        }

        public ObservableCollection<string> TiposDisponibles { get; } = new ObservableCollection<string>
        {
            "Todos", "Manual", "Ingreso", "Egreso", "Diario", "Apertura", "Cierre", "Ajuste", "Reversión"
        };

        public DateTime? FechaDesde
        {
            get => _fechaDesde;
            set { if (SetProperty(ref _fechaDesde, value)) AplicarFiltro(); }
        }

        public DateTime? FechaHasta
        {
            get => _fechaHasta;
            set { if (SetProperty(ref _fechaHasta, value)) AplicarFiltro(); }
        }

        public string TipoSeleccionado
        {
            get => _tipoSeleccionado;
            set { if (SetProperty(ref _tipoSeleccionado, value)) AplicarFiltro(); }
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
        public ICommand AplicarFiltroCommand { get; }
        public ICommand LimpiarFiltroCommand { get; }

        private IReadOnlyList<SistemaContableZulay.UI.Domain.ComprobanteContable> _comprobantesCache = new List<SistemaContableZulay.UI.Domain.ComprobanteContable>();
        private List<SistemaContableZulay.UI.Domain.CuentaContable> _cuentasCache = new List<SistemaContableZulay.UI.Domain.CuentaContable>();

        public LibroDiarioViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Lineas = new ObservableCollection<LineaLibroDiario>();
            
            _contabilidadService.OnEmpresaCambiada += CargarLibroDiario;
            CargarLibroDiario();

            ExportarPdfCommand = new RelayCommand(() => ExportarConValidacion("PDF"));
            ExportarExcelCommand = new RelayCommand(() => ExportarConValidacion("Excel"));
            ImprimirCommand = new RelayCommand(() => ExportarConValidacion("impresora"));
            AplicarFiltroCommand = new RelayCommand(AplicarFiltro);
            LimpiarFiltroCommand = new RelayCommand(LimpiarFiltro);
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

        private void ExportarConValidacion(string destino)
        {
            if (Lineas == null || Lineas.Count == 0)
            {
                System.Windows.MessageBox.Show("No hay datos en el Libro Diario para exportar con el filtro actual.", "Sin datos", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            System.Windows.MessageBox.Show($"Enviando {Lineas.Count} línea(s) del Libro Diario a {destino}...", "Exportación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void LimpiarFiltro()
        {
            _fechaDesde = null;
            _fechaHasta = null;
            _tipoSeleccionado = "Todos";
            OnPropertyChanged(nameof(FechaDesde));
            OnPropertyChanged(nameof(FechaHasta));
            OnPropertyChanged(nameof(TipoSeleccionado));
            AplicarFiltro();
        }

        private void CargarLibroDiario()
        {
            _comprobantesCache = _contabilidadService.ObtenerComprobantesGuardados();
            _cuentasCache = _contabilidadService.ObtenerCuentasContables();
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            var lineasTemp = new ObservableCollection<LineaLibroDiario>();
            decimal totalD = 0;
            decimal totalH = 0;

            var comprobantesFiltrados = _comprobantesCache.AsEnumerable();

            if (FechaDesde.HasValue)
                comprobantesFiltrados = comprobantesFiltrados.Where(c => c.Fecha.Date >= FechaDesde.Value.Date);
            if (FechaHasta.HasValue)
                comprobantesFiltrados = comprobantesFiltrados.Where(c => c.Fecha.Date <= FechaHasta.Value.Date);
            if (!string.IsNullOrEmpty(TipoSeleccionado) && TipoSeleccionado != "Todos")
                comprobantesFiltrados = comprobantesFiltrados.Where(c => c.TipoComprobante == TipoSeleccionado);

            comprobantesFiltrados = comprobantesFiltrados.OrderBy(c => c.Fecha);

            foreach (var comp in comprobantesFiltrados)
            {
                foreach (var linea in comp.Lineas)
                {
                    var cuenta = _cuentasCache.FirstOrDefault(c => c.Codigo == linea.CodigoCuenta);
                    var item = new LineaLibroDiario
                    {
                        Fecha = comp.Fecha,
                        Numero = comp.IdComprobante,
                        CodigoCuenta = linea.CodigoCuenta,
                        NombreCuenta = cuenta?.Nombre ?? "Cuenta no encontrada",
                        Descripcion = linea.DescripcionCuenta ?? comp.Descripcion,
                        TipoComprobante = comp.TipoComprobante,
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
