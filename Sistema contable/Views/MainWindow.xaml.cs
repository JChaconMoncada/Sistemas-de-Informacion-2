using System.Windows;

namespace Sistema_contable.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadPages();
        }

        private void LoadPages()
        {
            FrameDashboard.Navigate(new Dashboard());
            FrameComprobantes.Navigate(new Comprobantes());
            FrameEmpresas.Navigate(new Empresas());
            FramePlanCuentas.Navigate(new PlanCuentas());
            FrameLibroDiario.Navigate(new LibroDiario());
            FrameLibroMayor.Navigate(new LibroMayor());
            FrameInformes.Navigate(new Informes());
            FrameCobranza.Navigate(new Cobranza());
            FrameDocumentos.Navigate(new Documentos());
            FrameConfiguracion.Navigate(new Configuracion());
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 0;
        }

        private void BtnComprobantes_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
        }

        private void BtnLibros_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 4;
        }

        private void BtnInformes_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 5;
        }

        private void BtnCobranza_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 6;
        }

        private void BtnDocumentos_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 7;
        }
    }
}
