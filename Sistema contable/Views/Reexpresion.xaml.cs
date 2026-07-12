using System.Windows;
using System.Windows.Controls;
using Sistema_contable.ViewModels;

namespace Sistema_contable.Views
{
    public partial class Reexpresion : Page
    {
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

            bool esReconversion = CmbModo.SelectedIndex == 1;

            if (DataContext is ReexpresionViewModel vm)
                vm.SetModoReconversion2021(esReconversion);

            TxtTitulo.Text = esReconversion
                ? "Reconversión Monetaria 2021"
                : "Reexpresión Monetaria (Ajuste por Inflación)";

            TxtSubtitulo.Text = esReconversion
                ? "Reconversión según el Decreto N° 4.553 del BCV: 1.000.000 Bs. S equivalen a 1 Bs. D (Bolívar Digital)."
                : "Asistente para ajustar los valores contables según el Índice de Precios al Consumidor (IPC) del BCV";

            PanelAviso2021.Visibility = esReconversion ? Visibility.Visible : Visibility.Collapsed;
            GrpParametros.IsEnabled = !esReconversion;
        }
    }
}
