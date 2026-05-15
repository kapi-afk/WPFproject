using ServiceCenter.Repositories;
using ServiceCenter.Utilities;
using ServiceCenter.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenter.ViewModels
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
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
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

                if (!TryGetUserByLogin(out var user))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    ErrorMessage = GetString("RecoverEmailMissingMessage", "No email is specified for this account.");
                    return;
                }

                var recoveryCode = GenerateRecoveryCode();
                var expiresAt = DateTime.Now.Add(RecoveryCodeLifetime);
                SaveRecoveryRequest(user.Login, recoveryCode, expiresAt);

                if (!OrderEmailService.TrySendPasswordRecoveryCode(user, recoveryCode, expiresAt))
                {
                    ErrorMessage = GetString("RecoverSendFailedMessage", "Failed to send the recovery code. Check the mail settings.");
                    return;
                }

                IsRecoveryCodeSent = true;
                SuccessMessage = string.Format(
                    GetString("RecoverCodeSentMessage", "The recovery code was sent to {0}."),
                    user.Email);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format(GetString("GenericErrorFormat", "Error: {0}"), ex.Message);
            }
        }

        private void ResetPasswordExecute()
        {
            try
            {
                ClearMessages();

                if (!TryGetUserByLogin(out var user))
                {
                    return;
                }

                if (!TryValidateRecoveryRequest(user.Login, out var recoveryRequest))
                {
                    return;
                }

                if (!string.Equals(NormalizeRecoveryCode(RecoveryCode), recoveryRequest.Code, StringComparison.Ordinal))
                {
                    ErrorMessage = GetString("RecoverCodeInvalidMessage", "The recovery code is invalid.");
                    return;
                }

                if (!TryValidateNewPassword())
                {
                    return;
                }

                user.HashPassword = HashHelper.HashPassword(NewPassword);
                _userRepository.Update(user);
                CompletePasswordReset(user.Login);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format(GetString("GenericErrorFormat", "Error: {0}"), ex.Message);
            }
        }

        private void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        private bool TryGetUserByLogin(out Models.User user)
        {
            user = _userRepository.GetByLogin(Login);
            if (user != null)
            {
                return true;
            }

            ErrorMessage = GetString("ErrorUserNotFound", "User not found.");
            return false;
        }

        private bool TryValidateRecoveryRequest(string login, out RecoveryRequestInfo recoveryRequest)
        {
            if (!TryGetRecoveryRequest(login, out recoveryRequest))
            {
                ErrorMessage = GetString("RecoverCodeRequiredMessage", "Request a recovery code first.");
                return false;
            }

            if (recoveryRequest.ExpiresAt > DateTime.Now)
            {
                return true;
            }

            RemoveRecoveryRequest(login);
            IsRecoveryCodeSent = false;
            ErrorMessage = GetString("RecoverCodeExpiredMessage", "The recovery code has expired. Request a new one.");
            return false;
        }

        private bool TryValidateNewPassword()
        {
            var passwordValidationError = HashHelper.GetPasswordValidationError(NewPassword);
            if (!string.IsNullOrWhiteSpace(passwordValidationError))
            {
                ErrorMessage = passwordValidationError;
                return false;
            }

            if (string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
            {
                return true;
            }

            ErrorMessage = GetString("RecoverPasswordsMismatchMessage", "Passwords do not match.");
            return false;
        }

        private void CompletePasswordReset(string login)
        {
            RemoveRecoveryRequest(login);
            RecoveryCode = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            IsRecoveryCodeSent = false;
            SuccessMessage = GetString("RecoverPasswordChangedMessage", "The password has been changed successfully. You can sign in now.");
        }

        private void NavigateToLogin()
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

        private static string NormalizeRecoveryCode(string recoveryCode)
        {
            return string.IsNullOrWhiteSpace(recoveryCode)
                ? string.Empty
                : recoveryCode.Trim();
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
