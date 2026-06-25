using System.Windows.Controls;

namespace Sistema_contable.Views
{
    public partial class Dashboard : Page
    {
        public Dashboard()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.DashboardViewModel();
        }
    }
}
