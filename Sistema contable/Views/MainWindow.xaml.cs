using System.Windows;

namespace Sistema_contable.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.MainWindowViewModel();
            MainTabControl.SelectedIndex = 0; // Default to Dashboard
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
            FrameReexpresion.Navigate(new Reexpresion());
            FrameBancos.Navigate(new Bancos());
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

        private void BtnEmpresas_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
        }

        private void BtnPlanCuentas_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
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

        private void BtnReexpresion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 8;
        }

        private void BtnBancos_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 9;
        }

        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 10;
        }

        // Menu Handlers
        private void MenuBackup_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 10;
            // Optionally, trigger backup logic on the Configuracion page
        }

        private void MenuRestore_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 10;
        }

        private void MenuSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuNuevaEmpresa_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
            if (FrameEmpresas.Content is Empresas page && page.DataContext is ViewModels.EmpresasViewModel vm)
            {
                if (vm.NuevaEmpresaCommand.CanExecute(null))
                {
                    vm.NuevaEmpresaCommand.Execute(null);
                }
            }
        }

        private void MenuEmpresas_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
        }

        private void MenuComprobante_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
        }

        private void MenuPlanCuentas_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
        }

        private void MenuLibroDiario_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 4;
            LibrosTabControl.SelectedIndex = 0;
        }

        private void MenuLibroMayor_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 4;
            LibrosTabControl.SelectedIndex = 1;
        }

        private void MenuInformes_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 5;
        }

        private void MenuReexpresion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 8;
        }

        private void MenuBancos_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 9;
        }

        private void MenuConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 10;
        }

        private void MenuAyuda_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Manual de Usuario:\n\nUse el menú superior o la barra de herramientas para navegar entre los módulos.\nSeleccione una empresa activa en el panel izquierdo para ver sus libros contables.", "Ayuda - Sistema Contable ERP", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuAcercaDe_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Oficina Contable Zulay Angola - Sistema Contable ERP\nVersión 1.0.0\n\nDesarrollado en WPF y C# con patrón MVVM.", "Acerca de - Sistema Contable ERP", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
