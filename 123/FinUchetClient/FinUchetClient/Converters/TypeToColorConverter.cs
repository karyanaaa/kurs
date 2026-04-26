using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FinUchetClient.Converters
{
    public class TypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type.ToLower() == "income"
                    ? new SolidColorBrush(Color.FromRgb(46, 204, 113))  // Зеленый
                    : new SolidColorBrush(Color.FromRgb(231, 76, 60));  // Красный
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}