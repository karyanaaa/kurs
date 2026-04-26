using CommunityToolkit.Mvvm.Input;
using FinUchetClient.Models;
using FinUchetClient.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace FinUchetClient.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private ObservableCollection<TransactionModel> _transactions;
        private ObservableCollection<CategoryModel> _categories;

        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _totalIncome;
        private decimal _totalExpense;
        private decimal _balance;

        // Для графиков LiveCharts
        private object _expensesByCategoryChart;
        private object _incomeExpenseChart;

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                SetProperty(ref _startDate, value);
                CalculateStatistics();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                SetProperty(ref _endDate, value);
                CalculateStatistics();
            }
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set => SetProperty(ref _totalExpense, value);
        }

        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public object ExpensesByCategoryChart
        {
            get => _expensesByCategoryChart;
            set => SetProperty(ref _expensesByCategoryChart, value);
        }

        public object IncomeExpenseChart
        {
            get => _incomeExpenseChart;
            set => SetProperty(ref _incomeExpenseChart, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        public StatisticsViewModel(ApiService apiService)
        {
            _apiService = apiService;

            _transactions = new ObservableCollection<TransactionModel>();
            _categories = new ObservableCollection<CategoryModel>();

            // Устанавливаем период по умолчанию (текущий месяц)
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = DateTime.Now;

            // ИСПРАВЛЕНО: Убрали параметр _ в лямбда-выражении
            RefreshCommand = new RelayCommand(() => LoadStatisticsAsync());

            // ИСПРАВЛЕНО: Используем лямбда-выражения без параметров
            ExportToCsvCommand = new RelayCommand(() => ExecuteExportToCsv());
            ExportToExcelCommand = new RelayCommand(() => ExecuteExportToExcel());
        }

        public async System.Threading.Tasks.Task LoadStatisticsAsync()
        {
            try
            {
                var transactions = await _apiService.GetTransactionsAsync();
                var categories = await _apiService.GetCategoriesAsync();

                _transactions.Clear();
                if (transactions != null)
                {
                    foreach (var t in transactions)
                    {
                        _transactions.Add(t);
                    }
                }

                _categories.Clear();
                if (categories != null)
                {
                    foreach (var c in categories)
                    {
                        _categories.Add(c);
                    }
                }

                CalculateStatistics();
                BuildCharts();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private void CalculateStatistics()
        {
            var filteredTransactions = _transactions.Where(t =>
                t.Date.Date >= StartDate.Date &&
                t.Date.Date <= EndDate.Date);

            TotalIncome = (decimal)filteredTransactions
                .Where(t => t.Type == "income")
                .Sum(t => t.Amount);

            TotalExpense = (decimal)filteredTransactions
                .Where(t => t.Type == "expense")
                .Sum(t => t.Amount);

            Balance = TotalIncome - TotalExpense;
        }

        private void BuildCharts()
        {
            BuildExpensesByCategoryChart();
            BuildIncomeExpenseChart();
        }

        private void BuildExpensesByCategoryChart()
        {
            var filteredTransactions = _transactions
                .Where(t => t.Type == "expense" &&
                           t.Date.Date >= StartDate.Date &&
                           t.Date.Date <= EndDate.Date)
                .GroupBy(t => t.CategoryId)
                .Select(g => new
                {
                    CategoryName = _categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Без категории",
                    Total = g.Sum(t => t.Amount)
                })
                .Where(x => x.Total > 0)
                .ToList();

            // Здесь будет создание круговой диаграммы для LiveCharts
            // В реальном проекте нужно добавить пакет LiveCharts.Wpf
        }

        private void BuildIncomeExpenseChart()
        {
            var monthlyData = _transactions
                .Where(t => t.Date.Year == DateTime.Now.Year)
                .GroupBy(t => t.Date.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Income = g.Where(t => t.Type == "income").Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == "expense").Sum(t => t.Amount)
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Здесь будет создание линейного графика для LiveCharts
        }

        // ИСПРАВЛЕНО: Убрали параметр object parameter
        private void ExecuteExportToCsv()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"report_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                ExportToCsv(dialog.FileName);
            }
        }

        // ИСПРАВЛЕНО: Убрали параметр object parameter
        private void ExecuteExportToExcel()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"report_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                ExportToExcel(dialog.FileName);
            }
        }

        private void ExportToCsv(string filePath)
        {
            try
            {
                var filteredTransactions = _transactions
                    .Where(t => t.Date.Date >= StartDate.Date &&
                               t.Date.Date <= EndDate.Date)
                    .OrderByDescending(t => t.Date);

                using (var writer = new System.IO.StreamWriter(filePath))
                {
                    // Заголовки
                    writer.WriteLine("Дата;Тип;Категория;Сумма;Описание");

                    // Данные
                    foreach (var t in filteredTransactions)
                    {
                        var categoryName = _categories.FirstOrDefault(c => c.Id == t.CategoryId)?.Name ?? "";
                        var type = t.Type == "income" ? "Доход" : "Расход";
                        writer.WriteLine($"{t.Date:dd.MM.yyyy};{type};{categoryName};{t.Amount};{t.Description}");
                    }
                }

                System.Windows.MessageBox.Show($"Отчет сохранен: {filePath}", "Успех",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ExportToExcel(string filePath)
        {
            // Для Excel нужно добавить пакет EPPlus или ClosedXML
            System.Windows.MessageBox.Show("Функция экспорта в Excel будет доступна позже",
                "Информация", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}