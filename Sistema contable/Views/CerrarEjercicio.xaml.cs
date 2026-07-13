using System;
using System.Windows;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.Views
{
    public partial class CerrarEjercicio : Window
    {
        private readonly ContabilidadService _servicio = ContabilidadService.Instance;

        public CerrarEjercicio()
        {
            InitializeComponent();
            Loaded += CerrarEjercicio_Loaded;
        }

        private void CerrarEjercicio_Loaded(object sender, RoutedEventArgs e)
        {
            CmbEjercicio.Items.Clear();
            int anioActual = DateTime.Today.Year;
            for (int i = anioActual; i >= anioActual - 5; i--)
            {
                var item = new System.Windows.Controls.ComboBoxItem
                {
                    Content = i.ToString(),
                    Tag = i
                };
                if (_servicio.EjercicioCerrado(i))
                    item.Content = $"{i} (CERRADO)";

                CmbEjercicio.Items.Add(item);
            }
            if (CmbEjercicio.Items.Count > 0)
                CmbEjercicio.SelectedIndex = 0;
        }

        private void CmbEjercicio_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CmbEjercicio.SelectedItem is System.Windows.Controls.ComboBoxItem item && item.Tag is int anio)
            {
                TxtFechaInicio.Text = $"01/01/{anio}";
                TxtFechaFin.Text = $"31/12/{anio}";

                if (_servicio.EmpresaActivaId != null)
                {
                    var resumen = _servicio.ObtenerResumenEjercicio(anio);
                    TxtTotalIngresos.Text = $"Bs. {resumen.TotalIngresos:N2}";
                    TxtTotalGastos.Text = $"Bs. {resumen.TotalEgresos:N2}";
                    TxtResultado.Text = $"Bs. {resumen.Resultado:N2}";
                    TxtResultado.Foreground = resumen.Resultado >= 0
                        ? System.Windows.Media.Brushes.Green
                        : System.Windows.Media.Brushes.Red;
                }
            }
        }

        private void BtnCalcular_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio.EmpresaActivaId == null)
            {
                MessageBox.Show("Seleccione una empresa activa desde el panel principal.", "Sin empresa activa",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbEjercicio.SelectedItem is not System.Windows.Controls.ComboBoxItem item || item.Tag is not int anio)
                return;

            if (_servicio.EjercicioCerrado(anio))
            {
                MessageBox.Show($"El ejercicio {anio} ya fue cerrado.", "Ejercicio cerrado",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resumen = _servicio.ObtenerResumenEjercicio(anio);
            MessageBox.Show(
                $"Resumen del ejercicio {anio}:\n\n" +
                $"  Total Ingresos: Bs. {resumen.TotalIngresos:N2}\n" +
                $"  Total Egresos:  Bs. {resumen.TotalEgresos:N2}\n" +
                $"  Resultado:      Bs. {resumen.Resultado:N2}\n\n" +
                "Se generarán los asientos de cierre correspondientes.\nHaga clic en 'Cerrar Ejercicio' para confirmar.",
                "Cálculo de Cierre", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCerrarEjercicio_Click(object sender, RoutedEventArgs e)
        {
            if (_servicio.EmpresaActivaId == null)
            {
                MessageBox.Show("Seleccione una empresa activa desde el panel principal.", "Sin empresa activa",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ChkConfirmacion.IsChecked != true)
            {
                MessageBox.Show("Debe marcar la casilla de confirmación para continuar.", "Confirmación requerida",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbEjercicio.SelectedItem is not System.Windows.Controls.ComboBoxItem item || item.Tag is not int anio)
                return;

            var result = MessageBox.Show(
                $"¿Confirma el cierre del ejercicio fiscal {anio}?\n\nSe generarán los asientos de cierre y el período quedará bloqueado.\nEsta acción es irreversible.",
                "Confirmar Cierre de Ejercicio",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _servicio.CerrarEjercicio(anio);
                    MessageBox.Show($"Ejercicio {anio} cerrado exitosamente.\nLos asientos de cierre han sido generados y el período ha sido bloqueado.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CerrarEjercicio_Loaded(null, null);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cerrar el ejercicio: {ex.Message}", "Error",
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
