using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Views;
using Printinvest_WPF_app.Views.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class ServiceMasterPanelViewModel : BaseViewModel
    {
        private readonly OrderRepository _orderRepository;
        private readonly WarehouseRequestRepository _warehouseRequestRepository;
        private List<WarehouseRequest> _allMasterWarehouseRequests;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<WarehouseRequest> _currentOrderWarehouseRequests;
        private Order _selectedOrder;
        private OrderStatus _selectedOrderStatus;
        private decimal _selectedOrderMasterWorkCost;
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
            _allMasterWarehouseRequests = new List<WarehouseRequest>();
            Orders = new ObservableCollection<Order>();
            CurrentOrderWarehouseRequests = new ObservableCollection<WarehouseRequest>();

            RefreshCommand = new RelayCommand(LoadData);
            UpdateOrderStatusCommand = new RelayCommand(UpdateOrderStatus);
            LogoutCommand = new RelayCommand(Logout);
            OpenWarehouseWindowCommand = new RelayCommand(OpenWarehouseWindow);

            LoadData();
        }

        private void LoadData()
        {
            if (!SessionManager.IsAuthenticated || !SessionManager.IsMaster)
            {
                Orders.Clear();
                _allMasterWarehouseRequests.Clear();
                CurrentOrderWarehouseRequests.Clear();
                CompletedOrdersCount = 0;
                ActiveOrdersCount = 0;
                return;
            }

            Orders = new ObservableCollection<Order>(_orderRepository.GetByAssignedMasterId(SessionManager.CurrentUser.Id));
            CompletedOrdersCount = Orders.Count(order => order.Status == OrderStatus.Completed);
            ActiveOrdersCount = Orders.Count(order => order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled);
            if (SelectedOrder != null)
            {
                SelectedOrder = Orders.FirstOrDefault(order => order.Id == SelectedOrder.Id);
            }
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

            var warehouseWindow = new WarehouseWindow(SelectedOrder);
            if (Application.Current.MainWindow != null)
            {
                warehouseWindow.Owner = Application.Current.MainWindow;
            }

            warehouseWindow.ShowDialog();
            LoadData();
        }

        private void LoadCurrentOrderWarehouseRequests()
        {
            if (Orders == null || !Orders.Any())
            {
                _allMasterWarehouseRequests = new List<WarehouseRequest>();
                CurrentOrderWarehouseRequests = new ObservableCollection<WarehouseRequest>();
                return;
            }

            var orderIds = Orders.Select(order => order.Id).ToList();
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
