using System;
using System.Windows;
using QuestPDF.Infrastructure;

namespace Sistema_contable
{
    public partial class App : Application
    {
        public App()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Ocurrió un error inesperado:\n\n{args.Exception.Message}\n\nDetalle: {args.Exception.StackTrace}",
                                "Error Crítico del Sistema", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show($"Error fatal en hilo de fondo:\n\n{ex?.Message}",
                                "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }
    }
}
