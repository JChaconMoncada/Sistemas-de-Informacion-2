using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Sistema_contable.Models;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class ConfiguracionViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;

        private decimal _porcentajeIva;
        private decimal _porcentajeIslr;
        private string _regimenFiscal;
        private string _monedaBase;
        private string _ejercicioFiscal;
        private bool _autoguardadoHabilitado;
        private bool _backupAutomaticoHabilitado;
        private bool _mostrarAlertasVencimientos;
        private bool _confirmarAntesDeEliminar;
        private string _ultimoBackupTexto = "Ningún backup realizado aún";
        private string _ultimaUbicacionBackup = "—";

        public ObservableCollection<string> RegimenesFiscales { get; } = new ObservableCollection<string>
        {
            "Ordinario", "Simplificado", "Especial"
        };

        public ObservableCollection<string> MonedasBase { get; } = new ObservableCollection<string>
        {
            "Bolívares (Bs.)", "Dólares (USD)"
        };

        public ObservableCollection<string> EjerciciosFiscales { get; } = new ObservableCollection<string>
        {
            "Enero - Diciembre", "Julio - Junio", "Octubre - Septiembre"
        };

        public ObservableCollection<BackupInfo> HistorialBackups { get; } = new ObservableCollection<BackupInfo>();

        public decimal PorcentajeIva
        {
            get => _porcentajeIva;
            set => SetProperty(ref _porcentajeIva, value);
        }

        public decimal PorcentajeIslr
        {
            get => _porcentajeIslr;
            set => SetProperty(ref _porcentajeIslr, value);
        }

        public string RegimenFiscal
        {
            get => _regimenFiscal;
            set => SetProperty(ref _regimenFiscal, value);
        }

        public string MonedaBase
        {
            get => _monedaBase;
            set => SetProperty(ref _monedaBase, value);
        }

        public string EjercicioFiscal
        {
            get => _ejercicioFiscal;
            set => SetProperty(ref _ejercicioFiscal, value);
        }

        public bool AutoguardadoHabilitado
        {
            get => _autoguardadoHabilitado;
            set => SetProperty(ref _autoguardadoHabilitado, value);
        }

        public bool BackupAutomaticoHabilitado
        {
            get => _backupAutomaticoHabilitado;
            set => SetProperty(ref _backupAutomaticoHabilitado, value);
        }

        public bool MostrarAlertasVencimientos
        {
            get => _mostrarAlertasVencimientos;
            set => SetProperty(ref _mostrarAlertasVencimientos, value);
        }

        public bool ConfirmarAntesDeEliminar
        {
            get => _confirmarAntesDeEliminar;
            set => SetProperty(ref _confirmarAntesDeEliminar, value);
        }

        public string UltimoBackupTexto
        {
            get => _ultimoBackupTexto;
            set => SetProperty(ref _ultimoBackupTexto, value);
        }

        public string UltimaUbicacionBackup
        {
            get => _ultimaUbicacionBackup;
            set => SetProperty(ref _ultimaUbicacionBackup, value);
        }

        public ICommand CrearBackupCommand { get; }
        public ICommand RestaurarBackupCommand { get; }
        public ICommand AbrirCarpetaBackupsCommand { get; }
        public ICommand GuardarConfiguracionCommand { get; }
        public ICommand RestaurarValoresPredeterminadosCommand { get; }
        public ICommand RestaurarDesdeHistorialCommand { get; }

        public ConfiguracionViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;

            CargarConfiguracion();
            CargarHistorialBackups();

            CrearBackupCommand = new RelayCommand(CrearBackup);
            RestaurarBackupCommand = new RelayCommand(RestaurarBackupDesdeArchivo);
            AbrirCarpetaBackupsCommand = new RelayCommand(AbrirCarpetaBackups);
            GuardarConfiguracionCommand = new RelayCommand(GuardarConfiguracion);
            RestaurarValoresPredeterminadosCommand = new RelayCommand(RestaurarValoresPredeterminados);
            RestaurarDesdeHistorialCommand = new RelayCommand<BackupInfo>(RestaurarDesdeHistorial);
        }

        private void CargarConfiguracion()
        {
            var config = _contabilidadService.ObtenerConfiguracion();
            _porcentajeIva = config.PorcentajeIva;
            _porcentajeIslr = config.PorcentajeIslr;
            _regimenFiscal = config.RegimenFiscal;
            _monedaBase = config.MonedaBase;
            _ejercicioFiscal = config.EjercicioFiscal;
            _autoguardadoHabilitado = config.AutoguardadoHabilitado;
            _backupAutomaticoHabilitado = config.BackupAutomaticoHabilitado;
            _mostrarAlertasVencimientos = config.MostrarAlertasVencimientos;
            _confirmarAntesDeEliminar = config.ConfirmarAntesDeEliminar;

            OnPropertyChanged(nameof(PorcentajeIva));
            OnPropertyChanged(nameof(PorcentajeIslr));
            OnPropertyChanged(nameof(RegimenFiscal));
            OnPropertyChanged(nameof(MonedaBase));
            OnPropertyChanged(nameof(EjercicioFiscal));
            OnPropertyChanged(nameof(AutoguardadoHabilitado));
            OnPropertyChanged(nameof(BackupAutomaticoHabilitado));
            OnPropertyChanged(nameof(MostrarAlertasVencimientos));
            OnPropertyChanged(nameof(ConfirmarAntesDeEliminar));
        }

        private void CargarHistorialBackups()
        {
            HistorialBackups.Clear();
            foreach (var b in _contabilidadService.ObtenerHistorialBackups())
            {
                HistorialBackups.Add(b);
            }

            if (HistorialBackups.Count > 0)
            {
                var ultimo = HistorialBackups[0];
                UltimoBackupTexto = ultimo.Fecha.ToString("dd/MM/yyyy HH:mm");
                UltimaUbicacionBackup = ultimo.RutaCompleta;
            }
            else
            {
                UltimoBackupTexto = "Ningún backup realizado aún";
                UltimaUbicacionBackup = "—";
            }
        }

        private void CrearBackup()
        {
            try
            {
                var ruta = _contabilidadService.EjecutarBackupManual();
                if (string.IsNullOrEmpty(ruta))
                {
                    System.Windows.MessageBox.Show("No se pudo crear el backup. Verifique permisos de la carpeta de datos.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                CargarHistorialBackups();
                System.Windows.MessageBox.Show($"Backup creado exitosamente en:\n{ruta}", "Backup Exitoso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al crear el backup: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void RestaurarBackupDesdeArchivo()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos de Backup (*.zip)|*.zip",
                Title = "Seleccionar backup a restaurar",
                InitialDirectory = _contabilidadService.ObtenerCarpetaBackups()
            };

            if (dialog.ShowDialog() != true) return;

            EjecutarRestauracion(dialog.FileName);
        }

        private void RestaurarDesdeHistorial(BackupInfo backup)
        {
            if (backup == null) return;
            EjecutarRestauracion(backup.RutaCompleta);
        }

        private void EjecutarRestauracion(string ruta)
        {
            var confirmacion = System.Windows.MessageBox.Show(
                "Restaurar un backup sobrescribirá los datos actuales (empresas, cuentas, comprobantes, etc.). ¿Desea continuar?",
                "Confirmar restauración", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);

            if (confirmacion != System.Windows.MessageBoxResult.Yes) return;

            try
            {
                _contabilidadService.RestaurarBackup(ruta);
                CargarConfiguracion();
                CargarHistorialBackups();
                System.Windows.MessageBox.Show("Backup restaurado correctamente. Es posible que deba reiniciar la aplicación para ver todos los cambios.", "Restauración Exitosa", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al restaurar el backup: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void AbrirCarpetaBackups()
        {
            try
            {
                var carpeta = _contabilidadService.ObtenerCarpetaBackups();
                Process.Start(new ProcessStartInfo(carpeta) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"No se pudo abrir la carpeta de backups: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void GuardarConfiguracion()
        {
            if (PorcentajeIva < 0 || PorcentajeIva > 100 || PorcentajeIslr < 0 || PorcentajeIslr > 100)
            {
                System.Windows.MessageBox.Show("Los porcentajes de IVA e ISLR deben estar entre 0 y 100.", "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            try
            {
                var config = new ConfiguracionSistema
                {
                    PorcentajeIva = PorcentajeIva,
                    PorcentajeIslr = PorcentajeIslr,
                    RegimenFiscal = RegimenFiscal,
                    MonedaBase = MonedaBase,
                    EjercicioFiscal = EjercicioFiscal,
                    AutoguardadoHabilitado = AutoguardadoHabilitado,
                    BackupAutomaticoHabilitado = BackupAutomaticoHabilitado,
                    MostrarAlertasVencimientos = MostrarAlertasVencimientos,
                    ConfirmarAntesDeEliminar = ConfirmarAntesDeEliminar
                };

                _contabilidadService.GuardarConfiguracion(config);
                System.Windows.MessageBox.Show("Configuración guardada correctamente.", "Guardado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al guardar la configuración: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void RestaurarValoresPredeterminados()
        {
            var confirmacion = System.Windows.MessageBox.Show(
                "¿Desea restaurar todos los valores de configuración a sus valores predeterminados?",
                "Confirmar", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (confirmacion != System.Windows.MessageBoxResult.Yes) return;

            var defaults = new ConfiguracionSistema();
            PorcentajeIva = defaults.PorcentajeIva;
            PorcentajeIslr = defaults.PorcentajeIslr;
            RegimenFiscal = defaults.RegimenFiscal;
            MonedaBase = defaults.MonedaBase;
            EjercicioFiscal = defaults.EjercicioFiscal;
            AutoguardadoHabilitado = defaults.AutoguardadoHabilitado;
            BackupAutomaticoHabilitado = defaults.BackupAutomaticoHabilitado;
            MostrarAlertasVencimientos = defaults.MostrarAlertasVencimientos;
            ConfirmarAntesDeEliminar = defaults.ConfirmarAntesDeEliminar;
        }
    }
}
