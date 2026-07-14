using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sistema_contable.Models;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class HistorialCuentaViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;
        private readonly string _codigoCuenta;
        private readonly string _nombreCuenta;

        public ObservableCollection<HistorialReexpresion> Historiales { get; } = new ObservableCollection<HistorialReexpresion>();

        public string Titulo => $"Historial de Reexpresiones: {_codigoCuenta} - {_nombreCuenta}";

        private HistorialReexpresion _historialSeleccionado;
        public HistorialReexpresion HistorialSeleccionado
        {
            get => _historialSeleccionado;
            set => SetProperty(ref _historialSeleccionado, value);
        }

        public ICommand DeshacerCommand { get; }
        public ICommand CerrarCommand { get; }

        public Action CloseAction { get; set; }
        public Action OnHistorialAlterado { get; set; }

        public HistorialCuentaViewModel(string codigoCuenta, string nombreCuenta)
        {
            _contabilidadService = ContabilidadService.Instance;
            _codigoCuenta = codigoCuenta;
            _nombreCuenta = nombreCuenta;

            DeshacerCommand = new RelayCommand(Deshacer, () => HistorialSeleccionado != null);
            CerrarCommand = new RelayCommand(() => CloseAction?.Invoke());

            CargarHistorial();
        }

        private void CargarHistorial()
        {
            Historiales.Clear();
            var todos = _contabilidadService.ObtenerHistorialReexpresiones();
            var filtrados = todos.Where(h => h.CodigoCuenta == _codigoCuenta).OrderByDescending(h => h.FechaCalculo).ToList();
            
            foreach (var h in filtrados)
            {
                Historiales.Add(h);
            }
        }

        private void Deshacer()
        {
            if (HistorialSeleccionado == null) return;

            var result = MessageBox.Show($"¿Desea restaurar esta reexpresión y revertir su ajuste contable?", 
                                         "Confirmar Restauración", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _contabilidadService.RestaurarReexpresion(HistorialSeleccionado.Id);
                MessageBox.Show("Reexpresión revertida correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarHistorial();
                OnHistorialAlterado?.Invoke();
            }
        }
    }
}
