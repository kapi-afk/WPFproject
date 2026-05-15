using ServiceCenter.Models;
using ServiceCenter.Repositories;
using ServiceCenter.Views;
using ServiceCenter.Views.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenter.ViewModels
{
    public class ServiceMasterPanelViewModel : BaseViewModel
    {
        private readonly OrderRepository _orderRepository;
        private readonly WarehouseRequestRepository _warehouseRequestRepository;
        private List<Order> _allOrders;
        private List<WarehouseRequest> _allMasterWarehouseRequests;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<WarehouseRequest> _currentOrderWarehouseRequests;
        private Order _selectedOrder;
        private OrderStatus _selectedOrderStatus;
        private decimal _selectedOrderMasterWorkCost;
        private string _orderSearchText;
        private string _materialRequestSearchText;
        private int _completedOrdersCount;
        private int _activeOrdersCount;

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public ObservableCollection<WarehouseRequest> CurrentOrderWarehouseRequests
        {
            get => _currentOrderWarehouseRequests;
            set => SetProperty(ref _currentOrderWarehouseRequests, value);
        }

        public string MaterialRequestSearchText
        {
            get => _materialRequestSearchText;
            set
            {
                if (SetProperty(ref _materialRequestSearchText, value))
                {
                    ApplyMaterialRequestFilter();
                }
            }
        }

        public string OrderSearchText
        {
            get => _orderSearchText;
            set
            {
                if (SetProperty(ref _orderSearchText, value))
                {
                    ApplyOrderFilter();
                }
            }
        }

        public ObservableCollection<OrderStatus> OrderStatuses { get; } = new ObservableCollection<OrderStatus>
        {
            OrderStatus.Assigned,
            OrderStatus.Diagnosing,
            OrderStatus.WaitingForParts,
            OrderStatus.InProgress,
            OrderStatus.ReadyForPickup,
            OrderStatus.Completed,
            OrderStatus.Cancelled
        };

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);
                SelectedOrderStatus = value?.Status ?? OrderStatus.Assigned;
                SelectedOrderMasterWorkCost = value?.MasterWorkCost ?? 0;
                LoadCurrentOrderWarehouseRequests();
            }
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

        public int CompletedOrdersCount
        {
            get => _completedOrdersCount;
            set => SetProperty(ref _completedOrdersCount, value);
        }

        public int ActiveOrdersCount
        {
            get => _activeOrdersCount;
            set => SetProperty(ref _activeOrdersCount, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand UpdateOrderStatusCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand OpenWarehouseWindowCommand { get; }

        public ServiceMasterPanelViewModel()
        {
            _orderRepository = RepositoryManager.Orders;
            _warehouseRequestRepository = RepositoryManager.WarehouseRequests;
            _allOrders = new List<Order>();
            _allMasterWarehouseRequests = new List<WarehouseRequest>();
            Orders = new ObservableCollection<Order>();
            CurrentOrderWarehouseRequests = new ObservableCollection<WarehouseRequest>();

            RefreshCommand = new RelayCommand(LoadData);
            UpdateOrderStatusCommand = new RelayCommand(UpdateOrderStatus);
            LogoutCommand = new RelayCommand(Logout);
            OpenWarehouseWindowCommand = new RelayCommand(OpenWarehouseWindow);
            App.LanguageChanged += OnLanguageChanged;

            LoadData();
        }

        private void LoadData()
        {
            if (!SessionManager.IsAuthenticated || !SessionManager.IsMaster)
            {
                _allOrders.Clear();
                Orders.Clear();
                _allMasterWarehouseRequests.Clear();
                CurrentOrderWarehouseRequests.Clear();
                CompletedOrdersCount = 0;
                ActiveOrdersCount = 0;
                return;
            }

            _allOrders = _orderRepository.GetByAssignedMasterId(SessionManager.CurrentUser.Id);
            CompletedOrdersCount = _allOrders.Count(order => order.Status == OrderStatus.Completed);
            ActiveOrdersCount = _allOrders.Count(order => order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled);

            ApplyOrderFilter();
            LoadCurrentOrderWarehouseRequests();
        }

        private void UpdateOrderStatus()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Select an order.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedOrder.MasterWorkCost = SelectedOrderMasterWorkCost;
            SelectedOrder.EstimatedRepairCost = SelectedOrder.EstimatedPartsCost + SelectedOrder.MasterWorkCost;
            SelectedOrder.Status = SelectedOrderStatus;
            _orderRepository.Update(SelectedOrder);
            LoadData();
            MessageBox.Show(
                "Изменения по заявке сохранены. Уведомление клиенту отправляется в фоне.",
                "Готово",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OpenWarehouseWindow()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Выберите заявку, для которой нужны материалы.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var warehouseWindow = new WarehouseWindow(SelectedOrder);
                if (Application.Current.MainWindow != null)
                {
                    warehouseWindow.Owner = Application.Current.MainWindow;
                }

                warehouseWindow.ShowDialog();
                LoadData();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось открыть окно склада.{System.Environment.NewLine}{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadCurrentOrderWarehouseRequests()
        {
            if (_allOrders == null || !_allOrders.Any())
            {
                _allMasterWarehouseRequests = new List<WarehouseRequest>();
                CurrentOrderWarehouseRequests = new ObservableCollection<WarehouseRequest>();
                return;
            }

            var orderIds = _allOrders.Select(order => order.Id).ToList();
            _allMasterWarehouseRequests = _warehouseRequestRepository.GetAll()
                .Where(request => orderIds.Contains(request.OrderId))
                .ToList();

            ApplyMaterialRequestFilter();
        }

        private void ApplyMaterialRequestFilter()
        {
            IEnumerable<WarehouseRequest> requests = _allMasterWarehouseRequests;

            if (!string.IsNullOrWhiteSpace(MaterialRequestSearchText))
            {
                var search = MaterialRequestSearchText.ToLowerInvariant();
                requests = requests.Where(request =>
                    (request.OrderDisplayNumber?.ToLowerInvariant().Contains(search) ?? false));
            }

            CurrentOrderWarehouseRequests = new ObservableCollection<WarehouseRequest>(requests);
        }

        private void ApplyOrderFilter()
        {
            var selectedOrderId = SelectedOrder?.Id;
            IEnumerable<Order> orders = _allOrders ?? Enumerable.Empty<Order>();

            if (!string.IsNullOrWhiteSpace(OrderSearchText))
            {
                var search = OrderSearchText.Trim().ToLowerInvariant();
                orders = orders.Where(order =>
                    (order.DisplayNumber?.ToLowerInvariant().Contains(search) ?? false) ||
                    (order.User?.Name?.ToLowerInvariant().Contains(search) ?? false) ||
                    (order.DeviceType?.ToLowerInvariant().Contains(search) ?? false) ||
                    (order.DeviceBrand?.ToLowerInvariant().Contains(search) ?? false) ||
                    (order.DeviceModel?.ToLowerInvariant().Contains(search) ?? false) ||
                    (order.ProblemDescription?.ToLowerInvariant().Contains(search) ?? false));
            }

            var filteredOrders = orders
                .OrderBy(GetOrderStatusPriority)
                .ThenByDescending(order => order.UpdatedAt ?? order.CreatedAt)
                .ThenByDescending(order => order.Id)
                .ToList();

            Orders = new ObservableCollection<Order>(filteredOrders);

            var nextSelectedOrder = selectedOrderId.HasValue
                ? Orders.FirstOrDefault(order => order.Id == selectedOrderId.Value)
                : Orders.FirstOrDefault();

            if (!ReferenceEquals(SelectedOrder, nextSelectedOrder))
            {
                SelectedOrder = nextSelectedOrder;
            }
            else if (nextSelectedOrder == null)
            {
                LoadCurrentOrderWarehouseRequests();
            }
        }

        private static int GetOrderStatusPriority(Order order)
        {
            if (order == null)
            {
                return int.MaxValue;
            }

            switch (order.Status)
            {
                case OrderStatus.Assigned:
                    return 0;
                case OrderStatus.Diagnosing:
                    return 1;
                case OrderStatus.WaitingForParts:
                    return 2;
                case OrderStatus.InProgress:
                    return 3;
                case OrderStatus.ReadyForPickup:
                    return 4;
                case OrderStatus.Completed:
                    return 5;
                case OrderStatus.Cancelled:
                    return 6;
                default:
                    return 7;
            }
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

        private void OnLanguageChanged(object sender, System.EventArgs e)
        {
            LoadData();
        }
    }
}
