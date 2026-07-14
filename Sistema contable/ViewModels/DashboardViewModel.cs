using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class AlertaDashboard : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void Notify(string p) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(p));

        private string _icono       = string.Empty;
        private string _tipo        = string.Empty;
        private string _descripcion = string.Empty;
        private string _fecha       = string.Empty;
        private string _estado      = string.Empty;
        private string _colorEstado = "#FF9800";
        private bool   _esCobranza  = false;

        public string Icono       { get => _icono;       set { _icono       = value; Notify(nameof(Icono));       } }
        public string Tipo        { get => _tipo;        set { _tipo        = value; Notify(nameof(Tipo));        } }
        public string Descripcion { get => _descripcion; set { _descripcion = value; Notify(nameof(Descripcion)); } }
        public string Fecha       { get => _fecha;       set { _fecha       = value; Notify(nameof(Fecha));       } }
        public string Estado      { get => _estado;      set { _estado      = value; Notify(nameof(Estado));      } }
        public string ColorEstado { get => _colorEstado; set { _colorEstado = value; Notify(nameof(ColorEstado)); } }
        public bool   EsCobranza  { get => _esCobranza;  set { _esCobranza  = value; Notify(nameof(EsCobranza));  } }
        public int    IdFactura   { get; set; } = 0;
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

        // ── Comandos ──────────────────────────────────────────────────────────────
        public ICommand RefrescarCommand           { get; }
        public ICommand MarcarPagadaAlertaCommand  { get; }

        public DashboardViewModel()
        {
            _svc = ContabilidadService.Instance;
            RefrescarCommand          = new RelayCommand(() => CargarDatos());
            MarcarPagadaAlertaCommand = new RelayCommand<AlertaDashboard>(EjecutarMarcarPagadaAlerta);
            _svc.OnEmpresaCambiada     += () => System.Windows.Application.Current.Dispatcher.Invoke(CargarDatos);
            _svc.OnFacturasModificadas += OnFacturasModificadasHandler;
            _svc.OnDatosModificados    += () => System.Windows.Application.Current.Dispatcher.Invoke(CargarDatos);
            CargarDatos();
        }

        private void OnFacturasModificadasHandler()
            => System.Windows.Application.Current.Dispatcher.Invoke(CargarDatos);

        private void EjecutarMarcarPagadaAlerta(AlertaDashboard? alerta)
        {
            if (alerta == null || alerta.IdFactura == 0) return;
            try
            {
                // Desuscribir para que CargarDatos no borre la alerta que vamos a modificar
                _svc.OnFacturasModificadas -= OnFacturasModificadasHandler;

                _svc.MarcarFacturaPagada(alerta.IdFactura);

                alerta.Icono       = "✅";
                alerta.Tipo        = "Cobro Registrado";
                alerta.Descripcion = $"Su deuda ha sido pagada – {alerta.Descripcion}";
                alerta.Estado      = "Pagada";
                alerta.ColorEstado = "#4CAF50";
                alerta.EsCobranza  = false;

                var idx = Alertas.IndexOf(alerta);
                if (idx >= 0)
                {
                    Alertas.RemoveAt(idx);
                    Alertas.Insert(idx, alerta);
                }

                AlertasPendientes = Alertas.Count(a => a.EsCobranza);
                SubtituloAlertas  = AlertasPendientes > 0
                    ? $"{AlertasPendientes} requieren atención"
                    : "Sin alertas activas";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                // Volver a suscribir para cambios futuros (ej: pagar desde Cobranza)
                _svc.OnFacturasModificadas += OnFacturasModificadasHandler;
            }
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

            // ── Alertas de Cobranza ───────────────────────────────────────────────
            var facturasCobranza = _svc.ObtenerFacturas();

            foreach (var f in facturasCobranza.Where(f => f.Estado == "Vencida").Take(5))
                alertas.Add(new AlertaDashboard
                {
                    Icono       = "🔴",
                    Tipo        = "Cobranza Vencida",
                    Descripcion = $"{f.NumeroFactura} – {f.NombreCliente} | {f.TipoPago} | Bs. {f.Monto:N2}",
                    Fecha       = f.FechaVencimiento.ToString("dd/MM/yyyy"),
                    Estado      = $"Vencida ({f.DiasVencido}d)",
                    IdFactura   = f.Id,
                    ColorEstado = "#F44336",
                    EsCobranza  = true
                });

            foreach (var f in facturasCobranza
                .Where(f => f.Estado == "Pendiente"
                         && f.DiasRestantes <= 5
                         && f.DiasRestantes >= 0)
                .Take(5))
                alertas.Add(new AlertaDashboard
                {
                    Icono       = "⏰",
                    Tipo        = "Por Vencer",
                    Descripcion = $"{f.NumeroFactura} – {f.NombreCliente} | {f.TipoPago} | Bs. {f.Monto:N2}",
                    Fecha       = f.FechaVencimiento.ToString("dd/MM/yyyy"),
                    Estado      = f.DiasRestantes == 0 ? "Vence hoy" : $"Vence en {f.DiasRestantes}d",
                    IdFactura   = f.Id,
                    ColorEstado = "#FF9800",
                    EsCobranza  = true
                });

            foreach (var f in facturasCobranza
                .Where(f => f.Estado == "Pendiente"
                         && f.DiasRestantes > 5
                         && f.DiasRestantes <= 20)
                .Take(3))
                alertas.Add(new AlertaDashboard
                {
                    Icono       = "🔵",
                    Tipo        = "Próxima a Vencer",
                    Descripcion = $"{f.NumeroFactura} – {f.NombreCliente} | {f.TipoPago} | Bs. {f.Monto:N2}",
                    Fecha       = f.FechaVencimiento.ToString("dd/MM/yyyy"),
                    Estado      = $"Vence en {f.DiasRestantes}d",
                    IdFactura   = f.Id,
                    ColorEstado = "#1976D2",
                    EsCobranza  = true
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
