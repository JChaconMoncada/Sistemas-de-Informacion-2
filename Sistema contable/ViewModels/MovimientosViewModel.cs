using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class MovimientosViewModel : ViewModelBase
    {
        private DateTime _fecha = DateTime.Now;
        private string _descripcion = string.Empty;
        private decimal _monto;
        private CuentaContable _cuentaSeleccionada;
        private string _tipoTransaccion = "Ingreso";
        private string _monedaSeleccionada = "Bs";
        
        private readonly ContabilidadService _contabilidadService;

        public DateTime Fecha
        {
            get => _fecha;
            set => SetProperty(ref _fecha, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            set
            {
                if (SetProperty(ref _descripcion, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public decimal Monto
        {
            get => _monto;
            set
            {
                if (SetProperty(ref _monto, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public CuentaContable CuentaSeleccionada
        {
            get => _cuentaSeleccionada;
            set
            {
                if (SetProperty(ref _cuentaSeleccionada, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string TipoTransaccion
        {
            get => _tipoTransaccion;
            set => SetProperty(ref _tipoTransaccion, value);
        }

        public string MonedaSeleccionada
        {
            get => _monedaSeleccionada;
            set => SetProperty(ref _monedaSeleccionada, value);
        }

        public ObservableCollection<CuentaContable> CuentasDisponibles { get; } = new ObservableCollection<CuentaContable>();
        public ObservableCollection<string> TiposTransaccion { get; } = new ObservableCollection<string> { "Ingreso", "Egreso", "Activo", "Pasivo", "Patrimonio" };
        public ObservableCollection<string> MonedasDisponibles { get; } = new ObservableCollection<string> { "Pesos", "Bs", "Dólares", "Euros" };
        
        // Historial
        public ObservableCollection<ComprobanteContable> HistorialMovimientos { get; } = new ObservableCollection<ComprobanteContable>();

        private ComprobanteContable _comprobanteSeleccionado;
        public ComprobanteContable ComprobanteSeleccionado
        {
            get => _comprobanteSeleccionado;
            set
            {
                if (SetProperty(ref _comprobanteSeleccionado, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand GuardarMovimientoCommand { get; }
        public ICommand EliminarMovimientoCommand { get; }

        public MovimientosViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            GuardarMovimientoCommand = new RelayCommand(ExecuteGuardarMovimiento, CanGuardarMovimiento);
            EliminarMovimientoCommand = new RelayCommand(ExecuteEliminarMovimiento, () => ComprobanteSeleccionado != null);
            
            _contabilidadService.OnEmpresaCambiada += CargarDatos;
            CargarDatos();
        }

        private void CargarDatos()
        {
            CargarCuentas();
            RefrescarHistorialTransacciones();
        }

        private void CargarCuentas()
        {
            var cuentaAnteriorCodigo = _cuentaSeleccionada?.Codigo;
            CuentasDisponibles.Clear();
            var cuentas = _contabilidadService.ObtenerCuentasContables().Where(c => c.AceptaMovimiento);
            foreach (var cuenta in cuentas)
            {
                // Excluimos la cuenta de banco principal para no permitir que la seleccionen directamente como contrapartida en esta vista rápida
                if (cuenta.Codigo != "1.1.01.01")
                {
                    CuentasDisponibles.Add(cuenta);
                }
            }
            
            if (cuentaAnteriorCodigo != null)
            {
                CuentaSeleccionada = CuentasDisponibles.FirstOrDefault(c => c.Codigo == cuentaAnteriorCodigo);
            }
        }

        private void RefrescarHistorialTransacciones()
        {
            HistorialMovimientos.Clear();
            var comprobantes = _contabilidadService.ObtenerComprobantesGuardados().OrderByDescending(c => c.Fecha);
            foreach (var comp in comprobantes)
            {
                HistorialMovimientos.Add(comp);
            }
        }

        private bool CanGuardarMovimiento()
        {
            return Monto > 0 && !string.IsNullOrWhiteSpace(Descripcion) && CuentaSeleccionada != null;
        }

        private void ExecuteGuardarMovimiento()
        {
            if (_contabilidadService.EmpresaActivaId == null)
            {
                MessageBox.Show("Debe seleccionar una empresa activa primero.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Cuenta automática de contrapartida (Caja General por defecto)
            string codigoCajaBanco = "1.1.01.01";
            var cuentaCaja = _contabilidadService.ObtenerCuentasContables().FirstOrDefault(c => c.Codigo == codigoCajaBanco);
            if (cuentaCaja == null)
            {
                MessageBox.Show("No se encontró la cuenta de Caja General en el sistema.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. Instanciar Comprobante
            var nuevoComprobante = new ComprobanteContable
            {
                Fecha = this.Fecha,
                Descripcion = this.Descripcion,
                TipoComprobante = this.TipoTransaccion,
                IdEmpresa = _contabilidadService.EmpresaActivaId.Value,
                Estado = "Registrado",
                MontoTotal = this.Monto,
                Moneda = this.MonedaSeleccionada,
                CuentaAsociada = this.CuentaSeleccionada.Nombre
            };

            // 3. Reglas de Partida Doble
            switch (TipoTransaccion)
            {
                case "Ingreso":
                case "Pasivo":
                case "Patrimonio":
                    // Aumenta el efectivo (Debe), la contrapartida (Ingreso/Pasivo/Patrimonio) va al Haber
                    nuevoComprobante.Lineas.Add(new AsientoLinea
                    {
                        CodigoCuenta = cuentaCaja.Codigo,
                        DescripcionCuenta = cuentaCaja.Nombre,
                        Debe = this.Monto,
                        Haber = 0
                    });
                    nuevoComprobante.Lineas.Add(new AsientoLinea
                    {
                        CodigoCuenta = this.CuentaSeleccionada.Codigo,
                        DescripcionCuenta = this.CuentaSeleccionada.Nombre,
                        Debe = 0,
                        Haber = this.Monto
                    });
                    break;
                case "Egreso":
                case "Activo":
                    // Disminuye el efectivo (Haber), la contrapartida (Gasto/Activo) va al Debe
                    nuevoComprobante.Lineas.Add(new AsientoLinea
                    {
                        CodigoCuenta = this.CuentaSeleccionada.Codigo,
                        DescripcionCuenta = this.CuentaSeleccionada.Nombre,
                        Debe = this.Monto,
                        Haber = 0
                    });
                    nuevoComprobante.Lineas.Add(new AsientoLinea
                    {
                        CodigoCuenta = cuentaCaja.Codigo,
                        DescripcionCuenta = cuentaCaja.Nombre,
                        Debe = 0,
                        Haber = this.Monto
                    });
                    break;
            }

            // 4. Guardar
            _contabilidadService.GuardarComprobante(nuevoComprobante);

            RefrescarHistorialTransacciones();
            LimpiarFormulario();
            
            MessageBox.Show("Movimiento registrado correctamente con partida doble.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LimpiarFormulario()
        {
            Monto = 0;
            Descripcion = string.Empty;
            CuentaSeleccionada = null;
            MonedaSeleccionada = "Bs";
        }

        private void ExecuteEliminarMovimiento()
        {
            if (ComprobanteSeleccionado == null) return;
            var result = MessageBox.Show($"¿Desea eliminar permanentemente el comprobante #{ComprobanteSeleccionado.IdComprobante}?\n({ComprobanteSeleccionado.Descripcion})", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _contabilidadService.EliminarComprobante(ComprobanteSeleccionado.IdComprobante);
                RefrescarHistorialTransacciones();
            }
        }
    }
}
