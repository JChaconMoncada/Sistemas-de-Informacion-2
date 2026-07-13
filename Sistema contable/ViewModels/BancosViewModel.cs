using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Sistema_contable.Services;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class BancosViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;
        private string _filePath = string.Empty;
        private string _rawText = string.Empty;
        private bool _isProcessing;
        private int _progressValue;
        private int _selectedTransactionsCount;
        private ObservableCollection<TransaccionBancariaViewModel> _transacciones;
        private ObservableCollection<CuentaContable> _cuentasContables;
        private TransaccionBancariaViewModel? _selectedTransaccion;

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public string RawText
        {
            get => _rawText;
            set => SetProperty(ref _rawText, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public int ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        public int SelectedTransactionsCount
        {
            get => _selectedTransactionsCount;
            set => SetProperty(ref _selectedTransactionsCount, value);
        }

        public ObservableCollection<TransaccionBancariaViewModel> Transacciones
        {
            get => _transacciones;
            set => SetProperty(ref _transacciones, value);
        }

        public ObservableCollection<CuentaContable> CuentasContables
        {
            get => _cuentasContables;
            set => SetProperty(ref _cuentasContables, value);
        }

        public TransaccionBancariaViewModel? SelectedTransaccion
        {
            get => _selectedTransaccion;
            set 
            {
                if (SetProperty(ref _selectedTransaccion, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand SeleccionarPdfCommand { get; }
        public ICommand ImportarTransaccionesCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand AgregarFilaCommand { get; }
        public ICommand EliminarFilaCommand { get; }
        public ICommand EliminarErroresCommand { get; }

        public BancosViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Transacciones = new ObservableCollection<TransaccionBancariaViewModel>();
            CuentasContables = new ObservableCollection<CuentaContable>();

            Transacciones.CollectionChanged += Transacciones_CollectionChanged;

            SeleccionarPdfCommand = new RelayCommand(() => SeleccionarPdf());
            ImportarTransaccionesCommand = new RelayCommand(() => ImportarTransacciones(), () => Transacciones.Count > 0 && !IsProcessing);
            CancelarCommand = new RelayCommand(() => Cancelar());
            AgregarFilaCommand = new RelayCommand(() => AgregarFila());
            EliminarFilaCommand = new RelayCommand<TransaccionBancariaViewModel>(tx => EliminarFila(tx));
            EliminarErroresCommand = new RelayCommand(() => EliminarErrores());

            CargarCuentas();
            _contabilidadService.OnEmpresaCambiada += CargarCuentas;
        }

        private void CargarCuentas()
        {
            CuentasContables.Clear();
            var cuentas = _contabilidadService.ObtenerCuentasContables()
                                              .Where(c => c.AceptaMovimiento)
                                              .OrderBy(c => c.Codigo);
            foreach (var c in cuentas)
            {
                CuentasContables.Add(c);
            }
        }

        private void Transacciones_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (TransaccionBancariaViewModel item in e.NewItems)
                {
                    item.PropertyChanged += Transaccion_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (TransaccionBancariaViewModel item in e.OldItems)
                {
                    item.PropertyChanged -= Transaccion_PropertyChanged;
                }
            }
            RecalcularConteo();
        }

        private void Transaccion_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TransaccionBancariaViewModel.Importar))
            {
                RecalcularConteo();
            }
        }

        private void RecalcularConteo()
        {
            SelectedTransactionsCount = Transacciones.Count(t => t.Importar);
        }

        private void AgregarFila()
        {
            var nueva = new TransaccionBancariaViewModel
            {
                Fecha = DateTime.Now,
                Descripcion = "NUEVA TRANSACCIÓN MANUAL",
                Importar = true,
                Referencia = string.Empty,
                Debito = 0,
                Credito = 0
            };
            Transacciones.Add(nueva);
            SelectedTransaccion = nueva;
        }

        private void EliminarFila(TransaccionBancariaViewModel? tx = null)
        {
            var target = tx ?? SelectedTransaccion;
            if (target != null)
            {
                Transacciones.Remove(target);
                if (SelectedTransaccion == target)
                {
                    SelectedTransaccion = null;
                }
            }
        }

        private void EliminarErrores()
        {
            var errores = Transacciones.Where(t => t.IsError).ToList();
            foreach (var error in errores)
            {
                Transacciones.Remove(error);
            }
        }

        private async void SeleccionarPdf()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos PDF (*.pdf)|*.pdf",
                Title = "Seleccionar Estado de Cuenta PDF"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                await ProcesarArchivoPdf(FilePath);
            }
        }

        private async Task ProcesarArchivoPdf(string path)
        {
            IsProcessing = true;
            ProgressValue = 0;
            Transacciones.Clear();
            RawText = "Extrayendo y analizando texto del PDF...";

            // Simulación visual de procesamiento
            for (int i = 1; i <= 10; i++)
            {
                ProgressValue = i * 10;
                await Task.Delay(100);
            }

            try
            {
                // Extraer el texto completo para el panel de vista previa
                var raw = await Task.Run(() => PdfParserHelper.ExtractRawText(path));
                RawText = raw;

                // Analizar transacciones estructuradas
                var parsed = await Task.Run(() => PdfParserHelper.ParseBankStatement(path));

                if (parsed != null && parsed.Count > 0)
                {
                    foreach (var tx in parsed)
                    {
                        var vm = new TransaccionBancariaViewModel
                        {
                            Fecha = tx.Fecha,
                            Referencia = tx.Referencia,
                            Descripcion = tx.Descripcion,
                            Debito = tx.Debito,
                            Credito = tx.Credito,
                            IsError = tx.IsError,
                            RawLine = tx.RawLine,
                            Importar = !tx.IsError
                        };
                        if (!tx.IsError)
                        {
                            AutoAsignarCuenta(vm);
                        }
                        Transacciones.Add(vm);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "No se pudo extraer ninguna línea de texto del archivo. Asegúrese de que el PDF no esté vacío o sea una imagen sin texto.",
                        "Información de Análisis",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al analizar el archivo: {ex.Message}", "Error de Procesamiento", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
                ProgressValue = 0;
            }
        }

        private void AutoAsignarCuenta(TransaccionBancariaViewModel vm)
        {
            if (CuentasContables.Count == 0) return;

            string desc = vm.Descripcion.ToUpper();

            if (desc.Contains("ALQUILER"))
            {
                vm.CuentaSeleccionada = CuentasContables.FirstOrDefault(c => c.Codigo == "5.1.01.01");
            }
            else if (desc.Contains("COMISION") || desc.Contains("MANTENIMIENTO") || desc.Contains("IVA") || desc.Contains("COM BANCA") || desc.Contains("COM POR"))
            {
                vm.CuentaSeleccionada = CuentasContables.FirstOrDefault(c => c.Codigo == "5.1.01.03");
            }
            else if (desc.Contains("POLAR") || desc.Contains("CLIENTE") || desc.Contains("ABONO") || desc.Contains("INGRESO") || desc.Contains("PATRIA"))
            {
                vm.CuentaSeleccionada = CuentasContables.FirstOrDefault(c => c.Codigo == "1.1.02.01");
            }
            else if (desc.Contains("SERVICIOS") || desc.Contains("LUZ") || desc.Contains("AGUA") || desc.Contains("CANTV") || desc.Contains("MOVISTAR") || desc.Contains("PREPAGO"))
            {
                vm.CuentaSeleccionada = CuentasContables.FirstOrDefault(c => c.Codigo == "5.1.01.02");
            }
            else if (desc.Contains("NOMINA") || desc.Contains("HONORARIOS") || desc.Contains("PROFESIONAL"))
            {
                vm.CuentaSeleccionada = CuentasContables.FirstOrDefault(c => c.Codigo == "5.1.01.04");
            }
            else if (desc.Contains("PROVEEDOR") || desc.Contains("PAGO") || desc.Contains("PAGOMOVIL") || desc.Contains("TRASPASO"))
            {
                vm.CuentaSeleccionada = CuentasContables.FirstOrDefault(c => c.Codigo == "2.1.01.01");
            }
        }


        private void ImportarTransacciones()
        {
            if (_contabilidadService.EmpresaActivaId == null)
            {
                MessageBox.Show("Por favor, seleccione una Empresa Activa en el panel izquierdo antes de importar transacciones.", "Empresa no seleccionada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var seleccionadas = Transacciones.Where(t => t.Importar).ToList();
            if (seleccionadas.Count == 0)
            {
                MessageBox.Show("No ha seleccionado ninguna transacción para importar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (seleccionadas.Any(t => t.CuentaSeleccionada == null))
            {
                MessageBox.Show("Todas las transacciones seleccionadas para importar deben tener asignada una Cuenta Contable.", "Falta Asignación de Cuenta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Buscar la cuenta del banco de la empresa activa
            var cuentaBanco = CuentasContables.FirstOrDefault(c => c.Nombre.ToUpper().Contains("BANCO") || c.Codigo.StartsWith("1.1.01.02") || c.Codigo.StartsWith("1.1.01.03"));
            if (cuentaBanco == null)
            {
                cuentaBanco = CuentasContables.FirstOrDefault(c => c.Codigo.StartsWith("1.1.01")) 
                              ?? CuentasContables.FirstOrDefault();
            }

            if (cuentaBanco == null)
            {
                MessageBox.Show("No se encontró una cuenta bancaria o de efectivo activa en el Plan de Cuentas para realizar el contra-asiento.", "Error de Configuración", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int importadas = 0;
            try
            {
                foreach (var tx in seleccionadas)
                {
                    var cc = new ComprobanteContable
                    {
                        Fecha = tx.Fecha ?? DateTime.Now,
                        Descripcion = $"Importación bancaria: {tx.Descripcion} (Ref: {tx.Referencia})",
                        TipoComprobante = "Diario",
                        Estado = "Registrado",
                        MontoTotal = tx.Debito > 0 ? tx.Debito : tx.Credito,
                        CuentaAsociada = tx.CuentaSeleccionada?.Nombre ?? ""
                    };

                    if (tx.Debito > 0)
                    {
                        cc.Lineas.Add(new AsientoLinea
                        {
                            CodigoCuenta = tx.CuentaSeleccionada!.Codigo,
                            DescripcionCuenta = tx.CuentaSeleccionada.Nombre,
                            Debe = tx.Debito,
                            Haber = 0
                        });
                        cc.Lineas.Add(new AsientoLinea
                        {
                            CodigoCuenta = cuentaBanco.Codigo,
                            DescripcionCuenta = cuentaBanco.Nombre,
                            Debe = 0,
                            Haber = tx.Debito
                        });
                    }
                    else if (tx.Credito > 0)
                    {
                        cc.Lineas.Add(new AsientoLinea
                        {
                            CodigoCuenta = cuentaBanco.Codigo,
                            DescripcionCuenta = cuentaBanco.Nombre,
                            Debe = tx.Credito,
                            Haber = 0
                        });
                        cc.Lineas.Add(new AsientoLinea
                        {
                            CodigoCuenta = tx.CuentaSeleccionada!.Codigo,
                            DescripcionCuenta = tx.CuentaSeleccionada.Nombre,
                            Debe = 0,
                            Haber = tx.Credito
                        });
                    }
                    else
                    {
                        continue;
                    }

                    _contabilidadService.GuardarComprobante(cc);
                    importadas++;
                }

                MessageBox.Show($"Se han importado exitosamente {importadas} transacciones como comprobantes contables en estado 'Registrado'.", "Importación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                
                FilePath = string.Empty;
                RawText = string.Empty;
                Transacciones.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante la importación: {ex.Message}", "Error de Importación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancelar()
        {
            FilePath = string.Empty;
            RawText = string.Empty;
            Transacciones.Clear();
        }
    }
}
