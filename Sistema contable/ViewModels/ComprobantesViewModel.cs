using Sistema_contable.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using SistemaContableZulay.UI.Services;
using SistemaContableZulay.UI.Domain;

namespace Sistema_contable.ViewModels
{
    public class ComprobantesViewModel : ViewModelBase
    {
        private Asiento _asientoActual;
        private decimal _totalDebe;
        private decimal _totalHaber;
        private bool _estaCuadrado;
        private DetalleAsiento _selectedDetalle;
        private readonly ContabilidadService _contabilidadService;

        public Asiento AsientoActual
        {
            get => _asientoActual;
            set => SetProperty(ref _asientoActual, value);
        }

        public ObservableCollection<DetalleAsiento> Detalles { get; set; }

        public decimal TotalDebe
        {
            get => _totalDebe;
            set => SetProperty(ref _totalDebe, value);
        }

        public decimal TotalHaber
        {
            get => _totalHaber;
            set => SetProperty(ref _totalHaber, value);
        }

        public bool EstaCuadrado
        {
            get => _estaCuadrado;
            set => SetProperty(ref _estaCuadrado, value);
        }

        public DetalleAsiento SelectedDetalle
        {
            get => _selectedDetalle;
            set => SetProperty(ref _selectedDetalle, value);
        }

        public ICommand GuardarCommand { get; }
        public ICommand AgregarLineaCommand { get; }
        public ICommand EliminarLineaCommand { get; }
        public ICommand NuevoAsientoCommand { get; }
        public ICommand CancelarCommand { get; }

        public ComprobantesViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;

            Detalles = new ObservableCollection<DetalleAsiento>();
            Detalles.CollectionChanged += Detalles_CollectionChanged;

            GuardarCommand = new RelayCommand(() => GuardarComprobante(), () => Detalles.Count > 0);
            AgregarLineaCommand = new RelayCommand(() => AgregarLinea());
            EliminarLineaCommand = new RelayCommand(() => EliminarLinea(), () => SelectedDetalle != null);
            NuevoAsientoCommand = new RelayCommand(() => ReiniciarFormulario());
            CancelarCommand = new RelayCommand(() => ReiniciarFormulario());

            ReiniciarFormulario();
        }

        private void Detalles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (DetalleAsiento item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (DetalleAsiento item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }

            RecalcularTotales();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DetalleAsiento.Debe) ||
                e.PropertyName == nameof(DetalleAsiento.Haber))
            {
                RecalcularTotales();
            }
        }

        private void RecalcularTotales()
        {
            TotalDebe = Detalles.Sum(d => d.Debe);
            TotalHaber = Detalles.Sum(d => d.Haber);
            EstaCuadrado = TotalDebe == TotalHaber && TotalDebe > 0;
        }

        private void AgregarLinea()
        {
            Detalles.Add(new DetalleAsiento());
        }

        private void EliminarLinea()
        {
            if (SelectedDetalle != null)
            {
                Detalles.Remove(SelectedDetalle);
            }
        }

        private void ReiniciarFormulario()
        {
            AsientoActual = new Asiento
            {
                Fecha = DateTime.Now,
                Numero = ObtenerSiguienteNumero(),
                Tipo = "Manual"
            };

            Detalles.Clear();
            AgregarLinea();
            AgregarLinea();
        }

        private int ObtenerSiguienteNumero()
        {
            var comprobantes = _contabilidadService.ObtenerComprobantesGuardados();
            return comprobantes.Count > 0
                ? comprobantes.Max(c => c.IdComprobante) + 1
                : 1;
        }

        private void GuardarComprobante()
        {
            if (!EstaCuadrado)
            {
                System.Windows.MessageBox.Show(
                    "El comprobante está descuadrado. El Debe y el Haber deben ser iguales y mayores a cero.",
                    "Advertencia - Cuadre Contable",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                return;
            }

            var cc = new ComprobanteContable
            {
                Fecha = AsientoActual.Fecha,
                Descripcion = AsientoActual.Descripcion ?? "",
                TipoComprobante = AsientoActual.Tipo ?? ""
            };

            foreach (var d in Detalles)
            {
                cc.Lineas.Add(new AsientoLinea
                {
                    CodigoCuenta = d.CodigoCuenta ?? "",
                    Debe = d.Debe,
                    Haber = d.Haber,
                    DescripcionCuenta = d.NombreCuenta ?? d.Descripcion ?? ""
                });
            }

            try
            {
                _contabilidadService.GuardarComprobante(cc);

                System.Windows.MessageBox.Show(
                    "El comprobante contable se guardó con éxito.",
                    "Guardado Exitoso",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                ReiniciarFormulario();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error al guardar: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}