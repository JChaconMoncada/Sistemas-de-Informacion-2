using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;
using HistorialReexpresion = Sistema_contable.Models.HistorialReexpresion;

namespace Sistema_contable.ViewModels
{
    public class ReexpresionViewModel : ViewModelBase
    {
        private DateTime _fechaOrigen = new DateTime(2023, 1, 1);
        private DateTime _fechaDestino = DateTime.Now;
        private decimal _ipcOrigen;
        private decimal _ipcDestino;
        private decimal _factorCalculado = 1m;
        private int _partidasSeleccionadasCount;
        private decimal _ajusteTotal;
        private decimal _valorOriginalTotal;
        private decimal _valorAjustadoTotal;
        private readonly IpcService _ipcService;
        private readonly ContabilidadService _contabilidadService;

        public DateTime FechaOrigen
        {
            get => _fechaOrigen;
            set
            {
                if (SetProperty(ref _fechaOrigen, value))
                {
                    _ = ObtenerIpcOrigenAsync();
                }
            }
        }

        public DateTime FechaDestino
        {
            get => _fechaDestino;
            set
            {
                if (SetProperty(ref _fechaDestino, value))
                    _ = ObtenerIpcDestinoAsync();
            }
        }

        public decimal IpcOrigen
        {
            get => _ipcOrigen;
            set
            {
                if (SetProperty(ref _ipcOrigen, value))
                    CalcularFactor();
            }
        }

        public decimal IpcDestino
        {
            get => _ipcDestino;
            set
            {
                if (SetProperty(ref _ipcDestino, value))
                    CalcularFactor();
            }
        }

        public decimal FactorCalculado
        {
            get => _factorCalculado;
            set
            {
                if (SetProperty(ref _factorCalculado, value))
                    RecalcularPartidas();
            }
        }

        public int PartidasSeleccionadasCount
        {
            get => _partidasSeleccionadasCount;
            private set => SetProperty(ref _partidasSeleccionadasCount, value);
        }

        public decimal AjusteTotal
        {
            get => _ajusteTotal;
            private set => SetProperty(ref _ajusteTotal, value);
        }

        public decimal ValorOriginalTotal
        {
            get => _valorOriginalTotal;
            private set => SetProperty(ref _valorOriginalTotal, value);
        }

        public decimal ValorAjustadoTotal
        {
            get => _valorAjustadoTotal;
            private set => SetProperty(ref _valorAjustadoTotal, value);
        }

        public string AjusteTotalMonedaFormato => $"Bs {AjusteTotal:N2}";
        public string ValorOriginalTotalMonedaFormato => $"Bs {ValorOriginalTotal:N2}";
        public string ValorAjustadoTotalMonedaFormato => $"Bs {ValorAjustadoTotal:N2}";

        public ObservableCollection<PartidaReexpresion> Partidas { get; } = new ObservableCollection<PartidaReexpresion>();

