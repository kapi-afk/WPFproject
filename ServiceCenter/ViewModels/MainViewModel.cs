using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ServiceCenter.Views.Pages;
using ServiceCenter.Properties;

namespace ServiceCenter.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const double CompactNavigationThreshold = 1420;

        private Page _currentPage;
        private bool _isDark;
        private bool _isEnglish;
        private bool _navVisible = false;
        private bool _isCompactNavigationMode;
        private double _lastWindowWidth;

        public MainViewModel()
        {
            _isDark = string.Equals(Settings.Default.AppTheme, "Dark", StringComparison.OrdinalIgnoreCase);
            _isEnglish = string.Equals(Settings.Default.AppLanguage, "en-US", StringComparison.OrdinalIgnoreCase);
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
            ToggleNavCommand = new RelayCommand(() => { });
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
                OnPropertyChanged(nameof(CanShowClientNavigation));
                OnPropertyChanged(nameof(IsShellNavigationVisible));
                UpdateWindowWidth(_lastWindowWidth);
            }
        }
        public bool NavVisible
        {
            get => _navVisible;
            set
            {
                var effectiveValue = false;
                if (_navVisible == effectiveValue)
                    return;

                _navVisible = effectiveValue;
                OnPropertyChanged(nameof(NavVisible));
            }
        }

        public ICommand NavigateProfileCommand { get; }
        public ICommand NavigateCartCommand { get; }
        public ICommand ToggleNavCommand { get; }
        public ICommand ChangeThemeCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public ICommand LogoutCommand { get; }
        public bool IsAdminOrMasterAuthenticated => SessionManager.IsAdmin || SessionManager.IsMaster;
        public bool CanShowClientNavigation => SessionManager.IsAuthenticated && !SessionManager.IsAdmin && !SessionManager.IsMaster;
        public bool IsShellNavigationVisible => true;
        public bool IsCompactNavigationMode => _isCompactNavigationMode;
        public string ThemeButtonText => Application.Current.TryFindResource(_isDark ? "SwitchToLightTheme" : "SwitchToDarkTheme")?.ToString()
                                         ?? (_isDark ? "Light theme" : "Dark theme");
        public string ThemeButtonToolTip => Application.Current.TryFindResource(_isDark ? "SwitchThemeToLightHint" : "SwitchThemeToDarkHint")?.ToString()
                                            ?? (_isDark ? "Switch to the light theme." : "Switch to the dark theme.");

        private void ToggleTheme()
        {
            App.ApplyTheme(_isDark ? "Light" : "Dark");
            _isDark = !_isDark;
            OnPropertyChanged(nameof(ThemeButtonText));
            OnPropertyChanged(nameof(ThemeButtonToolTip));
        }

        private void ToggleLanguage()
        {
            string newCulture = _isEnglish ? "ru-RU" : "en-US";
            App.ApplyLanguage(newCulture);

            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(ThemeButtonText));
            OnPropertyChanged(nameof(ThemeButtonToolTip));

            _isEnglish = !_isEnglish;
        }

        public string WindowTitle => Application.Current.TryFindResource("AppTitle")?.ToString();

        public void UpdateWindowWidth(double windowWidth)
        {
            _lastWindowWidth = windowWidth;

            var shouldForceCollapsed = IsShellNavigationVisible && windowWidth > 0 && windowWidth < CompactNavigationThreshold;
            if (_isCompactNavigationMode == shouldForceCollapsed)
            {
                if (shouldForceCollapsed && _navVisible)
                {
                    _navVisible = false;
                    OnPropertyChanged(nameof(NavVisible));
                }

                return;
            }

            _isCompactNavigationMode = shouldForceCollapsed;
            OnPropertyChanged(nameof(IsCompactNavigationMode));

            if (_isCompactNavigationMode && _navVisible)
            {
                _navVisible = false;
                OnPropertyChanged(nameof(NavVisible));
            }
        }

        private void Logout()
        {
            SessionManager.Logout();
            CurrentPage = new LoginPage();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
