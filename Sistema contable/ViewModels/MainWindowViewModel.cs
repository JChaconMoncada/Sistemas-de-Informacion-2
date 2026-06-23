using Sistema_contable.Models;
using System.Collections.ObjectModel;

namespace Sistema_contable.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Empresa _empresaActiva;
        private string _statusBackup;

        public Empresa EmpresaActiva
        {
            get => _empresaActiva;
            set => SetProperty(ref _empresaActiva, value);
        }

        public string StatusBackup
        {
            get => _statusBackup;
            set => SetProperty(ref _statusBackup, value);
        }

        public ObservableCollection<Empresa> Empresas { get; set; }

        public MainWindowViewModel()
        {
            Empresas = new ObservableCollection<Empresa>();
            StatusBackup = "Backup: No realizado";
        }
    }
}
