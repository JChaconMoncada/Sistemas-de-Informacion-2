using SistemaContableZulay.UI.Domain;

namespace SistemaContableZulay.UI.ViewModels;

public class AsientoLineaViewModel : ViewModelBase
{
    private readonly AsientoLinea _model;
    private readonly Action _onTotalChanged;

    public AsientoLineaViewModel(AsientoLinea model, Action onTotalChanged)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _onTotalChanged = onTotalChanged ?? throw new ArgumentNullException(nameof(onTotalChanged));
    }

    public string CodigoCuenta
    {
        get => _model.CodigoCuenta;
        set
        {
            if (_model.CodigoCuenta == value)
                return;

            _model.CodigoCuenta = value;
            OnPropertyChanged();
        }
    }

    public string DescripcionCuenta
    {
        get => _model.DescripcionCuenta;
        set
        {
            if (_model.DescripcionCuenta == value)
                return;

            _model.DescripcionCuenta = value;
            OnPropertyChanged();
        }
    }

    public decimal Debe
    {
        get => _model.Debe;
        set
        {
            if (_model.Debe == value)
                return;

            _model.Debe = value;
            OnPropertyChanged();
            _onTotalChanged();
        }
    }

    public decimal Haber
    {
        get => _model.Haber;
        set
        {
            if (_model.Haber == value)
                return;

            _model.Haber = value;
            OnPropertyChanged();
            _onTotalChanged();
        }
    }

    public AsientoLinea GetModel() => _model;
}
