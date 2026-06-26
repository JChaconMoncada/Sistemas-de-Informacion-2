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
        private readonly ContabilidadService _contabilidadService;

        // ─── Lista ────────────────────────────────────────────────────────────────
        public ObservableCollection<EmpresaCliente> Empresas { get; set; }

        public EmpresaCliente EmpresaSeleccionada
        {
            get => _empresaSeleccionada;
            set => SetProperty(ref _empresaSeleccionada, value);
        }

        // ─── Estado del formulario ────────────────────────────────────────────────
        private bool _estaEnModoEdicion;
        public bool EstaEnModoEdicion
        {
            get => _estaEnModoEdicion;
            set => SetProperty(ref _estaEnModoEdicion, value);
        }

        private bool _esNuevaEmpresa;
        public bool EsNuevaEmpresa
        {
            get => _esNuevaEmpresa;
            set
            {
                if (SetProperty(ref _esNuevaEmpresa, value))
                    OnPropertyChanged(nameof(TituloFormulario));
            }
        }

        public string TituloFormulario => EsNuevaEmpresa ? "Nueva Empresa" : "Editar Empresa";

        private int _editandoId;

        // ─── Campos del formulario ────────────────────────────────────────────────
        private string _editNombre = string.Empty;
        public string EditNombre
        {
            get => _editNombre;
            set => SetProperty(ref _editNombre, value);
        }

        private string _editRif = string.Empty;
        public string EditRif
        {
            get => _editRif;
            set => SetProperty(ref _editRif, value);
        }

        private string _editRazonSocial = string.Empty;
        public string EditRazonSocial
        {
            get => _editRazonSocial;
            set => SetProperty(ref _editRazonSocial, value);
        }

        private string _editDireccion = string.Empty;
        public string EditDireccion
        {
            get => _editDireccion;
            set => SetProperty(ref _editDireccion, value);
        }

        private string _editTelefono = string.Empty;
        public string EditTelefono
        {
            get => _editTelefono;
            set => SetProperty(ref _editTelefono, value);
        }

        private string _editEmail = string.Empty;
        public string EditEmail
        {
            get => _editEmail;
            set => SetProperty(ref _editEmail, value);
        }

        // ─── Comandos ─────────────────────────────────────────────────────────────
        public ICommand NuevaEmpresaCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand EliminarCommand { get; }

        public EmpresasViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Empresas = new ObservableCollection<EmpresaCliente>();

            NuevaEmpresaCommand   = new RelayCommand(() => AbrirFormularioNuevo());
            EditarCommand         = new RelayCommand<EmpresaCliente>(emp => AbrirFormularioEditar(emp));
            GuardarCommand        = new RelayCommand(() => GuardarEmpresa(), () => EstaEnModoEdicion);
            CancelarEdicionCommand = new RelayCommand(() => CerrarFormulario());
            EliminarCommand       = new RelayCommand<EmpresaCliente>(emp => EliminarEmpresa(emp));

            CargarEmpresas();
        }

        private void CargarEmpresas()
        {
            var empresasCliente = _contabilidadService.ObtenerEmpresas();
            Empresas.Clear();
            foreach (var ec in empresasCliente)
                Empresas.Add(ec);
        }

        private void AbrirFormularioNuevo()
        {
            _editandoId = 0;
            EsNuevaEmpresa = true;
            EditNombre = string.Empty;
            EditRif = string.Empty;
            EditRazonSocial = string.Empty;
            EditDireccion = string.Empty;
            EditTelefono = string.Empty;
            EditEmail = string.Empty;
            EstaEnModoEdicion = true;
        }

        private void AbrirFormularioEditar(EmpresaCliente? emp)
        {
            var target = emp ?? EmpresaSeleccionada;
            if (target == null) return;

            _editandoId = target.Id;
            EsNuevaEmpresa = false;
            EditNombre = target.NombreEmpresa;
            EditRif = target.Rif;
            EditRazonSocial = target.RazonSocial;
            EditDireccion = target.Direccion;
            EditTelefono = target.Telefono;
            EditEmail = target.Email;
            EstaEnModoEdicion = true;
        }

        private void CerrarFormulario()
        {
            EstaEnModoEdicion = false;
            EsNuevaEmpresa = false;
        }

        private void GuardarEmpresa()
        {
            if (string.IsNullOrWhiteSpace(EditNombre) ||
                string.IsNullOrWhiteSpace(EditRif) ||
                string.IsNullOrWhiteSpace(EditRazonSocial) ||
                string.IsNullOrWhiteSpace(EditTelefono) ||
                string.IsNullOrWhiteSpace(EditEmail))
            {
                MessageBox.Show("Los campos Nombre, RIF, Razón Social, Teléfono y Email son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(EditTelefono, @"^\+?[0-9\s\-()]+$"))
            {
                MessageBox.Show("Formato de teléfono inválido. Use solo números, espacios, guiones o prefijo '+' (ej. +58 412-1234567).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rifNormalizado = EditRif.Trim().ToUpperInvariant();
            var nombreNormalizado = EditNombre.Trim().ToUpperInvariant();

            var duplicadoRif = Empresas.FirstOrDefault(e =>
                e.Id != _editandoId &&
                e.Rif.Trim().ToUpperInvariant() == rifNormalizado);

            if (duplicadoRif != null)
            {
                MessageBox.Show($"Ya existe una empresa registrada con el RIF «{EditRif.Trim()}»:\n\n  {duplicadoRif.NombreEmpresa}\n\nCada empresa debe tener un RIF único.", "RIF Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var duplicadoNombre = Empresas.FirstOrDefault(e =>
                e.Id != _editandoId &&
                e.NombreEmpresa.Trim().ToUpperInvariant() == nombreNormalizado);

            if (duplicadoNombre != null)
            {
                MessageBox.Show($"Ya existe una empresa con el nombre «{EditNombre.Trim()}» (ID {duplicadoNombre.Id}).\n\nVerifique que el nombre sea correcto o use uno diferente.", "Nombre Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var empresa = new EmpresaCliente
            {
                Id            = _editandoId,
                NombreEmpresa = EditNombre.Trim(),
                Rif           = EditRif.Trim().ToUpperInvariant(),
                RazonSocial   = EditRazonSocial.Trim(),
                Direccion     = EditDireccion.Trim(),
                Telefono      = EditTelefono.Trim(),
                Email         = EditEmail.Trim()
            };

            _contabilidadService.GuardarEmpresa(empresa);
            MessageBox.Show("Empresa guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            CerrarFormulario();
            CargarEmpresas();
        }

        private void EliminarEmpresa(EmpresaCliente? emp)
        {
            var target = emp ?? EmpresaSeleccionada;
            if (target == null || target.Id == 0) return;

            var result = MessageBox.Show(
                $"¿Eliminar la empresa «{target.NombreEmpresa}»?\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _contabilidadService.EliminarEmpresa(target.Id);
                if (EmpresaSeleccionada == target) EmpresaSeleccionada = null;
                if (EstaEnModoEdicion && _editandoId == target.Id) CerrarFormulario();
                CargarEmpresas();
            }
        }
    }
}
