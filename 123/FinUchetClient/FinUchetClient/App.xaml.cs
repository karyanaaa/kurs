using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using FinUchetClient.Services;

namespace FinUchetClient
{
    public partial class App : Application
    {
        public string Token { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Загружаем сохранённую тему
            ThemeManager.LoadThemePreference();
            // Создаем ресурсы программно
            // Добавьте эти конвертеры
            Resources.Add("ProfitLossColorConverter", new ProfitLossColorConverter());
            Resources.Add("ProfitPercentConverter", new ProfitPercentConverter());
            Resources.Add("BoolToStringConverter", new BoolToStringConverter());
            Resources.Add("TypeToColorConverter", new TypeToColorConverter());
            Resources.Add("DateTimeToStringConverter", new DateTimeToStringConverter());
            Resources.Add("NullToVisibilityConverter", new NullToVisibilityConverter());
            Resources.Add("TransactionTypeToColorConverter", new TransactionTypeToColorConverter());
            Resources.Add("BalanceToColorConverter", new BalanceToColorConverter());

            var apiService = new ApiService();
            var authService = new AuthService(apiService);

            var loginWindow = new Views.LoginWindow(authService, apiService);
            loginWindow.Show();
        }
    }

    // Конвертеры
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string param)
            {
                var options = param.Split('|');
                if (options.Length == 2)
                    return boolValue ? options[0] : options[1];
            }
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class TypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string type && type.ToLower() == "income"
                ? new SolidColorBrush(Color.FromRgb(46, 204, 113))
                : new SolidColorBrush(Color.FromRgb(231, 76, 60));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is DateTime dateTime ? dateTime.ToString("dd.MM.yyyy HH:mm") : string.Empty;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class TransactionTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string type && type.ToLower() == "income"
                ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
                : new SolidColorBrush(Color.FromRgb(244, 67, 54));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BalanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                if (balance > 0)
                    return new SolidColorBrush(Color.FromRgb(46, 204, 113));
                else if (balance < 0)
                    return new SolidColorBrush(Color.FromRgb(231, 76, 60));
            }
            return new SolidColorBrush(Color.FromRgb(52, 152, 219));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ProfitLossColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double profitLoss = System.Convert.ToDouble(value);
            return profitLoss >= 0
                ? new SolidColorBrush(Color.FromRgb(46, 204, 113))
                : new SolidColorBrush(Color.FromRgb(231, 76, 60));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ProfitPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double percent = System.Convert.ToDouble(value);
            return percent >= 0
                ? new SolidColorBrush(Color.FromRgb(46, 204, 113))
                : new SolidColorBrush(Color.FromRgb(231, 76, 60));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}