using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

    private List<ComprobanteContable> _comprobantesGuardados = new();
    private List<CuentaContable> _cuentasGuardadas = new();
    private List<EmpresaCliente> _empresasGuardadas = new();

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

        CargarDatos();
    }

    private void CargarDatos()
    {
        _empresasGuardadas = CargarLista<EmpresaCliente>(_empresasFile) ?? new List<EmpresaCliente>();
        _cuentasGuardadas = CargarLista<CuentaContable>(_cuentasFile) ?? new List<CuentaContable>();
        _comprobantesGuardados = CargarLista<ComprobanteContable>(_comprobantesFile) ?? new List<ComprobanteContable>();

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

    public void EliminarComprobante(int idComprobante)
    {
        var existente = _comprobantesGuardados.FirstOrDefault(c => c.IdComprobante == idComprobante);
        if (existente != null)
        {
            _comprobantesGuardados.Remove(existente);
            GuardarLista(_comprobantesGuardados, _comprobantesFile);
        }
    }

    public string EjecutarRespaldoAutomatico()
    {
        try
        {
            var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupDir, $"Respaldo_Reexpresion_{timestamp}.zip");

            if (Directory.Exists(_datosPath))
            {
                ZipFile.CreateFromDirectory(_datosPath, backupPath);
                return backupPath;
            }
            return string.Empty;
        }
        catch (Exception)
        {
            // Podríamos registrar el error
            return string.Empty;
        }
    }

    public decimal ObtenerSaldoCuentaAFecha(string codigoCuenta, DateTime fechaCorte)
    {
        if (EmpresaActivaId == null) return 0m;

        var cuenta = _cuentasGuardadas.FirstOrDefault(c => c.Codigo == codigoCuenta);
        if (cuenta == null) return 0m;

        // Comprobantes registrados de la empresa activa hasta la fecha de corte
        var comprobantes = _comprobantesGuardados
            .Where(c => c.IdEmpresa == EmpresaActivaId.Value && c.Fecha.Date <= fechaCorte.Date)
            .ToList(); // Eliminamos el filtro de "Estado" temporalmente si no se está usando estrictamente

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

        // Naturaleza de las cuentas:
        // Activos y Egresos aumentan por el Debe
        if (cuenta.Tipo == "Activo" || cuenta.Tipo == "Egreso")
        {
            return totalDebe - totalHaber;
        }
        
        // Pasivos, Patrimonio e Ingresos aumentan por el Haber
        if (cuenta.Tipo == "Pasivo" || cuenta.Tipo == "Patrimonio" || cuenta.Tipo == "Ingreso")
        {
            return totalHaber - totalDebe;
        }

        return 0m;
    }
}
