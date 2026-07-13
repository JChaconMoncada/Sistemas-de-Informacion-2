using System.Windows;
using System.Windows.Controls;

namespace Sistema_contable.Views
{
    public partial class Dashboard : Page
    {
        public Dashboard()
        {
            InitializeComponent();
            DataContext = new ViewModels.DashboardViewModel();
        }

        private MainWindow? GetMain() => Window.GetWindow(this) as MainWindow;

        private void BtnNuevoComprobante_Click(object sender, RoutedEventArgs e)
        {
            if (GetMain() is MainWindow main)
                main.MainTabControl.SelectedIndex = 1;
        }

        private void BtnNuevoMovimiento_Click(object sender, RoutedEventArgs e)
        {
            if (GetMain() is MainWindow main)
                main.MainTabControl.SelectedIndex = 2;
        }

        private void BtnVerLibroDiario_Click(object sender, RoutedEventArgs e)
        {
            if (GetMain() is MainWindow main)
            {
                main.MainTabControl.SelectedIndex = 4;
                main.LibrosTabControl.SelectedIndex = 0;
            }
        }

        private void BtnVerInformes_Click(object sender, RoutedEventArgs e)
        {
            if (GetMain() is MainWindow main)
                main.MainTabControl.SelectedIndex = 5;
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
            => GetMain()?.MenuBackup_Click(sender, e);

        private void BtnImportarBancos_Click(object sender, RoutedEventArgs e)
        {
            if (GetMain() is MainWindow main)
                main.MainTabControl.SelectedIndex = 9;
        }

    }
}
