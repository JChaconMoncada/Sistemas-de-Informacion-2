using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels;

public class AsientoViewModel : ViewModelBase
{
    private readonly ContabilidadService _contabilidadService;
    private readonly IMessageBoxService _messageBoxService;
    private ComprobanteContable _comprobanteActual;

    private decimal _totalDebe;
    private decimal _totalHaber;

    public ObservableCollection<AsientoLineaViewModel> Lineas { get; }
    public ObservableCollection<CuentaContable> CuentasDisponibles { get; }
    public ObservableCollection<EmpresaCliente> EmpresasDisponibles { get; }
    public ObservableCollection<string> TiposComprobante { get; }

    public decimal TotalDebe
    {
        get => _totalDebe;
        private set => SetProperty(ref _totalDebe, value);
    }

    public decimal TotalHaber
    {
        get => _totalHaber;
        private set => SetProperty(ref _totalHaber, value);
    }

    public string EstadoComprobante => _comprobanteActual.Estado;

    public ICommand ComandoGuardar { get; }
    public ICommand AgregarLineaCommand { get; }

    public AsientoViewModel(ContabilidadService contabilidadService, IMessageBoxService messageBoxService)
    {
        _contabilidadService = contabilidadService;
        _messageBoxService = messageBoxService;
        _comprobanteActual = new ComprobanteContable { Fecha = DateTime.Now };

        Lineas = new ObservableCollection<AsientoLineaViewModel>();
        CuentasDisponibles = new ObservableCollection<CuentaContable>(_contabilidadService.ObtenerCuentasContables());
        EmpresasDisponibles = new ObservableCollection<EmpresaCliente>(_contabilidadService.ObtenerEmpresas());
        TiposComprobante = new ObservableCollection<string>(_contabilidadService.ObtenerTiposComprobante());

        ComandoGuardar = new RelayCommand(ComandoGuardarExecute);
        AgregarLineaCommand = new RelayCommand(AgregarLineaBlanco);

        AgregarLineaBlanco();
    }

    public void CalcularTotales()
    {
        TotalDebe = Lineas.Sum(l => l.Debe);
        TotalHaber = Lineas.Sum(l => l.Haber);
    }

    public bool ValidarPartidaDoble()
    {
        CalcularTotales();
        return TotalDebe == TotalHaber && TotalDebe > 0;
    }

    private void AgregarLineaBlanco()
    {
        var lineaVm = new AsientoLineaViewModel(new AsientoLinea(), CalcularTotales);
        Lineas.Add(lineaVm);
    }

    private void ComandoGuardarExecute()
    {
        if (!ValidarPartidaDoble())
        {
            _messageBoxService.MostrarAlerta(
                "El asiento está descuadrado o vacío. Verifique que Total Debe sea igual a Total Haber y mayor que cero.",
                TipoAlerta.Error);
            return;
        }

        _comprobanteActual.Lineas = Lineas.Select(l => l.GetModel()).ToList();
        _comprobanteActual.ValidarComprobante();

        _contabilidadService.GuardarComprobante(_comprobanteActual);
        _comprobanteActual.RegistrarComprobante();

        _messageBoxService.MostrarAlerta(
            $"Asiento guardado correctamente. Estado actual: {_comprobanteActual.Estado}",
            TipoAlerta.Exito);

        ReiniciarFormulario();
    }

    private void ReiniciarFormulario()
    {
        Lineas.Clear();
        TotalDebe = 0;
        TotalHaber = 0;
        _comprobanteActual = new ComprobanteContable { Fecha = DateTime.Now };
        OnPropertyChanged(nameof(EstadoComprobante));
        AgregarLineaBlanco();
    }
}
