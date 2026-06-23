using Sistema_contable.Models;
using System.Collections.ObjectModel;

namespace Sistema_contable.ViewModels
{
    public class EmpresasViewModel : ViewModelBase
    {
        private Empresa _empresaSeleccionada;
        private string _filtroTexto;

        public ObservableCollection<Empresa> Empresas { get; set; }

        public Empresa EmpresaSeleccionada
        {
            get => _empresaSeleccionada;
            set => SetProperty(ref _empresaSeleccionada, value);
        }

        public string FiltroTexto
        {
            get => _filtroTexto;
            set => SetProperty(ref _filtroTexto, value);
        }

        public EmpresasViewModel()
        {
            Empresas = new ObservableCollection<Empresa>();
        }
    }
}
