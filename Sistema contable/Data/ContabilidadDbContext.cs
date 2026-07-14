using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Sistema_contable.Models;
using SistemaContableZulay.UI.Domain;



namespace Sistema_contable.Data
{
    public class ContabilidadDbContext : DbContext
    {
        private readonly string _dbPath;

        public ContabilidadDbContext()
        {
            var carpetaApp = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SistemaContable");

            Directory.CreateDirectory(carpetaApp);
            _dbPath = Path.Combine(carpetaApp, "contable.db");

        }

        // Entidades de base de datos (persistencia)
        public DbSet<Sistema_contable.Models.CuentaContable> CuentasContables { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<FacturaInterna> FacturasInternas { get; set; }
        public DbSet<PartidaReexpresion> PartidasReexpresion { get; set; }
        public DbSet<ConfiguracionSistema> ConfiguracionSistema { get; set; }
        public DbSet<BackupInfo> Backups { get; set; }
        public DbSet<IpcRecord> RegistrosIpc { get; set; }
        public DbSet<FacturaCobranza> FacturasCobranza { get; set; }
        public DbSet<Asiento> Asientos { get; set; }

        // Comprobantes / partida doble (namespace Domain)
        public DbSet<ComprobanteContable> ComprobantesContables { get; set; }
        public DbSet<AsientoLinea> LineasAsiento { get; set; }
        public DbSet<EmpresaCliente> EmpresasCliente { get; set; }
        public DbSet<Moneda> Monedas { get; set; }
        public DbSet<PeriodoFiscal> PeriodosFiscales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Clave primaria explícita (no sigue la convención de nombre)
            modelBuilder.Entity<ComprobanteContable>()
                .HasKey(c => c.IdComprobante);

            // Relación Comprobante -> Lineas, con la FK explícita que agregamos
            modelBuilder.Entity<ComprobanteContable>()
                .HasMany(c => c.Lineas)
                .WithOne()
                .HasForeignKey(l => l.ComprobanteContableId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PeriodoFiscal>()
                .HasKey(p => new { p.Anio, p.Mes });

            modelBuilder.Entity<Sistema_contable.Models.CuentaContable>()
                .HasIndex(c => new { c.EmpresaId, c.Codigo })
                .IsUnique();
        }

        public DbSet<ColaSincronizacion> ColaSincronizacion { get; set; }
    }
}