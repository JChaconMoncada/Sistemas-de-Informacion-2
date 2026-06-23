using Sistema_contable.Models;
using System;
using System.Collections.ObjectModel;

namespace Sistema_contable.ViewModels
{
    public class ComprobantesViewModel : ViewModelBase
    {
        private Asiento _asientoActual;
        private decimal _totalDebe;
        private decimal _totalHaber;
        private bool _estaCuadrado;

        public Asiento AsientoActual
        {
            get => _asientoActual;
            set => SetProperty(ref _asientoActual, value);
        }

        public ObservableCollection<DetalleAsiento> Detalles { get; set; }

        public decimal TotalDebe
        {
            get => _totalDebe;
            set => SetProperty(ref _totalDebe, value);
        }

        public decimal TotalHaber
        {
            get => _totalHaber;
            set => SetProperty(ref _totalHaber, value);
        }

        public bool EstaCuadrado
        {
            get => _estaCuadrado;
            set => SetProperty(ref _estaCuadrado, value);
        }

        public ComprobantesViewModel()
        {
            AsientoActual = new Asiento
            {
                Fecha = DateTime.Now,
                Numero = 1,
                Tipo = "Manual"
            };
            Detalles = new ObservableCollection<DetalleAsiento>();
        }
    }
}
