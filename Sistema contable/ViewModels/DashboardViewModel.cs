using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Measure;

namespace Sistema_contable.ViewModels
{
    public class AlertaDashboard
    {
        public string Icono       { get; set; } = string.Empty;
        public string Tipo        { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Fecha       { get; set; } = string.Empty;
        public string Estado      { get; set; } = string.Empty;


    }

    public class MovimientoReciente
    {
        public string  Numero      { get; set; } = string.Empty;
        public string  Tipo        { get; set; } = string.Empty;
        public string  Descripcion { get; set; } = string.Empty;
        public string  Fecha       { get; set; } = string.Empty;
        public decimal TotalDebe   { get; set; }
        public decimal TotalHaber  { get; set; }
        public string  Estado      { get; set; } = string.Empty;
    }

    public class DashboardViewModel : ViewModelBase
    {
        private readonly ContabilidadService _svc;

        // ── Tarjetas KPI ──────────────────────────────────────────────────────────
        private decimal _ingresosMes;   public decimal IngresosMes   { get => _ingresosMes;   set => SetProperty(ref _ingresosMes, value); }
        private decimal _gastosMes;     public decimal GastosMes     { get => _gastosMes;     set => SetProperty(ref _gastosMes, value); }
        private decimal _saldoTotal;    public decimal SaldoTotal    { get => _saldoTotal;    set => SetProperty(ref _saldoTotal, value); }
        private int _alertasPendientes; public int AlertasPendientes { get => _alertasPendientes; set => SetProperty(ref _alertasPendientes, value); }

        private string _subtituloIngresos = string.Empty;
        public string SubtituloIngresos { get => _subtituloIngresos; set => SetProperty(ref _subtituloIngresos, value); }

        private string _subtituloGastos = string.Empty;
        public string SubtituloGastos { get => _subtituloGastos; set => SetProperty(ref _subtituloGastos, value); }

        private string _subtituloSaldo = string.Empty;
        public string SubtituloSaldo { get => _subtituloSaldo; set => SetProperty(ref _subtituloSaldo, value); }

        private string _subtituloAlertas = string.Empty;
        public string SubtituloAlertas { get => _subtituloAlertas; set => SetProperty(ref _subtituloAlertas, value); }

        private SolidColorBrush _colorSaldo = new(Colors.Gray);
        public SolidColorBrush ColorSaldo { get => _colorSaldo; set => SetProperty(ref _colorSaldo, value); }

        // ── Resumen del período ───────────────────────────────────────────────────
        private string _empresaActiva = "Sin empresa seleccionada";
        public string EmpresaActiva { get => _empresaActiva; set => SetProperty(ref _empresaActiva, value); }

        private string _periodoActual = string.Empty;
        public string PeriodoActual { get => _periodoActual; set => SetProperty(ref _periodoActual, value); }

        private string _ultimaActualizacion = string.Empty;
        public string UltimaActualizacion { get => _ultimaActualizacion; set => SetProperty(ref _ultimaActualizacion, value); }

        private int _totalEmpresas;
        public int TotalEmpresas { get => _totalEmpresas; set => SetProperty(ref _totalEmpresas, value); }

        private int _totalCuentas;
        public int TotalCuentas { get => _totalCuentas; set => SetProperty(ref _totalCuentas, value); }

        private int _comprobantesDelMes;
        public int ComprobantesDelMes { get => _comprobantesDelMes; set => SetProperty(ref _comprobantesDelMes, value); }

        private int _movimientosDelMes;
        public int MovimientosDelMes { get => _movimientosDelMes; set => SetProperty(ref _movimientosDelMes, value); }

        private int _totalMovimientos;
        public int TotalMovimientos { get => _totalMovimientos; set => SetProperty(ref _totalMovimientos, value); }

        private decimal _saldoAcumulado;
        public decimal SaldoAcumulado { get => _saldoAcumulado; set => SetProperty(ref _saldoAcumulado, value); }

