using System.Windows;

namespace Sistema_contable
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            System.Environment.Exit(0);
        }
    }
}
