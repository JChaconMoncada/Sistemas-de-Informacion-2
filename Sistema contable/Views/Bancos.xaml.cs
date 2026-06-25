using System.ComponentModel;
using System.Windows.Controls;
using Sistema_contable.ViewModels;

namespace Sistema_contable.Views
{
    public partial class Bancos : Page
    {
        public Bancos()
        {
            InitializeComponent();
            var vm = new BancosViewModel();
            this.DataContext = vm;
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private async void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BancosViewModel.FilePath))
            {
                var vm = (BancosViewModel)sender!;
                if (!string.IsNullOrEmpty(vm.FilePath))
                {
#pragma warning disable CA1416
                    await PdfWebViewer.EnsureCoreWebView2Async();
                    PdfWebViewer.CoreWebView2.Navigate(vm.FilePath);
#pragma warning restore CA1416
                }
            }
        }
    }
}
