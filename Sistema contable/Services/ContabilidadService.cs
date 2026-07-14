using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SistemaContableZulay.UI.Domain;
using Documento = Sistema_contable.Models.Documento;
using ConfiguracionSistema = Sistema_contable.Models.ConfiguracionSistema;
using BackupInfo = Sistema_contable.Models.BackupInfo;
using HistorialReexpresion = Sistema_contable.Models.HistorialReexpresion;
using Sistema_contable.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Sistema_contable.Services;

namespace SistemaContableZulay.UI.Services;

public class ContabilidadService
{
    public static ContabilidadService Instance { get; } = new ContabilidadService();

    public int? EmpresaActivaId { get; set; }
    public event Action OnEmpresaCambiada;
    public event Action OnEmpresasModificadas;
    public event Action OnFacturasModificadas;
    public event Action OnDatosModificados;
    public void SeleccionarEmpresa(int? id)
    {
        if (EmpresaActivaId == id) return;

        EmpresaActivaId = id;
        OnEmpresaCambiada?.Invoke();
    }

    public EmpresaCliente ObtenerEmpresaActiva()
    {
        if (EmpresaActivaId == null) return null;
        return _empresasGuardadas.FirstOrDefault(e => e.Id == EmpresaActivaId.Value);
    }

    private readonly string _datosPath;
    private readonly string _comprobantesFile;
    private readonly string _cuentasFile;
    private readonly string _empresasFile;
    private readonly string _periodosFile;
    private readonly string _documentosFile;
    private readonly string _configuracionFile;
    private readonly string _historialReexpresionesFile;
    private readonly SupabaseSyncService _syncService = new SupabaseSyncService();
    private System.Timers.Timer _timerSincronizacion;

    private List<CuentaContable> _cuentasGuardadas = new();
    private List<EmpresaCliente> _empresasGuardadas = new();
    private List<Documento> _documentosGuardados = new();
    private List<HistorialReexpresion> _historialReexpresiones = new();
    private ConfiguracionSistema _configuracion = new();

    private ContabilidadService()
    {
        _datosPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Datos");
        if (!Directory.Exists(_datosPath))
        {
            Directory.CreateDirectory(_datosPath);
        }

        _comprobantesFile = Path.Combine(_datosPath, "comprobantes.xml");
        _cuentasFile = Path.Combine(_datosPath, "cuentas.xml");
        _empresasFile = Path.Combine(_datosPath, "empresas.xml");
        _periodosFile = Path.Combine(_datosPath, "periodos.xml");
        _documentosFile = Path.Combine(_datosPath, "documentos.xml");
        _configuracionFile = Path.Combine(_datosPath, "configuracion.xml");
        _historialReexpresionesFile = Path.Combine(_datosPath, "historial_reexpresiones.xml");

        CargarDatos();
    }

    private void CargarDatos()
    {
        _empresasGuardadas = CargarLista<EmpresaCliente>(_empresasFile) ?? new List<EmpresaCliente>();
        _cuentasGuardadas = CargarLista<CuentaContable>(_cuentasFile) ?? new List<CuentaContable>();
        _documentosGuardados = CargarLista<Documento>(_documentosFile) ?? new List<Documento>();
        _historialReexpresiones = CargarLista<HistorialReexpresion>(_historialReexpresionesFile) ?? new List<HistorialReexpresion>();
        _configuracion = CargarConfiguracion() ?? new ConfiguracionSistema();

        SembrarCuentasPorDefecto();

        GuardarEmpresas();
        GuardarCuentas();
    }

    private ConfiguracionSistema CargarConfiguracion()
    {
        if (!File.Exists(_configuracionFile)) return null;
        try
        {
            var serializer = new XmlSerializer(typeof(ConfiguracionSistema));
            using var stream = new FileStream(_configuracionFile, FileMode.Open);
            return (ConfiguracionSistema)serializer.Deserialize(stream);
        }
        catch
        {
            return null;
        }
    }

    public ConfiguracionSistema ObtenerConfiguracion() => _configuracion;

    public void GuardarConfiguracion(ConfiguracionSistema configuracion)
    {
        _configuracion = configuracion;
        var serializer = new XmlSerializer(typeof(ConfiguracionSistema));
        using var stream = new FileStream(_configuracionFile, FileMode.Create);
        serializer.Serialize(stream, _configuracion);
        OnDatosModificados?.Invoke();
    }

