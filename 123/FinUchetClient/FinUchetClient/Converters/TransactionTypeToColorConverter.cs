using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FinUchetClient.Converters
{
    public class TransactionTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type.ToLower() == "income"
                    ? new SolidColorBrush(Color.FromRgb(76, 175, 80))   // Зеленый
                    : new SolidColorBrush(Color.FromRgb(244, 67, 54));  // Красный
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}