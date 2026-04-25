using Printinvest_WPF_app.Contex;
using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views.Pages;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _login;
        private string _password;

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommandSec(o => ExecuteLogin(o), o => CanExecuteLogin(o));
            NavigateCommand = new RelayCommandSec(o => ExecuteNavigate(o));
        }

        private void ExecuteLogin(object parameter)
        {
            try
            {
                User user = Repositories.RepositoryManager.Users.GetByLogin(Login);
                if (user == null || !HashHelper.VerifyPassword(Password, user.HashPassword))
                {
                    MessageBox.Show(Application.Current.TryFindResource("ErrorInvalidCredentials")?.ToString() ?? "Invalid login or password",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
                SessionManager.Login(user);

                if (mainViewModel != null)
                {
                    if (user.Role == UserRole.Admin)
                    {
                        mainViewModel.CurrentPage = new AdminPanelPage();
                    }
                    else if (user.Role == UserRole.Master)
                    {
                        mainViewModel.CurrentPage = new ManagerPanelPage();
                    }
                    else
                    {
                        mainViewModel.CurrentPage = new ProfilePage();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteNavigate(object parameter)
        {
            if (parameter is string page)
            {
                var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
                if (mainViewModel != null)
                {
                    if (page == "Register")
                    {
                        mainViewModel.CurrentPage = new RegisterPage();
                    }
                    else if (page == "Recover")
                    {
                        mainViewModel.CurrentPage = new RecoverPage();
                    }
                }
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