    private async Task SincronizarFacturaAsync(FacturaCobranza factura)
    {
        var payload = new
        {
            sync_id = factura.SyncId,
            id = factura.Id,
            numero_factura = factura.NumeroFactura,
            id_empresa = factura.IdEmpresa,
            nombre_cliente = factura.NombreCliente,
            descripcion = factura.Descripcion,
            fecha_emision = factura.FechaEmision,
            fecha_vencimiento = factura.FechaVencimiento,
            monto = factura.Monto,
            estado = factura.Estado,
            fecha_pago = factura.FechaPago,
            id_comprobante_emision = factura.IdComprobanteEmision,
            id_comprobante_pago = factura.IdComprobantePago,
            id_comprobante_reversion = factura.IdComprobanteReversion
        };

        bool ok = await _syncService.UpsertAsync("facturas_cobranza", payload);

        using var db = new ContabilidadDbContext();
        if (ok)
        {
            var f = db.FacturasCobranza.FirstOrDefault(x => x.Id == factura.Id);
            if (f != null) { f.Sincronizado = true; db.SaveChanges(); }
        }
        else
        {
            db.ColaSincronizacion.Add(new SistemaContableZulay.UI.Domain.ColaSincronizacion
            {
                TipoEntidad = "FacturaCobranza",
                PayloadJson = JsonSerializer.Serialize(payload)
            });
            db.SaveChanges();
        }
    }

    private async Task SincronizarComprobanteAsync(ComprobanteContable comp)
    {
        var payload = new
        {
            sync_id = comp.SyncId,
            id_comprobante = comp.IdComprobante,
            fecha = comp.Fecha,
            descripcion = comp.Descripcion,
            tipo_comprobante = comp.TipoComprobante,
            id_empresa = comp.IdEmpresa,
            estado = comp.Estado,
            monto_total = comp.MontoTotal,
            moneda = comp.Moneda
        };

        bool ok = await _syncService.UpsertAsync("comprobantes_contables", payload);

        using var db = new ContabilidadDbContext();
        if (ok)
        {
            var c = db.ComprobantesContables.FirstOrDefault(x => x.IdComprobante == comp.IdComprobante);
            if (c != null) { c.Sincronizado = true; db.SaveChanges(); }

            // Sincronizar también las líneas del comprobante
            foreach (var linea in comp.Lineas)
            {
                var lineaPayload = new
                {
                    sync_id = Guid.NewGuid(),
                    comprobante_sync_id = comp.SyncId,
                    codigo_cuenta = linea.CodigoCuenta,
                    descripcion_cuenta = linea.DescripcionCuenta,
                    debe = linea.Debe,
                    haber = linea.Haber
                };
                await _syncService.UpsertAsync("asiento_lineas", lineaPayload);
            }
        }
        else
        {
            db.ColaSincronizacion.Add(new SistemaContableZulay.UI.Domain.ColaSincronizacion
            {
                TipoEntidad = "ComprobanteContable",
                PayloadJson = JsonSerializer.Serialize(payload)
            });
            db.SaveChanges();
        }
    }

    // ─── Documentos ───────────────────────────────────────────────────────────

    public List<Documento> ObtenerDocumentos()
    {
        if (EmpresaActivaId == null) return _documentosGuardados.OrderByDescending(d => d.FechaRecepcion).ToList();
        return _documentosGuardados
            .Where(d => d.EmpresaId == EmpresaActivaId.Value)
            .OrderByDescending(d => d.FechaRecepcion)
            .ToList();
    }

    public void GuardarDocumento(Documento documento)
    {
        if (documento.Id == 0)
        {
            documento.Id = _documentosGuardados.Count > 0 ? _documentosGuardados.Max(d => d.Id) + 1 : 1;
        }

        var existente = _documentosGuardados.FirstOrDefault(d => d.Id == documento.Id);
        if (existente != null) _documentosGuardados.Remove(existente);

        _documentosGuardados.Add(documento);
        GuardarLista(_documentosGuardados, _documentosFile);
        OnDatosModificados?.Invoke();
    }

    public void EliminarDocumento(int id)
    {
        var existente = _documentosGuardados.FirstOrDefault(d => d.Id == id);
        if (existente != null)
        {
            _documentosGuardados.Remove(existente);
            GuardarLista(_documentosGuardados, _documentosFile);
            OnDatosModificados?.Invoke();
        }
    }

