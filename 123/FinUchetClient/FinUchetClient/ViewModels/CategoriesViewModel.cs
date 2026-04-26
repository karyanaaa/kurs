using CommunityToolkit.Mvvm.Input;
using FinUchetClient.Models;
using FinUchetClient.Services;
using FinUchetClient.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinUchetClient.ViewModels
{
    public class CategoriesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private ObservableCollection<CategoryModel> _categories;
        private CategoryModel _selectedCategory;
        private string _searchText;
        private string _filterType;

        public ObservableCollection<CategoryModel> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public CategoryModel SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                (EditCategoryCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (DeleteCategoryCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterCategories();
            }
        }

        public string FilterType
        {
            get => _filterType;
            set
            {
                SetProperty(ref _filterType, value);
                FilterCategories();
            }
        }

        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        private ObservableCollection<CategoryModel> _allCategories;

        public CategoriesViewModel(ApiService apiService)
        {
            _apiService = apiService;

            Categories = new ObservableCollection<CategoryModel>();
            _allCategories = new ObservableCollection<CategoryModel>();

            AddCategoryCommand = new RelayCommand(() => ExecuteAddCategory());
            EditCategoryCommand = new RelayCommand(() => ExecuteEditCategory(), () => SelectedCategory != null);
            DeleteCategoryCommand = new RelayCommand(() => ExecuteDeleteCategory(), () => SelectedCategory != null);
            ClearFiltersCommand = new RelayCommand(() => ExecuteClearFilters());
        }

        public async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _apiService.GetCategoriesAsync();
                _allCategories.Clear();
                if (categories != null)
                {
                    foreach (var category in categories)
                    {
                        _allCategories.Add(category);
                    }
                }
                FilterCategories();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void FilterCategories()
        {
            if (_allCategories == null) return;

            var filtered = _allCategories.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(c =>
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(FilterType) && FilterType != "Все")
            {
                string type = FilterType == "Доход" ? "income" : "expense";
                filtered = filtered.Where(c => c.Type == type);
            }

            Categories.Clear();
            foreach (var category in filtered.OrderBy(c => c.Name))
            {
                Categories.Add(category);
            }
        }

        private void ExecuteAddCategory()
        {
            System.Windows.MessageBox.Show("Добавление категории");
        }

        private bool CanExecuteEditCategory()
        {
            return SelectedCategory != null;
        }

        private void ExecuteEditCategory()
        {
            if (SelectedCategory != null)
            {
                System.Windows.MessageBox.Show($"Редактирование: {SelectedCategory.Name}");
            }
        }

        private bool CanExecuteDeleteCategory()
        {
            return SelectedCategory != null;
        }

        private async void ExecuteDeleteCategory()
        {
            if (SelectedCategory != null)
            {
                var result = System.Windows.MessageBox.Show(
                    $"Удалить категорию '{SelectedCategory.Name}'?",
                    "Подтверждение",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        var success = await _apiService.DeleteCategoryAsync(SelectedCategory.Id);
                        if (success)
                        {
                            await LoadCategoriesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
        }

        private void ExecuteClearFilters()
        {
            SearchText = string.Empty;
            FilterType = string.Empty;
        }
    }
}