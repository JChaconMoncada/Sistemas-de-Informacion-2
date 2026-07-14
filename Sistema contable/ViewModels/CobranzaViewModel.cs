using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class CobranzaViewModel : ViewModelBase
    {
        private readonly ContabilidadService _svc;
        private List<FacturaCobranza> _todasLasFacturas = new();

        // ─── Lista visible ────────────────────────────────────────────────────────
        private ObservableCollection<FacturaCobranza> _facturas = new();
        public ObservableCollection<FacturaCobranza> Facturas
        {
            get => _facturas;
            set => SetProperty(ref _facturas, value);
        }

        private FacturaCobranza? _facturaSeleccionada;
        public FacturaCobranza? FacturaSeleccionada
        {
            get => _facturaSeleccionada;
            set => SetProperty(ref _facturaSeleccionada, value);
        }

        // ─── Tarjetas resumen ─────────────────────────────────────────────────────
        private int _pendientes;    public int Pendientes    { get => _pendientes;    set => SetProperty(ref _pendientes, value); }
        private int _vencidas;      public int Vencidas      { get => _vencidas;      set => SetProperty(ref _vencidas, value); }
        private int _cobradas;      public int Cobradas      { get => _cobradas;      set => SetProperty(ref _cobradas, value); }
        private decimal _montoPendientes; public decimal MontoPendientes { get => _montoPendientes; set => SetProperty(ref _montoPendientes, value); }
        private decimal _montoVencidas;   public decimal MontoVencidas   { get => _montoVencidas;   set => SetProperty(ref _montoVencidas, value); }
        private decimal _montoCobradas;   public decimal MontoCobradas   { get => _montoCobradas;   set => SetProperty(ref _montoCobradas, value); }

        // ─── Filtros ──────────────────────────────────────────────────────────────
        private string _filtroCliente = string.Empty;
        public string FiltroCliente { get => _filtroCliente; set => SetProperty(ref _filtroCliente, value); }

        private string _filtroEstado = "Todos los estados";
        public string FiltroEstado { get => _filtroEstado; set => SetProperty(ref _filtroEstado, value); }

        private DateTime? _filtroDesde;
        public DateTime? FiltroDesde { get => _filtroDesde; set => SetProperty(ref _filtroDesde, value); }

        private DateTime? _filtroHasta;
        public DateTime? FiltroHasta { get => _filtroHasta; set => SetProperty(ref _filtroHasta, value); }

        // ─── Modo edición / formulario ────────────────────────────────────────────
        private bool _estaEnModoEdicion;
        public bool EstaEnModoEdicion
        {
            get => _estaEnModoEdicion;
            set => SetProperty(ref _estaEnModoEdicion, value);
        }

        private bool _esNuevaFactura;
        public bool EsNuevaFactura
        {
            get => _esNuevaFactura;
            set { if (SetProperty(ref _esNuevaFactura, value)) OnPropertyChanged(nameof(TituloFormulario)); }
        }

        public string TituloFormulario => EsNuevaFactura ? "Nueva Factura" : "Editar Factura";

        private int _editandoId;

        private string _editCliente = string.Empty;
        public string EditCliente { get => _editCliente; set => SetProperty(ref _editCliente, value); }

        private string _editDescripcion = string.Empty;
        public string EditDescripcion { get => _editDescripcion; set => SetProperty(ref _editDescripcion, value); }

        private decimal _editMonto;
        public decimal EditMonto { get => _editMonto; set => SetProperty(ref _editMonto, value); }

        private DateTime _editFechaVencimiento = DateTime.Now.AddDays(30);
        public DateTime EditFechaVencimiento { get => _editFechaVencimiento; set => SetProperty(ref _editFechaVencimiento, value); }

        private bool _montoEditable = true;
        public bool MontoEditable { get => _montoEditable; set => SetProperty(ref _montoEditable, value); }

        private string _editTipoPago = "Mensualidad";
        public string EditTipoPago { get => _editTipoPago; set => SetProperty(ref _editTipoPago, value); }

        public static IReadOnlyList<string> OpcionesTipoPago { get; } = new List<string>
        {
            "Mensualidad",
            "Pago Único",
            "Pago Pendiente",
            "Abono Parcial",
            "Servicio",
            "Producto",
            "Otro"
        };

        // ─── Comandos ─────────────────────────────────────────────────────────────
        public ICommand NuevaFacturaCommand    { get; }
        public ICommand EditarCommand          { get; }
        public ICommand GuardarCommand         { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand MarcarPagadaCommand    { get; }
        public ICommand AnularCommand          { get; }
        public ICommand AplicarFiltrosCommand  { get; }
        public ICommand LimpiarFiltrosCommand  { get; }

        public CobranzaViewModel()
        {
            _svc = ContabilidadService.Instance;

            NuevaFacturaCommand    = new RelayCommand(() => AbrirFormularioNuevo());
            EditarCommand          = new RelayCommand<FacturaCobranza>(f => AbrirFormularioEditar(f));
            GuardarCommand         = new RelayCommand(() => GuardarFactura(), () => EstaEnModoEdicion);
            CancelarEdicionCommand = new RelayCommand(() => CerrarFormulario());
            MarcarPagadaCommand    = new RelayCommand<FacturaCobranza>(f => EjecutarMarcarPagada(f));
            AnularCommand          = new RelayCommand<FacturaCobranza>(f => EjecutarAnular(f));
            AplicarFiltrosCommand  = new RelayCommand(() => AplicarFiltros());
            LimpiarFiltrosCommand  = new RelayCommand(() => LimpiarFiltros());

            _svc.OnEmpresaCambiada      += CargarDatos;
            _svc.OnDatosModificados     += CargarDatos;
            _svc.OnFacturasModificadas  += CargarDatos;
            CargarDatos();
        }

        // ─── Carga y filtrado ─────────────────────────────────────────────────────
        private void CargarDatos()
        {
            _todasLasFacturas = _svc.ObtenerFacturas();
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var resultado = _todasLasFacturas.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FiltroCliente))
                resultado = resultado.Where(f =>
                    f.NombreCliente.Contains(FiltroCliente, StringComparison.OrdinalIgnoreCase) ||
                    f.NumeroFactura.Contains(FiltroCliente, StringComparison.OrdinalIgnoreCase));

            if (FiltroEstado != "Todos los estados" && !string.IsNullOrEmpty(FiltroEstado))
                resultado = resultado.Where(f => f.Estado == FiltroEstado);

            if (FiltroDesde.HasValue)
                resultado = resultado.Where(f => f.FechaEmision.Date >= FiltroDesde.Value.Date);

            if (FiltroHasta.HasValue)
                resultado = resultado.Where(f => f.FechaEmision.Date <= FiltroHasta.Value.Date);

            Facturas = new ObservableCollection<FacturaCobranza>(resultado.ToList());
            ActualizarTotales();
        }

        private void LimpiarFiltros()
        {
            FiltroCliente = string.Empty;
            FiltroEstado  = "Todos los estados";
            FiltroDesde   = null;
            FiltroHasta   = null;
            AplicarFiltros();
        }

        private void ActualizarTotales()
        {
            var base_ = _todasLasFacturas;
            Pendientes      = base_.Count(f => f.Estado == "Pendiente");
            Vencidas        = base_.Count(f => f.Estado == "Vencida");
            Cobradas        = base_.Count(f => f.Estado == "Pagada");
            MontoPendientes = base_.Where(f => f.Estado == "Pendiente").Sum(f => f.Monto);
            MontoVencidas   = base_.Where(f => f.Estado == "Vencida").Sum(f => f.Monto);
            MontoCobradas   = base_.Where(f => f.Estado == "Pagada").Sum(f => f.Monto);
        }

        // ─── Formulario ───────────────────────────────────────────────────────────
        private void AbrirFormularioNuevo()
        {
            if (_svc.EmpresaActivaId == null)
            {
                MessageBox.Show("Seleccione una empresa activa antes de crear facturas.", "Sin empresa", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _editandoId          = 0;
            EsNuevaFactura       = true;
            EditCliente          = string.Empty;
            EditDescripcion      = string.Empty;
            EditMonto            = 0;
            EditFechaVencimiento = DateTime.Now.AddDays(30);
            EditTipoPago         = "Mensualidad";
            MontoEditable        = true;
            EstaEnModoEdicion    = true;
        }

        private void AbrirFormularioEditar(FacturaCobranza? f)
        {
            var target = f ?? FacturaSeleccionada;
            if (target == null) return;
            if (target.Estado != "Pendiente" && target.Estado != "Vencida")
            {
                MessageBox.Show("Solo se pueden editar facturas en estado Pendiente o Vencida.", "No editable", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _editandoId          = target.Id;
            EsNuevaFactura       = false;
            EditCliente          = target.NombreCliente;
            EditDescripcion      = target.Descripcion;
            EditMonto            = target.Monto;
            EditFechaVencimiento = target.FechaVencimiento;
            EditTipoPago         = target.TipoPago;
            MontoEditable        = false;
            EstaEnModoEdicion    = true;
        }

        private void CerrarFormulario()
        {
            EstaEnModoEdicion = false;
            EsNuevaFactura    = false;
        }

        private void GuardarFactura()
        {
            if (string.IsNullOrWhiteSpace(EditCliente))
            {
                MessageBox.Show("El nombre del cliente es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EsNuevaFactura && EditMonto <= 0)
            {
                MessageBox.Show("El monto debe ser mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EditFechaVencimiento.Date < DateTime.Now.Date && EsNuevaFactura)
            {
                MessageBox.Show("La fecha de vencimiento no puede ser anterior a hoy.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var factura = new FacturaCobranza
                {
                    Id               = _editandoId,
                    NombreCliente    = EditCliente.Trim(),
                    Descripcion      = EditDescripcion.Trim(),
                    Monto            = EditMonto,
                    FechaEmision     = _editandoId == 0 ? DateTime.Now : DateTime.Now,
                    FechaVencimiento = EditFechaVencimiento,
                    TipoPago         = EditTipoPago,
                    Estado           = "Pendiente"
                };

                if (_editandoId > 0)
                {
                    var original = _todasLasFacturas.FirstOrDefault(x => x.Id == _editandoId);
                    if (original != null)
                    {
                        factura.FechaEmision         = original.FechaEmision;
                        factura.Monto                = original.Monto;
                        factura.NumeroFactura        = original.NumeroFactura;
                        factura.IdComprobanteEmision = original.IdComprobanteEmision;
                        factura.Estado               = original.Estado;
                        factura.TipoPago             = EditTipoPago;
                    }
                }

                _svc.GuardarFactura(factura);
                CerrarFormulario();
                CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─── Acciones de fila ─────────────────────────────────────────────────────
        private void EjecutarMarcarPagada(FacturaCobranza? f)
        {
            var target = f ?? FacturaSeleccionada;
            if (target == null) return;

            var confirm = MessageBox.Show(
                $"¿Confirmar cobro de la factura {target.NumeroFactura}?\n\nCliente: {target.NombreCliente}\nMonto:   {target.Monto:N2}\n\nSe generará el asiento contable:\n  Débito:  Caja General\n  Crédito: Clientes Nacionales",
                "Confirmar Cobro", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _svc.MarcarFacturaPagada(target.Id);
                CargarDatos();
                MessageBox.Show($"Factura {target.NumeroFactura} marcada como Pagada.\nAsiento contable generado.", "Cobro Registrado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EjecutarAnular(FacturaCobranza? f)
        {
            var target = f ?? FacturaSeleccionada;
            if (target == null) return;

            var confirm = MessageBox.Show(
                $"¿Anular la factura {target.NumeroFactura}?\n\nCliente: {target.NombreCliente}\nMonto:   {target.Monto:N2}\n\nEsta acción generará un asiento de anulación y no se puede deshacer.",
                "Confirmar Anulación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _svc.AnularFactura(target.Id);
                CargarDatos();
                MessageBox.Show($"Factura {target.NumeroFactura} anulada. Asiento de reversión generado.", "Factura Anulada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
