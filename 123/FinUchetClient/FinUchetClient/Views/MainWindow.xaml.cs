using System.Windows;
using System.Windows.Controls;
using FinUchetClient.Services;
using FinUchetClient.ViewModels;

namespace FinUchetClient.Views
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        private TransactionsViewModel _transactionsVM;
        private CategoriesViewModel _categoriesVM;
        private StatisticsViewModel _statisticsVM;

        public MainWindow(ApiService apiService, AuthService authService)
        {
            InitializeComponent();

            _apiService = apiService;
            _authService = authService;

            // Загружаем сохраненную тему
            ThemeManager.LoadThemePreference();

            // Обновляем текст кнопки темы
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.Content = ThemeManager.IsDarkTheme ? "☀️ Светлая тема" : "🌙 Тёмная тема";
            }

            // Устанавливаем имя пользователя
            UserNameText.Text = authService.CurrentUsername ?? "Пользователь";

            // Инициализируем ViewModels
            _transactionsVM = new TransactionsViewModel(_apiService);
            _categoriesVM = new CategoriesViewModel(_apiService);
            _statisticsVM = new StatisticsViewModel(_apiService);

            // Показываем страницу операций по умолчанию
            ShowTransactions_Click(null, null);
        }

        private async void ShowTransactions_Click(object sender, RoutedEventArgs e)
        {
            var view = new TransactionsView();
            view.DataContext = _transactionsVM;
            MainContentArea.Content = view;
            await _transactionsVM.LoadTransactionsAsync();
            await _transactionsVM.LoadCategoriesAsync();
        }

        private async void ShowCategories_Click(object sender, RoutedEventArgs e)
        {
            var view = new CategoriesView();
            view.DataContext = _categoriesVM;
            MainContentArea.Content = view;
            await _categoriesVM.LoadCategoriesAsync();
        }

        private async void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            var view = new StatisticsView();
            view.DataContext = _statisticsVM;
            MainContentArea.Content = view;
            await _statisticsVM.LoadStatisticsAsync();
        }

        private void ShowInvestments_Click(object sender, RoutedEventArgs e)
        {
            var view = new InvestmentsView();
            MainContentArea.Content = view;
        }

        private void ShowUseful_Click(object sender, RoutedEventArgs e)
        {
            var view = new UsefulView();
            MainContentArea.Content = view;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _authService.Logout();

                var loginWindow = new LoginWindow(_authService, _apiService);
                loginWindow.Show();
                this.Close();
            }
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            bool isDark = !ThemeManager.IsDarkTheme;
            ThemeManager.ApplyTheme(isDark);

            // Обновляем текст кнопки
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.Content = isDark ? "☀️ Светлая тема" : "🌙 Тёмная тема";
            }
        }
    }
}