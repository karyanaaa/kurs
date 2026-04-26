using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FinUchetClient.Models;
using FinUchetClient.Services;
using LiveCharts;
using LiveCharts.Wpf;

namespace FinUchetClient.Views
{
    public partial class StatisticsView : UserControl
    {
        private ApiService _apiService;
        private List<TransactionModel> _transactions;
        private List<CategoryModel> _categories;

        public StatisticsView()
        {
            InitializeComponent();
            _apiService = new ApiService();

            StartDatePicker.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDatePicker.SelectedDate = DateTime.Now;

            _ = LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var token = ((App)Application.Current).Token;
                if (!string.IsNullOrEmpty(token))
                    _apiService.SetToken(token);

                _transactions = await _apiService.GetTransactionsAsync();
                _categories = await _apiService.GetCategoriesAsync();

                CalculateStats();
                UpdatePieChart();
                UpdateLineChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void CalculateStats()
        {
            if (_transactions == null) return;

            var start = StartDatePicker.SelectedDate ?? DateTime.MinValue;
            var end = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

            var filtered = _transactions.Where(t => t.Date >= start && t.Date <= end).ToList();

            var income = filtered.Where(t => t.Type == "income").Sum(t => t.Amount);
            var expense = filtered.Where(t => t.Type == "expense").Sum(t => t.Amount);
            var balance = income - expense;

            TotalIncomeText.Text = $"{income:N2} ₽";
            TotalExpenseText.Text = $"{expense:N2} ₽";
            BalanceText.Text = $"{balance:N2} ₽";
        }

        private void UpdatePieChart()
        {
            if (_transactions == null) return;

            var start = StartDatePicker.SelectedDate ?? DateTime.MinValue;
            var end = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

            var expenseTransactions = _transactions
                .Where(t => t.Type == "expense" && t.Date >= start && t.Date <= end)
                .ToList();

            if (expenseTransactions.Count == 0)
            {
                var emptySeries = new SeriesCollection
                {
                    new PieSeries
                    {
                        Title = "Нет данных",
                        Values = new ChartValues<double> { 1 },
                        DataLabels = true,
                        LabelPoint = point => "Нет расходов",
                        Fill = new SolidColorBrush(Color.FromRgb(200, 200, 200))
                    }
                };
                ExpensesPieChart.Series = emptySeries;
                return;
            }

            var categoryTotals = expenseTransactions
                .GroupBy(t => t.CategoryId)
                .Select(g => new
                {
                    CategoryName = _categories?.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Без категории",
                    Total = g.Sum(t => t.Amount)
                })
                .Where(x => x.Total > 0)
                .OrderByDescending(x => x.Total)
                .ToList();

            var series = new SeriesCollection();
            var colors = new List<Color>
            {
                Color.FromRgb(255, 99, 132),
                Color.FromRgb(54, 162, 235),
                Color.FromRgb(255, 206, 86),
                Color.FromRgb(75, 192, 192),
                Color.FromRgb(153, 102, 255),
                Color.FromRgb(255, 159, 64),
                Color.FromRgb(199, 199, 199)
            };

            double totalExpense = categoryTotals.Sum(x => x.Total);

            for (int i = 0; i < categoryTotals.Count; i++)
            {
                var item = categoryTotals[i];
                double percentage = (item.Total / totalExpense) * 100;

                series.Add(new PieSeries
                {
                    Title = $"{item.CategoryName} ({percentage:F1}%)",
                    Values = new ChartValues<double> { item.Total },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y:N0} ₽",
                    Fill = new SolidColorBrush(colors[i % colors.Count])
                });
            }

            ExpensesPieChart.Series = series;
            ExpensesPieChart.LegendLocation = LegendLocation.Right;
        }

        private void UpdateLineChart()
        {
            if (_transactions == null) return;

            var start = StartDatePicker.SelectedDate ?? DateTime.MinValue;
            var end = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

            var months = new List<DateTime>();
            var currentMonth = new DateTime(start.Year, start.Month, 1);
            var endMonth = new DateTime(end.Year, end.Month, 1);

            while (currentMonth <= endMonth && months.Count < 12)
            {
                months.Add(currentMonth);
                currentMonth = currentMonth.AddMonths(1);
            }

            if (months.Count == 0) months.Add(DateTime.Now);

            var monthLabels = months.Select(m => m.ToString("MMM yyyy")).ToList();
            var incomeValues = new ChartValues<double>();
            var expenseValues = new ChartValues<double>();

            foreach (var month in months)
            {
                var monthStart = month;
                var monthEnd = month.AddMonths(1).AddDays(-1);

                incomeValues.Add(_transactions
                    .Where(t => t.Type == "income" && t.Date >= monthStart && t.Date <= monthEnd)
                    .Sum(t => t.Amount));

                expenseValues.Add(_transactions
                    .Where(t => t.Type == "expense" && t.Date >= monthStart && t.Date <= monthEnd)
                    .Sum(t => t.Amount));
            }

            IncomeExpenseChart.Series.Clear();

            var incomeSeries = new LineSeries
            {
                Title = "💰 Доходы",
                Values = incomeValues,
                Stroke = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                StrokeThickness = 3,
                Fill = Brushes.Transparent,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 8,
                PointForeground = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                LineSmoothness = 0.5
            };

            var expenseSeries = new LineSeries
            {
                Title = "💸 Расходы",
                Values = expenseValues,
                Stroke = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                StrokeThickness = 3,
                Fill = Brushes.Transparent,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 8,
                PointForeground = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                LineSmoothness = 0.5
            };

            IncomeExpenseChart.Series.Add(incomeSeries);
            IncomeExpenseChart.Series.Add(expenseSeries);

            IncomeExpenseChart.AxisX.Clear();
            IncomeExpenseChart.AxisX.Add(new Axis
            {
                Title = "Месяц",
                Labels = monthLabels,
                LabelsRotation = 45,
                FontSize = 12,
                Separator = new LiveCharts.Wpf.Separator { Step = 1 }
            });

            IncomeExpenseChart.AxisY.Clear();
            IncomeExpenseChart.AxisY.Add(new Axis
            {
                Title = "Сумма (₽)",
                LabelFormatter = value => value.ToString("N0"),
                FontSize = 12
            });

            IncomeExpenseChart.LegendLocation = LegendLocation.Top;
        }

