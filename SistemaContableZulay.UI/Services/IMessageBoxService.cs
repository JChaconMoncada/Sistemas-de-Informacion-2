namespace SistemaContableZulay.UI.Services;

public enum TipoAlerta
{
    Informativo,
    Advertencia,
    Error,
    Exito
}

public interface IMessageBoxService
{
    void MostrarAlerta(string mensaje, TipoAlerta tipo);
}
