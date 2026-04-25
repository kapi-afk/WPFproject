using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Views.Pages;
using System;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class RecoverViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private string _login;
        private string _errorMessage;
        private string _successMessage;

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public ICommand RecoverCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public RecoverViewModel()
        {
            _userRepository = RepositoryManager.Users;
            RecoverCommand = new RelayCommand(RecoverExecute, CanRecoverExecute);
            NavigateToLoginCommand = new RelayCommand(() => Navigate("Login"));
        }

        private bool CanRecoverExecute()
        {
            return !string.IsNullOrWhiteSpace(Login);
        }

        private async void RecoverExecute()
        {
            try
            {
                var user = _userRepository.GetByLogin(Login);
                if (user == null)
                {
                    ErrorMessage = Application.Current.Resources["ErrorUserNotFound"]?.ToString();
                    return;
                }

                // Здесь должна быть логика отправки временного пароля (например, по email)
                SuccessMessage = "Временный пароль отправлен на ваш email."; // Заглушка
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
        }

        private void Navigate(string page)
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow?.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.CurrentPage = new LoginPage();
            }
        }
    }
}