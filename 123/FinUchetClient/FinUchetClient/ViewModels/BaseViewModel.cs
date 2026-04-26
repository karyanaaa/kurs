using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace FinUchetClient.ViewModels
{
    public class BaseViewModel : ObservableObject
    {
        private bool _isBusy;
        private string _title;
        private string _errorMessage;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        protected async Task ExecuteAsync(Func<Task> operation, string errorMessage = "Произошла ошибка")
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                await operation();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{errorMessage}: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}