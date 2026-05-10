using Microsoft.Win32;
using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views;
using Printinvest_WPF_app.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Printinvest_WPF_app.ViewModels
{
    public class ServiceAdminPanelViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly OrderRepository _orderRepository;
        private readonly WarehouseRepository _warehouseRepository;
        private readonly WarehouseRequestRepository _warehouseRequestRepository;
        private List<WarehouseItem> _allWarehouseItems;
        private ObservableCollection<User> _users;
        private ObservableCollection<User> _masters;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<WarehouseItem> _warehouseItems;
        private ObservableCollection<WarehouseRequest> _warehouseRequests;
        private User _selectedUser;
        private UserRole _selectedUserRole;
        private bool _isLoadingSelectedUserData;
        private string _selectedUserLastName;
        private string _selectedUserFirstName;
        private string _selectedUserMiddleName;
        private string _selectedUserEmail;
        private Order _selectedOrder;
        private User _selectedMaster;
        private WarehouseItem _selectedWarehouseItem;
        private WarehouseRequest _selectedWarehouseRequest;
        private OrderStatus _selectedOrderStatus;
        private decimal _selectedOrderMasterWorkCost;
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
        private string _newOrderPaymentMethod;
        private byte[] _newOrderProblemPhoto;
        private string _newOrderProblemPhotoName;
        private string _warehouseName;
        private string _warehouseCategory;
        private int _warehouseQuantity;
        private decimal _warehouseUnitPrice;
        private string _warehouseSearchText;
        private string _selectedWarehouseSortOption;
        private string _selectedWarehouseCategoryFilter;
        private bool _isUserEditPanelVisible;
        private bool _isWarehouseFormVisible;
        private int _totalOrdersCount;
        private int _activeOrdersCount;
        private int _completedOrdersCount;
        private int _mastersCount;
        private int _clientsCount;
        private bool _showAdminOrderValidationErrors;
        private bool _wasLastAdminOrderCreationSuccessful;
        private static readonly OrderStatusChartDefinition[] OrderStatusChartDefinitions =
        {
            new OrderStatusChartDefinition(OrderStatus.Created, "ChartStatusCreated", "Created", "#5B8DEF"),
            new OrderStatusChartDefinition(OrderStatus.Assigned, "ChartStatusAssigned", "Assigned to master", "#7C6BF1"),
            new OrderStatusChartDefinition(OrderStatus.Diagnosing, "ChartStatusDiagnosing", "Diagnosing", "#F39C4A"),
            new OrderStatusChartDefinition(OrderStatus.WaitingForParts, "ChartStatusWaitingForParts", "Waiting for parts", "#E66A6A"),
            new OrderStatusChartDefinition(OrderStatus.InProgress, "ChartStatusInProgress", "In progress", "#31B099"),
            new OrderStatusChartDefinition(OrderStatus.ReadyForPickup, "ChartStatusReadyForPickup", "Ready for pickup", "#4DB6E5"),
            new OrderStatusChartDefinition(OrderStatus.Completed, "ChartStatusCompleted", "Completed", "#8796AC"),
            new OrderStatusChartDefinition(OrderStatus.Cancelled, "ChartStatusCancelled", "Cancelled", "#C26D7C")
        };

        private bool AreRepositoriesReady =>
            _userRepository != null &&
            _orderRepository != null &&
            _warehouseRepository != null &&
            _warehouseRequestRepository != null;

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

        public ObservableCollection<string> WarehouseSortOptions { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> WarehouseCategoryFilters { get; } = new ObservableCollection<string>();

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

        public ObservableCollection<string> PaymentMethods { get; } = new ObservableCollection<string>
        {
            Order.OnSitePaymentMethod,
            Order.OnlinePaymentMethod
        };

        public ObservableCollection<OrderStatusChartSlice> OrderStatusChartSlices { get; } =
            new ObservableCollection<OrderStatusChartSlice>();

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                _isLoadingSelectedUserData = true;
                LoadSelectedClientFields(value);
                SelectedUserRole = value?.Role ?? UserRole.Client;
                LoadSelectedUserSpecializations(value);
                _isLoadingSelectedUserData = false;
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
                    OnPropertyChanged(nameof(IsSelectedUserClient));
                    OnPropertyChanged(nameof(IsSelectedUserNonClient));
                    SyncSelectedUserPersonalFields();
                }
            }
        }

        public string SelectedUserLastName
        {
            get => _selectedUserLastName;
            set
            {
                if (SetProperty(ref _selectedUserLastName, value))
                {
                    SyncSelectedUserPersonalFields();
                }
            }
        }

        public string SelectedUserFirstName
        {
            get => _selectedUserFirstName;
            set
            {
                if (SetProperty(ref _selectedUserFirstName, value))
                {
                    SyncSelectedUserPersonalFields();
                }
            }
        }

        public string SelectedUserMiddleName
        {
            get => _selectedUserMiddleName;
            set
            {
                if (SetProperty(ref _selectedUserMiddleName, value))
                {
                    SyncSelectedUserPersonalFields();
                }
            }
        }

        public string SelectedUserEmail
        {
            get => _selectedUserEmail;
            set
            {
                if (SetProperty(ref _selectedUserEmail, value))
                {
                    SyncSelectedUserPersonalFields();
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
                SelectedOrderMasterWorkCost = value?.MasterWorkCost ?? 0;
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

        public decimal SelectedOrderMasterWorkCost
        {
            get => _selectedOrderMasterWorkCost;
            set
            {
                var normalizedValue = value < 0 ? 0 : value;
                if (!SetProperty(ref _selectedOrderMasterWorkCost, normalizedValue))
                {
                    return;
                }

                if (SelectedOrder == null)
                {
                    return;
                }

                SelectedOrder.MasterWorkCost = normalizedValue;
                SelectedOrder.EstimatedRepairCost = SelectedOrder.EstimatedPartsCost + normalizedValue;
            }
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
        public bool IsSelectedUserClient => SelectedUserRole == UserRole.Client;
        public bool IsSelectedUserNonClient => !IsSelectedUserClient;

        public string NewOrderClientName
        {
            get => _newOrderClientName;
            set
            {
                if (SetProperty(ref _newOrderClientName, value))
                {
                    NotifyAdminOrderValidationStateChanged();
                }
            }
        }

        public string NewOrderClientEmail
        {
            get => _newOrderClientEmail;
            set
            {
                if (SetProperty(ref _newOrderClientEmail, value))
                {
                    NotifyAdminOrderValidationStateChanged();
                }
            }
        }

        public string NewOrderContactPhone
        {
            get => _newOrderContactPhone;
            set
            {
                if (SetProperty(ref _newOrderContactPhone, value))
                {
                    NotifyAdminOrderValidationStateChanged();
                }
            }
        }

        public string NewOrderDeviceType
        {
            get => _newOrderDeviceType;
            set
            {
                if (SetProperty(ref _newOrderDeviceType, value))
                {
                    NotifyAdminOrderValidationStateChanged();
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
                    NotifyAdminOrderValidationStateChanged();
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
                    NotifyAdminOrderValidationStateChanged();
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
                    NotifyAdminOrderValidationStateChanged();
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

                    NotifyAdminOrderValidationStateChanged();
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
                    NotifyAdminOrderValidationStateChanged();
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
                    NotifyAdminOrderValidationStateChanged();
                }
            }
        }

        public bool NewOrderIsOnlinePayment
        {
            get => string.Equals(NewOrderPaymentMethod, Order.OnlinePaymentMethod, StringComparison.Ordinal);
            set => NewOrderPaymentMethod = value ? Order.OnlinePaymentMethod : Order.OnSitePaymentMethod;
        }

        public byte[] NewOrderProblemPhoto
        {
            get => _newOrderProblemPhoto;
            set => SetProperty(ref _newOrderProblemPhoto, value);
        }

        public string NewOrderProblemPhotoName
        {
            get => _newOrderProblemPhotoName;
            set => SetProperty(ref _newOrderProblemPhotoName, value);
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

        public string WarehouseSearchText
        {
            get => _warehouseSearchText;
            set
            {
                if (SetProperty(ref _warehouseSearchText, value))
                {
                    ApplyWarehouseFilters();
                }
            }
        }

        public string SelectedWarehouseSortOption
        {
            get => _selectedWarehouseSortOption;
            set
            {
                if (SetProperty(ref _selectedWarehouseSortOption, value))
                {
                    ApplyWarehouseFilters();
                }
            }
        }

        public string SelectedWarehouseCategoryFilter
        {
            get => _selectedWarehouseCategoryFilter;
            set
            {
                if (SetProperty(ref _selectedWarehouseCategoryFilter, value))
                {
                    ApplyWarehouseFilters();
                }
            }
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

        public bool HasOrderStatusChartData => OrderStatusChartSlices.Count > 0;

        public bool ShowAdminOrderValidationErrors
        {
            get => _showAdminOrderValidationErrors;
            private set
            {
                if (SetProperty(ref _showAdminOrderValidationErrors, value))
                {
                    NotifyAdminOrderValidationStateChanged();
                }
            }
        }

        public bool WasLastAdminOrderCreationSuccessful
        {
            get => _wasLastAdminOrderCreationSuccessful;
            private set => SetProperty(ref _wasLastAdminOrderCreationSuccessful, value);
        }

        public bool IsNewOrderClientNameInvalid => ShowAdminOrderValidationErrors && string.IsNullOrWhiteSpace(NewOrderClientName);
        public bool IsNewOrderClientEmailInvalid => ShowAdminOrderValidationErrors && !IsValidEmail(NewOrderClientEmail?.Trim());
        public bool IsNewOrderContactPhoneInvalid => ShowAdminOrderValidationErrors && !IsValidPhoneNumber(NewOrderContactPhone);
        public bool IsNewOrderDeviceBrandInvalid => ShowAdminOrderValidationErrors && string.IsNullOrWhiteSpace(NewOrderDeviceBrand);
        public bool IsNewOrderDeviceModelInvalid => ShowAdminOrderValidationErrors && string.IsNullOrWhiteSpace(NewOrderDeviceModel);
        public bool IsNewOrderProblemDescriptionInvalid => ShowAdminOrderValidationErrors && string.IsNullOrWhiteSpace(NewOrderProblemDescription);
        public bool IsNewOrderDeliveryAddressInvalid => ShowAdminOrderValidationErrors && IsCourierDeliverySelected && string.IsNullOrWhiteSpace(NewOrderDeliveryAddress);
        public bool HasAdminOrderValidationErrors =>
            IsNewOrderClientNameInvalid ||
            IsNewOrderClientEmailInvalid ||
            IsNewOrderContactPhoneInvalid ||
            IsNewOrderDeviceBrandInvalid ||
            IsNewOrderDeviceModelInvalid ||
            IsNewOrderProblemDescriptionInvalid ||
            IsNewOrderDeliveryAddressInvalid;

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
        public ICommand SelectProblemPhotoCommand { get; }
        public ICommand RemoveProblemPhotoCommand { get; }

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
            _allWarehouseItems = new List<WarehouseItem>();
            NewUserRole = UserRole.Master;
            RebuildWarehouseSortOptions();

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
            DeleteWarehouseItemCommand = new RelayCommandSec(DeleteWarehouseItem);
            ShowCreateWarehouseFormCommand = new RelayCommand(ShowCreateWarehouseForm);
            ToggleEditWarehouseItemCommand = new RelayCommandSec(ToggleEditWarehouseItem);
            CancelWarehouseFormCommand = new RelayCommand(CancelWarehouseForm);
            ResolveWarehouseRequestCommand = new RelayCommandSec(ResolveWarehouseRequest);
            ShowWarehouseRequestsCommand = new RelayCommand(ShowWarehouseRequests);
            SelectProblemPhotoCommand = new RelayCommand(SelectProblemPhoto);
            RemoveProblemPhotoCommand = new RelayCommand(RemoveProblemPhoto);

            ResetNewOrderForm();
            CancelWarehouseForm();
            if (AreRepositoriesReady)
            {
                LoadData();
            }
            else
            {
                ResetDataWhenRepositoriesUnavailable();
            }

            App.LanguageChanged += OnLanguageChanged;
        }

        private void LoadData()
        {
            if (!AreRepositoriesReady)
            {
                ResetDataWhenRepositoriesUnavailable();
                return;
            }

            Users = new ObservableCollection<User>(_userRepository.GetAll());
            Masters = new ObservableCollection<User>(_userRepository.GetByRole(UserRole.Master));
            Orders = new ObservableCollection<Order>(_orderRepository.GetAll());
            LoadWarehouseItems();
            WarehouseRequests = new ObservableCollection<WarehouseRequest>(
                _warehouseRequestRepository.GetAll()
                    .Where(request => request.Status != "Обработано"));

            TotalOrdersCount = Orders.Count;
            ActiveOrdersCount = Orders.Count(order => order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled);
            CompletedOrdersCount = Orders.Count(order => order.Status == OrderStatus.Completed);
            MastersCount = Users.Count(user => user.Role == UserRole.Master);
            ClientsCount = Users.Count(user => user.Role == UserRole.Client);
            UpdateOrderStatusChart();
        }

        private void ResetDataWhenRepositoriesUnavailable()
        {
            Users = new ObservableCollection<User>();
            Masters = new ObservableCollection<User>();
            Orders = new ObservableCollection<Order>();
            WarehouseItems = new ObservableCollection<WarehouseItem>();
            WarehouseRequests = new ObservableCollection<WarehouseRequest>();
            _allWarehouseItems = new List<WarehouseItem>();

            if (WarehouseCategoryFilters.Count == 0)
            {
                WarehouseCategoryFilters.Add(GetAllCategoriesLabel());
            }

            _selectedWarehouseCategoryFilter = WarehouseCategoryFilters[0];
            OnPropertyChanged(nameof(SelectedWarehouseCategoryFilter));

            if (string.IsNullOrWhiteSpace(_selectedWarehouseSortOption) && WarehouseSortOptions.Count > 0)
            {
                _selectedWarehouseSortOption = WarehouseSortOptions[0];
                OnPropertyChanged(nameof(SelectedWarehouseSortOption));
            }

            TotalOrdersCount = 0;
            ActiveOrdersCount = 0;
            CompletedOrdersCount = 0;
            MastersCount = 0;
            ClientsCount = 0;
            UpdateOrderStatusChart();
        }

        private void UpdateOrderStatusChart()
        {
            OrderStatusChartSlices.Clear();

            var totalOrders = Orders?.Count ?? 0;
            if (totalOrders == 0)
            {
                OnPropertyChanged(nameof(HasOrderStatusChartData));
                return;
            }

            const double canvasSize = 240d;
            const double outerRadius = 92d;
            const double innerRadius = 58d;
            var center = new Point(canvasSize / 2, canvasSize / 2);
            var startAngle = -90d;
            var statusEntries = OrderStatusChartDefinitions
                .Select((definition, index) => new
                {
                    Definition = definition,
                    Index = index,
                    Count = Orders.Count(order => order.Status == definition.Status)
                })
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.Index)
                .ToList();

            foreach (var entry in statusEntries)
            {
                var count = entry.Count;
                var sweepAngle = totalOrders == 0 ? 0 : 360d * count / totalOrders;
                var slice = new OrderStatusChartSlice
                {
                    Label = App.GetString(entry.Definition.ResourceKey, entry.Definition.FallbackLabel),
                    Count = count,
                    Percentage = totalOrders == 0 ? 0 : (double)count / totalOrders,
                    Fill = CreateBrush(entry.Definition.ColorHex),
                    Geometry = count == 0
                        ? null
                        : sweepAngle >= 359.999
                        ? CreateFullRingGeometry(center, outerRadius, innerRadius)
                        : CreateDonutSliceGeometry(center, outerRadius, innerRadius, startAngle, sweepAngle)
                };

                OrderStatusChartSlices.Add(slice);
                startAngle += sweepAngle;
            }

            OnPropertyChanged(nameof(HasOrderStatusChartData));
        }

        private static Brush CreateBrush(string colorHex)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(colorHex);
            brush.Freeze();
            return brush;
        }

        private static Geometry CreateFullRingGeometry(Point center, double outerRadius, double innerRadius)
        {
            var ring = new GeometryGroup
            {
                FillRule = FillRule.EvenOdd
            };

            ring.Children.Add(new EllipseGeometry(center, outerRadius, outerRadius));
            ring.Children.Add(new EllipseGeometry(center, innerRadius, innerRadius));
            ring.Freeze();
            return ring;
        }

        private static Geometry CreateDonutSliceGeometry(
            Point center,
            double outerRadius,
            double innerRadius,
            double startAngle,
            double sweepAngle)
        {
            var endAngle = startAngle + sweepAngle;
            var outerStart = PointOnCircle(center, outerRadius, startAngle);
            var outerEnd = PointOnCircle(center, outerRadius, endAngle);
            var innerEnd = PointOnCircle(center, innerRadius, endAngle);
            var innerStart = PointOnCircle(center, innerRadius, startAngle);
            var isLargeArc = sweepAngle > 180d;

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(outerStart, true, true);
                context.ArcTo(outerEnd, new Size(outerRadius, outerRadius), 0, isLargeArc, SweepDirection.Clockwise, true, false);
                context.LineTo(innerEnd, true, false);
                context.ArcTo(innerStart, new Size(innerRadius, innerRadius), 0, isLargeArc, SweepDirection.Counterclockwise, true, false);
            }

            geometry.Freeze();
            return geometry;
        }

        private static Point PointOnCircle(Point center, double radius, double angleInDegrees)
        {
            var angleInRadians = angleInDegrees * Math.PI / 180d;
            return new Point(
                center.X + radius * Math.Cos(angleInRadians),
                center.Y + radius * Math.Sin(angleInRadians));
        }

        private void LoadWarehouseItems()
        {
            _allWarehouseItems = _warehouseRepository.GetAll();

            var selectedCategory = SelectedWarehouseCategoryFilter;
            WarehouseCategoryFilters.Clear();
            WarehouseCategoryFilters.Add(GetAllCategoriesLabel());

            foreach (var category in _allWarehouseItems
                .Select(item => item.Category)
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct()
                .OrderBy(category => category))
            {
                WarehouseCategoryFilters.Add(category);
            }

            if (string.IsNullOrWhiteSpace(selectedCategory) || !WarehouseCategoryFilters.Contains(selectedCategory))
            {
                SelectedWarehouseCategoryFilter = WarehouseCategoryFilters[0];
            }
            else
            {
                _selectedWarehouseCategoryFilter = selectedCategory;
                OnPropertyChanged(nameof(SelectedWarehouseCategoryFilter));
            }

            if (string.IsNullOrWhiteSpace(SelectedWarehouseSortOption))
            {
                SelectedWarehouseSortOption = WarehouseSortOptions[0];
            }
            else
            {
                ApplyWarehouseFilters();
            }
        }

        private void ApplyWarehouseFilters()
        {
            IEnumerable<WarehouseItem> items = _allWarehouseItems ?? new List<WarehouseItem>();

            if (!string.IsNullOrWhiteSpace(WarehouseSearchText))
            {
                var search = WarehouseSearchText.ToLowerInvariant();
                items = items.Where(item =>
                    (item.Name?.ToLowerInvariant().Contains(search) ?? false) ||
                    (item.Category?.ToLowerInvariant().Contains(search) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(SelectedWarehouseCategoryFilter) &&
                SelectedWarehouseCategoryFilter != GetAllCategoriesLabel())
            {
                items = items.Where(item => item.Category == SelectedWarehouseCategoryFilter);
            }

            if (SelectedWarehouseSortOption == App.GetString("WarehouseSortQuantityAsc", "Lower stock first"))
            {
                items = items.OrderBy(item => item.Quantity).ThenBy(item => item.Name);
            }
            else if (SelectedWarehouseSortOption == App.GetString("WarehouseSortQuantityDesc", "Higher stock first"))
            {
                items = items.OrderByDescending(item => item.Quantity).ThenBy(item => item.Name);
            }
            else if (SelectedWarehouseSortOption == App.GetString("WarehouseSortPriceAsc", "Cheaper first"))
            {
                items = items.OrderBy(item => item.UnitPrice).ThenBy(item => item.Name);
            }
            else if (SelectedWarehouseSortOption == App.GetString("WarehouseSortPriceDesc", "More expensive first"))
            {
                items = items.OrderByDescending(item => item.UnitPrice).ThenBy(item => item.Name);
            }
            else
            {
                items = items.OrderBy(item => item.Name);
            }

            var selectedId = SelectedWarehouseItem?.Id;
            WarehouseItems = new ObservableCollection<WarehouseItem>(items.ToList());

            if (selectedId.HasValue)
            {
                SelectedWarehouseItem = WarehouseItems.FirstOrDefault(item => item.Id == selectedId.Value);
            }
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

            var passwordValidationMessage = HashHelper.GetPasswordValidationError(NewUserPassword);
            if (!string.IsNullOrWhiteSpace(passwordValidationMessage))
            {
                MessageBox.Show(passwordValidationMessage, "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            ShowDialog(
                () => new UserCreateWindow(this),
                App.GetString("CreateAccountButton", "Создать аккаунт"));
            LoadData();
        }

        private void ShowEditUserForm()
        {
            ShowDialog(
                () => new UserEditWindow(this),
                App.GetString("EditButton", "Изменить"));
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

            SelectedUser.Login = SelectedUser.Login?.Trim();
            SelectedUser.Name = SelectedUser.Name?.Trim();

            if (SelectedUserRole == UserRole.Client)
            {
                var fullName = BuildSelectedClientFullName();
                SelectedUser.Name = fullName;
                SelectedUser.Email = NormalizeEmail(SelectedUserEmail);

                if (string.IsNullOrWhiteSpace(SelectedUserLastName) ||
                    string.IsNullOrWhiteSpace(SelectedUserFirstName) ||
                    string.IsNullOrWhiteSpace(SelectedUser.Login) ||
                    string.IsNullOrWhiteSpace(SelectedUser.Email))
                {
                    MessageBox.Show("Для клиента заполните фамилию, имя, логин и электронную почту.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!IsValidEmail(SelectedUser.Email))
                {
                    MessageBox.Show("Укажите корректный адрес электронной почты клиента.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existingEmailUser = _userRepository.GetAll()
                    .FirstOrDefault(user => string.Equals(user.Email, SelectedUser.Email, StringComparison.OrdinalIgnoreCase));
                if (existingEmailUser != null && existingEmailUser.Id != SelectedUser.Id)
                {
                    MessageBox.Show("Пользователь с такой электронной почтой уже существует.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else if (string.IsNullOrWhiteSpace(SelectedUser.Name) || string.IsNullOrWhiteSpace(SelectedUser.Login))
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
            SelectedOrder.MasterWorkCost = SelectedOrderMasterWorkCost;
            SelectedOrder.EstimatedRepairCost = SelectedOrder.EstimatedPartsCost + SelectedOrder.MasterWorkCost;
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

            ShowDialog(
                () => new AdminOrderEditWindow(this),
                App.GetString("EditOrderTitle", "Управление заявкой"));
            LoadData();
        }

        private void ShowCreateAdminOrderForm()
        {
            ResetNewOrderForm();
            ShowDialog(
                () => new AdminOrderCreateWindow(this),
                App.GetString("CreateOrderButton", "Создать заявку"));
            LoadData();
        }

        private void SelectProblemPhoto()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            NewOrderProblemPhoto = File.ReadAllBytes(openFileDialog.FileName);
            NewOrderProblemPhotoName = Path.GetFileName(openFileDialog.FileName);
        }

        private void RemoveProblemPhoto()
        {
            NewOrderProblemPhoto = null;
            NewOrderProblemPhotoName = string.Empty;
        }

        private void CreateAdminOrder()
        {
            WasLastAdminOrderCreationSuccessful = false;
            ShowAdminOrderValidationErrors = true;
            NotifyAdminOrderValidationStateChanged();

            if (string.IsNullOrWhiteSpace(NewOrderDeviceType) ||
                string.IsNullOrWhiteSpace(NewOrderDeliveryMethod) ||
                HasAdminOrderValidationErrors)
            {
                MessageBox.Show(
                    "Заполните обязательные поля заявки. Для email и телефона укажите корректные значения, а для курьерской доставки заполните адрес.",
                    "Ошибка проверки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
                ProblemPhoto = NewOrderProblemPhoto,
                PaymentMethod = NewOrderPaymentMethod,
                Status = OrderStatus.Created,
                CreatedAt = DateTime.Now
            };

            AssignBestMaster(order);
            _orderRepository.Add(order);
            ResetNewOrderForm();
            LoadData();
            SelectedOrder = Orders.FirstOrDefault(item => item.Id == order.Id);
            WasLastAdminOrderCreationSuccessful = true;
            MessageBox.Show("Заявка успешно оформлена администратором.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetNewOrderForm()
        {
            WasLastAdminOrderCreationSuccessful = false;
            ShowAdminOrderValidationErrors = false;
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
            NewOrderPaymentMethod = PaymentMethods[0];
            NewOrderProblemPhoto = null;
            NewOrderProblemPhotoName = string.Empty;
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

        private void ToggleEditWarehouseItem(object parameter)
        {
            var item = parameter as WarehouseItem ?? SelectedWarehouseItem;
            if (item == null)
            {
                MessageBox.Show("Выберите позицию склада.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedWarehouseItem = item;
            OpenWarehouseItemWindow(item);
        }

        private void SaveWarehouseItem()
        {
            OpenWarehouseItemWindow(SelectedWarehouseItem);
        }

        private void OpenWarehouseItemWindow(WarehouseItem item)
        {
            WarehouseItemWindow window;
            try
            {
                window = new WarehouseItemWindow(item);
            }
            catch (Exception ex)
            {
                ShowDialogOpenError(App.GetString("WarehouseTab", "Склад"), ex);
                return;
            }

            if (!TryShowDialog(window, App.GetString("WarehouseTab", "Склад")))
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

        private void DeleteWarehouseItem(object parameter)
        {
            var item = parameter as WarehouseItem ?? SelectedWarehouseItem;
            if (item == null)
            {
                MessageBox.Show("Выберите позицию склада.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedWarehouseItem = item;
            _warehouseRepository.Delete(item.Id);
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
            ShowDialog(
                () => new WarehouseRequestsWindow(this),
                App.GetString("MaterialRequestsTitle", "Запросы на материалы"));
            LoadData();
        }

        private void ShowDialog(Func<Window> windowFactory, string dialogTitle)
        {
            try
            {
                var window = windowFactory?.Invoke();
                if (window == null)
                {
                    return;
                }

                TryShowDialog(window, dialogTitle);
            }
            catch (Exception ex)
            {
                ShowDialogOpenError(dialogTitle, ex);
            }
        }

        private bool TryShowDialog(Window window, string dialogTitle)
        {
            try
            {
                if (Application.Current.MainWindow != null && !ReferenceEquals(window, Application.Current.MainWindow))
                {
                    window.Owner = Application.Current.MainWindow;
                }

                return window.ShowDialog() == true;
            }
            catch (Exception ex)
            {
                ShowDialogOpenError(dialogTitle, ex);
                return false;
            }
        }

        private static void ShowDialogOpenError(string dialogTitle, Exception ex)
        {
            var title = string.IsNullOrWhiteSpace(dialogTitle) ? "окно" : dialogTitle;
            MessageBox.Show(
                $"Не удалось открыть \"{title}\".{Environment.NewLine}{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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

        private void LoadSelectedClientFields(User user)
        {
            var nameParts = SplitFullName(user?.Name);
            SelectedUserLastName = nameParts[0];
            SelectedUserFirstName = nameParts[1];
            SelectedUserMiddleName = nameParts[2];
            SelectedUserEmail = user?.Email ?? string.Empty;
        }

        private void SyncSelectedUserPersonalFields()
        {
            if (_isLoadingSelectedUserData || SelectedUser == null)
            {
                return;
            }

            if (SelectedUserRole == UserRole.Client)
            {
                SelectedUser.Name = BuildSelectedClientFullName();
                SelectedUser.Email = SelectedUserEmail;
            }
        }

        private string BuildSelectedClientFullName()
        {
            return string.Join(" ", new[] { SelectedUserLastName, SelectedUserFirstName, SelectedUserMiddleName }
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part.Trim()));
        }

        private static string[] SplitFullName(string fullName)
        {
            var parts = (fullName ?? string.Empty)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return new[]
            {
                parts.Length > 0 ? parts[0] : string.Empty,
                parts.Length > 1 ? parts[1] : string.Empty,
                parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : string.Empty
            };
        }

        private static string NormalizeEmail(string email)
        {
            return string.IsNullOrWhiteSpace(email)
                ? string.Empty
                : email.Trim().ToLowerInvariant();
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

        private void NotifyAdminOrderValidationStateChanged()
        {
            OnPropertyChanged(nameof(IsNewOrderClientNameInvalid));
            OnPropertyChanged(nameof(IsNewOrderClientEmailInvalid));
            OnPropertyChanged(nameof(IsNewOrderContactPhoneInvalid));
            OnPropertyChanged(nameof(IsNewOrderDeviceBrandInvalid));
            OnPropertyChanged(nameof(IsNewOrderDeviceModelInvalid));
            OnPropertyChanged(nameof(IsNewOrderProblemDescriptionInvalid));
            OnPropertyChanged(nameof(IsNewOrderDeliveryAddressInvalid));
            OnPropertyChanged(nameof(HasAdminOrderValidationErrors));
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            RebuildWarehouseSortOptions();
            LoadData();
        }

        private void RebuildWarehouseSortOptions()
        {
            var previousSelection = _selectedWarehouseSortOption;
            WarehouseSortOptions.Clear();
            WarehouseSortOptions.Add(App.GetString("WarehouseSortByName", "By name"));
            WarehouseSortOptions.Add(App.GetString("WarehouseSortQuantityAsc", "Lower stock first"));
            WarehouseSortOptions.Add(App.GetString("WarehouseSortQuantityDesc", "Higher stock first"));
            WarehouseSortOptions.Add(App.GetString("WarehouseSortPriceAsc", "Cheaper first"));
            WarehouseSortOptions.Add(App.GetString("WarehouseSortPriceDesc", "More expensive first"));

            if (!string.IsNullOrWhiteSpace(previousSelection) && WarehouseSortOptions.Contains(previousSelection))
            {
                _selectedWarehouseSortOption = previousSelection;
                OnPropertyChanged(nameof(SelectedWarehouseSortOption));
            }
            else if (WarehouseSortOptions.Count > 0)
            {
                _selectedWarehouseSortOption = WarehouseSortOptions[0];
                OnPropertyChanged(nameof(SelectedWarehouseSortOption));
            }
        }

        private static string GetAllCategoriesLabel()
        {
            return App.GetString("WarehouseAllCategories", "All categories");
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

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

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            var normalizedPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return Regex.IsMatch(normalizedPhone, @"^(?:375|80)(17|25|29|33|44)\d{7}$");
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

    public class OrderStatusChartSlice
    {
        public string Label { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public Brush Fill { get; set; }
        public Geometry Geometry { get; set; }
        public string PercentageText => $"{Percentage:P0}";
        public string SummaryText => string.Format(App.GetString("ChartItemsCountFormat", "{0} pcs."), Count);
    }

    public class OrderStatusChartDefinition
    {
        public OrderStatusChartDefinition(OrderStatus status, string resourceKey, string fallbackLabel, string colorHex)
        {
            Status = status;
            ResourceKey = resourceKey;
            FallbackLabel = fallbackLabel;
            ColorHex = colorHex;
        }

        public OrderStatus Status { get; }
        public string ResourceKey { get; }
        public string FallbackLabel { get; }
        public string ColorHex { get; }
    }
}
