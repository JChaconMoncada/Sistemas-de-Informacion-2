using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;

namespace Sistema_contable.Views
{
    public partial class Configuracion : Page
    {
        public Configuracion()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.ConfiguracionViewModel();
        }
    }
}
