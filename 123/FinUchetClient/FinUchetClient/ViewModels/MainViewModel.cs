using CommunityToolkit.Mvvm.Input;
using FinUchetClient.Services;
using FinUchetClient.Views;
using System.Windows.Input;
using FinUchetClient.Models;
using FinUchetClient.ViewModels;

namespace FinUchetClient.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;

        private object _currentView;
        private string _userName;

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public ICommand ShowTransactionsCommand { get; }
        public ICommand ShowCategoriesCommand { get; }
        public ICommand ShowStatisticsCommand { get; }
        public ICommand LogoutCommand { get; }

        public TransactionsViewModel TransactionsVM { get; }
        public CategoriesViewModel CategoriesVM { get; }
        public StatisticsViewModel StatisticsVM { get; }

        public MainViewModel(ApiService apiService, AuthService authService)
        {
            _apiService = apiService;
            _authService = authService;

            TransactionsVM = new TransactionsViewModel(apiService);
            CategoriesVM = new CategoriesViewModel(apiService);
            StatisticsVM = new StatisticsViewModel(apiService);

            // ИСПРАВЛЕНО: Убрали параметр _ в лямбда-выражениях
            ShowTransactionsCommand = new RelayCommand(() => CurrentView = TransactionsVM);
            ShowCategoriesCommand = new RelayCommand(() => CurrentView = CategoriesVM);
            ShowStatisticsCommand = new RelayCommand(() => CurrentView = StatisticsVM);

            // ИСПРАВЛЕНО: Для LogoutCommand используем лямбда-выражение
            LogoutCommand = new RelayCommand(() => ExecuteLogout());

            // Загружаем начальные данные
            LoadData();

            // Показываем транзакции по умолчанию
            CurrentView = TransactionsVM;
        }

        private async void LoadData()
        {
            await TransactionsVM.LoadTransactionsAsync();
            await CategoriesVM.LoadCategoriesAsync();
            await StatisticsVM.LoadStatisticsAsync();
        }

        // ИСПРАВЛЕНО: Убрали параметр object parameter
        private void ExecuteLogout()
        {
            _authService.Logout();

            var loginWindow = new LoginWindow(_authService, _apiService);
            loginWindow.Show();

            // Закрываем текущее окно
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window is Views.MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}