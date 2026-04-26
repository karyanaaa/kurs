using CommunityToolkit.Mvvm.ComponentModel;
using FinUchetClient.Services;

namespace FinUchetClient.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }
    }
}