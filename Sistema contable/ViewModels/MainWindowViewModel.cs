using System;
using Sistema_contable.Models;
using System.Collections.ObjectModel;
using SistemaContableZulay.UI.Services;
using System.Linq;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;
using System.Diagnostics;

namespace Sistema_contable.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;
        private EmpresaCliente _empresaSeleccionada;
        private string _textoEstado;
        private string _ultimoBackupStatus = "Backup: No realizado";

        public ObservableCollection<EmpresaCliente> Empresas { get; set; }

        public string UltimoBackupStatus
        {
            get => _ultimoBackupStatus;
            set => SetProperty(ref _ultimoBackupStatus, value);
        }

        public ICommand RealizarBackupCommand { get; }
        public ICommand RestaurarBackupCommand { get; }
        public ICommand AbrirBackupsCommand { get; }
        public ICommand EliminarEmpresaCommand { get; }

        public EmpresaCliente EmpresaSeleccionada
        {
            get => _empresaSeleccionada;
            set
            {
                if (SetProperty(ref _empresaSeleccionada, value))
                {
                    if (value != null)
                    {
                        _contabilidadService.SeleccionarEmpresa(value.Id);
                        TextoEstado = $"Empresa: {value.NombreEmpresa} (RIF: {value.Rif})";
                    }
                    else
                    {
                        _contabilidadService.SeleccionarEmpresa(null);
                        TextoEstado = "Empresa: Sin empresa seleccionada";
                    }
                }
            }
        }

        public string TextoEstado
        {
            get => _textoEstado;
            set => SetProperty(ref _textoEstado, value);
        }

        public MainWindowViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Empresas = new ObservableCollection<EmpresaCliente>();
            TextoEstado = "Empresa: Sin empresa seleccionada";

            RealizarBackupCommand = new RelayCommand(RealizarBackup);
            RestaurarBackupCommand = new RelayCommand(RestaurarBackup);
            AbrirBackupsCommand = new RelayCommand(AbrirCarpetaBackups);
            EliminarEmpresaCommand = new RelayCommand(EliminarEmpresaSeleccionada);

            _contabilidadService.OnEmpresasModificadas += CargarEmpresas;
            _contabilidadService.OnDatosModificados += CargarEmpresas;
            _contabilidadService.OnDatosModificados += ActualizarEstadoBackup;

            CargarEmpresas();
            ActualizarEstadoBackup();
        }

        private void EliminarEmpresaSeleccionada()
        {
            if (EmpresaSeleccionada == null)
            {
                System.Windows.MessageBox.Show("Por favor, seleccione una empresa para eliminar.", "Información", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"¿Está seguro de que desea eliminar la empresa '{EmpresaSeleccionada.NombreEmpresa}'?\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    _contabilidadService.EliminarEmpresa(EmpresaSeleccionada.Id);
                    EmpresaSeleccionada = null;
                    CargarEmpresas();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error al eliminar la empresa: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ActualizarEstadoBackup()
        {
            var backups = _contabilidadService.ObtenerHistorialBackups();
            if (backups.Count > 0)
            {
                UltimoBackupStatus = $"Backup: {backups[0].Fecha:dd/MM/yyyy HH:mm}";
            }
            else
            {
                UltimoBackupStatus = "Backup: No realizado";
            }
        }

        private void RealizarBackup()
        {
            try
            {
                var ruta = _contabilidadService.EjecutarBackupManual();
                if (!string.IsNullOrEmpty(ruta))
                {
                    System.Windows.MessageBox.Show($"Backup creado exitosamente en:\n{ruta}", "Backup Exitoso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    ActualizarEstadoBackup();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al crear backup: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void RestaurarBackup()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos de Backup (*.zip)|*.zip",
                Title = "Seleccionar backup a restaurar",
                InitialDirectory = _contabilidadService.ObtenerCarpetaBackups()
            };

            if (dialog.ShowDialog() == true)
            {
                var confirmacion = System.Windows.MessageBox.Show(
                    "Restaurar un backup sobrescribirá los datos actuales. ¿Desea continuar?",
                    "Confirmar restauración", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);

                if (confirmacion == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        _contabilidadService.RestaurarBackup(dialog.FileName);
                        ActualizarEstadoBackup();
                        System.Windows.MessageBox.Show("Backup restaurado correctamente.", "Restauración Exitosa", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error al restaurar backup: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CargarEmpresas()
        {
            var empresas = _contabilidadService.ObtenerEmpresas();
            Empresas.Clear();
            foreach (var e in empresas)
            {
                Empresas.Add(e);
            }

            // Restore selection if there's an active company
            if (_contabilidadService.EmpresaActivaId.HasValue)
            {
                _empresaSeleccionada = Empresas.FirstOrDefault(e => e.Id == _contabilidadService.EmpresaActivaId.Value);
                OnPropertyChanged(nameof(EmpresaSeleccionada));
            }
        }

        public void AbrirCarpetaBackups()
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
    }
}
