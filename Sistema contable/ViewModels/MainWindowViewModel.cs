using Sistema_contable.Models;
using System.Collections.ObjectModel;
using SistemaContableZulay.UI.Services;
using System.Linq;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;

namespace Sistema_contable.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;
        private EmpresaCliente _empresaSeleccionada;
        private string _textoEstado;

        public ObservableCollection<EmpresaCliente> Empresas { get; set; }

        public EmpresaCliente EmpresaSeleccionada
        {
            get => _empresaSeleccionada;
            set
            {
                if (SetProperty(ref _empresaSeleccionada, value))
                {
                    if (value != null)
                    {
                        _contabilidadService.SeleccionarEmpresa(value.Id);
                        TextoEstado = $"Empresa: {value.NombreEmpresa} (RIF: {value.Rif})";
                    }
                    else
                    {
                        _contabilidadService.SeleccionarEmpresa(null);
                        TextoEstado = "Empresa: Sin empresa seleccionada";
                    }
                }
            }
        }

        public string TextoEstado
        {
            get => _textoEstado;
            set => SetProperty(ref _textoEstado, value);
        }

        public MainWindowViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Empresas = new ObservableCollection<EmpresaCliente>();
            TextoEstado = "Empresa: Sin empresa seleccionada";

            _contabilidadService.OnEmpresasModificadas += CargarEmpresas;
            CargarEmpresas();
        }

        private void CargarEmpresas()
        {
            var empresas = _contabilidadService.ObtenerEmpresas();
            Empresas.Clear();
            foreach (var e in empresas)
            {
                Empresas.Add(e);
            }

            // Restore selection if there's an active company
            if (_contabilidadService.EmpresaActivaId.HasValue)
            {
                _empresaSeleccionada = Empresas.FirstOrDefault(e => e.Id == _contabilidadService.EmpresaActivaId.Value);
                OnPropertyChanged(nameof(EmpresaSeleccionada));
            }
        }
    }
}
