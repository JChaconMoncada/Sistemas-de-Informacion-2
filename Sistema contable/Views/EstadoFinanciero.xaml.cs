using System.Windows.Controls;

namespace Sistema_contable.Views
{
    public partial class EstadoFinanciero : Page
    {
        public EstadoFinanciero()
        {
            InitializeComponent();
            this.IsVisibleChanged += (s, e) =>
            {
                if (this.IsVisible && this.DataContext is ViewModels.EstadoFinancieroViewModel vm)
                {
                    if (vm.GenerarReporteCommand.CanExecute(null))
                        vm.GenerarReporteCommand.Execute(null);
                }
            };
        }

        private void DatePicker_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is DatePicker dp)
            {
                if (System.DateTime.TryParse(dp.Text, out System.DateTime date))
                {
                    dp.SelectedDate = date;
                }
            }
        }
    }
}
