using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SistemaContableZulay.UI.Domain;

namespace SistemaContableZulay.UI.Services;

public class ContabilidadService
{
    public static ContabilidadService Instance { get; } = new ContabilidadService();

    public int? EmpresaActivaId { get; set; }
    public event Action OnEmpresaCambiada;

    public void SeleccionarEmpresa(int? id)
    {
        EmpresaActivaId = id;
        OnEmpresaCambiada?.Invoke();
    }

    private readonly string _datosPath;
    private readonly string _comprobantesFile;
    private readonly string _cuentasFile;
    private readonly string _empresasFile;
    private readonly string _periodosFile;
    private readonly string _facturasFile;

    private List<ComprobanteContable> _comprobantesGuardados = new();
    private List<CuentaContable> _cuentasGuardadas = new();
    private List<EmpresaCliente> _empresasGuardadas = new();
    private List<PeriodoFiscal> _periodosFiscales = new();
    private List<FacturaCobranza> _facturasCobranza = new();

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
        _facturasFile  = Path.Combine(_datosPath, "facturas.xml");

        CargarDatos();
    }

    private void CargarDatos()
    {
        _empresasGuardadas = CargarLista<EmpresaCliente>(_empresasFile) ?? new List<EmpresaCliente>();
        _cuentasGuardadas = CargarLista<CuentaContable>(_cuentasFile) ?? new List<CuentaContable>();
        _comprobantesGuardados = CargarLista<ComprobanteContable>(_comprobantesFile) ?? new List<ComprobanteContable>();
        _periodosFiscales = CargarLista<PeriodoFiscal>(_periodosFile) ?? new List<PeriodoFiscal>();
        _facturasCobranza = CargarLista<FacturaCobranza>(_facturasFile) ?? new List<FacturaCobranza>();

        SembrarCuentasPorDefecto();

        GuardarEmpresas();
        GuardarCuentas();
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
        
        // Si se editó la empresa activa, disparamos el evento para que la interfaz se refresque
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
    }

    public void EliminarCuenta(string codigo)
    {
        var existente = _cuentasGuardadas.FirstOrDefault(c => c.Codigo == codigo);
        if (existente != null)
        {
            _cuentasGuardadas.Remove(existente);
            GuardarCuentas();
        }
    }

    private void GuardarEmpresas() => GuardarLista(_empresasGuardadas, _empresasFile);
    private void GuardarCuentas() => GuardarLista(_cuentasGuardadas, _cuentasFile);
    private void GuardarFacturas() => GuardarLista(_facturasCobranza, _facturasFile);

    public List<string> ObtenerTiposComprobante()
    {
        return new List<string> { "Ingreso", "Egreso", "Diario" };
    }

    public void GuardarComprobante(ComprobanteContable comprobante)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay una empresa activa seleccionada.");

        comprobante.IdEmpresa = EmpresaActivaId.Value;

        if (comprobante.IdComprobante == 0)
            comprobante.IdComprobante = _comprobantesGuardados.Count > 0 ? _comprobantesGuardados.Max(c => c.IdComprobante) + 1 : 1;
            
        var existente = _comprobantesGuardados.FirstOrDefault(c => c.IdComprobante == comprobante.IdComprobante);
        if(existente != null) _comprobantesGuardados.Remove(existente);
        
        _comprobantesGuardados.Add(comprobante);
        GuardarLista(_comprobantesGuardados, _comprobantesFile);
    }

    public IReadOnlyList<ComprobanteContable> ObtenerComprobantesGuardados() 
    {
        if (EmpresaActivaId == null) return new List<ComprobanteContable>().AsReadOnly();
        
        return _comprobantesGuardados.Where(c => c.IdEmpresa == EmpresaActivaId.Value).ToList().AsReadOnly();
    }

    public IReadOnlyList<ComprobanteContable> ObtenerComprobantesParaActualizar(DateTime? desde, DateTime? hasta, string tipo)
    {
        if (EmpresaActivaId == null) return new List<ComprobanteContable>().AsReadOnly();

        var query = _comprobantesGuardados.Where(c => c.IdEmpresa == EmpresaActivaId.Value);

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
        var comp = _comprobantesGuardados.FirstOrDefault(c => c.IdComprobante == idComprobante);
        if (comp == null) throw new InvalidOperationException($"Comprobante #{idComprobante} no encontrado.");

        comp.Estado = nuevoEstado;
        GuardarLista(_comprobantesGuardados, _comprobantesFile);
    }

    public ComprobanteContable ReversarComprobante(int idComprobante, DateTime fechaReversion, string motivo)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay una empresa activa seleccionada.");

        var original = _comprobantesGuardados.FirstOrDefault(c => c.IdComprobante == idComprobante);
        if (original == null) throw new InvalidOperationException($"Comprobante #{idComprobante} no encontrado.");
        if (original.Estado == "Reversado") throw new InvalidOperationException("El comprobante ya fue reversado.");

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

        contraAsiento.IdComprobante = _comprobantesGuardados.Count > 0
            ? _comprobantesGuardados.Max(c => c.IdComprobante) + 1 : 1;

        original.Estado = "Reversado";
        _comprobantesGuardados.Add(contraAsiento);
        GuardarLista(_comprobantesGuardados, _comprobantesFile);

        return contraAsiento;
    }

    public ResumenEjercicio ObtenerResumenEjercicio(int anio)
    {
        if (EmpresaActivaId == null) return new ResumenEjercicio();

        var comprobantes = _comprobantesGuardados
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
        if (_periodosFiscales.Any(p => p.Anio == anio && p.Cerrado))
            throw new InvalidOperationException($"El ejercicio {anio} ya está cerrado.");

        var resumen = ObtenerResumenEjercicio(anio);

        var asientoCierre = new ComprobanteContable
        {
            IdEmpresa = EmpresaActivaId.Value,
            Fecha = new DateTime(anio, 12, 31),
            Descripcion = $"Cierre del ejercicio fiscal {anio}",
            TipoComprobante = "Cierre",
            Estado = "Registrado",
            IdComprobante = _comprobantesGuardados.Count > 0
                ? _comprobantesGuardados.Max(c => c.IdComprobante) + 1 : 1
        };

        if (resumen.Resultado >= 0)
        {
            asientoCierre.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta = "4.0.00.00",
                DescripcionCuenta = "Cierre de Ingresos",
                Debe = resumen.TotalIngresos,
                Haber = 0
            });
            asientoCierre.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta = "3.1.00.00",
                DescripcionCuenta = "Resultado del Ejercicio (Utilidad)",
                Debe = 0,
                Haber = resumen.Resultado
            });
            asientoCierre.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta = "5.0.00.00",
                DescripcionCuenta = "Cierre de Egresos",
                Debe = 0,
                Haber = resumen.TotalEgresos
            });
        }
        else
        {
            asientoCierre.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta = "4.0.00.00",
                DescripcionCuenta = "Cierre de Ingresos",
                Debe = resumen.TotalIngresos,
                Haber = 0
            });
            asientoCierre.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta = "5.0.00.00",
                DescripcionCuenta = "Cierre de Egresos",
                Debe = 0,
                Haber = resumen.TotalEgresos
            });
            asientoCierre.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta = "3.1.00.00",
                DescripcionCuenta = "Resultado del Ejercicio (Pérdida)",
                Debe = Math.Abs(resumen.Resultado),
                Haber = 0
            });
        }

        _comprobantesGuardados.Add(asientoCierre);

        var periodo = _periodosFiscales.FirstOrDefault(p => p.Anio == anio);
        if (periodo == null)
        {
            periodo = new PeriodoFiscal { Anio = anio };
            _periodosFiscales.Add(periodo);
        }
        periodo.Cerrado = true;

        GuardarLista(_comprobantesGuardados, _comprobantesFile);
        GuardarLista(_periodosFiscales, _periodosFile);
    }

    public bool EjercicioCerrado(int anio)
        => _periodosFiscales.Any(p => p.Anio == anio && p.Cerrado);

    public List<PeriodoFiscal> ObtenerPeriodosFiscales()
        => _periodosFiscales.ToList();

    // ─── Cobranza ─────────────────────────────────────────────────────────────

    public List<FacturaCobranza> ObtenerFacturas()
    {
        if (EmpresaActivaId == null) return new List<FacturaCobranza>();

        var hoy = DateTime.Now.Date;
        bool huboActualizacion = false;
        foreach (var f in _facturasCobranza
            .Where(f => f.IdEmpresa == EmpresaActivaId.Value
                     && f.Estado == "Pendiente"
                     && f.FechaVencimiento.Date < hoy))
        {
            f.Estado = "Vencida";
            huboActualizacion = true;
        }
        if (huboActualizacion) GuardarFacturas();

        return _facturasCobranza
            .Where(f => f.IdEmpresa == EmpresaActivaId.Value)
            .OrderByDescending(f => f.FechaEmision)
            .ToList();
    }

    public void GuardarFactura(FacturaCobranza factura)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay empresa activa seleccionada.");

        factura.IdEmpresa = EmpresaActivaId.Value;
        bool esNueva = factura.Id == 0;

        if (esNueva)
        {
            factura.Id = _facturasCobranza.Count > 0 ? _facturasCobranza.Max(f => f.Id) + 1 : 1;
            factura.NumeroFactura = $"FAC-{EmpresaActivaId:D3}-{factura.Id:D4}";

            var idComp = _comprobantesGuardados.Count > 0
                ? _comprobantesGuardados.Max(c => c.IdComprobante) + 1 : 1;

            var compEmision = new ComprobanteContable
            {
                IdComprobante   = idComp,
                IdEmpresa       = EmpresaActivaId.Value,
                Fecha           = factura.FechaEmision,
                Descripcion     = $"Factura {factura.NumeroFactura} – {factura.NombreCliente}",
                TipoComprobante = "Ingreso",
                Estado          = "Registrado"
            };
            compEmision.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta     = "1.1.02.01",
                DescripcionCuenta = "Clientes Nacionales",
                Debe  = factura.Monto,
                Haber = 0
            });
            compEmision.Lineas.Add(new AsientoLinea
            {
                CodigoCuenta     = "4.1.01.01",
                DescripcionCuenta = "Ventas de Bienes / Servicios",
                Debe  = 0,
                Haber = factura.Monto
            });

            _comprobantesGuardados.Add(compEmision);
            GuardarLista(_comprobantesGuardados, _comprobantesFile);
            factura.IdComprobanteEmision = idComp;
        }

        var existente = _facturasCobranza.FirstOrDefault(f => f.Id == factura.Id);
        if (existente != null) _facturasCobranza.Remove(existente);
        _facturasCobranza.Add(factura);
        GuardarFacturas();
    }

    public void MarcarFacturaPagada(int idFactura)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay empresa activa seleccionada.");

        var factura = _facturasCobranza.FirstOrDefault(f => f.Id == idFactura)
            ?? throw new InvalidOperationException("Factura no encontrada.");

        if (factura.Estado == "Pagada")  throw new InvalidOperationException("La factura ya está pagada.");
        if (factura.Estado == "Anulada") throw new InvalidOperationException("No se puede cobrar una factura anulada.");

        var idComp = _comprobantesGuardados.Count > 0
            ? _comprobantesGuardados.Max(c => c.IdComprobante) + 1 : 1;

        var compPago = new ComprobanteContable
        {
            IdComprobante   = idComp,
            IdEmpresa       = EmpresaActivaId.Value,
            Fecha           = DateTime.Now,
            Descripcion     = $"Cobro {factura.NumeroFactura} – {factura.NombreCliente}",
            TipoComprobante = "Ingreso",
            Estado          = "Registrado"
        };
        compPago.Lineas.Add(new AsientoLinea
        {
            CodigoCuenta     = "1.1.01.01",
            DescripcionCuenta = "Caja General",
            Debe  = factura.Monto,
            Haber = 0
        });
        compPago.Lineas.Add(new AsientoLinea
        {
            CodigoCuenta     = "1.1.02.01",
            DescripcionCuenta = "Clientes Nacionales",
            Debe  = 0,
            Haber = factura.Monto
        });

        _comprobantesGuardados.Add(compPago);
        GuardarLista(_comprobantesGuardados, _comprobantesFile);

        factura.Estado            = "Pagada";
        factura.FechaPago         = DateTime.Now;
        factura.IdComprobantePago = idComp;
        GuardarFacturas();
    }

    public void AnularFactura(int idFactura)
    {
        if (EmpresaActivaId == null) throw new InvalidOperationException("No hay empresa activa seleccionada.");

        var factura = _facturasCobranza.FirstOrDefault(f => f.Id == idFactura)
            ?? throw new InvalidOperationException("Factura no encontrada.");

        if (factura.Estado == "Pagada")  throw new InvalidOperationException("No se puede anular una factura pagada. Use Reversión de comprobante.");
        if (factura.Estado == "Anulada") throw new InvalidOperationException("La factura ya está anulada.");

        if (factura.IdComprobanteEmision > 0)
        {
            var compOriginal = _comprobantesGuardados
                .FirstOrDefault(c => c.IdComprobante == factura.IdComprobanteEmision);

            if (compOriginal != null)
            {
                var idComp = _comprobantesGuardados.Max(c => c.IdComprobante) + 1;
                var compAnulacion = new ComprobanteContable
                {
                    IdComprobante   = idComp,
                    IdEmpresa       = EmpresaActivaId.Value,
                    Fecha           = DateTime.Now,
                    Descripcion     = $"Anulación {factura.NumeroFactura} – {factura.NombreCliente}",
                    TipoComprobante = "Egreso",
                    Estado          = "Registrado"
                };
                foreach (var linea in compOriginal.Lineas)
                    compAnulacion.Lineas.Add(new AsientoLinea
                    {
                        CodigoCuenta     = linea.CodigoCuenta,
                        DescripcionCuenta = $"Anulación – {linea.DescripcionCuenta}",
                        Debe  = linea.Haber,
                        Haber = linea.Debe
                    });

                compOriginal.Estado = "Anulado";
                _comprobantesGuardados.Add(compAnulacion);
                GuardarLista(_comprobantesGuardados, _comprobantesFile);
            }
        }

        factura.Estado = "Anulada";
        GuardarFacturas();
    }
}