    private void SembrarCuentasPorDefecto()
    {
        if (_cuentasGuardadas.Count > 0) return;

        var defaultCuentas = new List<CuentaContable>
        {
            new CuentaContable { Codigo = "1.0.00.00", Nombre = "Activo", Tipo = "Activo", Nivel = 1, AceptaMovimiento = false },
            new CuentaContable { Codigo = "1.1.00.00", Nombre = "Activo Corriente", Tipo = "Activo", Nivel = 2, CuentaPadre = "1.0.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "1.1.01.00", Nombre = "Efectivo y Equivalentes", Tipo = "Activo", Nivel = 3, CuentaPadre = "1.1.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "1.1.01.01", Nombre = "Caja General", Tipo = "Activo", Nivel = 4, CuentaPadre = "1.1.01.00", AceptaMovimiento = true },
            new CuentaContable { Codigo = "1.1.01.02", Nombre = "Banco Mercantil 0105", Tipo = "Activo", Nivel = 4, CuentaPadre = "1.1.01.00", AceptaMovimiento = true },
            new CuentaContable { Codigo = "1.1.01.03", Nombre = "Banco Banesco 0134", Tipo = "Activo", Nivel = 4, CuentaPadre = "1.1.01.00", AceptaMovimiento = true },
            new CuentaContable { Codigo = "1.1.02.00", Nombre = "Cuentas por Cobrar", Tipo = "Activo", Nivel = 3, CuentaPadre = "1.1.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "1.1.02.01", Nombre = "Clientes Nacionales", Tipo = "Activo", Nivel = 4, CuentaPadre = "1.1.02.00", AceptaMovimiento = true },
            
            new CuentaContable { Codigo = "2.0.00.00", Nombre = "Pasivo", Tipo = "Pasivo", Nivel = 1, AceptaMovimiento = false },
            new CuentaContable { Codigo = "2.1.00.00", Nombre = "Pasivo Corriente", Tipo = "Pasivo", Nivel = 2, CuentaPadre = "2.0.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "2.1.01.00", Nombre = "Cuentas por Pagar Comerciales", Tipo = "Pasivo", Nivel = 3, CuentaPadre = "2.1.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "2.1.01.01", Nombre = "Proveedores Locales", Tipo = "Pasivo", Nivel = 4, CuentaPadre = "2.1.01.00", AceptaMovimiento = true },
            
            new CuentaContable { Codigo = "3.0.00.00", Nombre = "Patrimonio", Tipo = "Patrimonio", Nivel = 1, AceptaMovimiento = false },
            new CuentaContable { Codigo = "3.1.00.00", Nombre = "Capital Social", Tipo = "Patrimonio", Nivel = 2, CuentaPadre = "3.0.00.00", AceptaMovimiento = true },
            new CuentaContable { Codigo = "3.2.00.00", Nombre = "Resultados Acumulados", Tipo = "Patrimonio", Nivel = 2, CuentaPadre = "3.0.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "3.2.01.00", Nombre = "Resultado por Exposición a la Inflación (REI)", Tipo = "Patrimonio", Nivel = 3, CuentaPadre = "3.2.00.00", AceptaMovimiento = true },
            
            new CuentaContable { Codigo = "4.0.00.00", Nombre = "Ingresos", Tipo = "Ingreso", Nivel = 1, AceptaMovimiento = false },
            new CuentaContable { Codigo = "4.1.00.00", Nombre = "Ingresos Operacionales", Tipo = "Ingreso", Nivel = 2, CuentaPadre = "4.0.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "4.1.01.00", Nombre = "Ventas", Tipo = "Ingreso", Nivel = 3, CuentaPadre = "4.1.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "4.1.01.01", Nombre = "Ventas de Bienes / Servicios", Tipo = "Ingreso", Nivel = 4, CuentaPadre = "4.1.01.00", AceptaMovimiento = true },
            
            new CuentaContable { Codigo = "5.0.00.00", Nombre = "Egresos y Gastos", Tipo = "Egreso", Nivel = 1, AceptaMovimiento = false },
            new CuentaContable { Codigo = "5.1.00.00", Nombre = "Gastos de Operación", Tipo = "Egreso", Nivel = 2, CuentaPadre = "5.0.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "5.1.01.00", Nombre = "Gastos Administrativos", Tipo = "Egreso", Nivel = 3, CuentaPadre = "5.1.00.00", AceptaMovimiento = false },
            new CuentaContable { Codigo = "5.1.01.01", Nombre = "Gastos de Alquiler", Tipo = "Egreso", Nivel = 4, CuentaPadre = "5.1.01.00", AceptaMovimiento = true },
            new CuentaContable { Codigo = "5.1.01.02", Nombre = "Gastos de Servicios Públicos", Tipo = "Egreso", Nivel = 4, CuentaPadre = "5.1.01.00", AceptaMovimiento = true },
            new CuentaContable { Codigo = "5.1.01.03", Nombre = "Comisiones Bancarias", Tipo = "Egreso", Nivel = 4, CuentaPadre = "5.1.01.00", AceptaMovimiento = true },
            new CuentaContable { Codigo = "5.1.01.04", Nombre = "Gastos de Honorarios Profesionales", Tipo = "Egreso", Nivel = 4, CuentaPadre = "5.1.01.00", AceptaMovimiento = true }
        };

        _cuentasGuardadas.AddRange(defaultCuentas);
        GuardarCuentas();
    }

    private List<T> CargarLista<T>(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            var serializer = new XmlSerializer(typeof(List<T>));
            using var stream = new FileStream(path, FileMode.Open);
            return (List<T>)serializer.Deserialize(stream);
        }
        catch
        {
            return null;
        }
    }

    private void GuardarLista<T>(List<T> lista, string path)
    {
        var serializer = new XmlSerializer(typeof(List<T>));
        using var stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, lista);
    }

    public List<CuentaContable> ObtenerCuentasContables() => _cuentasGuardadas.ToList();
    public List<EmpresaCliente> ObtenerEmpresas() => _empresasGuardadas.ToList();

    public void GuardarEmpresa(EmpresaCliente empresa)
    {
        if (empresa.Id == 0)
        {
            empresa.Id = _empresasGuardadas.Count > 0 ? _empresasGuardadas.Max(e => e.Id) + 1 : 1;
        }

        var existente = _empresasGuardadas.FirstOrDefault(e => e.Id == empresa.Id);
        if (existente != null)
        {
            _empresasGuardadas.Remove(existente);
        }
        _empresasGuardadas.Add(empresa);
        GuardarEmpresas();

        OnEmpresasModificadas?.Invoke();
        OnDatosModificados?.Invoke();

        if (EmpresaActivaId == empresa.Id)
        {
            OnEmpresaCambiada?.Invoke();
        }
    }

    public void EliminarEmpresa(int id)
    {
        var existente = _empresasGuardadas.FirstOrDefault(e => e.Id == id);
        if (existente != null)
        {
            _empresasGuardadas.Remove(existente);
            GuardarEmpresas();

            OnEmpresasModificadas?.Invoke();
            OnDatosModificados?.Invoke();

            if (EmpresaActivaId == id)
            {
                SeleccionarEmpresa(null);
            }
        }
    }

    public void GuardarCuenta(CuentaContable cuenta)
    {
        var existente = _cuentasGuardadas.FirstOrDefault(c => c.Codigo == cuenta.Codigo);
        if (existente != null)
        {
            _cuentasGuardadas.Remove(existente);
        }
        _cuentasGuardadas.Add(cuenta);
        GuardarCuentas();
        OnDatosModificados?.Invoke();
    }

    public void EliminarCuenta(string codigo)
    {
        var existente = _cuentasGuardadas.FirstOrDefault(c => c.Codigo == codigo);
        if (existente != null)
        {
            _cuentasGuardadas.Remove(existente);
            GuardarCuentas();
            OnDatosModificados?.Invoke();
        }
    }

    private void GuardarEmpresas() => GuardarLista(_empresasGuardadas, _empresasFile);
    private void GuardarCuentas() => GuardarLista(_cuentasGuardadas, _cuentasFile);

    public List<string> ObtenerTiposComprobante()
    {
        return new List<string> { "Ingreso", "Egreso", "Diario" };
    }

    public void GuardarComprobante(ComprobanteContable comprobante)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay una empresa activa seleccionada.");

        comprobante.IdEmpresa = EmpresaActivaId.Value;

        using var db = new ContabilidadDbContext();

        if (comprobante.IdComprobante == 0)
        {
            db.ComprobantesContables.Add(comprobante);
        }
        else
        {
            var existente = db.ComprobantesContables
                .Include(c => c.Lineas)
                .FirstOrDefault(c => c.IdComprobante == comprobante.IdComprobante);

            if (existente != null)
            {
                db.LineasAsiento.RemoveRange(existente.Lineas);
                db.Entry(existente).CurrentValues.SetValues(comprobante);
                existente.Lineas = comprobante.Lineas;
            }
            else
            {
                db.ComprobantesContables.Add(comprobante);
            }
        }

        db.SaveChanges();

        _ = SincronizarComprobanteAsync(comprobante);
    }

