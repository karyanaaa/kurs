using CommunityToolkit.Mvvm.Input;
using FinUchetClient.Models;
using FinUchetClient.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace FinUchetClient.ViewModels
{
    public class TransactionsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private ObservableCollection<TransactionModel> _transactions;
        private ObservableCollection<CategoryModel> _categories;
        private TransactionModel _selectedTransaction;
        public ObservableCollection<string> TransactionTypes { get; } = new() { "Все", "Доход", "Расход" };
        private string _searchText;
        private DateTime? _filterDateFrom;
        private DateTime? _filterDateTo;
        private string _filterType;

        public ObservableCollection<TransactionModel> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        public ObservableCollection<CategoryModel> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public TransactionModel SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterTransactions();
            }
        }

        public DateTime? FilterDateFrom
        {
            get => _filterDateFrom;
            set
            {
                SetProperty(ref _filterDateFrom, value);
                FilterTransactions();
            }
        }

        public DateTime? FilterDateTo
        {
            get => _filterDateTo;
            set
            {
                SetProperty(ref _filterDateTo, value);
                FilterTransactions();
            }
        }

        public string FilterType
        {
            get => _filterType;
            set
            {
                SetProperty(ref _filterType, value);
                FilterTransactions();
            }
        }

        public ICommand AddTransactionCommand { get; }
        public ICommand EditTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        private ObservableCollection<TransactionModel> _allTransactions;

        public TransactionsViewModel(ApiService apiService)
        {
            _apiService = apiService;

            Transactions = new ObservableCollection<TransactionModel>();
            Categories = new ObservableCollection<CategoryModel>();
            _allTransactions = new ObservableCollection<TransactionModel>();

            // ИСПРАВЛЕНО: Используем лямбда-выражения без параметров
            AddTransactionCommand = new RelayCommand(() => ExecuteAddTransaction());
            EditTransactionCommand = new RelayCommand(() => ExecuteEditTransaction(), () => SelectedTransaction != null);
            DeleteTransactionCommand = new RelayCommand(() => ExecuteDeleteTransaction(), () => SelectedTransaction != null);
            ClearFiltersCommand = new RelayCommand(() => ExecuteClearFilters());
        }

        public async System.Threading.Tasks.Task LoadTransactionsAsync()
        {
            try
            {
                var transactions = await _apiService.GetTransactionsAsync();
                _allTransactions.Clear();
                if (transactions != null)
                {
                    foreach (var transaction in transactions)
                    {
                        _allTransactions.Add(transaction);
                    }
                }
                FilterTransactions();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки транзакций: {ex.Message}");
            }
        }

        public async System.Threading.Tasks.Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _apiService.GetCategoriesAsync();
                Categories.Clear();
                if (categories != null)
                {
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void FilterTransactions()
        {
            if (_allTransactions == null) return;

            var filtered = _allTransactions.AsEnumerable();

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t =>
                    t.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Фильтр по дате
            if (FilterDateFrom.HasValue)
            {
                filtered = filtered.Where(t => t.Date.Date >= FilterDateFrom.Value.Date);
            }

            if (FilterDateTo.HasValue)
            {
                filtered = filtered.Where(t => t.Date.Date <= FilterDateTo.Value.Date);
            }

            // Фильтр по типу
            if (!string.IsNullOrWhiteSpace(FilterType) && FilterType != "Все")
            {
                string type = FilterType == "Доход" ? "income" : "expense";
                filtered = filtered.Where(t => t.Type == type);
            }

            Transactions.Clear();
            foreach (var transaction in filtered.OrderByDescending(t => t.Date))
            {
                Transactions.Add(transaction);
            }
        }

        // ИСПРАВЛЕНО: Убрали параметр object parameter
        private void ExecuteAddTransaction()
        {
            System.Windows.MessageBox.Show("Добавление транзакции");
            // Здесь будет открытие диалога добавления транзакции
        }

        // ИСПРАВЛЕНО: Убрали параметр object parameter
        private void ExecuteEditTransaction()
        {
            if (SelectedTransaction != null)
            {
                System.Windows.MessageBox.Show($"Редактирование: {SelectedTransaction.Description}");
            }
        }

        // ИСПРАВЛЕНО: Убрали параметр object parameter
        private async void ExecuteDeleteTransaction()
        {
            if (SelectedTransaction != null)
            {
                var result = System.Windows.MessageBox.Show(
                    "Удалить выбранную транзакцию?",
                    "Подтверждение",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        var success = await _apiService.DeleteTransactionAsync(SelectedTransaction.Id);
                        if (success)
                        {
                            await LoadTransactionsAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
        }

        // ИСПРАВЛЕНО: Убрали параметр object parameter
        // В методе ExecuteClearFilters (строки 230-233)
        private void ExecuteClearFilters()
        {
            SearchText = string.Empty;      // вместо null
            FilterDateFrom = null;          // null допустим для DateTime?
            FilterDateTo = null;            // null допустим для DateTime?
            FilterType = string.Empty;      // вместо null
        }
    }
}