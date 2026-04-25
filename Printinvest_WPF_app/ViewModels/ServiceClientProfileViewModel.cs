using Microsoft.Win32;
using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class ServiceClientProfileViewModel : BaseViewModel
    {
        private const string OtherOption = "Другое";

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
        private bool _isCreateOrderFormVisible;
        private string _newReviewText;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<Comment> _reviews;
        private string _newOrderDeviceType;
        private string _newOrderDeviceBrand;
        private string _newOrderDeviceModel;
        private string _newOrderProblemDescription;
        private string _newOrderDeliveryMethod;
        private string _newOrderDeliveryAddress;
        private string _newOrderContactPhone;
        private string _newOrderClientComment;
        private string _selectedBrandOption;
        private string _customBrand;
        private string _selectedModelOption;
        private string _customModel;
        private string _selectedProblemOption;
        private string _customProblemDescription;
        private byte[] _newOrderProblemPhoto;
        private string _newOrderProblemPhotoName;

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
        public string PhotoActionText => CurrentUser?.Photo == null ? "Добавить фото" : "Изменить фото";
        public ObservableCollection<string> DeviceTypes { get; } = new ObservableCollection<string>
        {
            "Ноутбук",
            "Стационарный ПК",
            "Моноблок",
            "Монитор",
            "Принтер",
            "Другое"
        };
        public ObservableCollection<string> BrandOptions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ModelOptions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ProblemOptions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> DeliveryMethods { get; } = new ObservableCollection<string>
        {
            "Самовывоз",
            "Курьер"
        };

        public string NewOrderDeviceType
        {
            get => _newOrderDeviceType;
            set
            {
                if (SetProperty(ref _newOrderDeviceType, value))
                {
                    RefreshBrandOptions();
                    RefreshProblemOptions();
                }
            }
        }

        public string NewOrderDeviceBrand
        {
            get => _newOrderDeviceBrand;
            set => SetProperty(ref _newOrderDeviceBrand, value);
        }

        public string NewOrderDeviceModel
        {
            get => _newOrderDeviceModel;
            set => SetProperty(ref _newOrderDeviceModel, value);
        }

        public string NewOrderProblemDescription
        {
            get => _newOrderProblemDescription;
            set => SetProperty(ref _newOrderProblemDescription, value);
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
                }
            }
        }

        public string NewOrderDeliveryAddress
        {
            get => _newOrderDeliveryAddress;
            set => SetProperty(ref _newOrderDeliveryAddress, value);
        }

        public bool IsCourierDeliverySelected => NewOrderDeliveryMethod == "Курьер";

        public string NewOrderContactPhone
        {
            get => _newOrderContactPhone;
            set => SetProperty(ref _newOrderContactPhone, value);
        }

        public string NewOrderClientComment
        {
            get => _newOrderClientComment;
            set => SetProperty(ref _newOrderClientComment, value);
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
        public string NewOrderProblemPhotoStatus => HasSelectedProblemPhoto ? $"Фото выбрано: {NewOrderProblemPhotoName}" : "Фото не добавлено";

        public ICommand ToggleEditCommand { get; }
        public ICommand SaveCommand { get; }
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

        public ServiceClientProfileViewModel()
        {
            _orderRepository = RepositoryManager.Orders;
            _commentRepository = RepositoryManager.Comments;
            CurrentUser = SessionManager.CurrentUser ?? new User { Name = "Guest", Login = "guest", Role = UserRole.Client };
            Orders = new ObservableCollection<Order>();
            Reviews = new ObservableCollection<Comment>();
            ResetOrderForm();

            ToggleEditCommand = new RelayCommand(ToggleEdit);
            SaveCommand = new RelayCommand(SaveProfile);
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

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Reviews = new ObservableCollection<Comment>(_commentRepository.GetPublicReviews());

                if (SessionManager.IsAuthenticated)
                {
                    Orders = new ObservableCollection<Order>(_orderRepository.GetByUserId(SessionManager.CurrentUser.Id));
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
            IsEditing = !IsEditing;
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
            CurrentUser.Name = CurrentUser.Name?.Trim();
            CurrentUser.Login = CurrentUser.Login?.Trim();
            CurrentUser.Email = NormalizeEmail(CurrentUser.Email);

            if (string.IsNullOrWhiteSpace(CurrentUser.Name) ||
                string.IsNullOrWhiteSpace(CurrentUser.Login) ||
                string.IsNullOrWhiteSpace(CurrentUser.Email))
            {
                MessageBox.Show("Имя, логин и электронная почта обязательны для заполнения.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(CurrentUser.Email))
            {
                MessageBox.Show("Укажите корректный адрес электронной почты.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingUser = RepositoryManager.Users.GetByLogin(CurrentUser.Login);
            if (existingUser != null && existingUser.Id != CurrentUser.Id)
            {
                MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingEmailUser = RepositoryManager.Users
                .GetAll()
                .FirstOrDefault(user => string.Equals(user.Email, CurrentUser.Email, System.StringComparison.OrdinalIgnoreCase));
            if (existingEmailUser != null && existingEmailUser.Id != CurrentUser.Id)
            {
                MessageBox.Show("Пользователь с такой электронной почтой уже существует.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RepositoryManager.Users.Update(CurrentUser);
            SessionManager.Login(CurrentUser);
            IsEditing = false;
            MessageBox.Show("Профиль успешно обновлен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
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

            var resolvedBrand = ResolveOptionValue(SelectedBrandOption, CustomBrand);
            var resolvedModel = ResolveOptionValue(SelectedModelOption, CustomModel);
            var resolvedProblem = ResolveOptionValue(SelectedProblemOption, CustomProblemDescription);

            if (string.IsNullOrWhiteSpace(NewOrderDeviceType) ||
                string.IsNullOrWhiteSpace(resolvedBrand) ||
                string.IsNullOrWhiteSpace(resolvedModel) ||
                string.IsNullOrWhiteSpace(resolvedProblem) ||
                string.IsNullOrWhiteSpace(NewOrderDeliveryMethod) ||
                string.IsNullOrWhiteSpace(NewOrderContactPhone))
            {
                MessageBox.Show("Заполните тип устройства, бренд, модель, способ передачи техники, телефон и неисправность.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsCourierDeliverySelected && string.IsNullOrWhiteSpace(NewOrderDeliveryAddress))
            {
                MessageBox.Show("Укажите адрес для курьера.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var order = new Order
            {
                UserId = SessionManager.CurrentUser.Id,
                User = SessionManager.CurrentUser,
                DeviceType = NewOrderDeviceType,
                DeviceBrand = resolvedBrand,
                DeviceModel = resolvedModel,
                ProblemDescription = resolvedProblem,
                DeliveryMethod = NewOrderDeliveryMethod,
                DeliveryAddress = IsCourierDeliverySelected ? NewOrderDeliveryAddress.Trim() : null,
                ProblemPhoto = NewOrderProblemPhoto,
                ContactPhone = NewOrderContactPhone.Trim(),
                ClientComment = string.IsNullOrWhiteSpace(NewOrderClientComment) ? null : NewOrderClientComment.Trim(),
                Status = OrderStatus.Created,
                CreatedAt = System.DateTime.Now
            };

            AssignBestMaster(order);
            _orderRepository.Add(order);
            Orders = new ObservableCollection<Order>(_orderRepository.GetByUserId(SessionManager.CurrentUser.Id));
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
            NewOrderDeliveryMethod = DeliveryMethods[0];
            NewOrderDeliveryAddress = string.Empty;
            NewOrderContactPhone = string.Empty;
            NewOrderClientComment = string.Empty;
            RemoveProblemPhoto();

            NewOrderDeviceType = DeviceTypes[0];
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

        private void ToggleOrderDetails(object parameter)
        {
            if (parameter is Order order)
            {
                order.IsDetailsExpanded = !order.IsDetailsExpanded;
            }
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
    }
}
