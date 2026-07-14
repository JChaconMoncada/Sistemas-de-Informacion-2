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