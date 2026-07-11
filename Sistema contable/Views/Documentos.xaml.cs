using System.Windows.Controls;

namespace Sistema_contable.Views
{
    public partial class Documentos : Page
    {
        public Documentos()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.DocumentosViewModel();
        }
    }
}
