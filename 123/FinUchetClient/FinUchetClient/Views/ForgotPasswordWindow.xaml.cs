using System;
using System.Windows;
using FinUchetClient.Services;

namespace FinUchetClient.Views
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly ApiService _apiService;
        private string _securityQuestion = "";

        public ForgotPasswordWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void UsernameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string username = UsernameBox.Text;
            if (!string.IsNullOrWhiteSpace(username))
            {
                try
                {
                    // Получаем контрольный вопрос пользователя
                    var question = await _apiService.GetSecurityQuestionAsync(username);
                    if (!string.IsNullOrEmpty(question))
                    {
                        _securityQuestion = question;
                        SecurityQuestionBox.Text = question;
                    }
                    else
                    {
                        SecurityQuestionBox.Text = "Пользователь не найден";
                    }
                }
                catch (Exception ex)
                {
                    SecurityQuestionBox.Text = "Ошибка загрузки вопроса";
                }
            }
            else
            {
                SecurityQuestionBox.Text = "";
            }
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

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            string username = UsernameBox.Text;
            string answer = AnswerBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Введите имя пользователя");
                return;
            }

            if (string.IsNullOrWhiteSpace(answer))
            {
                ShowError("Введите ответ на контрольный вопрос");
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ShowError("Введите новый пароль");
                return;
            }

            if (newPassword.Length < 4)
            {
                ShowError("Пароль должен быть не менее 4 символов");
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            try
            {
                var button = sender as System.Windows.Controls.Button;
                button.IsEnabled = false;
                button.Content = "Сброс...";

                var success = await _apiService.ResetPasswordAsync(username, answer, newPassword);

                if (success)
                {
                    MessageBox.Show("Пароль успешно изменен! Теперь войдите с новым паролем.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.Close(); // Просто закрываем, окно входа покажется само
                }
                else
                {
                    ShowError("Неверное имя пользователя или ответ на вопрос");
                    button.IsEnabled = true;
                    button.Content = "Сбросить пароль";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
                var button = sender as System.Windows.Controls.Button;
                button.IsEnabled = true;
                button.Content = "Сбросить пароль";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Просто закрываем, окно входа покажется через Closed событие
        }
    }
}