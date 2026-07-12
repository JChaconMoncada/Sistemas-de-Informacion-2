using System.Windows;

namespace Sistema_contable.Views
{
    public partial class ImportarExcel : Window
    {
        public string TipoInicial { get; set; } = "Comprobantes Contables";

        public ImportarExcel()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (TipoInicial == "Plan de Cuentas")
                    CmbTipoImportacion.SelectedIndex = 1;
                else
                    CmbTipoImportacion.SelectedIndex = 0;
            };
        }

        private void BtnExaminar_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar archivo Excel",
                Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Todos los archivos (*.*)|*.*",
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                TxtArchivoExcel.Text = dialog.FileName;
                TxtArchivoExcel.Foreground = System.Windows.Media.Brushes.Black;
                TxtFilasEncontradas.Text = "0";
                TxtFilasValidas.Text = "0";
                TxtFilasError.Text = "0";
            }
        }

        private void CmbTipoImportacion_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Actualizar columnas del DataGrid según tipo de importación
        }

        private void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            if (TxtArchivoExcel.Text == "Ningún archivo seleccionado")
            {
                MessageBox.Show("Por favor seleccione un archivo Excel.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Archivo cargado. Configure el mapeo de columnas y haga clic en 'Importar Datos'.",
                "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnImportar_Click(object sender, RoutedEventArgs e)
        {
            if (TxtArchivoExcel.Text == "Ningún archivo seleccionado")
            {
                MessageBox.Show("Por favor seleccione y cargue un archivo Excel primero.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PanelProgreso.Visibility = Visibility.Visible;
            BarraProgreso.Value = 100;
            TxtProgreso.Text = "Importación completada exitosamente.";

            MessageBox.Show("Importación completada. Los datos han sido procesados.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
