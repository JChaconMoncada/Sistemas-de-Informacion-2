using System.Windows.Controls;

namespace Sistema_contable.Views
{
    public partial class LibroMayor : Page
    {
        public LibroMayor()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.LibroMayorViewModel();
        }
    }
}
