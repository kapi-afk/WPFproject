using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class RecoverViewModel : BaseViewModel
    {
        private static readonly object RecoveryRequestsLock = new object();
        private static readonly Dictionary<string, RecoveryRequestInfo> PendingRecoveryRequests =
            new Dictionary<string, RecoveryRequestInfo>();
        private static readonly TimeSpan RecoveryCodeLifetime = TimeSpan.FromMinutes(15);

        private readonly UserRepository _userRepository;
        private string _login;
        private string _recoveryCode;
        private string _newPassword;
        private string _confirmPassword;
        private string _errorMessage;
        private string _successMessage;
        private bool _isRecoveryCodeSent;

        public RecoverViewModel()
        {
            _userRepository = RepositoryManager.Users;
            RecoverCommand = new RelayCommand(RecoverExecute, CanRecoverExecute);
            ResetPasswordCommand = new RelayCommand(ResetPasswordExecute, CanResetPasswordExecute);
            NavigateToLoginCommand = new RelayCommand(() => Navigate("Login"));
        }

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string RecoveryCode
        {
            get => _recoveryCode;
            set => SetProperty(ref _recoveryCode, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasErrorMessage));
                }
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                if (SetProperty(ref _successMessage, value))
                {
                    OnPropertyChanged(nameof(HasSuccessMessage));
                }
            }
        }

        public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);
        public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

        public bool IsRecoveryCodeSent
        {
            get => _isRecoveryCodeSent;
            set => SetProperty(ref _isRecoveryCodeSent, value);
        }

        public ICommand RecoverCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        private bool CanRecoverExecute()
        {
            return !string.IsNullOrWhiteSpace(Login);
        }

        private bool CanResetPasswordExecute()
        {
            return !string.IsNullOrWhiteSpace(Login) &&
                   !string.IsNullOrWhiteSpace(RecoveryCode) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword);
        }

        private void RecoverExecute()
        {
            try
            {
                ClearMessages();
                var user = _userRepository.GetByLogin(Login);
                if (user == null)
                {
                    ErrorMessage = GetString("ErrorUserNotFound", "Пользователь не найден.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    ErrorMessage = GetString("RecoverEmailMissingMessage", "Для учетной записи не указана электронная почта.");
                    return;
                }

                var recoveryCode = GenerateRecoveryCode();
                var expiresAt = DateTime.Now.Add(RecoveryCodeLifetime);
                SaveRecoveryRequest(user.Login, recoveryCode, expiresAt);

                if (!OrderEmailService.TrySendPasswordRecoveryCode(user, recoveryCode, expiresAt))
                {
                    ErrorMessage = GetString("RecoverSendFailedMessage", "Не удалось отправить код восстановления. Проверьте настройки почты.");
                    return;
                }

                IsRecoveryCodeSent = true;
                SuccessMessage = string.Format(
                    GetString("RecoverCodeSentMessage", "Код восстановления отправлен на email {0}."),
                    user.Email);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
        }

        private void ResetPasswordExecute()
        {
            try
            {
                ClearMessages();
                var user = _userRepository.GetByLogin(Login);
                if (user == null)
                {
                    ErrorMessage = GetString("ErrorUserNotFound", "Пользователь не найден.");
                    return;
                }

                if (!TryGetRecoveryRequest(user.Login, out var recoveryRequest))
                {
                    ErrorMessage = GetString("RecoverCodeRequiredMessage", "Сначала запросите код восстановления.");
                    return;
                }

                if (recoveryRequest.ExpiresAt <= DateTime.Now)
                {
                    RemoveRecoveryRequest(user.Login);
                    IsRecoveryCodeSent = false;
                    ErrorMessage = GetString("RecoverCodeExpiredMessage", "Срок действия кода истек. Запросите новый код.");
                    return;
                }

                if (!string.Equals((RecoveryCode ?? string.Empty).Trim(), recoveryRequest.Code, StringComparison.Ordinal))
                {
                    ErrorMessage = GetString("RecoverCodeInvalidMessage", "Введен неверный код восстановления.");
                    return;
                }

                var passwordValidationError = HashHelper.GetPasswordValidationError(NewPassword);
                if (!string.IsNullOrWhiteSpace(passwordValidationError))
                {
                    ErrorMessage = passwordValidationError;
                    return;
                }

                if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
                {
                    ErrorMessage = GetString("RecoverPasswordsMismatchMessage", "Пароли не совпадают.");
                    return;
                }

                user.HashPassword = HashHelper.HashPassword(NewPassword);
                _userRepository.Update(user);

                RemoveRecoveryRequest(user.Login);
                RecoveryCode = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
                IsRecoveryCodeSent = false;
                SuccessMessage = GetString("RecoverPasswordChangedMessage", "Пароль успешно изменен. Теперь вы можете войти в систему.");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
        }

        private void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        private void Navigate(string page)
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow?.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.CurrentPage = new LoginPage();
            }
        }

        private static string GenerateRecoveryCode()
        {
            using (var random = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                random.GetBytes(bytes);
                var value = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
                return value.ToString("D6");
            }
        }

        private static void SaveRecoveryRequest(string login, string code, DateTime expiresAt)
        {
            lock (RecoveryRequestsLock)
            {
                PendingRecoveryRequests[NormalizeLogin(login)] = new RecoveryRequestInfo
                {
                    Code = code,
                    ExpiresAt = expiresAt
                };
            }
        }

        private static bool TryGetRecoveryRequest(string login, out RecoveryRequestInfo recoveryRequest)
        {
            lock (RecoveryRequestsLock)
            {
                CleanupExpiredRecoveryRequests();
                return PendingRecoveryRequests.TryGetValue(NormalizeLogin(login), out recoveryRequest);
            }
        }

        private static void RemoveRecoveryRequest(string login)
        {
            lock (RecoveryRequestsLock)
            {
                PendingRecoveryRequests.Remove(NormalizeLogin(login));
            }
        }

        private static void CleanupExpiredRecoveryRequests()
        {
            var now = DateTime.Now;
            var expiredKeys = PendingRecoveryRequests
                .Where(item => item.Value.ExpiresAt <= now)
                .Select(item => item.Key)
                .ToList();

            foreach (var expiredKey in expiredKeys)
            {
                PendingRecoveryRequests.Remove(expiredKey);
            }
        }

        private static string NormalizeLogin(string login)
        {
            return string.IsNullOrWhiteSpace(login)
                ? string.Empty
                : login.Trim().ToLowerInvariant();
        }

        private static string GetString(string key, string fallback)
        {
            return Application.Current?.TryFindResource(key)?.ToString() ?? fallback;
        }

        private sealed class RecoveryRequestInfo
        {
            public string Code { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