        private PartidaReexpresion _partidaSeleccionada;
        public PartidaReexpresion PartidaSeleccionada
        {
            get => _partidaSeleccionada;
            set
            {
                if (SetProperty(ref _partidaSeleccionada, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AplicarReexpresionCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand EliminarPartidaCommand { get; }
        public ICommand ActualizarCommand { get; }
        public ICommand EliminarHistorialCommand { get; }
        public ICommand UsarHistorialCommand { get; }

        public ReexpresionViewModel()
        {
            _ipcService = IpcService.Instance;
            _contabilidadService = ContabilidadService.Instance;

            AplicarReexpresionCommand = new RelayCommand(AplicarReexpresion, () => Partidas.Any(p => p.Aplicar));
            CancelarCommand = new RelayCommand(Cancelar);
            EliminarPartidaCommand = new RelayCommand(ExecuteEliminarPartida, () => PartidaSeleccionada != null);
            ActualizarCommand = new RelayCommand(CargarPartidas);
            EliminarHistorialCommand = new RelayCommand<HistorialReexpresion>(ExecuteEliminarHistorial);
            UsarHistorialCommand = new RelayCommand<HistorialReexpresion>(ExecuteUsarHistorial);

            _contabilidadService.OnEmpresaCambiada += CargarPartidas;
            CargarPartidas();
            
            // Cargar IPCs iniciales
            _ = ObtenerIpcOrigenAsync();
            _ = ObtenerIpcDestinoAsync();
        }

        private void ActualizarSaldosOrigen()
        {
            // Ya no se actualizan los saldos al cambiar la fecha, porque ahora
            // se están reexpresando movimientos (transacciones) individuales, cuyo valor original es fijo.
            RecalcularPartidas();
        }

        private void CargarPartidas()
        {
            Partidas.Clear();
            var comprobantes = _contabilidadService.ObtenerComprobantesGuardados();
            var cuentas = _contabilidadService.ObtenerCuentasContables();
            var historiales = _contabilidadService.ObtenerHistorialReexpresiones();
            
            // El usuario quiere ver los movimientos de Patrimonio (y Activos no monetarios)
            var movimientosNoMonetarios = comprobantes.Where(c => c.TipoComprobante == "Patrimonio" || c.TipoComprobante == "Activo");
            
            foreach (var comp in movimientosNoMonetarios)
            {
                // Buscar la línea que NO es de efectivo (caja/bancos) para obtener la cuenta afectada
                var lineaNoCaja = comp.Lineas.FirstOrDefault(l => !l.DescripcionCuenta.ToLower().Contains("caja") && !l.DescripcionCuenta.ToLower().Contains("banco"));
                if (lineaNoCaja == null) lineaNoCaja = comp.Lineas.FirstOrDefault();
                if (lineaNoCaja == null) continue;

                var cuenta = cuentas.FirstOrDefault(c => c.Codigo == lineaNoCaja.CodigoCuenta);
                var historialesPrevios = historiales.Where(h => h.CodigoCuenta == lineaNoCaja.CodigoCuenta && (h.IdMovimientoOriginal == comp.IdComprobante || h.IdMovimientoOriginal == 0)).OrderByDescending(h => h.FechaCalculo).ToList();

                var partida = new PartidaReexpresion
                {
                    Codigo = lineaNoCaja.CodigoCuenta,
                    IdMovimientoOriginal = comp.IdComprobante,
                    Nombre = $"{comp.Descripcion} (MOV-{comp.IdComprobante})",
                    Tipo = comp.TipoComprobante,
                    ValorOriginal = lineaNoCaja.Debe > 0 ? lineaNoCaja.Debe : lineaNoCaja.Haber,
                    DescripcionDetalle = cuenta?.Descripcion ?? "Sin descripción adicional."
                };
                foreach (var h in historialesPrevios)
                {
                    partida.HistorialesAnteriores.Add(h);
                }
                
                partida.PropertyChanged += (s, e) => ActualizarTotales();
                Partidas.Add(partida);
            }
            RecalcularPartidas();
        }

        private void ActualizarTotales()
        {
            var seleccionadas = Partidas.Where(p => p.Aplicar).ToList();
            
            PartidasSeleccionadasCount = seleccionadas.Count;
            ValorOriginalTotal = seleccionadas.Sum(p => p.ValorOriginal);
            ValorAjustadoTotal = seleccionadas.Sum(p => p.ValorAjustado);
            AjusteTotal = seleccionadas.Sum(p => p.Diferencia);
            
            OnPropertyChanged(nameof(AjusteTotalMonedaFormato));
            OnPropertyChanged(nameof(ValorOriginalTotalMonedaFormato));
            OnPropertyChanged(nameof(ValorAjustadoTotalMonedaFormato));
            
            // Notificar a los comandos (CommandManager) por si cambia CanExecute
            CommandManager.InvalidateRequerySuggested();
        }

        private async Task ObtenerIpcOrigenAsync()
        {
            IpcOrigen = await _ipcService.ObtenerIpcAsync(FechaOrigen);
        }

        private async Task ObtenerIpcDestinoAsync()
        {
            IpcDestino = await _ipcService.ObtenerIpcAsync(FechaDestino);
        }

        private void CalcularFactor()
        {
            if (IpcOrigen > 0)
            {
                FactorCalculado = IpcDestino / IpcOrigen;
            }
        }

        private void RecalcularPartidas()
        {
            foreach (var partida in Partidas)
            {
                partida.Factor = FactorCalculado;
            }
            ActualizarTotales();
        }

        private void ExecuteEliminarPartida()
        {
            if (PartidaSeleccionada == null) return;
            var result = MessageBox.Show($"¿Desea excluir la cuenta {PartidaSeleccionada.Codigo} - {PartidaSeleccionada.Nombre} del cálculo de reexpresión?", "Confirmar Exclusión", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Partidas.Remove(PartidaSeleccionada);
                ActualizarTotales();
            }
        }

        private void AplicarReexpresion()
        {
            if (_contabilidadService.EmpresaActivaId == null)
            {
                return;
            }

            // 1. Respaldo Automático
            var backupPath = _contabilidadService.EjecutarRespaldoAutomatico();
            if (string.IsNullOrEmpty(backupPath))
            {
                return;
            }

            // Código de la cuenta REI definida en el sistema
            string codigoREI = "3.2.01.00";
            var cuentaREI = _contabilidadService.ObtenerCuentasContables().FirstOrDefault(c => c.Codigo == codigoREI);

            if (cuentaREI == null)
            {
                return;
            }

            foreach (var partida in Partidas.Where(p => p.Aplicar && p.Diferencia > 0))
            {
                // Eliminar comprobantes anteriores para no duplicar el ajuste en el saldo de la cuenta
                var historialesAnteriores = _contabilidadService.ObtenerHistorialReexpresiones(partida.Codigo);
                foreach (var ant in historialesAnteriores)
                {
                    if (ant.IdComprobanteAsociado > 0)
                    {
                        _contabilidadService.EliminarComprobante(ant.IdComprobanteAsociado);
                        ant.IdComprobanteAsociado = 0; // Desvincular el comprobante
                    }
                }

                // Generar Asiento individual
                var asiento = new ComprobanteContable
                {
                    Fecha = this.FechaDestino,
                    Descripcion = $"Ajuste por Inflación ({partida.Codigo}) {FechaOrigen:MMM-yy} a {FechaDestino:MMM-yy}",
                    TipoComprobante = "Ajuste",
                    IdEmpresa = _contabilidadService.EmpresaActivaId.Value,
                    Estado = "Registrado",
                    MontoTotal = partida.Diferencia,
                    Moneda = "Bs",
                    CuentaAsociada = partida.Codigo
                };

                // Línea 1: Aumentar la cuenta original (Activo por Debe, Patrimonio por Haber)
                asiento.Lineas.Add(new AsientoLinea
                {
                    CodigoCuenta = partida.Codigo,
                    DescripcionCuenta = partida.Nombre,
                    Debe = partida.Tipo == "Activo" ? partida.Diferencia : 0,
                    Haber = partida.Tipo == "Patrimonio" ? partida.Diferencia : 0
                });

                // Línea 2: Contrapartida a la cuenta REI
                asiento.Lineas.Add(new AsientoLinea
                {
                    CodigoCuenta = cuentaREI.Codigo,
                    DescripcionCuenta = cuentaREI.Nombre,
                    Debe = partida.Tipo == "Patrimonio" ? partida.Diferencia : 0,
                    Haber = partida.Tipo == "Activo" ? partida.Diferencia : 0
                });
                
                _contabilidadService.GuardarComprobante(asiento);

                // Guardar Historial Silenciosamente
                var historial = new HistorialReexpresion
                {
                    CodigoCuenta = partida.Codigo,
                    NombreCuenta = partida.Nombre,
                    FechaCalculo = this.FechaDestino,
                    ValorOriginal = partida.ValorOriginal,
                    MontoAjuste = partida.Diferencia,
                    ValorAjustado = partida.ValorAjustado,
                    FactorAplicado = partida.Factor,
                    FechaOrigen = this.FechaOrigen,
                    FechaDestino = this.FechaDestino,
                    IdMovimientoOriginal = partida.IdMovimientoOriginal,
                    IdComprobanteAsociado = asiento.IdComprobante,
                    Anulado = false
                };
                _contabilidadService.GuardarHistorialReexpresion(historial);
            }

            // 3. Notas Revelatorias (Generadas internamente o silenciosamente)
            string notas = GenerarNotasRevelatorias();
            
            CargarPartidas();
        }

        private void ExecuteEliminarHistorial(HistorialReexpresion historial)
        {
            if (historial == null) return;
            var res = MessageBox.Show($"¿Desea eliminar este registro del historial? Si era el activo, el comprobante se anulará.", "Eliminar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                _contabilidadService.RestaurarReexpresion(historial.Id);
                CargarPartidas();
            }
        }

        private void ExecuteUsarHistorial(HistorialReexpresion historial)
        {
            if (historial == null) return;
            var res = MessageBox.Show($"¿Desea usar esta reexpresión antigua para {historial.NombreCuenta}?", "Usar historial", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                var cuentaREI = _contabilidadService.ObtenerCuentasContables().FirstOrDefault(c => c.Codigo == "3.2.01.00");
                if (cuentaREI == null)
                {
                    MessageBox.Show("Falta la cuenta REI (3.2.01.00) en el plan de cuentas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 1. Eliminar la activa de este movimiento si existe
                var antiguaActiva = _contabilidadService.ObtenerHistorialReexpresiones(historial.CodigoCuenta)
                    .FirstOrDefault(h => !h.Anulado && h.IdComprobanteAsociado > 0 && h.IdMovimientoOriginal == historial.IdMovimientoOriginal);
                if (antiguaActiva != null)
                {
                    _contabilidadService.DesactivarComprobanteReexpresion(antiguaActiva.Id);
                }

                // 2. Crear comprobante de la que queremos usar
                var asiento = new ComprobanteContable
                {
                    IdEmpresa = _contabilidadService.EmpresaActivaId.Value,
                    Fecha = DateTime.Now,
                    TipoComprobante = "Diario",
                    Estado = "Registrado",
                    Descripcion = $"Ajuste por Inflación reactivado: {historial.NombreCuenta}",
                    CuentaAsociada = historial.CodigoCuenta
                };

                // Asumimos el tipo de cuenta por el codigo, ej Activo empieza con 1, Patrimonio con 3
                string tipo = historial.CodigoCuenta.StartsWith("1") ? "Activo" : "Patrimonio";

                asiento.Lineas.Add(new AsientoLinea
                {
                    CodigoCuenta = historial.CodigoCuenta,
                    DescripcionCuenta = historial.NombreCuenta,
                    Debe = tipo == "Activo" ? historial.MontoAjuste : 0,
                    Haber = tipo == "Patrimonio" ? historial.MontoAjuste : 0
                });

                asiento.Lineas.Add(new AsientoLinea
                {
                    CodigoCuenta = cuentaREI.Codigo,
                    DescripcionCuenta = cuentaREI.Nombre,
                    Debe = tipo == "Patrimonio" ? historial.MontoAjuste : 0,
                    Haber = tipo == "Activo" ? historial.MontoAjuste : 0
                });

                _contabilidadService.GuardarComprobante(asiento);

                // 3. Crear nuevo registro histórico clonando el antiguo y asociándole el comprobante
                var nuevoHistorial = new HistorialReexpresion
                {
                    CodigoCuenta = historial.CodigoCuenta,
                    NombreCuenta = historial.NombreCuenta,
                    FechaCalculo = DateTime.Now,
                    ValorOriginal = historial.ValorOriginal,
                    MontoAjuste = historial.MontoAjuste,
                    ValorAjustado = historial.ValorAjustado,
                    FactorAplicado = historial.FactorAplicado,
                    FechaOrigen = historial.FechaOrigen,
                    FechaDestino = historial.FechaDestino,
                    IdMovimientoOriginal = historial.IdMovimientoOriginal,
                    IdComprobanteAsociado = asiento.IdComprobante,
                    Anulado = false
                };
                _contabilidadService.GuardarHistorialReexpresion(nuevoHistorial);

                MessageBox.Show("Reexpresión aplicada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarPartidas();
            }
        }

        private string GenerarNotasRevelatorias()
        {
            var partidasAjustadas = Partidas.Where(p => p.Aplicar && p.Diferencia > 0).ToList();
            var totalAjuste = partidasAjustadas.Sum(p => p.Diferencia);
            
            var detallesPartidas = string.Join("\n", partidasAjustadas.Select(p => $"  - {p.Nombre} (Cta: {p.Codigo}): Bs {p.Diferencia:N2}"));
            
            return $"NOTA X - REEXPRESIÓN MONETARIA (DPC10/NIC29)\n\n" +
                   $"Los estados financieros han sido reexpresados para reflejar los efectos de la inflación.\n" +
                   $"El ajuste se realizó utilizando los Índices Nacionales de Precios al Consumidor (INPC).\n\n" +
                   $"- IPC Origen ({FechaOrigen:MMM yyyy}): {IpcOrigen:N2}\n" +
                   $"- IPC Destino ({FechaDestino:MMM yyyy}): {IpcDestino:N2}\n" +
                   $"- Factor de Reexpresión: {FactorCalculado:N4}\n\n" +
                   $"Detalle de las partidas reexpresadas:\n{detallesPartidas}\n\n" +
                   $"El efecto neto en el patrimonio debido a la reexpresión de partidas no monetarias " +
                   $"ascendió a Bs {totalAjuste:N2}, el cual fue registrado en la cuenta de Resultado por Exposición a la Inflación (REI).\n\n" +
                   $"Esta nota debe incluirse en los Estados Financieros definitivos.";
        }

        public void SetModoReconversion2021(bool activo)
        {
            if (activo)
            {
                _fechaOrigen  = new DateTime(2021, 1, 1);
                _fechaDestino = new DateTime(2021, 10, 1);
                _ipcOrigen    = 1_000_000m;
                _ipcDestino   = 1m;
                OnPropertyChanged(nameof(FechaOrigen));
                OnPropertyChanged(nameof(FechaDestino));
                OnPropertyChanged(nameof(IpcOrigen));
                OnPropertyChanged(nameof(IpcDestino));
                CalcularFactor();
            }
            else
            {
                // Restablecer fechas a valores por defecto del modo IPC
                // para que el fetch async use periodos significativos, no los de 2021.
                _fechaOrigen  = new DateTime(DateTime.Today.Year - 1, 1, 1);
                _fechaDestino = DateTime.Today;
                OnPropertyChanged(nameof(FechaOrigen));
                OnPropertyChanged(nameof(FechaDestino));

                // Resetear IPCs visualmente mientras llega el resultado async
                _ipcOrigen  = 0m;
                _ipcDestino = 0m;
                OnPropertyChanged(nameof(IpcOrigen));
                OnPropertyChanged(nameof(IpcDestino));

                // Ahora buscar los IPC reales para las fechas restauradas
                _ = ObtenerIpcOrigenAsync();
                _ = ObtenerIpcDestinoAsync();
            }
        }

        private void Cancelar()
        {
            CargarPartidas();
        }
    }
}
