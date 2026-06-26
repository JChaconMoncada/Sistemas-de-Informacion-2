using System.Windows.Controls;
using Sistema_contable.ViewModels;

namespace Sistema_contable.Views
{
    public partial class Movimientos : Page
    {
        public Movimientos()
        {
            InitializeComponent();
            DataContext = new MovimientosViewModel();
        }
    }
}
