using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Sistema_contable.Models;
using SistemaContableZulay.UI.Services;

namespace Sistema_contable.ViewModels
{
    public class DocumentosViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;

        private ObservableCollection<Documento> _documentos;
        private ObservableCollection<Documento> _documentosFiltrados;
        private Documento _documentoSeleccionado;
        private string _filtroTexto;
        private string _filtroEstado = "Todos los estados";
        private string _filtroTipo = "Todos los tipos";
        private bool _estaEnModoEdicion;
        private string _tituloFormulario;

        // Campos de edición
        private int _editId;
        private string _editTipoDocumento;
        private string _editDescripcion;
        private DateTime? _editFechaRecepcion = DateTime.Now;
        private DateTime? _editFechaEntrega;
        private string _editEstado = "En Revisión";
        private string _editObservaciones;
        private string _editArchivoAdjunto;

        public ObservableCollection<Documento> DocumentosFiltrados
        {
            get => _documentosFiltrados;
            set
            {
                if (SetProperty(ref _documentosFiltrados, value))
                {
                    OnPropertyChanged(nameof(SinDocumentosVisibility));
                }
            }
        }

        public System.Windows.Visibility SinDocumentosVisibility =>
            (DocumentosFiltrados == null || DocumentosFiltrados.Count == 0)
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

        public Documento DocumentoSeleccionado
        {
            get => _documentoSeleccionado;
            set => SetProperty(ref _documentoSeleccionado, value);
        }

        public string FiltroTexto
        {
            get => _filtroTexto;
            set { if (SetProperty(ref _filtroTexto, value)) AplicarFiltro(); }
        }

        public string FiltroEstado
        {
            get => _filtroEstado;
            set { if (SetProperty(ref _filtroEstado, value)) AplicarFiltro(); }
        }

        public string FiltroTipo
        {
            get => _filtroTipo;
            set { if (SetProperty(ref _filtroTipo, value)) AplicarFiltro(); }
        }

        public bool EstaEnModoEdicion
        {
            get => _estaEnModoEdicion;
            set => SetProperty(ref _estaEnModoEdicion, value);
        }

        public string TituloFormulario
        {
            get => _tituloFormulario;
            set => SetProperty(ref _tituloFormulario, value);
        }

        public ObservableCollection<string> EstadosDisponibles { get; } = new ObservableCollection<string>
        {
            "Todos los estados", "Recibido", "En Revisión", "Revisado", "Entregado"
        };

        public ObservableCollection<string> TiposDocumento { get; } = new ObservableCollection<string>
        {
            "Todos los tipos", "Declaración IVA", "Declaración ISLR", "Balance", "Factura", "Otro"
        };

        public ObservableCollection<string> EstadosEdicion { get; } = new ObservableCollection<string>
        {
            "Recibido", "En Revisión", "Revisado", "Entregado"
        };

        public ObservableCollection<string> TiposDocumentoEdicion { get; } = new ObservableCollection<string>
        {
            "Declaración IVA", "Declaración ISLR", "Balance", "Factura", "Otro"
        };

        public string EditTipoDocumento
        {
            get => _editTipoDocumento;
            set => SetProperty(ref _editTipoDocumento, value);
        }

        public string EditDescripcion
        {
            get => _editDescripcion;
            set => SetProperty(ref _editDescripcion, value);
        }

        public DateTime? EditFechaRecepcion
        {
            get => _editFechaRecepcion;
            set => SetProperty(ref _editFechaRecepcion, value);
        }

        public DateTime? EditFechaEntrega
        {
            get => _editFechaEntrega;
            set => SetProperty(ref _editFechaEntrega, value);
        }

        public string EditEstado
        {
            get => _editEstado;
            set => SetProperty(ref _editEstado, value);
        }

        public string EditObservaciones
        {
            get => _editObservaciones;
            set => SetProperty(ref _editObservaciones, value);
        }

        public string EditArchivoAdjunto
        {
            get => _editArchivoAdjunto;
            set => SetProperty(ref _editArchivoAdjunto, value);
        }

        public ICommand NuevoDocumentoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand AdjuntarArchivoCommand { get; }
        public ICommand VerArchivoCommand { get; }
        public ICommand AplicarFiltroCommand { get; }

        public DocumentosViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            _documentos = new ObservableCollection<Documento>();
            DocumentosFiltrados = new ObservableCollection<Documento>();

            _contabilidadService.OnEmpresaCambiada += CargarDocumentos;
            CargarDocumentos();