    public IReadOnlyList<ComprobanteContable> ObtenerComprobantesGuardados()
    {
        if (EmpresaActivaId == null) return new List<ComprobanteContable>().AsReadOnly();

        using var db = new ContabilidadDbContext();
        return db.ComprobantesContables
            .Include(c => c.Lineas)
            .Where(c => c.IdEmpresa == EmpresaActivaId.Value)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<ComprobanteContable> ObtenerComprobantesParaActualizar(DateTime? desde, DateTime? hasta, string tipo)
    {
        if (EmpresaActivaId == null) return new List<ComprobanteContable>().AsReadOnly();

        using var db = new ContabilidadDbContext();
        var query = db.ComprobantesContables
            .Include(c => c.Lineas)
            .Where(c => c.IdEmpresa == EmpresaActivaId.Value);

        if (desde.HasValue)
            query = query.Where(c => c.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(c => c.Fecha <= hasta.Value);
        if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
            query = query.Where(c => c.TipoComprobante == tipo);

        return query.OrderBy(c => c.Fecha).ToList().AsReadOnly();
    }

    public void ActualizarEstadoComprobante(int idComprobante, string nuevoEstado)
    {
        using var db = new ContabilidadDbContext();
        var comp = db.ComprobantesContables.FirstOrDefault(c => c.IdComprobante == idComprobante)
            ?? throw new InvalidOperationException($"Comprobante #{idComprobante} no encontrado.");

        comp.Estado = nuevoEstado;
        db.SaveChanges();
    }

    public void EliminarComprobante(int idComprobante)
    {
        using var db = new ContabilidadDbContext();
        var existente = db.ComprobantesContables
            .Include(c => c.Lineas)
            .FirstOrDefault(c => c.IdComprobante == idComprobante);

        if (existente != null)
        {
            db.ComprobantesContables.Remove(existente);
            db.SaveChanges();
        }
    }

    public ComprobanteContable ReversarComprobante(int idComprobante, DateTime fechaReversion, string motivo)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay una empresa activa seleccionada.");

        using var db = new ContabilidadDbContext();

        var original = db.ComprobantesContables
            .Include(c => c.Lineas)
            .FirstOrDefault(c => c.IdComprobante == idComprobante)
            ?? throw new InvalidOperationException($"Comprobante #{idComprobante} no encontrado.");

        if (original.Estado == "Reversado")
            throw new InvalidOperationException("El comprobante ya fue reversado.");

        var contraAsiento = new ComprobanteContable
        {
            IdEmpresa = EmpresaActivaId.Value,
            Fecha = fechaReversion,
            Descripcion = $"REVERSIÓN de #{idComprobante}: {motivo}",
            TipoComprobante = "Reversión",
            Estado = "Registrado"
        };

        foreach (var linea in original.Lineas)
        {
            contraAsiento.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta = linea.CodigoCuenta,
                DescripcionCuenta = $"Rev. - {linea.DescripcionCuenta}",
                Debe = linea.Haber,
                Haber = linea.Debe
            });
        }

