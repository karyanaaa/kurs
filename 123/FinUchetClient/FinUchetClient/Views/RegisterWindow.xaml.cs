using FinUchetClient.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FinUchetClient.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;

        public RegisterWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _authService = new AuthService(_apiService);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Метод нужен для события
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Метод нужен для события
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string securityQuestion = (SecurityQuestionBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            string securityAnswer = SecurityAnswerBox.Text.Trim();

            // Очищаем предыдущие ошибки
            ErrorText.Visibility = Visibility.Collapsed;

            // Валидация
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Введите имя пользователя");
                return;
            }

            if (username.Length < 3)
            {
                ShowError("Имя пользователя должно быть не менее 3 символов");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите пароль");
                return;
            }

            if (password.Length < 4)
            {
                ShowError("Пароль должен быть не менее 4 символов");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            if (string.IsNullOrWhiteSpace(securityQuestion))
            {
                ShowError("Выберите контрольный вопрос");
                return;
            }

            if (string.IsNullOrWhiteSpace(securityAnswer))
            {
                ShowError("Введите ответ на контрольный вопрос");
                return;
            }

            try
            {
                var button = sender as Button;
                button.IsEnabled = false;
                button.Content = "Регистрация...";

                // Используем метод с 5 параметрами
                var success = await _apiService.RegisterAsync(username, password, securityQuestion, securityAnswer, email);

                if (success)
                {
                    MessageBox.Show("Регистрация успешна! Теперь войдите в систему.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.Close(); // Просто закрываем, окно входа покажется само
                }
                else
                {
                    ShowError("Ошибка регистрации. Возможно, пользователь уже существует.");
                    button.IsEnabled = true;
                    button.Content = "Зарегистрироваться";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
                var button = sender as Button;
                button.IsEnabled = true;
                button.Content = "Зарегистрироваться";
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Просто закрываем, окно входа покажется через Closed событие
        }
    }
}