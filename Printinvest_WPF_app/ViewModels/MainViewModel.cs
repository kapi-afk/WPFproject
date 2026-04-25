using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Printinvest_WPF_app.Views.Pages;
using Printinvest_WPF_app.Properties;

namespace Printinvest_WPF_app.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Page _currentPage;
        private bool _isDark;
        private bool _isEnglish;
        private bool _navVisible = true;

        private readonly ResourceDictionary _lightTheme = new ResourceDictionary() { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) };
        private readonly ResourceDictionary _darkTheme = new ResourceDictionary() { Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative) };
        private readonly ResourceDictionary _ruLoc = new ResourceDictionary() { Source = new Uri("Localization/Strings.ru-RU.xaml", UriKind.Relative) };
        private readonly ResourceDictionary _enLoc = new ResourceDictionary() { Source = new Uri("Localization/Strings.en-US.xaml", UriKind.Relative) };

        public MainViewModel()
        {
            CurrentPage = new LoginPage();
            NavigateProfileCommand = new RelayCommand(() => {
                if (!SessionManager.IsAuthenticated)
                    CurrentPage = new LoginPage();
                else if (SessionManager.IsAdmin)
                    CurrentPage = new AdminPanelPage();
                else if (SessionManager.IsMaster)
                    CurrentPage = new ManagerPanelPage();
                else 
                    CurrentPage = new ProfilePage(); 
            });
            NavigateCartCommand = new RelayCommand(() => {
                if (!SessionManager.IsAuthenticated)
                    CurrentPage = new LoginPage();
                else if (SessionManager.IsAdmin)
                    CurrentPage = new AdminPanelPage();
                else if (SessionManager.IsMaster)
                    CurrentPage = new ManagerPanelPage();
                else
                    CurrentPage = new CartPage();
                });
            ToggleNavCommand = new RelayCommand(() => NavVisible = !NavVisible);
            ChangeThemeCommand = new RelayCommand(ToggleTheme);
            ChangeLanguageCommand = new RelayCommand(ToggleLanguage);
            LogoutCommand = new RelayCommand(Logout);
        }

        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(IsAdminOrMasterAuthenticated));
            }
        }
        public bool NavVisible
        {
            get => _navVisible;
            set { _navVisible = value; OnPropertyChanged(nameof(NavVisible)); }
        }

        public ICommand NavigateProfileCommand { get; }
        public ICommand NavigateCartCommand { get; }
        public ICommand ToggleNavCommand { get; }
        public ICommand ChangeThemeCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public ICommand LogoutCommand { get; }
        public bool IsAdminOrMasterAuthenticated => SessionManager.IsAdmin || SessionManager.IsMaster;

        private void ToggleTheme()
        {
            var app = Application.Current;

            // Удаляем текущую тему и добавляем противоположную
            app.Resources.MergedDictionaries.Remove(_isDark ? _darkTheme : _lightTheme);
            app.Resources.MergedDictionaries.Add(_isDark ? _lightTheme : _darkTheme);

            // Меняем флаг
            _isDark = !_isDark;

            // Сохраняем в настройки
            Settings.Default.AppTheme = _isDark ? "Dark" : "Light";
            Settings.Default.Save();
        }

        private void ToggleLanguage()
        {
            var app = Application.Current;

            // Удаляем текущую локализацию и добавляем другую
            app.Resources.MergedDictionaries.Remove(_isEnglish ? _enLoc : _ruLoc);
            app.Resources.MergedDictionaries.Add(_isEnglish ? _ruLoc : _enLoc);

            // Устанавливаем культуру
            string newCulture = _isEnglish ? "ru-RU" : "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(newCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(newCulture);

            // Обновляем UI, если требуется
            OnPropertyChanged(nameof(WindowTitle));

            // Меняем флаг
            _isEnglish = !_isEnglish;

            // Сохраняем в настройки
            Settings.Default.AppLanguage = newCulture;
            Settings.Default.Save();
        }

        public string WindowTitle => Application.Current.TryFindResource("AppTitle")?.ToString();

        private void Logout()
        {
            SessionManager.Logout();
            CurrentPage = new LoginPage();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
