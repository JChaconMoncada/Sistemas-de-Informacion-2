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

        private void BtnGestionarEmpresas_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new GestionEmpresasWindow();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void BtnEmpresas_Click(object sender, RoutedEventArgs e)
        {
            BtnGestionarEmpresas_Click(sender, e);
        }

        private void BtnPlanCuentas_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
        }

        private void BtnLibros_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
        }

        private void BtnInformes_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 4;
        }

        private void BtnCobranza_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 5;
        }

        private void BtnDocumentos_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 6;
        }

        private void BtnReexpresion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 7;
        }

        private void BtnBancos_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 8;
        }

        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 9;
        }

        // Menu Handlers
        internal void MenuBackup_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 9;
        }

        private void MenuRestore_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 9;
        }

        private void MenuSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
            MainTabControl.SelectedIndex = 2;
        }

        private void MenuLibroDiario_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
            LibrosTabControl.SelectedIndex = 0;
        }

        private void MenuLibroMayor_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
            LibrosTabControl.SelectedIndex = 1;
        }

        private void MenuInformes_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 4;
        }

        private void MenuReexpresion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 7;
        }

        private void MenuBancos_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 8;
        }

        private void MenuConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 9;
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
            MainTabControl.SelectedIndex = 7;
            if (FrameReexpresion.Content is Reexpresion page)
            {
                page.ActivarModoReconversion2021();
            }
        }

        // ─── Ayuda ───────────────────────────────────────────────────────────────
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
