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

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
            System.Environment.Exit(0);
        }

        private void LoadPages()
        {
            FrameDashboard.Navigate(new Dashboard());
            FrameComprobantes.Navigate(new Comprobantes());
            FrameMovimientos.Navigate(new Movimientos());
            FramePlanCuentas.Navigate(new PlanCuentas());
            FrameLibroDiario.Navigate(new LibroDiario());
            FrameLibroMayor.Navigate(new LibroMayor());
            FrameEstadoFinanciero.Navigate(new EstadoFinanciero());
            FrameInformes.Navigate(new Informes());
            FrameCobranza.Navigate(new Cobranza());
            FrameDocumentos.Navigate(new Documentos());
            FrameReexpresion.Navigate(new Reexpresion());
            FrameBancos.Navigate(new Bancos());
            FrameConfiguracion.Navigate(new Configuracion());
            FrameAyuda.Navigate(new Ayuda());
        }

        private void BtnGestionarEmpresas_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new GestionEmpresasWindow();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        // Menu Handlers
        internal void MenuBackup_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 11;
        }

        private void MenuRestore_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 11;
        }

        private void MenuSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuAbrirCarpetaBackups_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModels.MainWindowViewModel;
            if (viewModel != null)
            {
                viewModel.AbrirCarpetaBackups();
            }
        }

        private void MenuNuevaEmpresa_Click(object sender, RoutedEventArgs e)
        {
            BtnGestionarEmpresas_Click(sender, e);
        }

        private void MenuEmpresas_Click(object sender, RoutedEventArgs e)
        {
            BtnGestionarEmpresas_Click(sender, e);
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

        private void MenuEstadoFinanciero_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 5;
        }

        private void MenuInformes_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 6;
        }

        private void MenuReexpresion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 9;
        }

        private void MenuBancos_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 10;
        }

        private void MenuConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 11;
        }

        // ─── Menú Procesos ───────────────────────────────────────────────────────
        private void MenuActualizarComprobantes_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ActualizarComprobantes();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MenuReversarComprobante_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ReversarComprobantes();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MenuImportarExcelComprobantes_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ImportarExcel();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MenuImportarExcelPlanCuentas_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ImportarExcel();
            ventana.TipoInicial = "Plan de Cuentas";
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MenuPlanillasSueldos_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new Planillas();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MenuPlanillasImpuestos_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new Planillas();
            ventana.TabInicial = 1;
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MenuCerrarEjercicio_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CerrarEjercicio();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void MenuReconversion2021_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("El modo de reconversión ha sido deshabilitado.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ─── Ayuda ───────────────────────────────────────────────────────────────
        private void MenuAyuda_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 12;
        }

        private void MenuAcercaDe_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Oficina Contable Zulay Angola - Sistema Contable ERP\nVersión 1.0.0\n\nDesarrollado en WPF y C# con patrón MVVM.", "Acerca de - Sistema Contable ERP", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
