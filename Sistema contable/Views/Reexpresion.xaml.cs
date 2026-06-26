using System.Windows;
using System.Windows.Controls;
using Sistema_contable.ViewModels;

namespace Sistema_contable.Views
{
    public partial class Reexpresion : Page
    {
        private bool _modoReconversion2021 = false;

        public Reexpresion()
        {
            InitializeComponent();
            DataContext = new ReexpresionViewModel();
        }

        public void ActivarModoReconversion2021()
        {
            CmbModo.SelectedIndex = 1;
        }

        private void CmbModo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (CmbModo.SelectedIndex == 1)
            {
                _modoReconversion2021 = true;
                TxtTitulo.Text = "Reconversión Monetaria 2021";
                TxtSubtitulo.Text = "Reconversión según el Decreto N° 4.553 del BCV: 1.000.000 Bs. S equivalen a 1 Bs. D (Bolívar Digital).";
                PanelAviso2021.Visibility = Visibility.Visible;

                DtpFechaOrigen.SelectedDate = new System.DateTime(2021, 1, 1);
                DtpFechaDestino.SelectedDate = new System.DateTime(2021, 10, 1);
                TxtIpcOrigen.Text = "1,000,000";
                TxtIpcDestino.Text = "1";
                TxtFactor.Text = "0.000001";
                TxtLabelFactor.Text = "Factor de Reconversión (Decreto BCV):";
                TxtFormula.Text = "1 Bs. Digital = 1.000.000 Bs. Soberanos";

                DtpFechaOrigen.IsEnabled = false;
                DtpFechaDestino.IsEnabled = false;
                TxtIpcOrigen.IsEnabled = false;
                TxtIpcDestino.IsEnabled = false;
            }
            else
            {
                _modoReconversion2021 = false;
                TxtTitulo.Text = "Reexpresión Monetaria (Ajuste por Inflación)";
                TxtSubtitulo.Text = "Asistente para ajustar los valores contables según el Índice de Precios al Consumidor (IPC) del BCV";
                PanelAviso2021.Visibility = Visibility.Collapsed;

                DtpFechaOrigen.SelectedDate = null;
                DtpFechaDestino.SelectedDate = null;
                TxtIpcOrigen.Text = "1,250.50";
                TxtIpcDestino.Text = "1,580.75";
                TxtFactor.Text = "1.2642";
                TxtLabelFactor.Text = "Factor de Reexpresión Calculado:";
                TxtFormula.Text = "(IPC Destino / IPC Origen)";

                DtpFechaOrigen.IsEnabled = true;
                DtpFechaDestino.IsEnabled = true;
                TxtIpcOrigen.IsEnabled = true;
                TxtIpcDestino.IsEnabled = true;
            }
        }

        private void BtnCalcularFactor_Click(object sender, RoutedEventArgs e)
        {
            if (_modoReconversion2021)
            {
                MessageBox.Show("Factor preconfigurado: 0.000001\n(1 Bs. Digital = 1.000.000 Bs. Soberanos)",
                    "Factor Reconversión 2021", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (decimal.TryParse(TxtIpcOrigen.Text.Replace(",", ""), out decimal ipcOrigen) &&
                decimal.TryParse(TxtIpcDestino.Text.Replace(",", ""), out decimal ipcDestino) &&
                ipcOrigen != 0)
            {
                decimal factor = ipcDestino / ipcOrigen;
                TxtFactor.Text = factor.ToString("F4");
            }
            else
            {
                MessageBox.Show("Ingrese valores válidos para IPC Origen e IPC Destino.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
