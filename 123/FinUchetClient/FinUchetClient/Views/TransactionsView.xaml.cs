using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FinUchetClient.Models;
using FinUchetClient.Services;

namespace FinUchetClient.Views
{
    public partial class TransactionsView : UserControl
    {
        private ApiService _apiService;
        private List<TransactionModel> _allTransactions;
        private List<CategoryModel> _categories;
        private dynamic _selectedTransaction;

        public TransactionsView()
        {
            InitializeComponent();
            _apiService = new ApiService();
            NewDatePicker.SelectedDate = DateTime.Now;
            _ = LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var token = ((App)Application.Current).Token;
                if (!string.IsNullOrEmpty(token))
                    _apiService.SetToken(token);

                _allTransactions = await _apiService.GetTransactionsAsync();
                _categories = await _apiService.GetCategoriesAsync();

                NewCategoryBox.ItemsSource = _categories;
                NewCategoryBox.DisplayMemberPath = "Name";
                if (_categories != null && _categories.Count > 0)
                    NewCategoryBox.SelectedIndex = 0;

                UpdateGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void UpdateGrid()
        {
            if (_allTransactions == null) return;

            var filtered = _allTransactions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                filtered = filtered.Where(t => t.Description != null && t.Description.ToLower().Contains(SearchBox.Text.ToLower()));

            if (DateFromPicker.SelectedDate.HasValue)
                filtered = filtered.Where(t => t.Date >= DateFromPicker.SelectedDate.Value);
            if (DateToPicker.SelectedDate.HasValue)
                filtered = filtered.Where(t => t.Date <= DateToPicker.SelectedDate.Value);

            if (TypeFilterBox.SelectedItem is ComboBoxItem item && item.Content.ToString() != "Все")
            {
                string filterText = item.Content.ToString();
                if (filterText.Contains("Доход"))
                    filtered = filtered.Where(t => t.Type == "income");
                else if (filterText.Contains("Расход"))
                    filtered = filtered.Where(t => t.Type == "expense");
            }

            var filteredList = filtered.OrderByDescending(t => t.Date).ToList();

            var displayList = filteredList.Select(t => new
            {
                t.Id,
                Date = t.Date.ToString("dd.MM.yyyy"),
                Type = t.Type == "income" ? "💰 Доход" : "💸 Расход",
                Category = t.CategoryName,
                Description = t.Description,
                Amount = t.Type == "income" ? $"+{t.Amount:F2} ₽" : $"-{t.Amount:F2} ₽"
            }).ToList();

            TransactionsGrid.ItemsSource = displayList;
            CountTextBlock.Text = $"📊 Всего операций: {filteredList.Count}";
        }

        // Добавление транзакции
        private async void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var token = ((App)Application.Current).Token;
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Ошибка: вы не авторизованы");
                    return;
                }
                _apiService.SetToken(token);

                if (!double.TryParse(NewAmountBox.Text, out double amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму");
                    return;
                }

                if (NewCategoryBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите категорию");
                    return;
                }

                var selectedCategory = NewCategoryBox.SelectedItem as CategoryModel;
                if (selectedCategory == null)
                {
                    MessageBox.Show("Ошибка выбора категории");
                    return;
                }

                DateTime selectedDate = NewDatePicker.SelectedDate ?? DateTime.Now;
                string selectedType = ((ComboBoxItem)NewTypeBox.SelectedItem).Content.ToString();
                string transactionType = selectedType.Contains("Доход") ? "income" : "expense";

                var transaction = new TransactionModel
                {
                    Amount = amount,
                    Description = NewDescriptionBox.Text,
                    Type = transactionType,
                    CategoryId = selectedCategory.Id,
                    CategoryName = selectedCategory.Name,
                    Date = selectedDate
                };

                var success = await _apiService.AddTransactionAsync(transaction);
                if (success)
                {
                    await LoadData();
                    MessageBox.Show("Операция добавлена!");
                    NewAmountBox.Text = "0";
                    NewDescriptionBox.Text = "";
                    NewDatePicker.SelectedDate = DateTime.Now;
                }
                else
                {
                    MessageBox.Show("Ошибка добавления операции");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // РЕДАКТИРОВАНИЕ транзакции
        // РЕДАКТИРОВАНИЕ транзакции (полное)
        private async void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            var selected = TransactionsGrid.SelectedItem;
            if (selected == null)
            {
                MessageBox.Show("Выберите операцию для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем ID выбранной транзакции
            var prop = selected.GetType().GetProperty("Id");
            int id = (int)prop.GetValue(selected);
            var transaction = _allTransactions.FirstOrDefault(t => t.Id == id);

            if (transaction != null)
            {
                // Открываем окно редактирования
                var editWindow = new EditTransactionWindow(transaction, _categories);
                if (editWindow.ShowDialog() == true)
                {
                    var updatedTransaction = editWindow.UpdatedTransaction;
                    var success = await _apiService.UpdateTransactionAsync(updatedTransaction);

                    if (success)
                    {
                        await LoadData();
                        MessageBox.Show("Операция успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении операции", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // УДАЛЕНИЕ транзакции
        private async void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            var selected = TransactionsGrid.SelectedItem;
            if (selected == null)
            {
                MessageBox.Show("Выберите операцию для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var prop = selected.GetType().GetProperty("Id");
            int id = (int)prop.GetValue(selected);
            var transaction = _allTransactions.FirstOrDefault(t => t.Id == id);

            if (transaction != null)
            {
                var result = MessageBox.Show($"Удалить операцию '{transaction.Description}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var success = await _apiService.DeleteTransactionAsync(id);
                    if (success)
                    {
                        await LoadData();
                        MessageBox.Show("Операция удалена!");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка удаления");
                    }
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateGrid();
        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e) => UpdateGrid();
        private void TypeFilter_Changed(object sender, SelectionChangedEventArgs e) => UpdateGrid();
        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;
            TypeFilterBox.SelectedIndex = 0;
            UpdateGrid();
        }
    }
}