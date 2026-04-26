using System;
using System.Windows;
using System.Windows.Controls;
using FinUchetClient.Models;

namespace FinUchetClient.Views
{
    public partial class EditInvestmentWindow : Window
    {
        public InvestmentModel UpdatedInvestment { get; private set; }
        private readonly InvestmentModel _original;

        public EditInvestmentWindow(InvestmentModel investment)
        {
            InitializeComponent();
            _original = investment;
            LoadData();
        }

        private void LoadData()
        {
            NameBox.Text = _original.Name;

            int typeIndex = _original.Type switch
            {
                "stock" => 0,
                "bond" => 1,
                "crypto" => 2,
                "realty" => 3,
                "deposit" => 4,
                _ => 0
            };
            TypeBox.SelectedIndex = typeIndex;

            QuantityBox.Text = _original.Quantity.ToString();
            PurchasePriceBox.Text = _original.PurchasePrice.ToString();
            CurrentPriceBox.Text = _original.CurrentPrice.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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

            string selectedType = ((ComboBoxItem)TypeBox.SelectedItem).Content.ToString();
            string typeCode = selectedType switch
            {
                "📈 Акции" => "stock",
                "📉 Облигации" => "bond",
                "₿ Криптовалюта" => "crypto",
                "🏠 Недвижимость" => "realty",
                "🏦 Депозит" => "deposit",
                _ => "stock"
            };

            UpdatedInvestment = new InvestmentModel
            {
                Id = _original.Id,
                Name = NameBox.Text,
                Type = typeCode,
                Quantity = quantity,
                PurchasePrice = purchasePrice,
                CurrentPrice = currentPrice,
                Amount = quantity * purchasePrice,
                PurchaseDate = _original.PurchaseDate,
                Currency = "RUB"
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}