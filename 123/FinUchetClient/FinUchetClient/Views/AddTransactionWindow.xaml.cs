using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FinUchetClient.Models;

namespace FinUchetClient.Views
{
    public partial class AddTransactionWindow : Window
    {
        public TransactionModel Transaction { get; private set; }
        private readonly string _transactionType;

        public AddTransactionWindow(string type)
        {
            InitializeComponent();
            _transactionType = type;

            // Устанавливаем заголовок
            TitleText.Text = type == "income" ? "➕ Добавление дохода" : "➖ Добавление расхода";
            SaveButton.Background = type == "income"
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));

            // Устанавливаем сегодняшнюю дату
            DatePicker.SelectedDate = DateTime.Today;

            // Загружаем категории
            LoadCategoriesAsync();
        }

        private async void LoadCategoriesAsync()
        {
            try
            {
                var apiService = new Services.ApiService();
                // В реальном приложении токен нужно передать
                var categories = await apiService.GetCategoriesAsync();

                if (categories != null)
                {
                    // Фильтруем по типу операции
                    var filtered = categories.Where(c => c.Type == _transactionType).ToList();
                    CategoryBox.ItemsSource = filtered;

                    if (filtered.Any())
                        CategoryBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void AmountBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и одну точку
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }

            // Проверяем, что точка только одна
            if (e.Text.Contains('.') && ((TextBox)sender).Text.Contains('.'))
            {
                e.Handled = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (!double.TryParse(AmountBox.Text, out double amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка");
                return;
            }

            if (CategoryBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка");
                return;
            }

            var selectedCategory = CategoryBox.SelectedItem as CategoryModel;

            Transaction = new TransactionModel
            {
                Amount = amount,
                Description = DescriptionBox.Text,
                Type = _transactionType,
                CategoryId = selectedCategory.Id,
                CategoryName = selectedCategory.Name,
                Date = DatePicker.SelectedDate ?? DateTime.Now
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