            NuevoDocumentoCommand = new RelayCommand(NuevoDocumento);
            EditarCommand = new RelayCommand<Documento>(EditarDocumento);
            GuardarCommand = new RelayCommand(GuardarDocumento);
            CancelarCommand = new RelayCommand(() => EstaEnModoEdicion = false);
            EliminarCommand = new RelayCommand<Documento>(EliminarDocumento);
            AdjuntarArchivoCommand = new RelayCommand(AdjuntarArchivo);
            VerArchivoCommand = new RelayCommand(VerArchivo);
            AplicarFiltroCommand = new RelayCommand(AplicarFiltro);
        }

        private void CargarDocumentos()
        {
            var empresas = _contabilidadService.ObtenerEmpresas();
            var documentos = _contabilidadService.ObtenerDocumentos();
            foreach (var doc in documentos)
            {
                doc.NombreEmpresa = empresas.FirstOrDefault(e => e.Id == doc.EmpresaId)?.NombreEmpresa ?? "—";
            }
            _documentos = new ObservableCollection<Documento>(documentos);
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            var query = _documentos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FiltroTexto))
                query = query.Where(d => (d.Descripcion ?? "").Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(FiltroEstado) && FiltroEstado != "Todos los estados")
                query = query.Where(d => d.Estado == FiltroEstado);

            if (!string.IsNullOrEmpty(FiltroTipo) && FiltroTipo != "Todos los tipos")
                query = query.Where(d => d.TipoDocumento == FiltroTipo);

            DocumentosFiltrados = new ObservableCollection<Documento>(query);
        }

        private void NuevoDocumento()
        {
            if (_contabilidadService.EmpresaActivaId == null)
            {
                System.Windows.MessageBox.Show("Seleccione una empresa activa antes de crear un documento.", "Empresa requerida", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            _editId = 0;
            EditTipoDocumento = TiposDocumentoEdicion.First();
            EditDescripcion = string.Empty;
            EditFechaRecepcion = DateTime.Now;
            EditFechaEntrega = null;
            EditEstado = "En Revisión";
            EditObservaciones = string.Empty;
            EditArchivoAdjunto = string.Empty;
            TituloFormulario = "Nuevo Documento";
            EstaEnModoEdicion = true;
        }

        private void EditarDocumento(Documento doc)
        {
            if (doc == null) return;

            _editId = doc.Id;
            EditTipoDocumento = doc.TipoDocumento;
            EditDescripcion = doc.Descripcion;
            EditFechaRecepcion = doc.FechaRecepcion;
            EditFechaEntrega = doc.FechaEntrega;
            EditEstado = doc.Estado;
            EditObservaciones = doc.Observaciones;
            EditArchivoAdjunto = string.Empty;
            TituloFormulario = $"Editar Documento #{doc.Id}";
            EstaEnModoEdicion = true;
        }

        private void GuardarDocumento()
        {
            if (string.IsNullOrWhiteSpace(EditDescripcion))
            {
                System.Windows.MessageBox.Show("La descripción del documento es obligatoria.", "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            if (_contabilidadService.EmpresaActivaId == null)
            {
                System.Windows.MessageBox.Show("No hay una empresa activa seleccionada.", "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            try
            {
                var doc = new Documento
                {
                    Id = _editId,
                    EmpresaId = _contabilidadService.EmpresaActivaId.Value,
                    TipoDocumento = EditTipoDocumento,
                    Descripcion = EditDescripcion,
                    FechaRecepcion = EditFechaRecepcion,
                    FechaEntrega = EditFechaEntrega,
                    Estado = EditEstado,
                    Observaciones = EditObservaciones
                };

                _contabilidadService.GuardarDocumento(doc);
                CargarDocumentos();
                EstaEnModoEdicion = false;
                System.Windows.MessageBox.Show("Documento guardado correctamente.", "Guardado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al guardar el documento: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void EliminarDocumento(Documento doc)
        {
            if (doc == null) return;

            var resultado = System.Windows.MessageBox.Show(
                $"¿Está seguro de eliminar el documento \"{doc.Descripcion}\"? Esta acción no se puede deshacer.",
                "Confirmar eliminación", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (resultado != System.Windows.MessageBoxResult.Yes) return;

            _contabilidadService.EliminarDocumento(doc.Id);
            CargarDocumentos();
        }

        private void AdjuntarArchivo()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Todos los archivos (*.*)|*.*",
                Title = "Seleccionar archivo a adjuntar"
            };

            if (dialog.ShowDialog() == true)
            {
                EditArchivoAdjunto = dialog.FileName;
            }
        }

        private void VerArchivo()
        {
            if (string.IsNullOrEmpty(EditArchivoAdjunto) || !File.Exists(EditArchivoAdjunto))
            {
                System.Windows.MessageBox.Show("No hay un archivo adjunto válido para mostrar.", "Sin archivo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(EditArchivoAdjunto) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"No se pudo abrir el archivo: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
