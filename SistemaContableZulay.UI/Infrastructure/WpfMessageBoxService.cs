using System.Windows;
using SistemaContableZulay.UI.Services;

namespace SistemaContableZulay.UI.Infrastructure;

public class WpfMessageBoxService : IMessageBoxService
{
    public void MostrarAlerta(string mensaje, TipoAlerta tipo)
    {
        var icono = tipo switch
        {
            TipoAlerta.Error => MessageBoxImage.Error,
            TipoAlerta.Advertencia => MessageBoxImage.Warning,
            TipoAlerta.Exito => MessageBoxImage.Information,
            _ => MessageBoxImage.Information
        };

        MessageBox.Show(mensaje, "Sistema Contable Zulay Angola", MessageBoxButton.OK, icono);
    }
}
