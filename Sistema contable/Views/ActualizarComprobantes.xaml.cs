using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.Views
{
    public partial class ActualizarComprobantes : Window
    {
        private readonly ContabilidadService _servicio = ContabilidadService.Instance;
        private ObservableCollection<ComprobanteContable> _comprobantes = new();

        public ActualizarComprobantes()
        {
            InitializeComponent();
            GridComprobantes.ItemsSource = _comprobantes;
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio.EmpresaActivaId == null)
            {
                MessageBox.Show("Seleccione una empresa activa desde el panel principal.", "Sin empresa activa",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tipo = (CmbTipo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Todos";
            var lista = _servicio.ObtenerComprobantesParaActualizar(DtpDesde.SelectedDate, DtpHasta.SelectedDate, tipo);

            _comprobantes.Clear();
            foreach (var c in lista)
                _comprobantes.Add(c);

            if (_comprobantes.Count == 0)
                MessageBox.Show("No se encontraron comprobantes con los filtros indicados.", "Sin resultados",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSeleccionarTodos_Click(object sender, RoutedEventArgs e)
        {
            GridComprobantes.SelectAll();
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            bool recalcular = ChkRecalcularSaldos.IsChecked == true;
            bool verificar = ChkVerificarIntegridad.IsChecked == true;

            if (!recalcular && !verificar && ChkActualizarReferencias.IsChecked != true)
            {
                MessageBox.Show("Seleccione al menos una opción de actualización.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_comprobantes.Count == 0)
            {
                MessageBox.Show("Primero realice una búsqueda de comprobantes.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PanelProgreso.Visibility = Visibility.Visible;
            int procesados = 0;

            try
            {
                foreach (var comp in _comprobantes)
                {
                    if (verificar && comp.Estado == "Pendiente de Validación")
                    {
                        _servicio.ActualizarEstadoComprobante(comp.IdComprobante, "Validado");
                    }
                    procesados++;
                    BarraProgreso.Value = (procesados * 100) / _comprobantes.Count;
                }

                TxtProgreso.Text = $"Actualización completada: {procesados} comprobante(s) procesado(s).";
                MessageBox.Show($"Se actualizaron {procesados} comprobante(s) correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                BtnBuscar_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
