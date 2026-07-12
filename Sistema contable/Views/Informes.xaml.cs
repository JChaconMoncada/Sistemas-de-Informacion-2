using System.Windows.Controls;

namespace Sistema_contable.Views
{
    public partial class Informes : Page
    {
        public Informes()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.InformesViewModel();
        }
    }
}
