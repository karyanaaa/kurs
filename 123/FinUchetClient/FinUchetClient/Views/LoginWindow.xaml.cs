using FinUchetClient.Services;
using System.Windows;
using System.Windows.Controls;

namespace FinUchetClient.Views
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        private RegisterWindow _registerWindow;
        private ForgotPasswordWindow _forgotWindow;

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public LoginWindow(AuthService authService, ApiService apiService)
        {
            InitializeComponent();
            _authService = authService;
            _apiService = apiService;
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Username = ((TextBox)sender).Text;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = ((PasswordBox)sender).Password;
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            try
            {
                var token = await _apiService.LoginAsync(username, password);

                if (!string.IsNullOrEmpty(token))
                {
                    ((App)Application.Current).Token = token;
                    _apiService.SetToken(token);
                    await _authService.LoginAsync(username, password);

                    var mainWindow = new MainWindow(_apiService, _authService);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка подключения к серверу: {ex.Message}");
            }
        }

        private void RegisterHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (_registerWindow == null || !_registerWindow.IsVisible)
            {
                _registerWindow = new RegisterWindow();
                _registerWindow.Closed += (s, args) => { this.Show(); _registerWindow = null; };
            }
            this.Hide();
            _registerWindow.Show();
        }

        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_forgotWindow == null || !_forgotWindow.IsVisible)
            {
                _forgotWindow = new ForgotPasswordWindow();
                _forgotWindow.Closed += (s, args) => { this.Show(); _forgotWindow = null; };
            }
            this.Hide();
            _forgotWindow.Show();
        }
    }
}