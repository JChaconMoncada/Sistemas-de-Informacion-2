using System.Windows;
using QuestPDF.Infrastructure;

namespace Sistema_contable
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }
    }
}
