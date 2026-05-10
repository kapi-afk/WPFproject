using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views.Pages;
using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Printinvest_WPF_app.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private const string NamePattern = @"^[A-Za-z\u0410-\u042F\u0430-\u044F\u0401\u0451]+(?:-[A-Za-z\u0410-\u042F\u0430-\u044F\u0401\u0451]+)*$";

        private readonly UserRepository _userRepository;
        private string _login;
        private string _password;
        private string _confirmPassword;
        private string _lastName;
        private string _firstName;
        private string _middleName;
        private string _email;
        private string _errorMessage;
        private bool _hasAttemptedRegister;
        private string _lastNameValidationMessage;
        private string _firstNameValidationMessage;
        private string _middleNameValidationMessage;
        private string _loginValidationMessage;
        private string _emailValidationMessage;
        private string _passwordValidationMessage;
        private string _confirmPasswordValidationMessage;

        public string Login
        {
            get => _login;
            set
            {
                if (SetProperty(ref _login, value))
                {
                    ClearGlobalError();
                    ValidateLogin(_hasAttemptedRegister);
                    RefreshRegisterState();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ClearGlobalError();
                    ValidatePassword(_hasAttemptedRegister);
                    ValidateConfirmPassword(_hasAttemptedRegister);
                    RefreshRegisterState();
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    ClearGlobalError();
                    ValidateConfirmPassword(_hasAttemptedRegister);
                    RefreshRegisterState();
                }
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                {
                    ClearGlobalError();
                    ValidateLastName(_hasAttemptedRegister);
                    RefreshRegisterState();
                }
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (SetProperty(ref _firstName, value))
                {
                    ClearGlobalError();
                    ValidateFirstName(_hasAttemptedRegister);
                    RefreshRegisterState();
                }
            }
        }

        public string MiddleName
        {
            get => _middleName;
            set
            {
                if (SetProperty(ref _middleName, value))
                {
                    ClearGlobalError();
                    ValidateMiddleName(_hasAttemptedRegister);
                    RefreshRegisterState();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ClearGlobalError();
                    ValidateEmail(_hasAttemptedRegister);
                    RefreshRegisterState();
                }
            }
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

        public string LastNameValidationMessage
        {
            get => _lastNameValidationMessage;
            set => SetValidationMessage(ref _lastNameValidationMessage, value, nameof(LastNameValidationMessage), nameof(HasLastNameError));
        }

        public string FirstNameValidationMessage
        {
            get => _firstNameValidationMessage;
            set => SetValidationMessage(ref _firstNameValidationMessage, value, nameof(FirstNameValidationMessage), nameof(HasFirstNameError));
        }

        public string MiddleNameValidationMessage
        {
            get => _middleNameValidationMessage;
            set => SetValidationMessage(ref _middleNameValidationMessage, value, nameof(MiddleNameValidationMessage), nameof(HasMiddleNameError));
        }

        public string LoginValidationMessage
        {
            get => _loginValidationMessage;
            set => SetValidationMessage(ref _loginValidationMessage, value, nameof(LoginValidationMessage), nameof(HasLoginValidationError));
        }

        public string EmailValidationMessage
        {
            get => _emailValidationMessage;
            set => SetValidationMessage(ref _emailValidationMessage, value, nameof(EmailValidationMessage), nameof(HasEmailError));
        }

        public string PasswordValidationMessage
        {
            get => _passwordValidationMessage;
            set => SetValidationMessage(ref _passwordValidationMessage, value, nameof(PasswordValidationMessage), nameof(HasPasswordError));
        }

        public string ConfirmPasswordValidationMessage
        {
            get => _confirmPasswordValidationMessage;
            set => SetValidationMessage(ref _confirmPasswordValidationMessage, value, nameof(ConfirmPasswordValidationMessage), nameof(HasConfirmPasswordError));
        }

        public bool HasLastNameError => !string.IsNullOrWhiteSpace(LastNameValidationMessage);
        public bool HasFirstNameError => !string.IsNullOrWhiteSpace(FirstNameValidationMessage);
        public bool HasMiddleNameError => !string.IsNullOrWhiteSpace(MiddleNameValidationMessage);
        public bool HasLoginValidationError => !string.IsNullOrWhiteSpace(LoginValidationMessage);
        public bool HasEmailError => !string.IsNullOrWhiteSpace(EmailValidationMessage);
        public bool HasPasswordError => !string.IsNullOrWhiteSpace(PasswordValidationMessage);
        public bool HasConfirmPasswordError => !string.IsNullOrWhiteSpace(ConfirmPasswordValidationMessage);
        public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);
        public bool HasValidationErrors =>
            HasLastNameError ||
            HasFirstNameError ||
            HasMiddleNameError ||
            HasLoginValidationError ||
            HasEmailError ||
            HasPasswordError ||
            HasConfirmPasswordError;

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public RegisterViewModel()
        {
            _userRepository = RepositoryManager.Users;
            RegisterCommand = new RelayCommand(RegisterExecute, CanRegisterExecute);
            NavigateToLoginCommand = new RelayCommand(() => Navigate("Login"));
        }

        private string FullName => string.Join(" ", new[] { LastName, FirstName, MiddleName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim()));

        private bool CanRegisterExecute()
        {
            return !string.IsNullOrWhiteSpace(Login) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !HasValidationErrors;
        }

        private void RegisterExecute()
        {
            try
            {
                ErrorMessage = string.Empty;
                _hasAttemptedRegister = true;

                if (!ValidateAll())
                {
                    RefreshRegisterState();
                    return;
                }

                var normalizedLogin = Login.Trim();
                var normalizedEmail = NormalizeEmail(Email);

                var newUser = new Models.User
                {
                    Login = normalizedLogin,
                    HashPassword = HashHelper.HashPassword(Password),
                    Name = FullName,
                    Email = normalizedEmail,
                    Role = Models.UserRole.Client
                };

                _userRepository.Add(newUser);
                SessionManager.Login(newUser);
                Navigate("Profile");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"\u041E\u0448\u0438\u0431\u043A\u0430: {ex.Message}";
            }
        }

        private bool ValidateAll()
        {
            ValidateLastName(true);
            ValidateFirstName(true);
            ValidateMiddleName(true);
            ValidateLogin(true);
            ValidateEmail(true);
            ValidatePassword(true);
            ValidateConfirmPassword(true);
            return !HasValidationErrors;
        }

        private void ValidateLastName(bool showRequired)
        {
            LastNameValidationMessage = ValidateNamePart(
                LastName,
                showRequired,
                "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u0444\u0430\u043C\u0438\u043B\u0438\u044E.");
        }

        private void ValidateFirstName(bool showRequired)
        {
            FirstNameValidationMessage = ValidateNamePart(
                FirstName,
                showRequired,
                "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u0438\u043C\u044F.");
        }

        private void ValidateMiddleName(bool showRequired)
        {
            MiddleNameValidationMessage = ValidateNamePart(
                MiddleName,
                false,
                string.Empty);
        }

        private void ValidateLogin(bool showRequired)
        {
            var normalizedLogin = Login?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedLogin))
            {
                LoginValidationMessage = showRequired
                    ? "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u043B\u043E\u0433\u0438\u043D."
                    : string.Empty;
                return;
            }

            var existingUser = _userRepository.GetByLogin(normalizedLogin);
            LoginValidationMessage = existingUser == null
                ? string.Empty
                : "\u042D\u0442\u043E\u0442 \u043B\u043E\u0433\u0438\u043D \u0443\u0436\u0435 \u0437\u0430\u043D\u044F\u0442. \u041F\u043E\u043C\u0435\u043D\u044F\u0439\u0442\u0435 \u043B\u043E\u0433\u0438\u043D.";
        }

        private void ValidateEmail(bool showRequired)
        {
            var normalizedEmail = NormalizeEmail(Email);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                EmailValidationMessage = showRequired
                    ? "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u044D\u043B\u0435\u043A\u0442\u0440\u043E\u043D\u043D\u0443\u044E \u043F\u043E\u0447\u0442\u0443."
                    : string.Empty;
                return;
            }

            if (!IsValidEmail(normalizedEmail))
            {
                EmailValidationMessage = "\u0423\u043A\u0430\u0436\u0438\u0442\u0435 \u043A\u043E\u0440\u0440\u0435\u043A\u0442\u043D\u044B\u0439 email.";
                return;
            }

            var existingEmailUser = _userRepository
                .GetAll()
                .FirstOrDefault(user => string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

            EmailValidationMessage = existingEmailUser == null
                ? string.Empty
                : "\u042D\u0442\u0430 \u044D\u043B\u0435\u043A\u0442\u0440\u043E\u043D\u043D\u0430\u044F \u043F\u043E\u0447\u0442\u0430 \u0443\u0436\u0435 \u0437\u0430\u0440\u0435\u0433\u0438\u0441\u0442\u0440\u0438\u0440\u043E\u0432\u0430\u043D\u0430.";
        }

        private void ValidatePassword(bool showRequired)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordValidationMessage = showRequired
                    ? "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u043F\u0430\u0440\u043E\u043B\u044C."
                    : string.Empty;
                return;
            }

            PasswordValidationMessage = HashHelper.GetPasswordValidationError(Password);
        }

        private void ValidateConfirmPassword(bool showRequired)
        {
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfirmPasswordValidationMessage = showRequired
                    ? "\u041F\u043E\u0432\u0442\u043E\u0440\u0438\u0442\u0435 \u043F\u0430\u0440\u043E\u043B\u044C."
                    : string.Empty;
                return;
            }

            ConfirmPasswordValidationMessage = Password == ConfirmPassword
                ? string.Empty
                : "\u041F\u0430\u0440\u043E\u043B\u0438 \u043D\u0435 \u0441\u043E\u0432\u043F\u0430\u0434\u0430\u044E\u0442.";
        }

        private static string ValidateNamePart(string value, bool isRequired, string requiredMessage)
        {
            var normalizedValue = value?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                return isRequired ? requiredMessage : string.Empty;
            }

            return Regex.IsMatch(normalizedValue, NamePattern)
                ? string.Empty
                : "\u0414\u043E\u043F\u0443\u0441\u0442\u0438\u043C\u044B \u0442\u043E\u043B\u044C\u043A\u043E \u043A\u0438\u0440\u0438\u043B\u043B\u0438\u0446\u0430, \u043B\u0430\u0442\u0438\u043D\u0438\u0446\u0430 \u0438 \u0434\u0435\u0444\u0438\u0441.";
        }

        private void SetValidationMessage(ref string field, string value, string propertyName, string flagPropertyName)
        {
            if (SetProperty(ref field, value, propertyName))
            {
                OnPropertyChanged(flagPropertyName);
                OnPropertyChanged(nameof(HasValidationErrors));
            }
        }

        private void ClearGlobalError()
        {
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                ErrorMessage = string.Empty;
            }
        }

        private static void RefreshRegisterState()
        {
            Application.Current?.Dispatcher?.BeginInvoke(
                DispatcherPriority.Background,
                new Action(CommandManager.InvalidateRequerySuggested));
        }

        private static string NormalizeEmail(string email)
        {
            return string.IsNullOrWhiteSpace(email)
                ? string.Empty
                : email.Trim().ToLowerInvariant();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var parsedEmail = new MailAddress(email);
                return string.Equals(parsedEmail.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void Navigate(string page)
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow?.DataContext is MainViewModel mainViewModel)
            {
                if (page == "Login")
                {
                    mainViewModel.CurrentPage = new LoginPage();
                }
                else
                {
                    mainViewModel.CurrentPage = new ProfilePage();
                }
            }
        }
    }
}
