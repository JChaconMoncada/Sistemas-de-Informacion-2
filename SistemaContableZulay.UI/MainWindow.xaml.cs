using System.Windows;
using SistemaContableZulay.UI.Infrastructure;
using SistemaContableZulay.UI.Services;
using SistemaContableZulay.UI.ViewModels;

namespace SistemaContableZulay.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var contabilidadService = new ContabilidadService();
        var messageBoxService = new WpfMessageBoxService();
        DataContext = new AsientoViewModel(contabilidadService, messageBoxService);
    }
}
