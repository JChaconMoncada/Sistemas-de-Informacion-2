using System.Windows;

namespace Sistema_contable.Views
{
    public partial class GestionEmpresasWindow : Window
    {
        public GestionEmpresasWindow()
        {
            InitializeComponent();
            FrameContenido.Navigate(new Empresas());
        }
    }
}
