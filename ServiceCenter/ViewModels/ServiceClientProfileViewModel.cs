using Microsoft.Win32;
using ServiceCenter.Models;
using ServiceCenter.Repositories;
using ServiceCenter.Utilities;
using ServiceCenter.Views;
using ServiceCenter.Views.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenter.ViewModels
{
    public class ServiceClientProfileViewModel : BaseViewModel
    {
        private const string OtherOption = "Другое";

        private readonly UserRepository _userRepository;
        private readonly OrderRepository _orderRepository;
        private readonly CommentRepository _commentRepository;
        private readonly Dictionary<string, string[]> _brandCatalog = new Dictionary<string, string[]>
        {
            ["Ноутбук"] = new[] { "Lenovo", "ASUS", "HP", "Acer", "Dell", "Apple", "MSI" },
            ["Стационарный ПК"] = new[] { "Dell", "HP", "Lenovo", "ASUS", "MSI", "Acer" },
            ["Моноблок"] = new[] { "Lenovo", "HP", "Apple", "Acer", "ASUS" },
            ["Монитор"] = new[] { "Samsung", "LG", "AOC", "Philips", "Dell", "BenQ" },
            ["Принтер"] = new[] { "HP", "Canon", "Epson", "Brother", "Xerox" },
            ["Другое"] = new string[0]
        };
        private readonly Dictionary<string, string[]> _defaultModelCatalog = new Dictionary<string, string[]>
        {
            ["Ноутбук"] = new[] { "IdeaPad", "ThinkPad", "VivoBook", "Pavilion", "Aspire", "MacBook" },
            ["Стационарный ПК"] = new[] { "OptiPlex", "ProDesk", "ThinkCentre", "ROG", "MAG", "Nitro" },
            ["Моноблок"] = new[] { "iMac", "IdeaCentre AIO", "Aspire C", "Zen AiO", "ProOne" },
            ["Монитор"] = new[] { "Odyssey", "UltraGear", "ThinkVision", "P-series", "GW", "24MK" },
            ["Принтер"] = new[] { "LaserJet", "DeskJet", "PIXMA", "EcoTank", "HL-L", "WorkCentre" },
            ["Другое"] = new string[0]
        };
        private readonly Dictionary<string, Dictionary<string, string[]>> _brandModelCatalog = new Dictionary<string, Dictionary<string, string[]>>
        {
            ["Ноутбук"] = new Dictionary<string, string[]>
            {
                ["Lenovo"] = new[] { "IdeaPad", "ThinkPad", "Legion", "Yoga" },
                ["ASUS"] = new[] { "VivoBook", "Zenbook", "ROG", "TUF" },
                ["HP"] = new[] { "Pavilion", "Victus", "ProBook", "EliteBook" },
                ["Acer"] = new[] { "Aspire", "Nitro", "Swift", "Predator" },
                ["Dell"] = new[] { "Inspiron", "Latitude", "Vostro", "XPS" },
                ["Apple"] = new[] { "MacBook Air", "MacBook Pro" },
                ["MSI"] = new[] { "Modern", "Katana", "Prestige", "Stealth" }
            },
            ["Стационарный ПК"] = new Dictionary<string, string[]>
            {
                ["Dell"] = new[] { "OptiPlex", "Precision", "Inspiron" },
                ["HP"] = new[] { "ProDesk", "EliteDesk", "Pavilion" },
                ["Lenovo"] = new[] { "ThinkCentre", "IdeaCentre", "Legion" },
                ["ASUS"] = new[] { "ROG", "ExpertCenter", "ProArt" },
                ["MSI"] = new[] { "MAG", "Aegis", "Trident" },
                ["Acer"] = new[] { "Aspire", "Veriton", "Predator" }
            },
            ["Моноблок"] = new Dictionary<string, string[]>
            {
                ["Lenovo"] = new[] { "IdeaCentre AIO", "Yoga AIO" },
                ["HP"] = new[] { "All-in-One", "ProOne" },
                ["Apple"] = new[] { "iMac" },
                ["Acer"] = new[] { "Aspire C" },
                ["ASUS"] = new[] { "Zen AiO", "Vivo AiO" }
            },
            ["Монитор"] = new Dictionary<string, string[]>
            {
                ["Samsung"] = new[] { "Odyssey", "ViewFinity", "S24" },
                ["LG"] = new[] { "UltraGear", "UltraWide", "24MK" },
                ["AOC"] = new[] { "Gaming", "Value Line", "Professional" },
                ["Philips"] = new[] { "P-line", "V-line", "Momentum" },
                ["Dell"] = new[] { "P-series", "S-series", "UltraSharp" },
                ["BenQ"] = new[] { "GW", "EX", "PD" }
            },
            ["Принтер"] = new Dictionary<string, string[]>
            {
                ["HP"] = new[] { "LaserJet", "DeskJet", "OfficeJet" },
                ["Canon"] = new[] { "PIXMA", "i-SENSYS", "MAXIFY" },
                ["Epson"] = new[] { "EcoTank", "WorkForce", "L-series" },
                ["Brother"] = new[] { "HL-L", "DCP", "MFC" },
                ["Xerox"] = new[] { "Phaser", "VersaLink", "WorkCentre" }
            }
        };
        private readonly Dictionary<string, string[]> _problemCatalog = new Dictionary<string, string[]>
        {
            ["Ноутбук"] = new[] { "Не включается", "Сильно греется", "Шумит", "Не заряжается", "Разбит экран", "Тормозит" },
            ["Стационарный ПК"] = new[] { "Не включается", "Перезагружается", "Шумит", "Нет изображения", "Тормозит", "Не видит диск" },
            ["Моноблок"] = new[] { "Не включается", "Нет изображения", "Сильно греется", "Тормозит", "Не работает сенсор" },
            ["Монитор"] = new[] { "Нет изображения", "Мерцает экран", "Полосы на экране", "Разбит экран", "Не работает подсветка" },
            ["Принтер"] = new[] { "Не печатает", "Зажевывает бумагу", "Полосы при печати", "Ошибка картриджа", "Не подключается" },
            ["Другое"] = new[] { "Не включается", "Работает нестабильно", "Проблема с экраном", "Проблема с подключением" }
        };
        private User _currentUser;
        private bool _isEditing;
        private string _editName;
        private string _editLogin;
        private string _editEmail;
        private string _editNameValidationMessage;
        private string _editLoginValidationMessage;
        private string _editEmailValidationMessage;
        private bool _isCreateOrderFormVisible;
        private bool _hasAttemptedProfileSave;
        private string _newReviewText;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<Comment> _reviews;
        private string _newOrderDeviceType;
        private string _customDeviceType;
        private string _newOrderDeviceBrand;
        private string _newOrderDeviceModel;
        private string _newOrderProblemDescription;
        private string _newOrderDeliveryMethod;
        private string _newOrderDeliveryAddress;
        private string _newOrderContactPhone;
        private string _newOrderClientComment;
        private string _newOrderPaymentMethod;
        private string _selectedBrandOption;
        private string _customBrand;
        private string _selectedModelOption;
        private string _customModel;
        private string _selectedProblemOption;
        private string _customProblemDescription;
        private byte[] _newOrderProblemPhoto;
        private string _newOrderProblemPhotoName;
        private bool _showOrderValidationErrors;

        private bool AreRepositoriesReady =>
            _orderRepository != null &&
            _commentRepository != null &&
            RepositoryManager.Users != null;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    OnPropertyChanged(nameof(PhotoActionText));
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string EditName
        {
            get => _editName;
            set
            {
                if (SetProperty(ref _editName, value) && IsEditing)
                {
                    ValidateEditName(_hasAttemptedProfileSave);
                }
            }
        }

        public string EditLogin
        {
            get => _editLogin;
            set
            {
                if (SetProperty(ref _editLogin, value) && IsEditing)
                {
                    ValidateEditLogin(_hasAttemptedProfileSave);
                }
            }
        }

        public string EditEmail
        {
            get => _editEmail;
            set
            {
                if (SetProperty(ref _editEmail, value) && IsEditing)
                {
                    ValidateEditEmail(_hasAttemptedProfileSave);
                }
            }
        }

        public string EditNameValidationMessage
        {
            get => _editNameValidationMessage;
            private set => SetValidationMessage(ref _editNameValidationMessage, value, nameof(EditNameValidationMessage), nameof(HasEditNameValidationError));
        }

        public string EditLoginValidationMessage
        {
            get => _editLoginValidationMessage;
            private set => SetValidationMessage(ref _editLoginValidationMessage, value, nameof(EditLoginValidationMessage), nameof(HasEditLoginValidationError));
        }

        public string EditEmailValidationMessage
        {
            get => _editEmailValidationMessage;
            private set => SetValidationMessage(ref _editEmailValidationMessage, value, nameof(EditEmailValidationMessage), nameof(HasEditEmailValidationError));
        }

        public bool HasEditNameValidationError => !string.IsNullOrWhiteSpace(EditNameValidationMessage);
        public bool HasEditLoginValidationError => !string.IsNullOrWhiteSpace(EditLoginValidationMessage);
        public bool HasEditEmailValidationError => !string.IsNullOrWhiteSpace(EditEmailValidationMessage);

        public bool IsCreateOrderFormVisible
        {
            get => _isCreateOrderFormVisible;
            set => SetProperty(ref _isCreateOrderFormVisible, value);
        }

        public string NewReviewText
        {
            get => _newReviewText;
            set => SetProperty(ref _newReviewText, value);
        }

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set
            {
                if (SetProperty(ref _orders, value))
                {
                    OnPropertyChanged(nameof(HasOrders));
                }
            }
        }

        public ObservableCollection<Comment> Reviews
        {
            get => _reviews;
            set
            {
                if (SetProperty(ref _reviews, value))
                {
                    OnPropertyChanged(nameof(HasReviews));
                }
            }
        }

        public bool HasOrders => Orders != null && Orders.Count > 0;
        public bool HasReviews => Reviews != null && Reviews.Count > 0;
        public string PhotoActionText => CurrentUser?.Photo == null
            ? App.GetString("AddPhotoButton", "Add photo")
            : App.GetString("ChangePhotoActionButton", "Change photo");
        public ObservableCollection<string> DeviceTypes { get; } = new ObservableCollection<string>
        {
            "Ноутбук",
            "Стационарный ПК",
            "Моноблок",
            "Монитор",
            "Принтер",
            "Другое"
        };
        public ObservableCollection<string> PaymentMethods { get; } = new ObservableCollection<string>
        {
            Order.OnSitePaymentMethod,
            Order.OnlinePaymentMethod
        };
        public ObservableCollection<string> BrandOptions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ModelOptions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ProblemOptions { get; } = new ObservableCollection<string>();
        public string NewOrderDeviceType
        {
            get => _newOrderDeviceType;
            set
            {
                if (SetProperty(ref _newOrderDeviceType, value))
                {
                    RefreshBrandOptions();
                    RefreshProblemOptions();
                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public string CustomDeviceType
        {
            get => _customDeviceType;
            set
            {
                if (SetProperty(ref _customDeviceType, value))
                {
                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public string NewOrderDeviceBrand
        {
            get => _newOrderDeviceBrand;
            set
            {
                if (SetProperty(ref _newOrderDeviceBrand, value))
                {
                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public string NewOrderDeviceModel
        {
            get => _newOrderDeviceModel;
            set
            {
                if (SetProperty(ref _newOrderDeviceModel, value))
                {
                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public string NewOrderProblemDescription
        {
            get => _newOrderProblemDescription;
            set
            {
                if (SetProperty(ref _newOrderProblemDescription, value))
                {
                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public string NewOrderDeliveryMethod
        {
            get => _newOrderDeliveryMethod;
            set
            {
                if (SetProperty(ref _newOrderDeliveryMethod, value))
                {
                    OnPropertyChanged(nameof(IsCourierDeliverySelected));
                    if (!IsCourierDeliverySelected)
                    {
                        NewOrderDeliveryAddress = string.Empty;
                    }

                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public string NewOrderDeliveryAddress
        {
            get => _newOrderDeliveryAddress;
            set
            {
                if (SetProperty(ref _newOrderDeliveryAddress, value))
                {
                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public bool IsCourierDeliverySelected => NewOrderDeliveryMethod == "Курьер";

        public string NewOrderContactPhone
        {
            get => _newOrderContactPhone;
            set
            {
                if (SetProperty(ref _newOrderContactPhone, value))
                {
                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public string NewOrderClientComment
        {
            get => _newOrderClientComment;
            set => SetProperty(ref _newOrderClientComment, value);
        }

        public string NewOrderPaymentMethod
        {
            get => _newOrderPaymentMethod;
            set
            {
                if (SetProperty(ref _newOrderPaymentMethod, value))
                {
                    OnPropertyChanged(nameof(NewOrderIsOnlinePayment));
                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public bool NewOrderIsOnlinePayment
        {
            get => string.Equals(NewOrderPaymentMethod, Order.OnlinePaymentMethod, System.StringComparison.Ordinal);
            set => NewOrderPaymentMethod = value ? Order.OnlinePaymentMethod : Order.OnSitePaymentMethod;
        }

        public string SelectedBrandOption
        {
            get => _selectedBrandOption;
            set
            {
                if (SetProperty(ref _selectedBrandOption, value))
                {
                    OnPropertyChanged(nameof(IsCustomBrandVisible));
                    if (!IsCustomBrandVisible)
                    {
                        CustomBrand = string.Empty;
                    }

                    SyncBrandValue();
                    RefreshModelOptions();
                }
            }
        }

        public string CustomBrand
        {
            get => _customBrand;
            set
            {
                if (SetProperty(ref _customBrand, value))
                {
                    SyncBrandValue();
                }
            }
        }

        public string SelectedModelOption
        {
            get => _selectedModelOption;
            set
            {
                if (SetProperty(ref _selectedModelOption, value))
                {
                    OnPropertyChanged(nameof(IsCustomModelVisible));
                    if (!IsCustomModelVisible)
                    {
                        CustomModel = string.Empty;
                    }

                    SyncModelValue();
                }
            }
        }

        public string CustomModel
        {
            get => _customModel;
            set
            {
                if (SetProperty(ref _customModel, value))
                {
                    SyncModelValue();
                }
            }
        }

        public string SelectedProblemOption
        {
            get => _selectedProblemOption;
            set
            {
                if (SetProperty(ref _selectedProblemOption, value))
                {
                    OnPropertyChanged(nameof(IsCustomProblemVisible));
                    if (!IsCustomProblemVisible)
                    {
                        CustomProblemDescription = string.Empty;
                    }

                    SyncProblemValue();
                }
            }
        }

        public string CustomProblemDescription
        {
            get => _customProblemDescription;
            set
            {
                if (SetProperty(ref _customProblemDescription, value))
                {
                    SyncProblemValue();
                }
            }
        }

        public bool IsCustomBrandVisible => SelectedBrandOption == OtherOption;
        public bool IsCustomModelVisible => SelectedModelOption == OtherOption;
        public bool IsCustomProblemVisible => SelectedProblemOption == OtherOption;
        public bool IsCustomDeviceTypeVisible => NewOrderDeviceType == OtherOption;

        public byte[] NewOrderProblemPhoto
        {
            get => _newOrderProblemPhoto;
            set
            {
                if (SetProperty(ref _newOrderProblemPhoto, value))
                {
                    OnPropertyChanged(nameof(HasSelectedProblemPhoto));
                    OnPropertyChanged(nameof(NewOrderProblemPhotoStatus));
                }
            }
        }

        public string NewOrderProblemPhotoName
        {
            get => _newOrderProblemPhotoName;
            set
            {
                if (SetProperty(ref _newOrderProblemPhotoName, value))
                {
                    OnPropertyChanged(nameof(NewOrderProblemPhotoStatus));
                }
            }
        }

        public bool HasSelectedProblemPhoto => NewOrderProblemPhoto != null && NewOrderProblemPhoto.Length > 0;
        public string NewOrderProblemPhotoStatus => HasSelectedProblemPhoto
            ? string.Format(App.GetString("ProblemPhotoSelectedFormat", "Selected photo: {0}"), NewOrderProblemPhotoName)
            : App.GetString("ProblemPhotoNotAdded", "No photo added");
        public bool ShowOrderValidationErrors
        {
            get => _showOrderValidationErrors;
            private set
            {
                if (SetProperty(ref _showOrderValidationErrors, value))
                {
                    NotifyOrderValidationStateChanged();
                }
            }
        }

        public bool IsNewOrderDeviceTypeInvalid => ShowOrderValidationErrors && IsOrderFieldInvalid(NewOrderDeviceType, IsCustomDeviceTypeVisible);
        public bool IsNewOrderDeviceBrandInvalid => ShowOrderValidationErrors && IsOrderFieldInvalid(NewOrderDeviceBrand, IsCustomBrandVisible);
        public bool IsNewOrderDeviceModelInvalid => ShowOrderValidationErrors && IsOrderFieldInvalid(NewOrderDeviceModel, IsCustomModelVisible);
        public bool IsNewOrderProblemDescriptionInvalid => ShowOrderValidationErrors && IsOrderFieldInvalid(NewOrderProblemDescription, IsCustomProblemVisible);
        public bool IsNewOrderContactPhoneInvalid => ShowOrderValidationErrors && !IsValidPhoneNumber(NewOrderContactPhone);
        public bool HasOrderValidationErrors =>
            IsNewOrderDeviceTypeInvalid ||
            IsNewOrderDeviceBrandInvalid ||
            IsNewOrderDeviceModelInvalid ||
            IsNewOrderProblemDescriptionInvalid ||
            IsNewOrderContactPhoneInvalid;

        public ICommand ToggleEditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ChangePhotoCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand AddReviewCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleCreateOrderFormCommand { get; }
        public ICommand CancelCreateOrderFormCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand SelectProblemPhotoCommand { get; }
        public ICommand RemoveProblemPhotoCommand { get; }
        public ICommand ToggleOrderDetailsCommand { get; }
        public ICommand PayOrderOnlineCommand { get; }
        public ICommand CancelOrderCommand { get; }

        public ServiceClientProfileViewModel()
        {
            _userRepository = RepositoryManager.Users;
            _orderRepository = RepositoryManager.Orders;
            _commentRepository = RepositoryManager.Comments;
            CurrentUser = SessionManager.CurrentUser ?? new User { Name = "Guest", Login = "guest", Role = UserRole.Client };
            Orders = new ObservableCollection<Order>();
            Reviews = new ObservableCollection<Comment>();
            ResetOrderForm();

            ToggleEditCommand = new RelayCommand(ToggleEdit);
            SaveCommand = new RelayCommand(SaveProfile);
            CancelEditCommand = new RelayCommand(CancelEdit);
            ChangePhotoCommand = new RelayCommand(ChangePhoto);
            LogoutCommand = new RelayCommand(Logout);
            AddReviewCommand = new RelayCommand(AddReview);
            RefreshCommand = new RelayCommand(LoadData);
            ToggleCreateOrderFormCommand = new RelayCommand(ToggleCreateOrderForm);
            CancelCreateOrderFormCommand = new RelayCommand(CancelCreateOrderForm);
            CreateOrderCommand = new RelayCommand(CreateOrder);
            SelectProblemPhotoCommand = new RelayCommand(SelectProblemPhoto);
            RemoveProblemPhotoCommand = new RelayCommand(RemoveProblemPhoto);
            ToggleOrderDetailsCommand = new RelayCommandSec(ToggleOrderDetails);
            PayOrderOnlineCommand = new RelayCommandSec(PayOrderOnline);
            CancelOrderCommand = new RelayCommandSec(CancelOrder);
            App.LanguageChanged += OnLanguageChanged;

            if (AreRepositoriesReady)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            if (!AreRepositoriesReady)
            {
                Reviews = new ObservableCollection<Comment>();
                Orders = new ObservableCollection<Order>();
                return;
            }

            try
            {
                var currentSessionUser = SessionManager.CurrentUser;
                if (currentSessionUser != null)
                {
                    CurrentUser = currentSessionUser;
                }

                Reviews = new ObservableCollection<Comment>(_commentRepository.GetPublicReviews());

                if (SessionManager.IsAuthenticated && currentSessionUser != null)
                {
                    ReloadOrdersForCurrentUser();
                }
                else
                {
                    Orders = new ObservableCollection<Order>();
                }
            }
            catch (System.Exception ex)
            {
                Reviews = new ObservableCollection<Comment>();
                Orders = new ObservableCollection<Order>();
                MessageBox.Show($"Не удалось загрузить профиль клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleEdit()
        {
            if (IsEditing)
            {
                CancelEdit();
                return;
            }

            EditName = CurrentUser?.Name ?? string.Empty;
            EditLogin = CurrentUser?.Login ?? string.Empty;
            EditEmail = CurrentUser?.Email ?? string.Empty;
            ClearProfileValidation();
            _hasAttemptedProfileSave = false;
            IsEditing = true;
        }

        private void ToggleCreateOrderForm()
        {
            IsCreateOrderFormVisible = !IsCreateOrderFormVisible;

            if (IsCreateOrderFormVisible && string.IsNullOrWhiteSpace(NewOrderDeviceType))
            {
                ResetOrderForm();
            }
        }

        private void CancelCreateOrderForm()
        {
            ResetOrderForm();
            IsCreateOrderFormVisible = false;
        }

        private void SaveProfile()
        {
            _hasAttemptedProfileSave = true;
            if (!ValidateProfileEditor(true))
            {
                MessageBox.Show("Исправьте ошибки в полях профиля перед сохранением.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CurrentUser.Name = EditName.Trim();
            CurrentUser.Login = EditLogin.Trim();
            CurrentUser.Email = NormalizeEmail(EditEmail);

            _userRepository.Update(CurrentUser);
            SessionManager.Login(CurrentUser);
            OnPropertyChanged(nameof(CurrentUser));
            ClearProfileValidation();
            _hasAttemptedProfileSave = false;
            IsEditing = false;
            MessageBox.Show("Профиль успешно обновлен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelEdit()
        {
            EditName = CurrentUser?.Name ?? string.Empty;
            EditLogin = CurrentUser?.Login ?? string.Empty;
            EditEmail = CurrentUser?.Email ?? string.Empty;
            ClearProfileValidation();
            _hasAttemptedProfileSave = false;
            IsEditing = false;
        }

        private void ChangePhoto()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CurrentUser.Photo = File.ReadAllBytes(openFileDialog.FileName);
                RepositoryManager.Users.Update(CurrentUser);
                SessionManager.Login(CurrentUser);
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(PhotoActionText));
                MessageBox.Show("Фото профиля обновлено.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SelectProblemPhoto()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                NewOrderProblemPhoto = File.ReadAllBytes(openFileDialog.FileName);
                NewOrderProblemPhotoName = Path.GetFileName(openFileDialog.FileName);
            }
        }

        private void RemoveProblemPhoto()
        {
            NewOrderProblemPhoto = null;
            NewOrderProblemPhotoName = string.Empty;
        }

        private void Logout()
        {
            SessionManager.Logout();
            var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
            if (mainViewModel != null)
            {
                mainViewModel.CurrentPage = new LoginPage();
            }
        }

        private void CreateOrder()
        {
            if (!SessionManager.IsAuthenticated)
            {
                MessageBox.Show("Чтобы создать заявку, нужно войти в систему.", "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentUser?.Email) || !IsValidEmail(CurrentUser.Email))
            {
                MessageBox.Show("Укажите корректную электронную почту в профиле, чтобы получать уведомления о статусе заявки.", "Нужна электронная почта", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resolvedDeviceType = ResolveOptionValue(NewOrderDeviceType, CustomDeviceType);
            var resolvedBrand = ResolveOptionValue(SelectedBrandOption, CustomBrand);
            var resolvedModel = ResolveOptionValue(SelectedModelOption, CustomModel);
            var resolvedProblem = ResolveOptionValue(SelectedProblemOption, CustomProblemDescription);
            ShowOrderValidationErrors = true;
            NotifyOrderValidationStateChanged();

            if (HasOrderValidationErrors)
            {
                MessageBox.Show(
                    BuildOrderValidationMessage(),
                    "Ошибка проверки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var order = new Order
            {
                UserId = SessionManager.CurrentUser.Id,
                User = SessionManager.CurrentUser,
                DeviceType = resolvedDeviceType,
                DeviceBrand = resolvedBrand,
                DeviceModel = resolvedModel,
                ProblemDescription = resolvedProblem,
                DeliveryMethod = "Самовывоз",
                DeliveryAddress = null,
                ProblemPhoto = NewOrderProblemPhoto,
                ContactPhone = NewOrderContactPhone.Trim(),
                ClientComment = string.IsNullOrWhiteSpace(NewOrderClientComment) ? null : NewOrderClientComment.Trim(),
                PaymentMethod = NewOrderPaymentMethod,
                Status = OrderStatus.Created,
                CreatedAt = System.DateTime.Now
            };

            AssignBestMaster(order);
            _orderRepository.Add(order);
            ReloadOrdersForCurrentUser();
            ShowOrderValidationErrors = false;
            CancelCreateOrderForm();
            MessageBox.Show("Заявка успешно создана.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddReview()
        {
            if (!SessionManager.IsAuthenticated)
            {
                MessageBox.Show("Оставлять отзывы могут только авторизованные пользователи.", "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewReviewText))
            {
                MessageBox.Show("Текст отзыва не должен быть пустым.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var review = new Comment
            {
                UserId = SessionManager.CurrentUser.Id,
                User = SessionManager.CurrentUser,
                Text = NewReviewText.Trim(),
                Timestamp = System.DateTime.Now
            };

            _commentRepository.Add(review);
            Reviews.Insert(0, review);
            NewReviewText = string.Empty;
            OnPropertyChanged(nameof(HasReviews));
        }

        private void AssignBestMaster(Order order)
        {
            var master = MasterAssignmentService.FindBestMaster(
                order.DeviceType,
                RepositoryManager.Users.GetByRole(UserRole.Master),
                _orderRepository.GetAll());

            if (master == null)
            {
                order.Status = OrderStatus.Created;
                return;
            }

            order.AssignedMasterId = master.Id;
            order.AssignedMaster = master;
            order.Status = OrderStatus.Assigned;
        }

        private void ResetOrderForm()
        {
            ShowOrderValidationErrors = false;
            NewOrderDeliveryMethod = string.Empty;
            NewOrderDeliveryAddress = string.Empty;
            NewOrderContactPhone = string.Empty;
            NewOrderClientComment = string.Empty;
            NewOrderPaymentMethod = PaymentMethods[0];
            RemoveProblemPhoto();

            NewOrderDeviceType = DeviceTypes[0];
            CustomDeviceType = string.Empty;
            SelectedBrandOption = null;
            CustomBrand = string.Empty;
            SelectedModelOption = null;
            CustomModel = string.Empty;
            SelectedProblemOption = null;
            CustomProblemDescription = string.Empty;

            SyncBrandValue();
            SyncModelValue();
            SyncProblemValue();
        }

        private void NotifyOrderValidationStateChanged()
        {
            OnPropertyChanged(nameof(IsCustomDeviceTypeVisible));
            OnPropertyChanged(nameof(IsNewOrderDeviceTypeInvalid));
            OnPropertyChanged(nameof(IsNewOrderDeviceBrandInvalid));
            OnPropertyChanged(nameof(IsNewOrderDeviceModelInvalid));
            OnPropertyChanged(nameof(IsNewOrderProblemDescriptionInvalid));
            OnPropertyChanged(nameof(IsNewOrderContactPhoneInvalid));
            OnPropertyChanged(nameof(HasOrderValidationErrors));
        }

        private void ToggleOrderDetails(object parameter)
        {
            if (parameter is Order order)
            {
                order.IsDetailsExpanded = !order.IsDetailsExpanded;
            }
        }

        private void PayOrderOnline(object parameter)
        {
            var order = parameter as Order;
            if (order == null)
            {
                return;
            }

            if (!order.IsOnlinePayment || order.IsOnlinePaymentCompleted)
            {
                return;
            }

            if (order.EstimatedRepairCost <= 0)
            {
                MessageBox.Show(
                    "Онлайн-оплата станет доступна после того, как по заявке будет рассчитана предварительная сумма.",
                    "Сумма еще не рассчитана",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var paymentWindow = new OnlinePaymentWindow(order);
            if (Application.Current.MainWindow != null)
            {
                paymentWindow.Owner = Application.Current.MainWindow;
            }

            if (paymentWindow.ShowDialog() != true)
            {
                return;
            }

            order.IsOnlinePaymentCompleted = true;
            order.OnlinePaymentPaidAt = System.DateTime.Now;
            order.Status = OrderStatus.Completed;
            order.CompletedAt = System.DateTime.Now;
            _orderRepository.Update(order);
            ReloadOrdersForCurrentUser();

            MessageBox.Show(
                "Онлайн-оплата успешно проведена. Заявка закрыта и убрана из вашего списка.",
                "Оплата выполнена",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CancelOrder(object parameter)
        {
            var order = parameter as Order;
            if (order == null)
            {
                return;
            }

            if (order.IsOnlinePaymentCompleted)
            {
                MessageBox.Show(
                    "Оплаченную заявку нельзя отменить из профиля. Если нужна помощь, обратитесь к администратору.",
                    "Отмена недоступна",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (!order.CanBeCancelledByClient)
            {
                MessageBox.Show(
                    "Эту заявку уже нельзя отменить из профиля. Обратитесь к администратору.",
                    "Отмена недоступна",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var confirmation = MessageBox.Show(
                $"Отменить заявку {order.DisplayNumber}?",
                "Подтверждение отмены",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            order.Status = OrderStatus.Cancelled;
            _orderRepository.Update(order);
            ReloadOrdersForCurrentUser();

            MessageBox.Show(
                "Заявка отменена.",
                "Готово",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void RefreshBrandOptions()
        {
            ReplaceOptions(BrandOptions, GetOptionsForDevice(_brandCatalog, NewOrderDeviceType));
            SelectedBrandOption = null;
            CustomBrand = string.Empty;
            RefreshModelOptions();
        }

        private void RefreshModelOptions()
        {
            ReplaceOptions(ModelOptions, GetModelOptions());
            SelectedModelOption = null;
            CustomModel = string.Empty;
            SyncModelValue();
        }

        private void RefreshProblemOptions()
        {
            ReplaceOptions(ProblemOptions, GetOptionsForDevice(_problemCatalog, NewOrderDeviceType));
            SelectedProblemOption = null;
            CustomProblemDescription = string.Empty;
            SyncProblemValue();
        }

        private void ReplaceOptions(ObservableCollection<string> collection, IEnumerable<string> values)
        {
            collection.Clear();
            foreach (var value in values)
            {
                collection.Add(value);
            }

            collection.Add(OtherOption);
        }

        private void ReloadOrdersForCurrentUser()
        {
            if (!SessionManager.IsAuthenticated || SessionManager.CurrentUser == null)
            {
                Orders = new ObservableCollection<Order>();
                return;
            }

            Orders = new ObservableCollection<Order>(
                _orderRepository
                    .GetByUserId(SessionManager.CurrentUser.Id)
                    .Where(order => order.Status != OrderStatus.Completed));
        }

        private void OnLanguageChanged(object sender, System.EventArgs e)
        {
            OnPropertyChanged(nameof(PhotoActionText));
            OnPropertyChanged(nameof(NewOrderProblemPhotoStatus));
            if (SessionManager.IsAuthenticated)
            {
                ReloadOrdersForCurrentUser();
            }
        }

        private IEnumerable<string> GetOptionsForDevice(Dictionary<string, string[]> catalog, string deviceType)
        {
            if (!string.IsNullOrWhiteSpace(deviceType) && catalog.TryGetValue(deviceType, out var values))
            {
                return values;
            }

            return new string[0];
        }

        private IEnumerable<string> GetModelOptions()
        {
            if (!string.IsNullOrWhiteSpace(NewOrderDeviceType) &&
                !string.IsNullOrWhiteSpace(SelectedBrandOption) &&
                SelectedBrandOption != OtherOption &&
                _brandModelCatalog.TryGetValue(NewOrderDeviceType, out var brandModels) &&
                brandModels.TryGetValue(SelectedBrandOption, out var specificModels))
            {
                return specificModels;
            }

            return GetOptionsForDevice(_defaultModelCatalog, NewOrderDeviceType);
        }

        private void SyncBrandValue()
        {
            NewOrderDeviceBrand = ResolveOptionValue(SelectedBrandOption, CustomBrand);
        }

        private void SyncModelValue()
        {
            NewOrderDeviceModel = ResolveOptionValue(SelectedModelOption, CustomModel);
        }

        private void SyncProblemValue()
        {
            NewOrderProblemDescription = ResolveOptionValue(SelectedProblemOption, CustomProblemDescription);
        }

        private string ResolveOptionValue(string selectedValue, string customValue)
        {
            if (selectedValue == OtherOption)
            {
                return string.IsNullOrWhiteSpace(customValue) ? string.Empty : customValue.Trim();
            }

            return string.IsNullOrWhiteSpace(selectedValue) ? string.Empty : selectedValue.Trim();
        }

        private static bool IsOrderFieldInvalid(string value, bool validateCustomEntry)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            return validateCustomEntry && !IsValidCustomOrderText(value);
        }

        private bool HasMissingOrderRequiredFields()
        {
            return string.IsNullOrWhiteSpace(NewOrderDeviceType) ||
                   string.IsNullOrWhiteSpace(NewOrderDeviceBrand) ||
                   string.IsNullOrWhiteSpace(NewOrderDeviceModel) ||
                   string.IsNullOrWhiteSpace(NewOrderProblemDescription) ||
                   string.IsNullOrWhiteSpace(NewOrderContactPhone);
        }

        private bool HasInvalidOrderData()
        {
            return HasInvalidCustomOrderField(NewOrderDeviceType, IsCustomDeviceTypeVisible) ||
                   HasInvalidCustomOrderField(NewOrderDeviceBrand, IsCustomBrandVisible) ||
                   HasInvalidCustomOrderField(NewOrderDeviceModel, IsCustomModelVisible) ||
                   HasInvalidCustomOrderField(NewOrderProblemDescription, IsCustomProblemVisible) ||
                   HasFilledButInvalidPhoneNumber(NewOrderContactPhone);
        }

        private string BuildOrderValidationMessage()
        {
            var messages = new List<string>();

            if (HasMissingOrderRequiredFields())
            {
                messages.Add("Заполните обязательные поля заявки.");
            }

            if (HasInvalidOrderData())
            {
                var invalidFields = new List<string>();
                if (HasFilledButInvalidPhoneNumber(NewOrderContactPhone))
                {
                    invalidFields.Add("номер телефона");
                }

                if (HasInvalidCustomOrderField(NewOrderDeviceType, IsCustomDeviceTypeVisible) ||
                    HasInvalidCustomOrderField(NewOrderDeviceBrand, IsCustomBrandVisible) ||
                    HasInvalidCustomOrderField(NewOrderDeviceModel, IsCustomModelVisible) ||
                    HasInvalidCustomOrderField(NewOrderProblemDescription, IsCustomProblemVisible))
                {
                    invalidFields.Add("значения, введённые для варианта \"Другое\"");
                }

                messages.Add($"Проверьте корректность введённых данных: {string.Join(" и ", invalidFields)}.");
            }

            return messages.Count == 0
                ? "Проверьте корректность заполнения заявки."
                : string.Join(" ", messages);
        }

        private bool ValidateProfileEditor(bool showRequired)
        {
            ValidateEditName(showRequired);
            ValidateEditLogin(showRequired);
            ValidateEditEmail(showRequired);
            return !HasEditNameValidationError &&
                   !HasEditLoginValidationError &&
                   !HasEditEmailValidationError;
        }

        private void ValidateEditName(bool showRequired)
        {
            var normalizedName = EditName?.Trim();
            EditNameValidationMessage = string.IsNullOrWhiteSpace(normalizedName)
                ? (showRequired ? "Введите имя." : string.Empty)
                : string.Empty;
        }

        private void ValidateEditLogin(bool showRequired)
        {
            var normalizedLogin = EditLogin?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedLogin))
            {
                EditLoginValidationMessage = showRequired ? "Введите логин." : string.Empty;
                return;
            }

            if (_userRepository == null || CurrentUser == null)
            {
                EditLoginValidationMessage = string.Empty;
                return;
            }

            var existingUser = _userRepository
                .GetAll()
                .FirstOrDefault(user =>
                    user.Id != CurrentUser.Id &&
                    string.Equals(user.Login, normalizedLogin, System.StringComparison.OrdinalIgnoreCase));

            EditLoginValidationMessage = existingUser == null
                ? string.Empty
                : "Этот логин уже занят. Поменяйте логин.";
        }

        private void ValidateEditEmail(bool showRequired)
        {
            var normalizedEmail = NormalizeEmail(EditEmail);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                EditEmailValidationMessage = showRequired ? "Введите электронную почту." : string.Empty;
                return;
            }

            if (!IsValidEmail(normalizedEmail))
            {
                EditEmailValidationMessage = "Укажите корректный email.";
                return;
            }

            if (_userRepository == null || CurrentUser == null)
            {
                EditEmailValidationMessage = string.Empty;
                return;
            }

            var existingUser = _userRepository
                .GetAll()
                .FirstOrDefault(user =>
                    user.Id != CurrentUser.Id &&
                    string.Equals(user.Email, normalizedEmail, System.StringComparison.OrdinalIgnoreCase));

            EditEmailValidationMessage = existingUser == null
                ? string.Empty
                : "Эта электронная почта уже зарегистрирована.";
        }

        private void ClearProfileValidation()
        {
            EditNameValidationMessage = string.Empty;
            EditLoginValidationMessage = string.Empty;
            EditEmailValidationMessage = string.Empty;
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
                return string.Equals(parsedEmail.Address, email, System.StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            var normalizedPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return Regex.IsMatch(normalizedPhone, @"^(?:375|80)(17|25|29|33|44)\d{7}$");
        }

        private static bool HasFilledButInvalidPhoneNumber(string phoneNumber)
        {
            return !string.IsNullOrWhiteSpace(phoneNumber) && !IsValidPhoneNumber(phoneNumber);
        }

        private static bool HasInvalidCustomOrderField(string value, bool validateCustomEntry)
        {
            return validateCustomEntry &&
                   !string.IsNullOrWhiteSpace(value) &&
                   !IsValidCustomOrderText(value);
        }

        private static bool IsValidCustomOrderText(string value)
        {
            var normalizedValue = value?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                return false;
            }

            var hasLetter = false;
            foreach (var character in normalizedValue)
            {
                if (char.IsLetter(character))
                {
                    hasLetter = true;
                    continue;
                }

                if (char.IsDigit(character) || char.IsWhiteSpace(character) || IsAllowedCustomOrderSymbol(character))
                {
                    continue;
                }

                return false;
            }

            return hasLetter;
        }

        private static bool IsAllowedCustomOrderSymbol(char character)
        {
            const string allowedSymbols = "-_.,:;!?()[]{}\\/+#№\"'&%";
            return allowedSymbols.IndexOf(character) >= 0;
        }

        private void SetValidationMessage(ref string field, string value, string propertyName, string flagPropertyName)
        {
            if (SetProperty(ref field, value, propertyName))
            {
                OnPropertyChanged(flagPropertyName);
            }
        }
    }
}
