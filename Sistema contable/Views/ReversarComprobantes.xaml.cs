using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.Views
{
    public partial class ReversarComprobantes : Window
    {
        private readonly ContabilidadService _servicio = ContabilidadService.Instance;
        private ComprobanteContable _comprobanteSeleccionado;

        public ReversarComprobantes()
        {
            InitializeComponent();
            DtpFechaReversion.SelectedDate = DateTime.Today;
            CargarComprobantes();
        }

        private void CargarComprobantes()
        {
            CmbComprobante.Items.Clear();

            if (_servicio.EmpresaActivaId == null)
            {
                CmbComprobante.IsEnabled = false;
                return;
            }

            var comprobantes = _servicio.ObtenerComprobantesGuardados()
                .Where(c => c.Estado != "Reversado")
                .OrderByDescending(c => c.Fecha)
                .ToList();

            foreach (var c in comprobantes)
            {
                var item = new System.Windows.Controls.ComboBoxItem
                {
                    Content = $"#{c.IdComprobante} - {c.Fecha:dd/MM/yyyy} - {c.Descripcion}",
                    Tag = c.IdComprobante
                };
                CmbComprobante.Items.Add(item);
            }
        }

        private void CmbComprobante_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            GridOriginal.ItemsSource = null;
            GridContraAsiento.ItemsSource = null;
            _comprobanteSeleccionado = null;
            TxtFechaOriginal.Text = "";
            TxtDescripcionOriginal.Text = "";
        }

        private void BtnCargarComprobante_Click(object sender, RoutedEventArgs e)
        {
            if (CmbComprobante.SelectedItem is not System.Windows.Controls.ComboBoxItem item)
            {
                MessageBox.Show("Seleccione un comprobante.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int id = (int)item.Tag;
            _comprobanteSeleccionado = _servicio.ObtenerComprobantesGuardados()
                .FirstOrDefault(c => c.IdComprobante == id);

            if (_comprobanteSeleccionado == null) return;

            TxtFechaOriginal.Text = _comprobanteSeleccionado.Fecha.ToString("dd/MM/yyyy");
            TxtDescripcionOriginal.Text = _comprobanteSeleccionado.Descripcion;
            GridOriginal.ItemsSource = _comprobanteSeleccionado.Lineas;

            var preview = _comprobanteSeleccionado.Lineas.Select(l => new AsientoLinea
            {
                CodigoCuenta = l.CodigoCuenta,
                DescripcionCuenta = $"Rev. - {l.DescripcionCuenta}",
                Debe = l.Haber,
                Haber = l.Debe
            }).ToList();
            GridContraAsiento.ItemsSource = preview;
        }

        private void BtnVistaPrevia_Click(object sender, RoutedEventArgs e)
        {
            if (_comprobanteSeleccionado == null)
            {
                MessageBox.Show("Cargue un comprobante primero con el botón 'Cargar'.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("La vista previa del contra-asiento se muestra en la parte inferior de la ventana.",
                "Vista Previa", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReversarComprobante_Click(object sender, RoutedEventArgs e)
        {
            if (_comprobanteSeleccionado == null)
            {
                MessageBox.Show("Seleccione y cargue un comprobante para reversar.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DtpFechaReversion.SelectedDate == null)
            {
                MessageBox.Show("Indique la fecha de reversión.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtMotivoReversion.Text))
            {
                MessageBox.Show("Ingrese el motivo de la reversión.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"¿Confirma la reversión del comprobante #{_comprobanteSeleccionado.IdComprobante}?\n\nMotivo: {TxtMotivoReversion.Text}\n\nEsta acción no puede deshacerse.",
                "Confirmar Reversión", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var contra = _servicio.ReversarComprobante(
                        _comprobanteSeleccionado.IdComprobante,
                        DtpFechaReversion.SelectedDate.Value,
                        TxtMotivoReversion.Text);

                    MessageBox.Show(
                        $"Reversión generada exitosamente.\nNuevo comprobante de reversión: #{contra.IdComprobante}",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al reversar: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
