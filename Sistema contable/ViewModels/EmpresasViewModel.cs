using System.Collections.ObjectModel;
using SistemaContableZulay.UI.Services;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Text.RegularExpressions;
using SistemaContableZulay.UI.Domain;

namespace Sistema_contable.ViewModels
{
    public class EmpresasViewModel : ViewModelBase
    {
        private EmpresaCliente _empresaSeleccionada;
        private string _filtroTexto;
        private readonly ContabilidadService _contabilidadService;

        public ObservableCollection<EmpresaCliente> Empresas { get; set; }

        public EmpresaCliente EmpresaSeleccionada
        {
            get => _empresaSeleccionada;
            set => SetProperty(ref _empresaSeleccionada, value);
        }

        public string FiltroTexto
        {
            get => _filtroTexto;
            set => SetProperty(ref _filtroTexto, value);
        }

        public ICommand NuevaEmpresaCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand EliminarCommand { get; }

        public EmpresasViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Empresas = new ObservableCollection<EmpresaCliente>();
            
            NuevaEmpresaCommand = new RelayCommand(() => NuevaEmpresa());
            GuardarCommand = new RelayCommand(() => GuardarEmpresa());
            EliminarCommand = new RelayCommand<EmpresaCliente>(emp => EliminarEmpresa(emp));

            CargarEmpresas();
        }

        private void CargarEmpresas()
        {
            var empresasCliente = _contabilidadService.ObtenerEmpresas();
            Empresas.Clear();
            foreach (var ec in empresasCliente)
            {
                Empresas.Add(ec);
            }
        }

        private void NuevaEmpresa()
        {
            // Buscar el menor ID disponible (llenar huecos)
            int nextId = 1;
            var idsEnUso = Empresas.Select(e => e.Id).Where(id => id > 0).Distinct().OrderBy(id => id).ToList();
            
            foreach (var id in idsEnUso)
            {
                if (id == nextId)
                {
                    nextId++;
                }
                else
                {
                    break;
                }
            }

            EmpresaSeleccionada = new EmpresaCliente { Id = nextId };
            Empresas.Add(EmpresaSeleccionada);
        }

        private void GuardarEmpresa()
        {
            if (EmpresaSeleccionada == null)
            {
                MessageBox.Show("Por favor, seleccione o cree una empresa antes de guardar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Validaciones
            if (string.IsNullOrWhiteSpace(EmpresaSeleccionada.NombreEmpresa) ||
                string.IsNullOrWhiteSpace(EmpresaSeleccionada.Rif) ||
                string.IsNullOrWhiteSpace(EmpresaSeleccionada.RazonSocial) ||
                string.IsNullOrWhiteSpace(EmpresaSeleccionada.Telefono) ||
                string.IsNullOrWhiteSpace(EmpresaSeleccionada.Email))
            {
                MessageBox.Show("Todos los campos (Nombre, RIF, Razón Social, Teléfono, Email) son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Regex Teléfono: permite números, espacios, guiones y un prefijo '+' opcional.
            if (!Regex.IsMatch(EmpresaSeleccionada.Telefono, @"^\+?[0-9\s\-()]+$"))
            {
                MessageBox.Show("El teléfono ingresado tiene un formato inválido. Asegúrese de colocar solo números o el código de país (ej. +57).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _contabilidadService.GuardarEmpresa(EmpresaSeleccionada);
            MessageBox.Show("Empresa guardada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            CargarEmpresas(); 
        }

        private void EliminarEmpresa(EmpresaCliente? emp = null)
        {
            var target = emp ?? EmpresaSeleccionada;
            if (target != null && target.Id > 0)
            {
                var result = MessageBox.Show($"¿Está seguro que desea eliminar la empresa {target.NombreEmpresa}?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _contabilidadService.EliminarEmpresa(target.Id);
                    CargarEmpresas();
                    if (EmpresaSeleccionada == target)
                    {
                        EmpresaSeleccionada = null;
                    }
                }
            }
            else if (target != null && target.Id == 0)
            {
                // Si es una fila nueva que no se ha guardado
                Empresas.Remove(target);
                if (EmpresaSeleccionada == target)
                {
                    EmpresaSeleccionada = null;
                }
            }
        }
    }
}
