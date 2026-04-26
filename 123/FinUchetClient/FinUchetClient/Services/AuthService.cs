using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FinUchetClient.Services
{
    public class AuthService
    {
        private readonly ApiService _apiService;
        private readonly string _tokenPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FinUchetClient", "token.dat");

        public string CurrentToken { get; private set; }
        public string CurrentUsername { get; private set; }

        public AuthService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var token = await _apiService.LoginAsync(username, password);
                if (!string.IsNullOrEmpty(token))
                {
                    CurrentToken = token;
                    CurrentUsername = username;
                    _apiService.SetToken(token);
                    await SaveTokenAsync(token);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string password, string securityQuestion, string securityAnswer, string email = "")
        {
            try
            {
                return await _apiService.RegisterAsync(username, password, securityQuestion, securityAnswer, email);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryAutoLoginAsync()
        {
            try
            {
                var token = await LoadTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    CurrentToken = token;
                    _apiService.SetToken(token);

                    // Проверяем, работает ли токен (пытаемся получить данные)
                    var transactions = await _apiService.GetTransactionsAsync();
                    return transactions != null;
                }
            }
            catch
            {
                // Токен невалидный - удаляем
                await DeleteTokenAsync();
            }
            return false;
        }

        public void Logout()
        {
            CurrentToken = null;
            CurrentUsername = null;
            _apiService.SetToken(null);
            DeleteTokenAsync().Wait();
        }

        private async Task SaveTokenAsync(string token)
        {
            try
            {
                var directory = Path.GetDirectoryName(_tokenPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var encrypted = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(token),
                    null,
                    DataProtectionScope.CurrentUser);

                await File.WriteAllBytesAsync(_tokenPath, encrypted);
            }
            catch { /* Игнорируем ошибки сохранения */ }
        }

        private async Task<string> LoadTokenAsync()
        {
            try
            {
                if (File.Exists(_tokenPath))
                {
                    var encrypted = await File.ReadAllBytesAsync(_tokenPath);
                    var decrypted = ProtectedData.Unprotect(
                        encrypted,
                        null,
                        DataProtectionScope.CurrentUser);

                    return Encoding.UTF8.GetString(decrypted);
                }
            }
            catch { /* Игнорируем ошибки загрузки */ }
            return null;
        }

        private async Task DeleteTokenAsync()
        {
            try
            {
                if (File.Exists(_tokenPath))
                    File.Delete(_tokenPath);
            }
            catch { /* Игнорируем ошибки удаления */ }
        }
    }
}