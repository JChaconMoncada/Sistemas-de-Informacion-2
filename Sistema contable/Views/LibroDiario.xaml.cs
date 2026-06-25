using System.Windows.Controls;

namespace Sistema_contable.Views
{
    public partial class LibroDiario : Page
    {
        public LibroDiario()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.LibroDiarioViewModel();
        }
    }
}
