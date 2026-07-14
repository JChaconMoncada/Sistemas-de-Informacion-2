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
        private DateTime? _fechaDesde;
        private DateTime? _fechaHasta;

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

        public DateTime? FechaDesde
        {
            get => _fechaDesde;
            set { if (SetProperty(ref _fechaDesde, value)) FiltrarMovimientos(); }
        }

        public DateTime? FechaHasta
        {
            get => _fechaHasta;
            set { if (SetProperty(ref _fechaHasta, value)) FiltrarMovimientos(); }
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
        public ICommand AplicarFiltroCommand { get; }
        public ICommand LimpiarFiltroCommand { get; }

        public LibroMayorViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Lineas = new ObservableCollection<LineaLibroMayor>();
            CuentasDisponibles = new ObservableCollection<CuentaContable>();
            
            _contabilidadService.OnEmpresaCambiada += CargarDatos;
            _contabilidadService.OnDatosModificados += CargarDatos;
            CargarDatos();

            ExportarPdfCommand = new RelayCommand(() => ExportarConValidacion("PDF"));
            ExportarExcelCommand = new RelayCommand(() => ExportarConValidacion("Excel"));
            ImprimirCommand = new RelayCommand(() => ExportarConValidacion("impresora"));
            AplicarFiltroCommand = new RelayCommand(FiltrarMovimientos);
            LimpiarFiltroCommand = new RelayCommand(() =>
            {
                _fechaDesde = null;
                _fechaHasta = null;
                OnPropertyChanged(nameof(FechaDesde));
                OnPropertyChanged(nameof(FechaHasta));
                CuentaSeleccionada = CuentasDisponibles.FirstOrDefault();
            });
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

        private void ExportarConValidacion(string destino)
        {
            if (Lineas == null || Lineas.Count == 0)
            {
                System.Windows.MessageBox.Show("No hay datos en el Libro Mayor para exportar con el filtro actual.", "Sin datos", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var exportacionService = new ExportacionService();
            var nombreEmpresa = _contabilidadService.ObtenerEmpresas()
                .FirstOrDefault(e => e.Id == _contabilidadService.EmpresaActivaId)?.NombreEmpresa ?? "Empresa";

            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                if (destino == "PDF")
                {
                    saveFileDialog.Filter = "Archivos PDF (*.pdf)|*.pdf";
                    saveFileDialog.DefaultExt = "pdf";
                    saveFileDialog.FileName = $"Libro_Mayor_{DateTime.Now:yyyyMMdd}";
                }
                else if (destino == "Excel")
                {
                    saveFileDialog.Filter = "Archivos Excel (*.xlsx)|*.xlsx";
                    saveFileDialog.DefaultExt = "xlsx";
                    saveFileDialog.FileName = $"Libro_Mayor_{DateTime.Now:yyyyMMdd}";
                }
                else
                {
                    System.Windows.MessageBox.Show("Función de impresión no implementada aún.", "Información", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }

                if (saveFileDialog.ShowDialog() == true)
                {
                    if (destino == "PDF")
                    {
                        exportacionService.ExportarLibroMayorAPdf(Lineas.ToList(), saveFileDialog.FileName, nombreEmpresa);
                    }
                    else if (destino == "Excel")
                    {
                        exportacionService.ExportarLibroMayorAExcel(Lineas.ToList(), saveFileDialog.FileName, nombreEmpresa);
                    }

                    System.Windows.MessageBox.Show($"Archivo exportado exitosamente a:\n{saveFileDialog.FileName}", "Exportación Exitosa", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al exportar: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void FiltrarMovimientos()
        {
            var comprobantes = _contabilidadService.ObtenerComprobantesGuardados();
            var cuentas = _contabilidadService.ObtenerCuentasContables();

            var lineasTemp = new ObservableCollection<LineaLibroMayor>();
            decimal saldoActual = 0;

            var query = comprobantes.AsEnumerable();
            if (FechaDesde.HasValue)
                query = query.Where(c => c.Fecha.Date >= FechaDesde.Value.Date);
            if (FechaHasta.HasValue)
                query = query.Where(c => c.Fecha.Date <= FechaHasta.Value.Date);

            var comprobantesFiltrados = query.OrderBy(c => c.Fecha);

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
