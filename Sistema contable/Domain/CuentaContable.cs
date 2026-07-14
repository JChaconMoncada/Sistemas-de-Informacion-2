namespace SistemaContableZulay.UI.Domain;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
public class CuentaContable : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Activo";
    public int Nivel { get; set; } = 1;
    public string CuentaPadre { get; set; } = string.Empty;
    public bool AceptaMovimiento { get; set; } = true;
    public bool Activo { get; set; } = true;
    public ObservableCollection<CuentaContable> Hijos { get; set; } = new ObservableCollection<CuentaContable>(); 
    public string Descripcion { get; set; } = string.Empty;
    public string DisplayName => $"{Codigo} - {Nombre}";
    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                OnPropertyChanged(); // Le avisa a WPF que la visibilidad cambió
            }
        }
    }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged(); // Le avisa a WPF que expanda o contraiga la rama
            }
        }
    }


    // --- MOTOR DE NOTIFICACIONES DE WPF ---
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

 }