        private string _textoResultado = string.Empty;
        public string TextoResultado { get => _textoResultado; set => SetProperty(ref _textoResultado, value); }



        // ── Colecciones ───────────────────────────────────────────────────────────
        private ObservableCollection<AlertaDashboard> _alertas = new();
        public ObservableCollection<AlertaDashboard> Alertas
        {
            get => _alertas;
            set => SetProperty(ref _alertas, value);
        }

        private ObservableCollection<MovimientoReciente> _ultimosMovimientos = new();
        public ObservableCollection<MovimientoReciente> UltimosMovimientos
        {
            get => _ultimosMovimientos;
            set => SetProperty(ref _ultimosMovimientos, value);

        }


        public ISeries[] Series { get; set; } = Array.Empty<ISeries>();

        public Axis[] XAxes { get; set; } = Array.Empty<Axis>();

        // ── Comandos ──────────────────────────────────────────────────────────────
        public ICommand RefrescarCommand { get; }

        public DashboardViewModel()
        {
            _svc = ContabilidadService.Instance;
            RefrescarCommand = new RelayCommand(() => CargarDatos());
            _svc.OnEmpresaCambiada += CargarDatos;
            CargarDatos();
        }

        private void CargarDatos()
        {
            var hoy = DateTime.Now;
            UltimaActualizacion = $"Actualizado: {hoy:dd/MM/yyyy  HH:mm}";
            PeriodoActual       = hoy.ToString("MMMM yyyy").ToUpperInvariant();
            TotalEmpresas       = _svc.ObtenerEmpresas().Count;
            TotalCuentas        = _svc.ObtenerCuentasContables().Count;
            

            var empresaId = _svc.EmpresaActivaId;
            if (empresaId == null)
            {
                EmpresaActiva       = "Sin empresa seleccionada";
                IngresosMes         = GastosMes = SaldoTotal = SaldoAcumulado = 0;
                ComprobantesDelMes  = MovimientosDelMes = TotalMovimientos = 0;
                AlertasPendientes   = 0;
                SubtituloIngresos   = SubtituloGastos = "Sin datos";
                SubtituloSaldo      = "Seleccione una empresa";
                SubtituloAlertas    = "—";
                TextoResultado      = "—";
                ColorSaldo          = new SolidColorBrush(Colors.Gray);
                Alertas.Clear();
                UltimosMovimientos.Clear();
                return;
            }

            var empresa = _svc.ObtenerEmpresas().FirstOrDefault(e => e.Id == empresaId);
            EmpresaActiva = empresa?.NombreEmpresa ?? "Empresa desconocida";

            // ── Comprobantes del mes ──────────────────────────────────────────────
            var todosComprobantes = _svc.ObtenerComprobantesGuardados().ToList();
            var compsMes = todosComprobantes
                .Where(c => c.Fecha.Year == hoy.Year && c.Fecha.Month == hoy.Month)
                .ToList();
            ComprobantesDelMes = compsMes.Count;

            // ── Ingresos y Gastos reales via cuentas contables ────────────────────
            var cuentas = _svc.ObtenerCuentasContables();
            decimal ingresos = 0, gastos = 0;
            foreach (var comp in compsMes)
            {
                foreach (var linea in comp.Lineas)
                {
                    var cuenta = cuentas.FirstOrDefault(c => c.Codigo == linea.CodigoCuenta);
                    if (cuenta == null) continue;
                    if (cuenta.Tipo == "Ingreso")
                        ingresos += linea.Haber - linea.Debe;
                    else if (cuenta.Tipo == "Egreso")
                        gastos += linea.Debe - linea.Haber;
                }
            }
            IngresosMes = Math.Max(0, ingresos);
            GastosMes   = Math.Max(0, gastos);
            SaldoTotal  = IngresosMes - GastosMes;

            int nIngreso = compsMes.Count(c => c.TipoComprobante == "Ingreso");
            int nEgreso  = compsMes.Count(c => c.TipoComprobante == "Egreso");
            SubtituloIngresos = $"{nIngreso} comprobante(s) de ingreso";
            SubtituloGastos   = $"{nEgreso} comprobante(s) de egreso";

            if (SaldoTotal >= 0)
            {
                TextoResultado = "Utilidad del período";
                ColorSaldo     = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                SubtituloSaldo = "Resultado positivo";
            }
            else
            {
                TextoResultado = "Pérdida del período";
                ColorSaldo     = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
                SubtituloSaldo = "Resultado negativo";
            }


            // =========================
            // Gráfico Ingresos vs Gastos
            // =========================
            Series = new ISeries[]
            {
    new ColumnSeries<double>
    {
        Name = "Monto",
        Values = new double[]
        {
            (double)IngresosMes,
            (double)GastosMes
        }
    }
            };

            XAxes = new Axis[]
            {
    new Axis
    {
        Labels = new[]
        {
            "Ingresos",
            "Gastos"
        },
        LabelsRotation = 0
    }
            };

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));

