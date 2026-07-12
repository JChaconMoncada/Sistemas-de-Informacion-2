using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class LineaReporte
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Monto { get; set; }
        public bool EsTotal { get; set; }
        public bool EsEncabezado { get; set; }
    }

    public class InformesViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;

        private ObservableCollection<LineaReporte> _lineas;
        private string _tipoReporteSeleccionado;
        private DateTime _fechaCorte;
        private int _ejercicioSeleccionado;
        private string _tituloReporte;
        private string _subtituloReporte;
        private bool _tieneDatos;

        public ObservableCollection<LineaReporte> Lineas
        {
            get => _lineas;
            set => SetProperty(ref _lineas, value);
        }

        public ObservableCollection<string> TiposReporte { get; } = new ObservableCollection<string>
        {
            "Balance General", "Estado de Resultados", "Flujo de Efectivo (simplificado)"
        };

        public ObservableCollection<int> EjerciciosDisponibles { get; } = new ObservableCollection<int>();

        public string TipoReporteSeleccionado
        {
            get => _tipoReporteSeleccionado;
            set { if (SetProperty(ref _tipoReporteSeleccionado, value)) GenerarReporte(); }
        }

        public DateTime FechaCorte
        {
            get => _fechaCorte;
            set { if (SetProperty(ref _fechaCorte, value)) GenerarReporte(); }
        }

        public int EjercicioSeleccionado
        {
            get => _ejercicioSeleccionado;
            set { if (SetProperty(ref _ejercicioSeleccionado, value)) GenerarReporte(); }
        }

        public string TituloReporte
        {
            get => _tituloReporte;
            set => SetProperty(ref _tituloReporte, value);
        }

        public string SubtituloReporte
        {
            get => _subtituloReporte;
            set => SetProperty(ref _subtituloReporte, value);
        }

        public bool TieneDatos
        {
            get => _tieneDatos;
            set
            {
                if (SetProperty(ref _tieneDatos, value))
                {
                    OnPropertyChanged(nameof(SinDatosVisibility));
                }
            }
        }

        public System.Windows.Visibility SinDatosVisibility =>
            TieneDatos ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        public ICommand GenerarReporteCommand { get; }
        public ICommand ExportarPdfCommand { get; }
        public ICommand ExportarExcelCommand { get; }
        public ICommand ImprimirCommand { get; }

        public InformesViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Lineas = new ObservableCollection<LineaReporte>();

            var anioActual = DateTime.Now.Year;
            for (int a = anioActual; a >= anioActual - 5; a--)
            {
                EjerciciosDisponibles.Add(a);
            }

            _fechaCorte = DateTime.Now;
            _ejercicioSeleccionado = anioActual;
            _tipoReporteSeleccionado = TiposReporte.First();

            _contabilidadService.OnEmpresaCambiada += GenerarReporte;

            GenerarReporteCommand = new RelayCommand(GenerarReporte);
            ExportarPdfCommand = new RelayCommand(() => ExportarConValidacion("PDF"));
            ExportarExcelCommand = new RelayCommand(() => ExportarConValidacion("Excel"));
            ImprimirCommand = new RelayCommand(() => ExportarConValidacion("impresora"));

            GenerarReporte();
        }

        private void ExportarConValidacion(string destino)
        {
            if (!TieneDatos)
            {
                System.Windows.MessageBox.Show("Genere un reporte con datos antes de exportar o imprimir.", "Sin datos", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                    saveFileDialog.FileName = $"{TipoReporteSeleccionado}_{DateTime.Now:yyyyMMdd}";
                }
                else if (destino == "Excel")
                {
                    saveFileDialog.Filter = "Archivos Excel (*.xlsx)|*.xlsx";
                    saveFileDialog.DefaultExt = "xlsx";
                    saveFileDialog.FileName = $"{TipoReporteSeleccionado}_{DateTime.Now:yyyyMMdd}";
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
                        exportacionService.ExportarInformeAPdf(Lineas.ToList(), TituloReporte, SubtituloReporte, saveFileDialog.FileName, nombreEmpresa);
                    }
                    else if (destino == "Excel")
                    {
                        exportacionService.ExportarInformeAExcel(Lineas.ToList(), TituloReporte, SubtituloReporte, saveFileDialog.FileName, nombreEmpresa);
                    }

                    System.Windows.MessageBox.Show($"Archivo exportado exitosamente a:\n{saveFileDialog.FileName}", "Exportación Exitosa", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al exportar: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void GenerarReporte()
        {
            if (_contabilidadService.EmpresaActivaId == null)
            {
                Lineas = new ObservableCollection<LineaReporte>();
                TituloReporte = "Seleccione una empresa";
                SubtituloReporte = "Debe seleccionar una empresa activa en el panel izquierdo para generar informes.";
                TieneDatos = false;
                return;
            }

            switch (TipoReporteSeleccionado)
            {
                case "Estado de Resultados":
                    GenerarEstadoResultados();
                    break;
                case "Flujo de Efectivo (simplificado)":
                    GenerarFlujoEfectivo();
                    break;
                default:
                    GenerarBalanceGeneral();
                    break;
            }
        }

        private void GenerarBalanceGeneral()
        {
            var cuentas = _contabilidadService.ObtenerCuentasContables()
                .Where(c => c.AceptaMovimiento)
                .OrderBy(c => c.Codigo)
                .ToList();

            var resultado = new ObservableCollection<LineaReporte>();
            decimal totalActivo = 0, totalPasivo = 0, totalPatrimonio = 0;

            void AgregarGrupo(string tipo, string encabezado, ref decimal totalGrupo)
            {
                resultado.Add(new LineaReporte { Nombre = encabezado, EsEncabezado = true });
                var cuentasTipo = cuentas.Where(c => c.Tipo == tipo).ToList();
                foreach (var c in cuentasTipo)
                {
                    var saldo = _contabilidadService.ObtenerSaldoCuentaAFecha(c.Codigo, FechaCorte);
                    if (saldo == 0) continue;
                    resultado.Add(new LineaReporte { Codigo = c.Codigo, Nombre = c.Nombre, Monto = saldo });
                    totalGrupo += saldo;
                }
                resultado.Add(new LineaReporte { Nombre = $"Total {encabezado}", Monto = totalGrupo, EsTotal = true });
            }

            AgregarGrupo("Activo", "ACTIVO", ref totalActivo);
            AgregarGrupo("Pasivo", "PASIVO", ref totalPasivo);
            AgregarGrupo("Patrimonio", "PATRIMONIO", ref totalPatrimonio);

            resultado.Add(new LineaReporte
            {
                Nombre = "TOTAL PASIVO + PATRIMONIO",
                Monto = totalPasivo + totalPatrimonio,
                EsTotal = true
            });

            Lineas = resultado;
            TituloReporte = "BALANCE GENERAL";
            SubtituloReporte = $"Al {FechaCorte:dd 'de' MMMM 'de' yyyy}";
            TieneDatos = resultado.Any(l => !l.EsEncabezado);

            if (totalActivo != totalPasivo + totalPatrimonio)
            {
                resultado.Add(new LineaReporte
                {
                    Nombre = "⚠ Diferencia (revise resultados no cerrados del ejercicio)",
                    Monto = totalActivo - (totalPasivo + totalPatrimonio)
                });
            }
        }

        private void GenerarEstadoResultados()
        {
            var resumen = _contabilidadService.ObtenerResumenEjercicio(EjercicioSeleccionado);
            var resultado = new ObservableCollection<LineaReporte>
            {
                new LineaReporte { Nombre = "INGRESOS", EsEncabezado = true },
                new LineaReporte { Nombre = "Total Ingresos", Monto = resumen.TotalIngresos, EsTotal = true },
                new LineaReporte { Nombre = "EGRESOS Y GASTOS", EsEncabezado = true },
                new LineaReporte { Nombre = "Total Egresos", Monto = resumen.TotalEgresos, EsTotal = true },
                new LineaReporte
                {
                    Nombre = resumen.Resultado >= 0 ? "UTILIDAD DEL EJERCICIO" : "PÉRDIDA DEL EJERCICIO",
                    Monto = resumen.Resultado,
                    EsTotal = true
                }
            };

            Lineas = resultado;
            TituloReporte = "ESTADO DE RESULTADOS";
            SubtituloReporte = $"Ejercicio fiscal {EjercicioSeleccionado}";
            TieneDatos = resumen.TotalIngresos != 0 || resumen.TotalEgresos != 0;
        }

        private void GenerarFlujoEfectivo()
        {
            var cuentas = _contabilidadService.ObtenerCuentasContables()
                .Where(c => c.AceptaMovimiento && c.Codigo.StartsWith("1.1.01"))
                .OrderBy(c => c.Codigo)
                .ToList();

            var resultado = new ObservableCollection<LineaReporte>
            {
                new LineaReporte { Nombre = "EFECTIVO Y EQUIVALENTES", EsEncabezado = true }
            };

            decimal total = 0;
            foreach (var c in cuentas)
            {
                var saldo = _contabilidadService.ObtenerSaldoCuentaAFecha(c.Codigo, FechaCorte);
                resultado.Add(new LineaReporte { Codigo = c.Codigo, Nombre = c.Nombre, Monto = saldo });
                total += saldo;
            }
            resultado.Add(new LineaReporte { Nombre = "Total Efectivo Disponible", Monto = total, EsTotal = true });
            resultado.Add(new LineaReporte { Nombre = "Nota: cálculo simplificado con saldos a la fecha de corte. No incluye clasificación por actividades." });

            Lineas = resultado;
            TituloReporte = "FLUJO DE EFECTIVO (SIMPLIFICADO)";
            SubtituloReporte = $"Al {FechaCorte:dd/MM/yyyy}";
            TieneDatos = cuentas.Count > 0;
        }
    }
}
