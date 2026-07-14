using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Sistema_contable.Data;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Manejador global de excepciones para evitar cierres inesperados
            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Ha ocurrido un error inesperado:\n\n{args.Exception.Message}",
                                "Error del Sistema", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true; // Prevenir el cierre de la aplicación si es posible
            };

            try
            {
                using var db = new ContabilidadDbContext();
                db.Database.Migrate();

                _ = ContabilidadService.Instance.ProcesarColaPendienteAsync();
                ContabilidadService.Instance.IniciarSincronizacionPeriodica(5); // cada 5 minutos
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al preparar la base de datos: {ex.Message}",
                    "Error de inicio",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ContabilidadService.Instance.DetenerSincronizacionPeriodica();
            base.OnExit(e);
            System.Environment.Exit(0);
        }
    }
}