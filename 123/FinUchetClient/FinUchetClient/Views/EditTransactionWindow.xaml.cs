using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FinUchetClient.Models;
using FinUchetClient.Services;

namespace FinUchetClient.Views
{
    public partial class EditTransactionWindow : Window
    {
        public TransactionModel UpdatedTransaction { get; private set; }
        private readonly TransactionModel _originalTransaction;
        private readonly System.Collections.Generic.List<CategoryModel> _categories;

        public EditTransactionWindow(TransactionModel transaction, System.Collections.Generic.List<CategoryModel> categories)
        {
            InitializeComponent();
            _originalTransaction = transaction;
            _categories = categories;

            LoadTransactionData();
            LoadCategories();
        }

        private void LoadCategories()
        {
            if (_categories != null)
            {
                CategoryBox.ItemsSource = _categories;

                // Устанавливаем категорию
                var category = _categories.FirstOrDefault(c => c.Id == _originalTransaction.CategoryId);
                if (category != null)
                    CategoryBox.SelectedItem = category;
            }
        }

        private void LoadTransactionData()
        {
            // Устанавливаем тип
            TypeBox.SelectedIndex = _originalTransaction.Type == "income" ? 0 : 1;

            // Устанавливаем сумму
            AmountBox.Text = _originalTransaction.Amount.ToString();

            // Устанавливаем описание
            DescriptionBox.Text = _originalTransaction.Description;

            // Устанавливаем дату
            DatePicker.SelectedDate = _originalTransaction.Date;
        }

        private void TypeBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Обновляем список категорий при смене типа
            if (_categories != null && CategoryBox != null)
            {
                var selectedItem = TypeBox.SelectedItem as System.Windows.Controls.ComboBoxItem;
                if (selectedItem != null)
                {
                    string selectedType = selectedItem.Content.ToString();
                    string type = selectedType.Contains("Доход") ? "income" : "expense";

                    var filtered = _categories.Where(c => c.Type == type).ToList();
                    CategoryBox.ItemsSource = filtered;

                    if (filtered.Any())
                        CategoryBox.SelectedIndex = 0;
                }
            }
        }

        private void AmountBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }

            if (e.Text.Contains('.') && ((System.Windows.Controls.TextBox)sender).Text.Contains('.'))
            {
                e.Handled = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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
            var selectedTypeItem = TypeBox.SelectedItem as System.Windows.Controls.ComboBoxItem;
            string transactionType = selectedTypeItem.Content.ToString().Contains("Доход") ? "income" : "expense";

            UpdatedTransaction = new TransactionModel
            {
                Id = _originalTransaction.Id,
                Amount = amount,
                Description = DescriptionBox.Text,
                Type = transactionType,
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}