        original.Estado = "Reversado";
        db.ComprobantesContables.Add(contraAsiento);
        db.SaveChanges();

        _ = SincronizarComprobanteAsync(original);
        _ = SincronizarComprobanteAsync(contraAsiento);

        return contraAsiento;
    }

    public ResumenEjercicio ObtenerResumenEjercicio(int anio)
    {
        if (EmpresaActivaId == null) return new ResumenEjercicio();

        using var db = new ContabilidadDbContext();
        var comprobantes = db.ComprobantesContables
            .Include(c => c.Lineas)
            .Where(c => c.IdEmpresa == EmpresaActivaId.Value && c.Fecha.Year == anio)
            .ToList();

        var cuentas = _cuentasGuardadas;

        decimal totalIngresos = 0;
        decimal totalEgresos = 0;

        foreach (var comp in comprobantes)
        {
            foreach (var linea in comp.Lineas)
            {
                var cuenta = cuentas.FirstOrDefault(c => c.Codigo == linea.CodigoCuenta);
                if (cuenta == null) continue;

                if (cuenta.Tipo == "Ingreso")
                    totalIngresos += linea.Haber - linea.Debe;
                else if (cuenta.Tipo == "Egreso")
                    totalEgresos += linea.Debe - linea.Haber;
            }
        }

        return new ResumenEjercicio
        {
            Anio = anio,
            TotalIngresos = totalIngresos,
            TotalEgresos = totalEgresos,
            Resultado = totalIngresos - totalEgresos
        };
    }

    public void CerrarEjercicio(int anio)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay una empresa activa seleccionada.");

        using var db = new ContabilidadDbContext();

        if (db.PeriodosFiscales.Any(p => p.Anio == anio && p.Cerrado))
            throw new InvalidOperationException($"El ejercicio {anio} ya está cerrado.");

        var resumen = ObtenerResumenEjercicio(anio);

        var asientoCierre = new ComprobanteContable
        {
            IdEmpresa = EmpresaActivaId.Value,
            Fecha = new DateTime(anio, 12, 31),
            Descripcion = $"Cierre del ejercicio fiscal {anio}",
            TipoComprobante = "Cierre",
            Estado = "Registrado"
        };

        if (resumen.Resultado >= 0)
        {
            asientoCierre.Lineas.Add(new AsientoLinea { CodigoCuenta = "4.0.00.00", DescripcionCuenta = "Cierre de Ingresos", Debe = resumen.TotalIngresos, Haber = 0 });
            asientoCierre.Lineas.Add(new AsientoLinea { CodigoCuenta = "3.1.00.00", DescripcionCuenta = "Resultado del Ejercicio (Utilidad)", Debe = 0, Haber = resumen.Resultado });
            asientoCierre.Lineas.Add(new AsientoLinea { CodigoCuenta = "5.0.00.00", DescripcionCuenta = "Cierre de Egresos", Debe = 0, Haber = resumen.TotalEgresos });
        }
        else
        {
            asientoCierre.Lineas.Add(new AsientoLinea { CodigoCuenta = "4.0.00.00", DescripcionCuenta = "Cierre de Ingresos", Debe = resumen.TotalIngresos, Haber = 0 });
            asientoCierre.Lineas.Add(new AsientoLinea { CodigoCuenta = "5.0.00.00", DescripcionCuenta = "Cierre de Egresos", Debe = 0, Haber = resumen.TotalEgresos });
            asientoCierre.Lineas.Add(new AsientoLinea { CodigoCuenta = "3.1.00.00", DescripcionCuenta = "Resultado del Ejercicio (Pérdida)", Debe = Math.Abs(resumen.Resultado), Haber = 0 });
        }

        db.ComprobantesContables.Add(asientoCierre);

        var periodo = db.PeriodosFiscales.FirstOrDefault(p => p.Anio == anio);
        if (periodo == null)
        {
            periodo = new PeriodoFiscal { Anio = anio };
            db.PeriodosFiscales.Add(periodo);
        }
        periodo.Cerrado = true;
        periodo.FechaCierre = DateTime.Now;

        db.SaveChanges();
    }

    public bool EjercicioCerrado(int anio)
    {
        using var db = new ContabilidadDbContext();
        return db.PeriodosFiscales.Any(p => p.Anio == anio && p.Cerrado);
    }

    public List<PeriodoFiscal> ObtenerPeriodosFiscales()
    {
        using var db = new ContabilidadDbContext();
        return db.PeriodosFiscales.ToList();
    }

    // ─── Cobranza ─────────────────────────────────────────────────────────────

    public List<FacturaCobranza> ObtenerFacturas()
    {
        if (EmpresaActivaId == null) return new List<FacturaCobranza>();

        using var db = new ContabilidadDbContext();
        var hoy = DateTime.Now.Date;

        var vencidas = db.FacturasCobranza
            .Where(f => f.IdEmpresa == EmpresaActivaId.Value
                     && f.Estado == "Pendiente"
                     && f.FechaVencimiento.Date < hoy)
            .ToList();

        foreach (var f in vencidas)
            f.Estado = "Vencida";

        if (vencidas.Count > 0)
            db.SaveChanges();

        return db.FacturasCobranza
            .Where(f => f.IdEmpresa == EmpresaActivaId.Value)
            .OrderByDescending(f => f.FechaEmision)
            .ToList();
    }

    public void GuardarFactura(FacturaCobranza factura)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay empresa activa seleccionada.");

        using var db = new ContabilidadDbContext();
        factura.IdEmpresa = EmpresaActivaId.Value;
        bool esNueva = factura.Id == 0;

        if (esNueva)
        {
            var siguienteId = db.FacturasCobranza.Any()
                ? db.FacturasCobranza.Max(f => f.Id) + 1 : 1;
            factura.NumeroFactura = $"FAC-{EmpresaActivaId:D3}-{siguienteId:D4}";

            var compEmision = new ComprobanteContable
            {
                IdEmpresa = EmpresaActivaId.Value,
                Fecha = factura.FechaEmision,
                Descripcion = $"Factura {factura.NumeroFactura} – {factura.NombreCliente}",
                TipoComprobante = "Ingreso",
                Estado = "Registrado"
            };
            compEmision.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta = "1.1.02.01",
                DescripcionCuenta = "Clientes Nacionales",
                Debe = factura.Monto,
                Haber = 0
            });
            compEmision.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta = "4.1.01.01",
                DescripcionCuenta = "Ventas de Bienes / Servicios",
                Debe = 0,
                Haber = factura.Monto
            });

            db.ComprobantesContables.Add(compEmision);
            db.FacturasCobranza.Add(factura);
            db.SaveChanges();

            factura.IdComprobanteEmision = compEmision.IdComprobante;
            db.SaveChanges();

            _ = SincronizarFacturaAsync(factura);
            _ = SincronizarComprobanteAsync(compEmision);
        }
        else
        {
            var existente = db.FacturasCobranza.FirstOrDefault(f => f.Id == factura.Id);
            if (existente != null)
            {
                db.Entry(existente).CurrentValues.SetValues(factura);
            }
            else
            {
                db.FacturasCobranza.Add(factura);
            }
            db.SaveChanges();

            _ = SincronizarFacturaAsync(factura);
        }
    }

    public void MarcarFacturaPagada(int idFactura)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay empresa activa seleccionada.");

        using var db = new ContabilidadDbContext();

        var factura = db.FacturasCobranza.FirstOrDefault(f => f.Id == idFactura)
            ?? throw new InvalidOperationException("Factura no encontrada.");

        if (factura.Estado == "Pagada") throw new InvalidOperationException("La factura ya está pagada.");
        if (factura.Estado == "Anulada") throw new InvalidOperationException("No se puede cobrar una factura anulada.");

        var compPago = new ComprobanteContable
        {
            IdEmpresa = EmpresaActivaId.Value,
            Fecha = DateTime.Now,
            Descripcion = $"Cobro {factura.NumeroFactura} – {factura.NombreCliente}",
            TipoComprobante = "Ingreso",
            Estado = "Registrado"
        };
        compPago.Lineas.Add(new AsientoLinea
        {
            CodigoCuenta = "1.1.01.01",
            DescripcionCuenta = "Caja General",
            Debe = factura.Monto,
            Haber = 0
        });
        compPago.Lineas.Add(new AsientoLinea
        {
            CodigoCuenta = "1.1.02.01",
            DescripcionCuenta = "Clientes Nacionales",
            Debe = 0,
            Haber = factura.Monto
        });

        db.ComprobantesContables.Add(compPago);
        db.SaveChanges();

        factura.Estado = "Pagada";
        factura.FechaPago = DateTime.Now;
        factura.IdComprobantePago = compPago.IdComprobante;
        db.SaveChanges();
        OnFacturasModificadas?.Invoke();

        _ = SincronizarFacturaAsync(factura);
        _ = SincronizarComprobanteAsync(compPago);
    }

    public void AnularFactura(int idFactura)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay empresa activa seleccionada.");

        using var db = new ContabilidadDbContext();

        var factura = db.FacturasCobranza.FirstOrDefault(f => f.Id == idFactura)
            ?? throw new InvalidOperationException("Factura no encontrada.");

        if (factura.Estado == "Pagada") throw new InvalidOperationException("No se puede anular una factura pagada. Use Reversión de comprobante.");
        if (factura.Estado == "Anulada") throw new InvalidOperationException("La factura ya está anulada.");

        ComprobanteContable compAnulacion = null;

        if (factura.IdComprobanteEmision > 0)
        {
            var compOriginal = db.ComprobantesContables
                .Include(c => c.Lineas)
                .FirstOrDefault(c => c.IdComprobante == factura.IdComprobanteEmision);

            if (compOriginal != null)
            {
                compAnulacion = new ComprobanteContable
                {
                    IdEmpresa = EmpresaActivaId.Value,
                    Fecha = DateTime.Now,
                    Descripcion = $"Anulación {factura.NumeroFactura} – {factura.NombreCliente}",
                    TipoComprobante = "Egreso",
                    Estado = "Registrado"
                };
                foreach (var linea in compOriginal.Lineas)
                    compAnulacion.Lineas.Add(new AsientoLinea
                    {
                        CodigoCuenta = linea.CodigoCuenta,
                        DescripcionCuenta = $"Anulación – {linea.DescripcionCuenta}",
                        Debe = linea.Haber,
                        Haber = linea.Debe
                    });

                compOriginal.Estado = "Anulado";
                db.ComprobantesContables.Add(compAnulacion);
                db.SaveChanges();

                _ = SincronizarComprobanteAsync(compOriginal);
                _ = SincronizarComprobanteAsync(compAnulacion);
            }
        }

        factura.Estado = "Anulada";
        db.SaveChanges();
        OnFacturasModificadas?.Invoke();

        _ = SincronizarFacturaAsync(factura);
    }

    public string EjecutarRespaldoAutomatico() => EjecutarBackup("Respaldo_Reexpresion");

    public string EjecutarBackupManual() => EjecutarBackup("Backup");

    private string EjecutarBackup(string prefijo)
    {
        try
        {
            var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupDir, $"{prefijo}_{timestamp}.zip");

            if (Directory.Exists(_datosPath))
            {
                ZipFile.CreateFromDirectory(_datosPath, backupPath);
                return backupPath;
            }
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public string ObtenerCarpetaBackups()
    {
        var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
        return backupDir;
    }

    public List<BackupInfo> ObtenerHistorialBackups()
    {
        var backupDir = ObtenerCarpetaBackups();
        return Directory.GetFiles(backupDir, "*.zip")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .Select(f => new BackupInfo
            {
                Fecha = f.LastWriteTime,
                NombreArchivo = f.Name,
                RutaCompleta = f.FullName,
                Tamaño = $"{f.Length / 1024.0:N0} KB"
            })
            .ToList();
    }

    public void RestaurarBackup(string rutaZip)
    {
        if (!File.Exists(rutaZip)) throw new InvalidOperationException("El archivo de backup no existe.");

        var tempDir = Path.Combine(Path.GetTempPath(), "SistemaContable_Restore_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        try
        {
            ZipFile.ExtractToDirectory(rutaZip, tempDir);

            foreach (var file in Directory.GetFiles(tempDir))
            {
                var destino = Path.Combine(_datosPath, Path.GetFileName(file));
                File.Copy(file, destino, overwrite: true);
            }

            CargarDatos();
            OnDatosModificados?.Invoke();
            OnEmpresasModificadas?.Invoke();
            OnEmpresaCambiada?.Invoke();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    public decimal ObtenerSaldoCuentaAFecha(string codigoCuenta, DateTime fechaCorte)
    {
        return ObtenerSaldoCuentaEntreFechas(codigoCuenta, null, fechaCorte);
    }

    public decimal ObtenerSaldoCuentaEntreFechas(string codigoCuenta, DateTime? fechaInicio, DateTime fechaFin)
    {
        if (EmpresaActivaId == null) return 0m;

        var cuenta = _cuentasGuardadas.FirstOrDefault(c => c.Codigo == codigoCuenta);
        if (cuenta == null) return 0m;

        using var db = new ContabilidadDbContext();
        var query = db.ComprobantesContables
            .Include(c => c.Lineas)
            .Where(c => c.IdEmpresa == EmpresaActivaId.Value && c.Fecha.Date <= fechaFin.Date);

        if (fechaInicio.HasValue)
        {
            query = query.Where(c => c.Fecha.Date >= fechaInicio.Value.Date);
        }

        var comprobantes = query.ToList();

        decimal totalDebe = 0m;
        decimal totalHaber = 0m;

        foreach (var comp in comprobantes)
        {
            foreach (var linea in comp.Lineas.Where(l => l.CodigoCuenta == codigoCuenta))
            {
                totalDebe += linea.Debe;
                totalHaber += linea.Haber;
            }
        }

        if (cuenta.Tipo == "Activo" || cuenta.Tipo == "Egreso")
            return totalDebe - totalHaber;

        if (cuenta.Tipo == "Pasivo" || cuenta.Tipo == "Patrimonio" || cuenta.Tipo == "Ingreso")
            return totalHaber - totalDebe;

        return 0m;
    }

    // ─── Historial de Reexpresiones ───────────────────────────────────────────

    public List<HistorialReexpresion> ObtenerHistorialReexpresiones(string codigoCuenta = null)
    {
        if (EmpresaActivaId == null) return new List<HistorialReexpresion>();

        var query = _historialReexpresiones.Where(h => h.IdEmpresa == EmpresaActivaId.Value && !h.Anulado);

        if (!string.IsNullOrEmpty(codigoCuenta))
        {
            query = query.Where(h => h.CodigoCuenta == codigoCuenta);
        }

        return query.OrderByDescending(h => h.FechaCalculo).ToList();
    }

    public void GuardarHistorialReexpresion(HistorialReexpresion historial)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay empresa activa seleccionada.");

        if (historial.Id == 0)
        {
            historial.Id = _historialReexpresiones.Count > 0 ? _historialReexpresiones.Max(h => h.Id) + 1 : 1;
        }

        historial.IdEmpresa = EmpresaActivaId.Value;

        var existente = _historialReexpresiones.FirstOrDefault(h => h.Id == historial.Id);
        if (existente != null) _historialReexpresiones.Remove(existente);

        _historialReexpresiones.Add(historial);
        GuardarLista(_historialReexpresiones, _historialReexpresionesFile);
    }

    public void RestaurarReexpresion(int idHistorial)
    {
        var historial = _historialReexpresiones.FirstOrDefault(h => h.Id == idHistorial);
        if (historial == null || historial.Anulado) return;

        // 1. Eliminar o reversar el comprobante generado
        if (historial.IdComprobanteAsociado > 0)
        {
            EliminarComprobante(historial.IdComprobanteAsociado);
        }

        // 2. Marcar el historial como anulado
        historial.Anulado = true;
        GuardarLista(_historialReexpresiones, _historialReexpresionesFile);
    }

    public void DesactivarComprobanteReexpresion(int idHistorial)
    {
        var historial = _historialReexpresiones.FirstOrDefault(h => h.Id == idHistorial);
        if (historial == null || historial.Anulado) return;

        if (historial.IdComprobanteAsociado > 0)
        {
            EliminarComprobante(historial.IdComprobanteAsociado);
            historial.IdComprobanteAsociado = 0;
            GuardarLista(_historialReexpresiones, _historialReexpresionesFile);
        }
    }

    public async Task ProcesarColaPendienteAsync()
    {
        using var db = new ContabilidadDbContext();
        var pendientes = db.ColaSincronizacion.OrderBy(c => c.FechaCreacion).ToList();

        foreach (var item in pendientes)
        {
            var tabla = item.TipoEntidad == "FacturaCobranza" ? "facturas_cobranza" : "comprobantes_contables";
            var payload = JsonSerializer.Deserialize<object>(item.PayloadJson);

            bool ok = await _syncService.UpsertAsync(tabla, payload);
            if (ok)
                db.ColaSincronizacion.Remove(item);
            else
                item.Intentos++;
        }
        db.SaveChanges();
    }

    public void IniciarSincronizacionPeriodica(double intervaloMinutos = 5)
    {
        if (_timerSincronizacion != null) return; // evita duplicar el timer si se llama dos veces

        _timerSincronizacion = new System.Timers.Timer(intervaloMinutos * 60 * 1000);
        _timerSincronizacion.Elapsed += async (sender, e) =>
        {
            try
            {
                await ProcesarColaPendienteAsync();
            }
            catch
            {
                // Silencioso: si falla un intento, el timer vuelve a correr en el siguiente ciclo.
            }
        };
        _timerSincronizacion.AutoReset = true;
        _timerSincronizacion.Start();
    }

    public void DetenerSincronizacionPeriodica()
    {
        _timerSincronizacion?.Stop();
        _timerSincronizacion?.Dispose();
        _timerSincronizacion = null;
    }
}
