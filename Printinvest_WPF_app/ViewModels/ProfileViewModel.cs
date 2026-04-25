using Microsoft.Win32;
using Printinvest_WPF_app.Contex;
using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views.Pages;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private User _currentUser;
        private bool _isEditing;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public ICommand ToggleEditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ChangePhotoCommand { get; }
        public ICommand LogoutCommand { get; }

        public ProfileViewModel()
        {
            CurrentUser = SessionManager.CurrentUser ?? new User { Name = "Guest", Login = "guest", Role = UserRole.Client };
            if (SessionManager.CurrentUser == null)
            {
                //MessageBox.Show("Внимание: Пользователь не авторизован. Отображаются данные по умолчанию.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            ToggleEditCommand = new RelayCommandSec(o => ExecuteToggleEdit(o));
            SaveCommand = new RelayCommandSec(async o => await ExecuteSaveAsync(o), o => CanExecuteSave(o));
            ChangePhotoCommand = new RelayCommandSec(o => ExecuteChangePhoto(o));
            LogoutCommand = new RelayCommandSec(o => ExecuteLogout(o));
        }

        private void ExecuteToggleEdit(object parameter)
        {
            IsEditing = !IsEditing;
        }

        private async Task ExecuteSaveAsync(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentUser.Name) || string.IsNullOrWhiteSpace(CurrentUser.Login))
                {
                    MessageBox.Show(Application.Current.TryFindResource("ErrorInvalidInput")?.ToString() ?? "Please fill in all fields",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingUser = Repositories.RepositoryManager.Users.GetByLogin(CurrentUser.Login);
                if (existingUser != null && existingUser.Id != CurrentUser.Id)
                {
                    MessageBox.Show(Application.Current.TryFindResource("ErrorUserExists")?.ToString() ?? "User with this login already exists",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Repositories.RepositoryManager.Users.Update(CurrentUser);
                SessionManager.Login(CurrentUser); // Обновляем сессию
                MessageBox.Show(Application.Current.TryFindResource("ProfileUpdated")?.ToString() ?? "Profile updated successfully!",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении профиля: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CurrentUser.Name) && !string.IsNullOrWhiteSpace(CurrentUser.Login);
        }

        private void ExecuteChangePhoto(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    CurrentUser.Photo = File.ReadAllBytes(openFileDialog.FileName);
                    OnPropertyChanged(nameof(CurrentUser)); // Обновить привязку
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteLogout(object parameter)
        {
            SessionManager.Logout();
            var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
            if (mainViewModel != null)
            {
                mainViewModel.CurrentPage = new LoginPage();
            }
        }
    }
}