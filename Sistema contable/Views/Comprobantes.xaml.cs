using System.Windows;
using System.Windows.Controls;

namespace Sistema_contable.Views
{
    public partial class Comprobantes : Page
    {
        public Comprobantes()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.ComprobantesViewModel();
        }

        private void BtnActualizarComprobantes_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ActualizarComprobantes();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
        }

        private void BtnReversarComprobante_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ReversarComprobantes();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
        }
    }
}
