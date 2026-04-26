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
    public partial class CategoriesView : UserControl
    {
        private ApiService _apiService;
        private List<CategoryModel> _allCategories;

        public CategoriesView()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _ = LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var token = ((App)Application.Current).Token;
                if (!string.IsNullOrEmpty(token))
                    _apiService.SetToken(token);

                _allCategories = await _apiService.GetCategoriesAsync();

                // Подсчет категорий для статистики
                if (_allCategories != null)
                {
                    int incomeCount = _allCategories.Count(c => c.Type == "income");
                    int expenseCount = _allCategories.Count(c => c.Type == "expense");

                    IncomeCategoriesCount.Text = incomeCount.ToString();
                    ExpenseCategoriesCount.Text = expenseCount.ToString();
                }

                // Преобразуем для отображения
                var displayList = _allCategories?.Select(c => new
                {
                    c.Id,
                    c.Name,
                    Тип = c.Type == "income" ? "💰 Доход" : "💸 Расход"
                }).ToList();

                CategoriesGrid.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewCategoryName.Text))
                {
                    MessageBox.Show("Введите название категории");
                    return;
                }

                string selectedType = ((ComboBoxItem)NewCategoryType.SelectedItem).Content.ToString();
                string categoryType = selectedType.Contains("Доход") ? "income" : "expense";

                var category = new CategoryModel
                {
                    Name = NewCategoryName.Text,
                    Type = categoryType
                };

                var success = await _apiService.AddCategoryAsync(category);
                if (success)
                {
                    await LoadData();
                    MessageBox.Show("Категория добавлена!");
                    NewCategoryName.Text = "";
                }
                else
                    MessageBox.Show("Ошибка добавления");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag == null) return;

                int id = (int)button.Tag;
                var category = _allCategories?.FirstOrDefault(c => c.Id == id);

                if (category != null)
                {
                    var newName = Microsoft.VisualBasic.Interaction.InputBox("Введите новое название:", "Редактирование", category.Name);
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        category.Name = newName;
                        var success = await _apiService.UpdateCategoryAsync(category);
                        if (success)
                            await LoadData();
                        else
                            MessageBox.Show("Ошибка редактирования");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag == null) return;

                int id = (int)button.Tag;

                var result = MessageBox.Show("Удалить категорию? (Будут удалены все связанные операции!)", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var success = await _apiService.DeleteCategoryAsync(id);
                    if (success)
                        await LoadData();
                    else
                        MessageBox.Show("Ошибка удаления. Возможно, есть связанные операции.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
            MessageBox.Show("Список категорий обновлен!", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}