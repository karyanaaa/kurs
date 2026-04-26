using System;
using System.Windows;
using System.Windows.Controls;
using FinUchetClient.Services;

namespace FinUchetClient.Views
{
    public partial class UsefulView : UserControl
    {
        private CurrencyService _currencyService;

        public UsefulView()
        {
            InitializeComponent();
            _currencyService = new CurrencyService();

            // Устанавливаем валюты по умолчанию
            if (FromCurrencyBox.Items.Count > 0)
                FromCurrencyBox.SelectedIndex = 0;
            if (ToCurrencyBox.Items.Count > 0)
                ToCurrencyBox.SelectedIndex = 1;

            // Обновляем курсы в фоне
            _ = UpdateRatesAsync();
        }

        private void CalculateSavings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double monthly = double.Parse(MonthlyAmountBox.Text);
                double periodValue = double.Parse(PeriodValueBox.Text);
                string unit = (PeriodUnitBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Месяцев";

                double total = 0;
                string periodText = "";

                switch (unit)
                {
                    case "Дней":
                        total = monthly * (periodValue / 30.0);
                        periodText = $"{periodValue} дней";
                        break;
                    case "Месяцев":
                        total = monthly * periodValue;
                        periodText = $"{periodValue} месяцев";
                        break;
                    case "Лет":
                        total = monthly * periodValue * 12;
                        periodText = $"{periodValue} лет";
                        break;
                }

                SavingsResult.Text = $"💰 За {periodText} вы накопите: {total:N0} ₽";

                if (total > 0)
                {
                    SavingsDetail.Visibility = Visibility.Visible;
                    SavingsDetail.Text = $"📊 Ежемесячный взнос: {monthly:N0} ₽\n" +
                                        $"💡 Совет: если увеличить взнос на 20%, вы накопите {(total * 1.2):N0} ₽\n" +
                                        $"⭐ В день это: {(monthly / 30):N2} ₽";
                }
            }
            catch
            {
                SavingsResult.Text = "❌ Введите корректные числа";
                SavingsDetail.Visibility = Visibility.Collapsed;
            }
        }

        private async void Currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Автоматически конвертируем при смене валюты
            ConvertCurrency_Click(sender, null);
        }

        private async void ConvertCurrency_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FromCurrencyBox.SelectedItem == null || ToCurrencyBox.SelectedItem == null)
                    return;

                double amount = double.Parse(AmountToConvert.Text);

                string fromCode = ((ComboBoxItem)FromCurrencyBox.SelectedItem).Tag.ToString();
                string toCode = ((ComboBoxItem)ToCurrencyBox.SelectedItem).Tag.ToString();

                var fromCurrency = _currencyService.GetCurrency(fromCode);
                var toCurrency = _currencyService.GetCurrency(toCode);

                if (fromCurrency == null || toCurrency == null) return;

                double result = 0;
                string conversionText = "";

                if (fromCode == "RUB")
                {
                    result = amount / toCurrency.RateToRub;
                    conversionText = $"{amount:N2} ₽ → {result:N2} {toCurrency.Symbol}";
                }
                else if (toCode == "RUB")
                {
                    result = amount * fromCurrency.RateToRub;
                    conversionText = $"{amount:N2} {fromCurrency.Symbol} → {result:N2} ₽";
                }
                else
                {
                    // Конвертируем через рубль
                    double inRub = amount * fromCurrency.RateToRub;
                    result = inRub / toCurrency.RateToRub;
                    conversionText = $"{amount:N2} {fromCurrency.Symbol} → {result:N2} {toCurrency.Symbol}";
                }

                ConversionResult.Text = $"{result:N2} {toCurrency.Symbol}";
                ExchangeRateInfo.Text = $"Курс: 1 {fromCurrency.Symbol} = {fromCurrency.RateToRub:N2} ₽ | " +
                                       $"1 {toCurrency.Symbol} = {toCurrency.RateToRub:N2} ₽";
            }
            catch (FormatException)
            {
                ConversionResult.Text = "❌ Введите корректную сумму";
            }
            catch (Exception ex)
            {
                ConversionResult.Text = $"❌ Ошибка: {ex.Message}";
            }
        }

        private async void UpdateRates_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            button.Content = "Обновление...";

            try
            {
                await _currencyService.UpdateRatesAsync();
                MessageBox.Show("Курсы валют успешно обновлены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Пересчитываем результат
                ConvertCurrency_Click(sender, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления курсов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                button.IsEnabled = true;
                button.Content = "🔄 Обновить курсы валют";
            }
        }

        private async Task UpdateRatesAsync()
        {
            try
            {
                await _currencyService.UpdateRatesAsync();
            }
            catch { }
        }
    }
}