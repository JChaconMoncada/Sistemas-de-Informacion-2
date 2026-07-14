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
        public bool EsCuentaPadre { get; set; }
        public System.Windows.Thickness Margen { get; set; }
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

        private string _notasExplicativas;
        private string _conclusionesRecomendaciones;
        private string _nombreEmpresa;
        private string _periodoCubierto;

        public string NotasExplicativas
        {
            get => _notasExplicativas;
            set => SetProperty(ref _notasExplicativas, value);
        }

        public string ConclusionesRecomendaciones
        {
            get => _conclusionesRecomendaciones;
            set => SetProperty(ref _conclusionesRecomendaciones, value);
        }

        public string NombreEmpresa
        {
            get => _nombreEmpresa;
            set => SetProperty(ref _nombreEmpresa, value);
        }

        public string PeriodoCubierto
        {
            get => _periodoCubierto;
            set => SetProperty(ref _periodoCubierto, value);
        }

        public ObservableCollection<string> TiposReporte { get; } = new ObservableCollection<string>
        {
            "Estado de Resultados", "Estado de Flujo de Caja", "Notas y Conclusiones"
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
            _contabilidadService.OnDatosModificados += GenerarReporte;

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
                        exportacionService.ExportarInformeAPdf(Lineas.ToList(), TituloReporte, SubtituloReporte, saveFileDialog.FileName, nombreEmpresa, NotasExplicativas, ConclusionesRecomendaciones);
                    }
                    else if (destino == "Excel")
                    {
                        exportacionService.ExportarInformeAExcel(Lineas.ToList(), TituloReporte, SubtituloReporte, saveFileDialog.FileName, nombreEmpresa, NotasExplicativas, ConclusionesRecomendaciones);
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
                case "Estado de Flujo de Caja":
                    GenerarFlujoCaja();
                    break;
                case "Notas y Conclusiones":
                    GenerarNotasConclusiones();
                    break;
            }
        }



        private void GenerarEstadoResultados()
        {
            ActualizarEncabezado();

            var resumen = _contabilidadService.ObtenerResumenEjercicio(EjercicioSeleccionado);
            var detalle = _contabilidadService.ObtenerDetalleEstadoResultados(EjercicioSeleccionado);

            var resultado = new ObservableCollection<LineaReporte>();

            // ── INGRESOS ──────────────────────────────────────────────
            resultado.Add(new LineaReporte { Nombre = "INGRESOS", EsEncabezado = true });
            if (detalle.DetalleIngresos.Any())
            {
                foreach (var item in detalle.DetalleIngresos)
                    resultado.Add(new LineaReporte { Codigo = item.Codigo, Nombre = item.Nombre, Monto = item.Monto });
            }
            resultado.Add(new LineaReporte { Nombre = "Total Ingresos", Monto = resumen.TotalIngresos, EsTotal = true });

            // ── EGRESOS ───────────────────────────────────────────────
            resultado.Add(new LineaReporte { Nombre = "EGRESOS Y GASTOS", EsEncabezado = true });
            if (detalle.DetalleEgresos.Any())
            {
                foreach (var item in detalle.DetalleEgresos)
                    resultado.Add(new LineaReporte { Codigo = item.Codigo, Nombre = item.Nombre, Monto = item.Monto });
            }
            resultado.Add(new LineaReporte { Nombre = "Total Egresos", Monto = resumen.TotalEgresos, EsTotal = true });

            // ── RESULTADO ─────────────────────────────────────────────
            resultado.Add(new LineaReporte
            {
                Nombre = resumen.Resultado >= 0 ? "UTILIDAD DEL EJERCICIO" : "PÉRDIDA DEL EJERCICIO",
                Monto = resumen.Resultado,
                EsTotal = true
            });

            Lineas = resultado;
            TituloReporte = "ESTADO DE RESULTADOS";
            SubtituloReporte = $"{NombreEmpresa} — Ejercicio Fiscal {EjercicioSeleccionado}";
            TieneDatos = resumen.TotalIngresos != 0 || resumen.TotalEgresos != 0;
        }


        private void GenerarFlujoCaja()
        {
            ActualizarEncabezado();
            var comprobantes = _contabilidadService.ObtenerComprobantesGuardados()
                .Where(c => c.Fecha.Year == EjercicioSeleccionado)
                .ToList();

            var cuentas = _contabilidadService.ObtenerCuentasContables()
                .Where(c => c.Codigo.StartsWith("1.1.01"))
                .OrderBy(c => c.Codigo)
                .ToList();

            decimal entradas = 0;
            decimal salidas = 0;

            foreach (var comp in comprobantes)
            {
                if (comp.TipoComprobante == "Ingreso")
                {
                    entradas += comp.Lineas.Sum(l => l.Debe);
                }
                else if (comp.TipoComprobante == "Egreso")
                {
                    salidas += comp.Lineas.Sum(l => l.Haber);
                }
            }

            var resultado = new ObservableCollection<LineaReporte>
            {
                new LineaReporte { Nombre = "ACTIVIDADES OPERATIVAS Y OTROS", EsEncabezado = true },
                new LineaReporte { Nombre = "Entradas de Efectivo (Ingresos)", Monto = entradas },
                new LineaReporte { Nombre = "Salidas de Efectivo (Egresos)", Monto = -salidas },
                new LineaReporte { Nombre = "FLUJO NETO DE EFECTIVO", Monto = entradas - salidas, EsTotal = true }
            };

            var saldos = new System.Collections.Generic.Dictionary<string, decimal>();
            foreach (var c in cuentas)
            {
                saldos[c.Codigo] = _contabilidadService.ObtenerSaldoCuentaAFecha(c.Codigo, FechaCorte);
            }

            foreach (var c in cuentas.OrderByDescending(x => x.Nivel))
            {
                if (!string.IsNullOrEmpty(c.CuentaPadre) && saldos.ContainsKey(c.CuentaPadre))
                {
                    saldos[c.CuentaPadre] += saldos[c.Codigo];
                }
            }

            decimal total = cuentas.Any() ? cuentas.Where(c => c.Nivel == cuentas.Min(x => x.Nivel)).Sum(c => saldos[c.Codigo]) : 0;

            foreach (var c in cuentas)
            {
                var saldo = saldos[c.Codigo];
                if (saldo == 0) continue;
                resultado.Add(new LineaReporte 
                { 
                    Codigo = c.Codigo, 
                    Nombre = c.Nombre, 
                    Monto = saldo,
                    Margen = new System.Windows.Thickness((c.Nivel - 1) * 20, 2, 0, 2),
                    EsCuentaPadre = !c.AceptaMovimiento
                });
            }
            resultado.Add(new LineaReporte { Nombre = "Total Efectivo Disponible", Monto = total, EsTotal = true });
            resultado.Add(new LineaReporte { Nombre = "Nota: cálculo simplificado con saldos a la fecha de corte. No incluye clasificación por actividades." });

            Lineas = resultado;
            TituloReporte = "ESTADO DE FLUJO DE CAJA";
            SubtituloReporte = $"{NombreEmpresa} — Al {FechaCorte:dd 'de' MMMM 'de' yyyy}";
            TieneDatos = entradas != 0 || salidas != 0;
        }

        private void GenerarNotasConclusiones()
        {
            Lineas = new ObservableCollection<LineaReporte>();
            TituloReporte = "NOTAS, CONCLUSIONES Y RECOMENDACIONES";
            ActualizarEncabezado();
            SubtituloReporte = $"{NombreEmpresa} — {PeriodoCubierto}";
            TieneDatos = !string.IsNullOrEmpty(NotasExplicativas) || !string.IsNullOrEmpty(ConclusionesRecomendaciones);
        }

        private void ActualizarEncabezado()
        {
            var empresa = _contabilidadService.ObtenerEmpresaActiva();
            NombreEmpresa = empresa?.NombreEmpresa ?? "Sin Empresa";
            
            if (TipoReporteSeleccionado == "Estado de Resultados")
            {
                PeriodoCubierto = $"Ejercicio Fiscal {EjercicioSeleccionado}";
            }
            else
            {
                PeriodoCubierto = $"Al {FechaCorte:dd 'de' MMMM 'de' yyyy}";
            }
        }
    }
}
