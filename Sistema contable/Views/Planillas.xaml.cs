using System.Windows;

namespace Sistema_contable.Views
{
    public partial class Planillas : Window
    {
        public int TabInicial { get; set; } = 0;

        public Planillas()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var tab = this.FindName("TabPrincipal") as System.Windows.Controls.TabControl;
                if (tab != null && TabInicial >= 0 && TabInicial < tab.Items.Count)
                    tab.SelectedIndex = TabInicial;
            };
        }

        private void BtnAgregarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad: Agregar empleado a la planilla.", "Información",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEliminarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            if (GridSueldos.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un empleado para eliminar.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("¿Está seguro de eliminar este empleado de la planilla?",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        private void BtnCalcularSueldos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Calculando planilla de sueldos...", "Procesando",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnNuevoImpuesto_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad: Nueva declaración de impuesto.", "Información",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCalcularImpuestos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Calculando impuestos del período seleccionado...", "Procesando",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Planilla guardada exitosamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnGenerarAsiento_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Asiento contable generado para esta planilla.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
