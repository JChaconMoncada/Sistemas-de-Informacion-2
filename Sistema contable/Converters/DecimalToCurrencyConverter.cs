using System;
using System.Globalization;
using System.Windows.Data;

namespace Sistema_contable.Converters
{
    public class DecimalToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return $"Bs. {decimalValue:N2}";
            }
            return "Bs. 0,00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                stringValue = stringValue.Replace("Bs.", "").Replace(",", "").Trim();
                if (decimal.TryParse(stringValue, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }
}
