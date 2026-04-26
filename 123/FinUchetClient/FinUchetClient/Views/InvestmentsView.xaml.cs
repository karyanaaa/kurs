using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FinUchetClient.Models;
using FinUchetClient.Services;

namespace FinUchetClient.Views
{
    public partial class InvestmentsView : UserControl
    {
        private ApiService _apiService;
        private List<InvestmentModel> _investments;

        public InvestmentsView()
        {
            InitializeComponent();
            _apiService = new ApiService();
            Loaded += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var token = ((App)Application.Current).Token;
                if (!string.IsNullOrEmpty(token))
                    _apiService.SetToken(token);

                _investments = await _apiService.GetInvestmentsAsync();

                if (_investments == null)
                    _investments = new List<InvestmentModel>();

                UpdateStatistics();

                InvestmentsGrid.ItemsSource = _investments.Select(i => new
                {
                    i.Id,
                    i.Name,
                    i.Type,
                    TypeDisplay = i.TypeDisplay,
                    i.Quantity,
                    i.PurchasePrice,
                    i.CurrentPrice,
                    TotalInvested = i.TotalInvested,
                    CurrentValue = i.CurrentValue,
                    ProfitLoss = i.ProfitLoss,
                    ProfitLossPercent = i.ProfitLossPercent
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки инвестиций: {ex.Message}");
            }
        }

        private void UpdateStatistics()
        {
            if (_investments == null) return;

            double totalInvested = _investments.Sum(i => i.TotalInvested);
            double currentValue = _investments.Sum(i => i.CurrentValue);
            double profitLoss = currentValue - totalInvested;
            double profitPercent = totalInvested > 0 ? (profitLoss / totalInvested) * 100 : 0;

            TotalInvestedText.Text = $"{totalInvested:N2} ₽";
            CurrentValueText.Text = $"{currentValue:N2} ₽";

            if (profitLoss >= 0)
            {
                ProfitLossText.Text = $"+{profitLoss:N2} ₽";
                ProfitLossText.Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                ProfitPercentText.Text = $"+{profitPercent:F1}%";
                ProfitPercentText.Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113));
            }
            else
            {
                ProfitLossText.Text = $"{profitLoss:N2} ₽";
                ProfitLossText.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                ProfitPercentText.Text = $"{profitPercent:F1}%";
                ProfitPercentText.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
            }
        }

        private async void AddInvestment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InvestmentNameBox.Text))
                {
                    MessageBox.Show("Введите название инвестиции");
                    return;
                }

                if (!double.TryParse(QuantityBox.Text, out double quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество");
                    return;
                }

                if (!double.TryParse(PurchasePriceBox.Text, out double purchasePrice) || purchasePrice <= 0)
                {
                    MessageBox.Show("Введите корректную цену покупки");
                    return;
                }

                if (!double.TryParse(CurrentPriceBox.Text, out double currentPrice) || currentPrice <= 0)
                {
                    MessageBox.Show("Введите корректную текущую цену");
                    return;
                }

                string selectedType = ((ComboBoxItem)InvestmentTypeBox.SelectedItem).Content.ToString();
                string typeCode = selectedType switch
                {
                    "📈 Акции" => "stock",
                    "📉 Облигации" => "bond",
                    "₿ Криптовалюта" => "crypto",
                    "🏠 Недвижимость" => "realty",
                    "🏦 Депозит" => "deposit",
                    _ => "stock"
                };

                var investment = new InvestmentModel
                {
                    Name = InvestmentNameBox.Text,
                    Type = typeCode,
                    Quantity = quantity,
                    PurchasePrice = purchasePrice,
                    CurrentPrice = currentPrice,
                    PurchaseDate = DateTime.Now,
                    Currency = "RUB",
                    Amount = quantity * purchasePrice
                };

                var success = await _apiService.AddInvestmentAsync(investment);
                if (success)
                {
                    await LoadData();
                    MessageBox.Show("Инвестиция добавлена!");

                    InvestmentNameBox.Text = "";
                    QuantityBox.Text = "1";
                    PurchasePriceBox.Text = "0";
                    CurrentPriceBox.Text = "0";
                }
                else
                {
                    MessageBox.Show("Ошибка добавления инвестиции");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void EditInvestment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                dynamic item = button.Tag;
                int id = item.Id;

                var investment = _investments.FirstOrDefault(i => i.Id == id);
                if (investment != null)
                {
                    var dialog = new EditInvestmentWindow(investment);
                    if (dialog.ShowDialog() == true)
                    {
                        var success = await _apiService.UpdateInvestmentAsync(dialog.UpdatedInvestment);
                        if (success)
                            await LoadData();
                        else
                            MessageBox.Show("Ошибка обновления");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void DeleteInvestment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                dynamic item = button.Tag;
                int id = item.Id;

                var result = MessageBox.Show("Удалить инвестицию?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var success = await _apiService.DeleteInvestmentAsync(id);
                    if (success)
                        await LoadData();
                    else
                        MessageBox.Show("Ошибка удаления");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}