namespace SistemaContableZulay.UI.Domain
{
    public class PartidaReexpresion : Sistema_contable.ViewModels.ViewModelBase
    {
        public int Id { get; set; }
        private bool _aplicar = false;
        private string _codigo = string.Empty;
        private string _nombre = string.Empty;
        private string _tipo = string.Empty;
        private decimal _valorOriginal;
        private decimal _factor = 1m;
        private decimal _valorAjustado;
        private decimal _diferencia;
        private string _moneda = "Bs";

        public bool Aplicar
        {
            get => _aplicar;
            set => SetProperty(ref _aplicar, value);
        }

        public string Codigo
        {
            get => _codigo;
            set => SetProperty(ref _codigo, value);
        }

        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        public string Tipo
        {
            get => _tipo;
            set => SetProperty(ref _tipo, value);
        }

        public string Moneda
        {
            get => _moneda;
            set => SetProperty(ref _moneda, value);
        }

        public decimal ValorOriginal
        {
            get => _valorOriginal;
            set => SetProperty(ref _valorOriginal, value);
        }

        public decimal Factor
        {
            get => _factor;
            set
            {
                if (SetProperty(ref _factor, value))
                {
                    CalcularAjuste();
                }
            }
        }

        public decimal ValorAjustado
        {
            get => _valorAjustado;
            set => SetProperty(ref _valorAjustado, value);
        }

        public decimal Diferencia
        {
            get => _diferencia;
            set => SetProperty(ref _diferencia, value);
        }

        public void CalcularAjuste()
        {
            ValorAjustado = ValorOriginal * Factor;
            Diferencia = ValorAjustado - ValorOriginal;
        }
    }
}
