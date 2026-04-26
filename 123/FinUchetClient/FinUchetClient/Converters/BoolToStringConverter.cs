using System;
using System.Globalization;
using System.Windows.Data;

namespace FinUchetClient.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string param)
            {
                var options = param.Split('|');
                if (options.Length == 2)
                {
                    return boolValue ? options[0] : options[1];
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}