        private void DateRange_Changed(object sender, SelectionChangedEventArgs e)
        {
            CalculateStats();
            UpdatePieChart();
            UpdateLineChart();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        // ЭКСПОРТ В CSV С ДИАЛОГОМ ВЫБОРА ФАЙЛА
        private async void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var start = StartDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-1);
                var end = EndDatePicker.SelectedDate ?? DateTime.Now;

                var filtered = _transactions?.Where(t => t.Date >= start && t.Date <= end).ToList() ?? new List<TransactionModel>();

                if (filtered.Count == 0)
                {
                    MessageBox.Show("Нет данных за выбранный период для экспорта.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Диалог сохранения файла
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Сохранить отчет в CSV",
                    Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"Финансовый_отчет_{start:yyyyMMdd}_{end:yyyyMMdd}.csv",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dialog.ShowDialog() == true)
                {
                    string filePath = dialog.FileName;

                    using (var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                    {
                        // Заголовки
                        writer.WriteLine("Дата;Тип;Категория;Сумма;Описание");

                        // Данные
                        foreach (var t in filtered.OrderByDescending(x => x.Date))
                        {
                            string type = t.Type == "income" ? "Доход" : "Расход";
                            string category = _categories?.FirstOrDefault(c => c.Id == t.CategoryId)?.Name ?? "Без категории";
                            writer.WriteLine($"{t.Date:dd.MM.yyyy};{type};{category};{t.Amount:F2};{t.Description}");
                        }
                    }

                    MessageBox.Show($"Отчет успешно сохранен!\n\n{filePath}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ЭКСПОРТ В EXCEL (CSV формат с диалогом)
        private async void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var start = StartDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-1);
                var end = EndDatePicker.SelectedDate ?? DateTime.Now;

                var filtered = _transactions?.Where(t => t.Date >= start && t.Date <= end).ToList() ?? new List<TransactionModel>();

                if (filtered.Count == 0)
                {
                    MessageBox.Show("Нет данных за выбранный период для экспорта.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Диалог сохранения файла
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Сохранить отчет в Excel",
                    Filter = "Excel файлы (*.xlsx)|*.xlsx|CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                    DefaultExt = "xlsx",
                    FileName = $"Финансовый_отчет_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dialog.ShowDialog() == true)
                {
                    string filePath = dialog.FileName;
                    string extension = System.IO.Path.GetExtension(filePath).ToLower();

                    if (extension == ".xlsx")
                    {
                        // Для Excel используем CSV формат с разделителем (Excel откроет)
                        string csvPath = System.IO.Path.ChangeExtension(filePath, ".csv");

                        using (var writer = new System.IO.StreamWriter(csvPath, false, System.Text.Encoding.UTF8))
                        {
                            writer.WriteLine("Дата;Тип;Категория;Сумма;Описание");

                            foreach (var t in filtered.OrderByDescending(x => x.Date))
                            {
                                string type = t.Type == "income" ? "Доход" : "Расход";
                                string category = _categories?.FirstOrDefault(c => c.Id == t.CategoryId)?.Name ?? "Без категории";
                                writer.WriteLine($"{t.Date:dd.MM.yyyy};{type};{category};{t.Amount:F2};{t.Description}");
                            }
                        }

                        MessageBox.Show($"Отчет сохранен как CSV файл (можно открыть в Excel):\n{csvPath}\n\n" +
                            "Совет: Откройте Excel → Данные → Из текстового файла → выберите разделитель ';'",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Обычный CSV
                        using (var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                        {
                            writer.WriteLine("Дата;Тип;Категория;Сумма;Описание");

                            foreach (var t in filtered.OrderByDescending(x => x.Date))
                            {
                                string type = t.Type == "income" ? "Доход" : "Расход";
                                string category = _categories?.FirstOrDefault(c => c.Id == t.CategoryId)?.Name ?? "Без категории";
                                writer.WriteLine($"{t.Date:dd.MM.yyyy};{type};{category};{t.Amount:F2};{t.Description}");
                            }
                        }

                        MessageBox.Show($"Отчет успешно сохранен!\n\n{filePath}", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}