using System.Windows.Controls;

namespace Sistema_contable.Views
{
    public partial class PlanCuentas : Page
    {
        public PlanCuentas()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.PlanCuentasViewModel();
        }

        private void TreeCuentas_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.DataContext is ViewModels.PlanCuentasViewModel viewModel)
            {
                viewModel.CuentaSeleccionada = e.NewValue as SistemaContableZulay.UI.Domain.CuentaContable;
            }
        }
    }
}
