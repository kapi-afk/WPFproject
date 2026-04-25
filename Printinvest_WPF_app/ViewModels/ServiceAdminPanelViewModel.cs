using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views;
using Printinvest_WPF_app.Views.Pages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class ServiceAdminPanelViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly OrderRepository _orderRepository;
        private readonly WarehouseRepository _warehouseRepository;
        private readonly WarehouseRequestRepository _warehouseRequestRepository;
        private ObservableCollection<User> _users;
        private ObservableCollection<User> _masters;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<WarehouseItem> _warehouseItems;
        private ObservableCollection<WarehouseRequest> _warehouseRequests;
        private User _selectedUser;
        private UserRole _selectedUserRole;
        private Order _selectedOrder;
        private User _selectedMaster;
        private WarehouseItem _selectedWarehouseItem;
        private WarehouseRequest _selectedWarehouseRequest;
        private OrderStatus _selectedOrderStatus;
        private string _newUserLogin;
        private string _newUserPassword;
        private string _newUserName;
        private UserRole _newUserRole;
        private bool _newUserSpecializesLaptops;
        private bool _newUserSpecializesComputers;
        private bool _newUserSpecializesOfficeEquipment;
        private bool _selectedUserSpecializesLaptops;
        private bool _selectedUserSpecializesComputers;
        private bool _selectedUserSpecializesOfficeEquipment;
        private string _newOrderClientName;
        private string _newOrderClientEmail;
        private string _newOrderContactPhone;
        private string _newOrderDeviceType;
        private string _newOrderDeviceBrand;
        private string _newOrderDeviceModel;
        private string _newOrderProblemDescription;
        private string _newOrderDeliveryMethod;
        private string _newOrderDeliveryAddress;
        private string _newOrderClientComment;
        private string _warehouseName;
        private string _warehouseCategory;
        private int _warehouseQuantity;
        private decimal _warehouseUnitPrice;
        private bool _isUserEditPanelVisible;
        private bool _isWarehouseFormVisible;
        private int _totalOrdersCount;
        private int _activeOrdersCount;
        private int _completedOrdersCount;
        private int _mastersCount;
        private int _clientsCount;

        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public ObservableCollection<User> Masters
        {
            get => _masters;
            set => SetProperty(ref _masters, value);
        }

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public ObservableCollection<WarehouseItem> WarehouseItems
        {
            get => _warehouseItems;
            set => SetProperty(ref _warehouseItems, value);
        }

        public ObservableCollection<WarehouseRequest> WarehouseRequests
        {
            get => _warehouseRequests;
            set => SetProperty(ref _warehouseRequests, value);
        }

        public ObservableCollection<UserRole> AvailableRoles { get; } = new ObservableCollection<UserRole>
        {
            UserRole.Admin,
            UserRole.Client,
            UserRole.Master
        };

        public ObservableCollection<OrderStatus> OrderStatuses { get; } = new ObservableCollection<OrderStatus>
        {
            OrderStatus.Created,
            OrderStatus.Assigned,
            OrderStatus.Diagnosing,
            OrderStatus.WaitingForParts,
            OrderStatus.InProgress,
            OrderStatus.ReadyForPickup,
            OrderStatus.Completed,
            OrderStatus.Cancelled
        };

        public ObservableCollection<string> DeviceTypes { get; } = new ObservableCollection<string>
        {
            "Ноутбук",
            "Стационарный ПК",
            "Моноблок",
            "Монитор",
            "Принтер",
            "Другое"
        };

        public ObservableCollection<string> DeliveryMethods { get; } = new ObservableCollection<string>
        {
            "Самовывоз",
            "Курьер"
        };

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                SelectedUserRole = value?.Role ?? UserRole.Client;
                LoadSelectedUserSpecializations(value);
            }
        }

        public UserRole SelectedUserRole
        {
            get => _selectedUserRole;
            set
            {
                if (SetProperty(ref _selectedUserRole, value))
                {
                    OnPropertyChanged(nameof(IsSelectedUserMaster));
                }
            }
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);
                SelectedOrderStatus = value?.Status ?? OrderStatus.Created;
                SelectedMaster = value == null ? null : Masters.FirstOrDefault(master => master.Id == value.AssignedMasterId);
            }
        }

        public User SelectedMaster
        {
            get => _selectedMaster;
            set
            {
                if (SetProperty(ref _selectedMaster, value))
                {
                    ApplyMasterAssignmentStatus();
                }
            }
        }

        public WarehouseItem SelectedWarehouseItem
        {
            get => _selectedWarehouseItem;
            set
            {
                if (SetProperty(ref _selectedWarehouseItem, value))
                {
                    OnPropertyChanged(nameof(HasWarehouseSelection));
                    OnPropertyChanged(nameof(WarehouseFormTitle));
                    OnPropertyChanged(nameof(WarehouseSaveButtonText));
                }
            }
        }

        public WarehouseRequest SelectedWarehouseRequest
        {
            get => _selectedWarehouseRequest;
            set => SetProperty(ref _selectedWarehouseRequest, value);
        }

        public OrderStatus SelectedOrderStatus
        {
            get => _selectedOrderStatus;
            set => SetProperty(ref _selectedOrderStatus, value);
        }

        public string NewUserLogin
        {
            get => _newUserLogin;
            set => SetProperty(ref _newUserLogin, value);
        }

        public string NewUserPassword
        {
            get => _newUserPassword;
            set => SetProperty(ref _newUserPassword, value);
        }

        public string NewUserName
        {
            get => _newUserName;
            set => SetProperty(ref _newUserName, value);
        }

        public UserRole NewUserRole
        {
            get => _newUserRole;
            set
            {
                if (SetProperty(ref _newUserRole, value))
                {
                    OnPropertyChanged(nameof(IsNewUserMaster));
                }
            }
        }

        public bool NewUserSpecializesLaptops
        {
            get => _newUserSpecializesLaptops;
            set => SetProperty(ref _newUserSpecializesLaptops, value);
        }

        public bool NewUserSpecializesComputers
        {
            get => _newUserSpecializesComputers;
            set => SetProperty(ref _newUserSpecializesComputers, value);
        }

        public bool NewUserSpecializesOfficeEquipment
        {
            get => _newUserSpecializesOfficeEquipment;
            set => SetProperty(ref _newUserSpecializesOfficeEquipment, value);
        }

        public bool SelectedUserSpecializesLaptops
        {
            get => _selectedUserSpecializesLaptops;
            set => SetProperty(ref _selectedUserSpecializesLaptops, value);
        }

        public bool SelectedUserSpecializesComputers
        {
            get => _selectedUserSpecializesComputers;
            set => SetProperty(ref _selectedUserSpecializesComputers, value);
        }

        public bool SelectedUserSpecializesOfficeEquipment
        {
            get => _selectedUserSpecializesOfficeEquipment;
            set => SetProperty(ref _selectedUserSpecializesOfficeEquipment, value);
        }

        public bool IsNewUserMaster => NewUserRole == UserRole.Master;
        public bool IsSelectedUserMaster => SelectedUserRole == UserRole.Master;

        public string NewOrderClientName
        {
            get => _newOrderClientName;
            set => SetProperty(ref _newOrderClientName, value);
        }

        public string NewOrderClientEmail
        {
            get => _newOrderClientEmail;
            set => SetProperty(ref _newOrderClientEmail, value);
        }

        public string NewOrderContactPhone
        {
            get => _newOrderContactPhone;
            set => SetProperty(ref _newOrderContactPhone, value);
        }

        public string NewOrderDeviceType
        {
            get => _newOrderDeviceType;
            set => SetProperty(ref _newOrderDeviceType, value);
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

        public string NewOrderClientComment
        {
            get => _newOrderClientComment;
            set => SetProperty(ref _newOrderClientComment, value);
        }

        public string WarehouseName
        {
            get => _warehouseName;
            set => SetProperty(ref _warehouseName, value);
        }

        public string WarehouseCategory
        {
            get => _warehouseCategory;
            set => SetProperty(ref _warehouseCategory, value);
        }

        public int WarehouseQuantity
        {
            get => _warehouseQuantity;
            set => SetProperty(ref _warehouseQuantity, value);
        }

        public decimal WarehouseUnitPrice
        {
            get => _warehouseUnitPrice;
            set => SetProperty(ref _warehouseUnitPrice, value);
        }

        public bool IsCourierDeliverySelected => NewOrderDeliveryMethod == "Курьер";

        public bool IsUserEditPanelVisible
        {
            get => _isUserEditPanelVisible;
            set => SetProperty(ref _isUserEditPanelVisible, value);
        }

        public bool IsWarehouseFormVisible
        {
            get => _isWarehouseFormVisible;
            set => SetProperty(ref _isWarehouseFormVisible, value);
        }

        public bool HasWarehouseSelection => SelectedWarehouseItem != null;
        public string WarehouseFormTitle => SelectedWarehouseItem == null ? "Новая позиция" : "Изменить позицию";
        public string WarehouseSaveButtonText => SelectedWarehouseItem == null ? "Добавить позицию" : "Сохранить изменения";

        public int TotalOrdersCount
        {
            get => _totalOrdersCount;
            set => SetProperty(ref _totalOrdersCount, value);
        }

        public int ActiveOrdersCount
        {
            get => _activeOrdersCount;
            set => SetProperty(ref _activeOrdersCount, value);
        }

        public int CompletedOrdersCount
        {
            get => _completedOrdersCount;
            set => SetProperty(ref _completedOrdersCount, value);
        }

        public int MastersCount
        {
            get => _mastersCount;
            set => SetProperty(ref _mastersCount, value);
        }

        public int ClientsCount
        {
            get => _clientsCount;
            set => SetProperty(ref _clientsCount, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand SaveUserRoleCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand SaveOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand BeginEditUserCommand { get; }
        public ICommand CancelEditUserCommand { get; }
        public ICommand ShowCreateUserFormCommand { get; }
        public ICommand CreateAdminOrderCommand { get; }
        public ICommand ShowCreateAdminOrderFormCommand { get; }
        public ICommand ShowEditOrderFormCommand { get; }
        public ICommand SaveWarehouseItemCommand { get; }
        public ICommand DeleteWarehouseItemCommand { get; }
        public ICommand ShowCreateWarehouseFormCommand { get; }
        public ICommand ToggleEditWarehouseItemCommand { get; }
        public ICommand CancelWarehouseFormCommand { get; }
        public ICommand ResolveWarehouseRequestCommand { get; }
        public ICommand ShowWarehouseRequestsCommand { get; }

        public ServiceAdminPanelViewModel()
        {
            _userRepository = RepositoryManager.Users;
            _orderRepository = RepositoryManager.Orders;
            _warehouseRepository = RepositoryManager.Warehouse;
            _warehouseRequestRepository = RepositoryManager.WarehouseRequests;

            Users = new ObservableCollection<User>();
            Masters = new ObservableCollection<User>();
            Orders = new ObservableCollection<Order>();
            WarehouseItems = new ObservableCollection<WarehouseItem>();
            WarehouseRequests = new ObservableCollection<WarehouseRequest>();
            NewUserRole = UserRole.Master;

            RefreshCommand = new RelayCommand(LoadData);
            DeleteUserCommand = new RelayCommandSec(DeleteUser);
            SaveUserRoleCommand = new RelayCommand(SaveUserRole);
            CreateUserCommand = new RelayCommand(CreateUser);
            SaveOrderCommand = new RelayCommand(SaveOrder);
            DeleteOrderCommand = new RelayCommand(DeleteOrder);
            LogoutCommand = new RelayCommand(Logout);
            BeginEditUserCommand = new RelayCommandSec(BeginEditUser);
            CancelEditUserCommand = new RelayCommand(CancelEditUser);
            ShowCreateUserFormCommand = new RelayCommand(ShowCreateUserForm);
            CreateAdminOrderCommand = new RelayCommand(CreateAdminOrder);
            ShowCreateAdminOrderFormCommand = new RelayCommand(ShowCreateAdminOrderForm);
            ShowEditOrderFormCommand = new RelayCommand(ShowEditOrderForm);
            SaveWarehouseItemCommand = new RelayCommand(SaveWarehouseItem);
            DeleteWarehouseItemCommand = new RelayCommand(DeleteWarehouseItem);
            ShowCreateWarehouseFormCommand = new RelayCommand(ShowCreateWarehouseForm);
            ToggleEditWarehouseItemCommand = new RelayCommand(ToggleEditWarehouseItem);
            CancelWarehouseFormCommand = new RelayCommand(CancelWarehouseForm);
            ResolveWarehouseRequestCommand = new RelayCommandSec(ResolveWarehouseRequest);
            ShowWarehouseRequestsCommand = new RelayCommand(ShowWarehouseRequests);

            ResetNewOrderForm();
            CancelWarehouseForm();
            LoadData();
        }

        private void LoadData()
        {
            Users = new ObservableCollection<User>(_userRepository.GetAll());
            Masters = new ObservableCollection<User>(_userRepository.GetByRole(UserRole.Master));
            Orders = new ObservableCollection<Order>(_orderRepository.GetAll());
            WarehouseItems = new ObservableCollection<WarehouseItem>(_warehouseRepository.GetAll());
            WarehouseRequests = new ObservableCollection<WarehouseRequest>(
                _warehouseRequestRepository.GetAll()
                    .Where(request => request.Status != "Обработано"));

            TotalOrdersCount = Orders.Count;
            ActiveOrdersCount = Orders.Count(order => order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled);
            CompletedOrdersCount = Orders.Count(order => order.Status == OrderStatus.Completed);
            MastersCount = Users.Count(user => user.Role == UserRole.Master);
            ClientsCount = Users.Count(user => user.Role == UserRole.Client);
        }

        private void CreateUser()
        {
            if (string.IsNullOrWhiteSpace(NewUserLogin) ||
                string.IsNullOrWhiteSpace(NewUserPassword) ||
                string.IsNullOrWhiteSpace(NewUserName))
            {
                MessageBox.Show("Fill in login, name, and password to create an account.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_userRepository.GetByLogin(NewUserLogin) != null)
            {
                MessageBox.Show("A user with this login already exists.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = new User
            {
                Login = NewUserLogin.Trim(),
                HashPassword = HashHelper.HashPassword(NewUserPassword),
                Name = NewUserName.Trim(),
                Role = NewUserRole,
                MasterSpecializations = GetNewUserSpecializations()
            };

            if (NewUserRole == UserRole.Master && string.IsNullOrWhiteSpace(user.MasterSpecializations))
            {
                MessageBox.Show("Выберите хотя бы одну специализацию мастера.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _userRepository.Add(user);
            NewUserLogin = string.Empty;
            NewUserPassword = string.Empty;
            NewUserName = string.Empty;
            NewUserRole = UserRole.Master;
            ResetNewUserSpecializations();
            LoadData();
        }

        private void BeginEditUser(object parameter)
        {
            var user = parameter as User;
            if (user == null)
            {
                MessageBox.Show("Select a user.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedUser != null &&
                SelectedUser.Id == user.Id &&
                IsUserEditPanelVisible)
            {
                CancelEditUser();
                return;
            }

            SelectedUser = user;
            SelectedUserRole = user.Role;
            IsUserEditPanelVisible = true;
            ShowEditUserForm();
        }

        private void CancelEditUser()
        {
            IsUserEditPanelVisible = false;
            LoadData();
        }

        private void ShowCreateUserForm()
        {
            NewUserLogin = string.Empty;
            NewUserPassword = string.Empty;
            NewUserName = string.Empty;
            NewUserRole = UserRole.Master;
            ResetNewUserSpecializations();

            var window = new UserCreateWindow(this);
            ShowDialog(window);
            LoadData();
        }

        private void ShowEditUserForm()
        {
            var window = new UserEditWindow(this);
            ShowDialog(window);
            IsUserEditPanelVisible = false;
            LoadData();
        }

        private void SaveUserRole()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Select a user.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedUser.Name) || string.IsNullOrWhiteSpace(SelectedUser.Login))
            {
                MessageBox.Show("Name and login are required.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingUser = _userRepository.GetByLogin(SelectedUser.Login);
            if (existingUser != null && existingUser.Id != SelectedUser.Id)
            {
                MessageBox.Show("A user with this login already exists.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedUser.Role = SelectedUserRole;
            SelectedUser.MasterSpecializations = GetSelectedUserSpecializations();
            if (SelectedUserRole == UserRole.Master && string.IsNullOrWhiteSpace(SelectedUser.MasterSpecializations))
            {
                MessageBox.Show("Выберите хотя бы одну специализацию мастера.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _userRepository.Update(SelectedUser);

            if (SessionManager.CurrentUser != null && SessionManager.CurrentUser.Id == SelectedUser.Id)
            {
                SessionManager.Login(SelectedUser);
            }

            IsUserEditPanelVisible = false;
            LoadData();
        }

        private void DeleteUser(object parameter)
        {
            var userToDelete = parameter as User ?? SelectedUser;

            if (userToDelete == null)
            {
                MessageBox.Show("Select a user.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SessionManager.CurrentUser != null && userToDelete.Id == SessionManager.CurrentUser.Id)
            {
                MessageBox.Show("You cannot delete the currently active account.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _userRepository.Delete(userToDelete.Id);
            if (SelectedUser != null && SelectedUser.Id == userToDelete.Id)
            {
                SelectedUser = null;
            }
            IsUserEditPanelVisible = false;
            LoadData();
        }

        private void SaveOrder()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Select an order.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedOrder.AssignedMasterId = SelectedMaster?.Id;
            SelectedOrder.AssignedMaster = SelectedMaster;
            SelectedOrder.Status = GetAutoOrderStatusForAdminSave();

            _orderRepository.Update(SelectedOrder);
            LoadData();
        }

        private void DeleteOrder()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Select an order.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _orderRepository.Delete(SelectedOrder.Id);
            SelectedOrder = null;
            LoadData();
        }

        private void ShowEditOrderForm()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Выберите заявку.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedOrderStatus = SelectedOrder.Status;
            SelectedMaster = SelectedOrder.AssignedMasterId == null
                ? null
                : Masters.FirstOrDefault(master => master.Id == SelectedOrder.AssignedMasterId);

            var window = new AdminOrderEditWindow(this);
            ShowDialog(window);
            LoadData();
        }

        private void ShowCreateAdminOrderForm()
        {
            ResetNewOrderForm();
            var window = new AdminOrderCreateWindow(this);
            ShowDialog(window);
            LoadData();
        }

        private void CreateAdminOrder()
        {
            if (string.IsNullOrWhiteSpace(NewOrderClientName) ||
                string.IsNullOrWhiteSpace(NewOrderClientEmail) ||
                string.IsNullOrWhiteSpace(NewOrderContactPhone) ||
                string.IsNullOrWhiteSpace(NewOrderDeviceType) ||
                string.IsNullOrWhiteSpace(NewOrderDeviceBrand) ||
                string.IsNullOrWhiteSpace(NewOrderDeviceModel) ||
                string.IsNullOrWhiteSpace(NewOrderProblemDescription) ||
                string.IsNullOrWhiteSpace(NewOrderDeliveryMethod))
            {
                MessageBox.Show("Заполните имя клиента, email, телефон, устройство и неисправность.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsCourierDeliverySelected && string.IsNullOrWhiteSpace(NewOrderDeliveryAddress))
            {
                MessageBox.Show("Укажите адрес для курьера.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var email = NewOrderClientEmail.Trim();
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                user = new User
                {
                    Name = NewOrderClientName.Trim(),
                    Email = email,
                    Login = GenerateUniqueClientLogin(NewOrderClientName, email),
                    HashPassword = HashHelper.HashPassword(Guid.NewGuid().ToString("N")),
                    Role = UserRole.Client
                };
                _userRepository.Add(user);
            }
            else if (user.Name != NewOrderClientName.Trim())
            {
                user.Name = NewOrderClientName.Trim();
                _userRepository.Update(user);
            }

            var order = new Order
            {
                UserId = user.Id,
                User = user,
                DeviceType = NewOrderDeviceType,
                DeviceBrand = NewOrderDeviceBrand.Trim(),
                DeviceModel = NewOrderDeviceModel.Trim(),
                ProblemDescription = NewOrderProblemDescription.Trim(),
                DeliveryMethod = NewOrderDeliveryMethod,
                DeliveryAddress = IsCourierDeliverySelected ? NewOrderDeliveryAddress.Trim() : null,
                ContactPhone = NewOrderContactPhone.Trim(),
                ClientComment = string.IsNullOrWhiteSpace(NewOrderClientComment) ? null : NewOrderClientComment.Trim(),
                Status = OrderStatus.Created,
                CreatedAt = DateTime.Now
            };

            AssignBestMaster(order);
            _orderRepository.Add(order);
            ResetNewOrderForm();
            LoadData();
            SelectedOrder = Orders.FirstOrDefault(item => item.Id == order.Id);
            MessageBox.Show("Заявка успешно оформлена администратором.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetNewOrderForm()
        {
            NewOrderClientName = string.Empty;
            NewOrderClientEmail = string.Empty;
            NewOrderContactPhone = string.Empty;
            NewOrderDeviceType = DeviceTypes[0];
            NewOrderDeviceBrand = string.Empty;
            NewOrderDeviceModel = string.Empty;
            NewOrderProblemDescription = string.Empty;
            NewOrderDeliveryMethod = DeliveryMethods[0];
            NewOrderDeliveryAddress = string.Empty;
            NewOrderClientComment = string.Empty;
        }

        private void PopulateWarehouseForm(WarehouseItem item)
        {
            if (item == null)
            {
                WarehouseName = string.Empty;
                WarehouseCategory = string.Empty;
                WarehouseQuantity = 0;
                WarehouseUnitPrice = 0;
                return;
            }

            WarehouseName = item.Name;
            WarehouseCategory = item.Category;
            WarehouseQuantity = item.Quantity;
            WarehouseUnitPrice = item.UnitPrice;
        }

        private void ShowCreateWarehouseForm()
        {
            OpenWarehouseItemWindow(null);
        }

        private void ToggleEditWarehouseItem()
        {
            if (SelectedWarehouseItem == null)
            {
                MessageBox.Show("Выберите позицию склада.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OpenWarehouseItemWindow(SelectedWarehouseItem);
        }

        private void SaveWarehouseItem()
        {
            OpenWarehouseItemWindow(SelectedWarehouseItem);
        }

        private void OpenWarehouseItemWindow(WarehouseItem item)
        {
            var window = new WarehouseItemWindow(item);
            if (Application.Current.MainWindow != null)
            {
                window.Owner = Application.Current.MainWindow;
            }

            if (window.ShowDialog() != true)
            {
                return;
            }

            if (item == null)
            {
                _warehouseRepository.Add(new WarehouseItem
                {
                    Name = window.ItemName,
                    Category = window.ItemCategory,
                    Quantity = window.ItemQuantity,
                    UnitPrice = window.ItemUnitPrice
                });
            }
            else
            {
                item.Name = window.ItemName;
                item.Category = window.ItemCategory;
                item.Quantity = window.ItemQuantity;
                item.UnitPrice = window.ItemUnitPrice;
                _warehouseRepository.Update(item);
            }

            LoadData();
            CancelWarehouseForm();
        }

        private void DeleteWarehouseItem()
        {
            if (SelectedWarehouseItem == null)
            {
                MessageBox.Show("Выберите позицию склада.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _warehouseRepository.Delete(SelectedWarehouseItem.Id);
            LoadData();
            CancelWarehouseForm();
        }

        private void CancelWarehouseForm()
        {
            SelectedWarehouseItem = null;
            PopulateWarehouseForm(null);
            IsWarehouseFormVisible = false;
            OnPropertyChanged(nameof(HasWarehouseSelection));
            OnPropertyChanged(nameof(WarehouseFormTitle));
            OnPropertyChanged(nameof(WarehouseSaveButtonText));
        }

        private void ResolveWarehouseRequest(object parameter)
        {
            var request = parameter as WarehouseRequest ?? SelectedWarehouseRequest;
            if (request == null)
            {
                MessageBox.Show("Выберите запрос мастера.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            request.Status = "Обработано";
            _warehouseRequestRepository.Update(request);
            UpdateOrderAfterWarehouseRequestResolved(request.OrderId);
            LoadData();
        }

        private void ShowWarehouseRequests()
        {
            var window = new WarehouseRequestsWindow(this);
            ShowDialog(window);
            LoadData();
        }

        private void ShowDialog(Window window)
        {
            if (Application.Current.MainWindow != null)
            {
                window.Owner = Application.Current.MainWindow;
            }

            window.ShowDialog();
        }

        private string GetNewUserSpecializations()
        {
            if (NewUserRole != UserRole.Master)
            {
                return null;
            }

            return MasterAssignmentService.BuildSpecializations(
                NewUserSpecializesLaptops,
                NewUserSpecializesComputers,
                NewUserSpecializesOfficeEquipment);
        }

        private string GetSelectedUserSpecializations()
        {
            if (SelectedUserRole != UserRole.Master)
            {
                return null;
            }

            return MasterAssignmentService.BuildSpecializations(
                SelectedUserSpecializesLaptops,
                SelectedUserSpecializesComputers,
                SelectedUserSpecializesOfficeEquipment);
        }

        private void ResetNewUserSpecializations()
        {
            NewUserSpecializesLaptops = false;
            NewUserSpecializesComputers = false;
            NewUserSpecializesOfficeEquipment = false;
        }

        private void LoadSelectedUserSpecializations(User user)
        {
            SelectedUserSpecializesLaptops = MasterAssignmentService.HasSpecialization(user, MasterAssignmentService.LaptopSpecialization);
            SelectedUserSpecializesComputers = MasterAssignmentService.HasSpecialization(user, MasterAssignmentService.ComputerSpecialization);
            SelectedUserSpecializesOfficeEquipment = MasterAssignmentService.HasSpecialization(user, MasterAssignmentService.OfficeEquipmentSpecialization);
        }

        private void AssignBestMaster(Order order)
        {
            var master = MasterAssignmentService.FindBestMaster(
                order.DeviceType,
                _userRepository.GetByRole(UserRole.Master),
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

        private void ApplyMasterAssignmentStatus()
        {
            if (SelectedOrder == null)
            {
                return;
            }

            if (SelectedMaster != null && SelectedOrderStatus == OrderStatus.Created)
            {
                SelectedOrderStatus = OrderStatus.Assigned;
            }
            else if (SelectedMaster == null && SelectedOrderStatus == OrderStatus.Assigned)
            {
                SelectedOrderStatus = OrderStatus.Created;
            }
        }

        private OrderStatus GetAutoOrderStatusForAdminSave()
        {
            if (SelectedMaster == null)
            {
                return SelectedOrderStatus == OrderStatus.Cancelled
                    ? OrderStatus.Cancelled
                    : OrderStatus.Created;
            }

            return SelectedOrderStatus == OrderStatus.Created
                ? OrderStatus.Assigned
                : SelectedOrderStatus;
        }

        private void UpdateOrderAfterWarehouseRequestResolved(int orderId)
        {
            var order = _orderRepository.GetById(orderId);
            if (order == null || order.Status != OrderStatus.WaitingForParts)
            {
                return;
            }

            var hasOpenMaterialRequests = _warehouseRequestRepository.GetByOrderId(orderId)
                .Any(item => item.Status != "Обработано");

            if (hasOpenMaterialRequests)
            {
                return;
            }

            order.Status = order.AssignedMasterId.HasValue
                ? OrderStatus.InProgress
                : OrderStatus.Created;
            _orderRepository.Update(order);
        }

        private string GenerateUniqueClientLogin(string name, string email)
        {
            var baseValue = string.IsNullOrWhiteSpace(email)
                ? name
                : email.Split('@').FirstOrDefault();

            var sanitized = new string((baseValue ?? "client")
                .ToLowerInvariant()
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '.')
                .ToArray());

            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "client";
            }

            var login = sanitized;
            var suffix = 1;
            while (_userRepository.GetByLogin(login) != null)
            {
                login = $"{sanitized}_{suffix}";
                suffix++;
            }

            return login;
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
    }
}
