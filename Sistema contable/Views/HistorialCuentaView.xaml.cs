using System.Windows;
using Sistema_contable.ViewModels;

namespace Sistema_contable.Views
{
    public partial class HistorialCuentaView : Window
    {
        public HistorialCuentaView(HistorialCuentaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