            // ── Movimientos ────────────────────────────────────────────────────────
            TotalMovimientos = todosComprobantes.Count;
            MovimientosDelMes = compsMes.Count;
            SaldoAcumulado    = todosComprobantes.Sum(c => c.Lineas.Sum(l => l.Haber) - c.Lineas.Sum(l => l.Debe));

            // ── Alertas ───────────────────────────────────────────────────────────
            var alertas = new List<AlertaDashboard>();

            foreach (var comp in todosComprobantes
                .Where(c => c.Lineas.Count > 0 &&
                            Math.Abs(c.Lineas.Sum(l => l.Debe) - c.Lineas.Sum(l => l.Haber)) > 0.01m)
                .Take(3))
                alertas.Add(new AlertaDashboard
                {
                    Icono       = "⚡",
                    Tipo        = "Comprobante Descuadrado",
                    Descripcion = $"#{comp.IdComprobante} – {comp.Descripcion}",
                    Fecha       = comp.Fecha.ToString("dd/MM/yyyy"),
                    Estado      = "Revisar"
                });

            int anioAnterior = hoy.Year - 1;
            if (todosComprobantes.Any(c => c.Fecha.Year == anioAnterior)
                && !_svc.EjercicioCerrado(anioAnterior))
                alertas.Add(new AlertaDashboard
                {
                    Icono       = "📋",
                    Tipo        = "Ejercicio Abierto",
                    Descripcion = $"El ejercicio fiscal {anioAnterior} no ha sido cerrado",
                    Fecha       = $"31/12/{anioAnterior}",
                    Estado      = "Pendiente"
                });

            AlertasPendientes = alertas.Count;
            SubtituloAlertas  = AlertasPendientes > 0
                ? $"{AlertasPendientes} requieren atención"
                : "Sin alertas activas";
            Alertas = new ObservableCollection<AlertaDashboard>(alertas);

            // ── Últimos movimientos ───────────────────────────────────────────────
            var recientes = todosComprobantes
                .OrderByDescending(c => c.Fecha)
                .Take(8)
                .Select(c => new MovimientoReciente
                {
                    Numero      = $"#{c.IdComprobante}",
                    Tipo        = c.TipoComprobante,
                    Descripcion = c.Descripcion?.Length > 55
                                    ? c.Descripcion[..52] + "…"
                                    : c.Descripcion ?? string.Empty,
                    Fecha       = c.Fecha.ToString("dd/MM/yyyy"),
                    TotalDebe   = c.Lineas.Sum(l => l.Debe),
                    TotalHaber  = c.Lineas.Sum(l => l.Haber),
                    Estado      = c.Estado
                });
            UltimosMovimientos = new ObservableCollection<MovimientoReciente>(recientes);
        }
    }
}
