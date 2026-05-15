using ServiceCenter.Repositories;
using ServiceCenter.Utilities;
using ServiceCenter.Views.Pages;
using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ServiceCenter.ViewModels
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
                    HandleInputChanged(() => ValidateLogin(_hasAttemptedRegister));
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
                    HandleInputChanged(
                        () => ValidatePassword(_hasAttemptedRegister),
                        () => ValidateConfirmPassword(_hasAttemptedRegister));
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
                    HandleInputChanged(() => ValidateConfirmPassword(_hasAttemptedRegister));
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
                    HandleInputChanged(() => ValidateLastName(_hasAttemptedRegister));
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
                    HandleInputChanged(() => ValidateFirstName(_hasAttemptedRegister));
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
                    HandleInputChanged(() => ValidateMiddleName(_hasAttemptedRegister));
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
                    HandleInputChanged(() => ValidateEmail(_hasAttemptedRegister));
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
            App.LanguageChanged += OnLanguageChanged;
        }

        private string FullName => string.Join(" ", new[] { LastName, FirstName, MiddleName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim()));

        private bool HasRequiredRegisterFields =>
            !string.IsNullOrWhiteSpace(Login) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !string.IsNullOrWhiteSpace(ConfirmPassword) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(Email);

        private bool CanRegisterExecute()
        {
            return HasRequiredRegisterFields && !HasValidationErrors;
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

                var newUser = CreateUser();
                _userRepository.Add(newUser);
                SessionManager.Login(newUser);
                Navigate("Profile");
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format(GetString("GenericErrorFormat", "Error: {0}"), ex.Message);
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
                GetString("RegistrationRequiredLastName", "Enter your last name."));
        }

        private void ValidateFirstName(bool showRequired)
        {
            FirstNameValidationMessage = ValidateNamePart(
                FirstName,
                showRequired,
                GetString("RegistrationRequiredFirstName", "Enter your first name."));
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
                LoginValidationMessage = GetRequiredValidationMessage(
                    showRequired,
                    "RegistrationRequiredLogin",
                    "Enter your login.");
                return;
            }

            var existingUser = _userRepository.GetByLogin(normalizedLogin);
            LoginValidationMessage = existingUser == null
                ? string.Empty
                : GetString("RegistrationLoginTaken", "This login is already taken. Choose another one.");
        }

        private void ValidateEmail(bool showRequired)
        {
            var normalizedEmail = NormalizeEmail(Email);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                EmailValidationMessage = GetRequiredValidationMessage(
                    showRequired,
                    "RegistrationRequiredEmail",
                    "Enter your email address.");
                return;
            }

            if (!IsValidEmail(normalizedEmail))
            {
                EmailValidationMessage = GetString("RegistrationInvalidEmail", "Enter a valid email address.");
                return;
            }

            var existingEmailUser = _userRepository
                .GetAll()
                .FirstOrDefault(user => string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

            EmailValidationMessage = existingEmailUser == null
                ? string.Empty
                : GetString("RegistrationEmailTaken", "This email address is already registered.");
        }

        private void ValidatePassword(bool showRequired)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordValidationMessage = GetRequiredValidationMessage(
                    showRequired,
                    "PasswordRequiredMessage",
                    "Enter a password.");
                return;
            }

            PasswordValidationMessage = HashHelper.GetPasswordValidationError(Password);
        }

        private void ValidateConfirmPassword(bool showRequired)
        {
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfirmPasswordValidationMessage = GetRequiredValidationMessage(
                    showRequired,
                    "RegistrationRequiredConfirmPassword",
                    "Repeat the password.");
                return;
            }

            ConfirmPasswordValidationMessage = Password == ConfirmPassword
                ? string.Empty
                : GetString("RegistrationPasswordsMismatch", "Passwords do not match.");
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
                : GetString("RegistrationNameInvalid", "Only Cyrillic, Latin letters, and hyphens are allowed.");
        }

        private void SetValidationMessage(ref string field, string value, string propertyName, string flagPropertyName)
        {
            if (SetProperty(ref field, value, propertyName))
            {
                OnPropertyChanged(flagPropertyName);
                OnPropertyChanged(nameof(HasValidationErrors));
            }
        }

        private Models.User CreateUser()
        {
            return new Models.User
            {
                Login = Login.Trim(),
                HashPassword = HashHelper.HashPassword(Password),
                Name = FullName,
                Email = NormalizeEmail(Email),
                Role = Models.UserRole.Client
            };
        }

        private static string GetRequiredValidationMessage(bool showRequired, string key, string fallback)
        {
            return showRequired
                ? GetString(key, fallback)
                : string.Empty;
        }

        private void ClearGlobalError()
        {
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                ErrorMessage = string.Empty;
            }
        }

        private void HandleInputChanged(params Action[] validationActions)
        {
            ClearGlobalError();

            foreach (var validationAction in validationActions)
            {
                validationAction();
            }

            RefreshRegisterState();
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

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            if (_hasAttemptedRegister || HasValidationErrors)
            {
                ValidateAll();
            }
        }

        private static string GetString(string key, string fallback)
        {
            return Application.Current?.TryFindResource(key)?.ToString() ?? fallback;
        }
    }
}
