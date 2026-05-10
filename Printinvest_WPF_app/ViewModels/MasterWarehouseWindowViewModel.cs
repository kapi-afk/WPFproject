using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class MasterWarehouseWindowViewModel : BaseViewModel
    {
        private readonly WarehouseRepository _warehouseRepository;
        private readonly WarehouseRequestRepository _warehouseRequestRepository;
        private readonly OrderRepository _orderRepository;
        private readonly Order _targetOrder;
        private List<WarehouseItem> _allItems;
        private ObservableCollection<WarehouseItem> _warehouseItems;
        private WarehouseItem _selectedWarehouseItem;
        private string _searchText;
        private string _selectedSortOption;
        private string _selectedCategoryFilter;
        private string _materialUsageQuantity = "1";
        private string _requestMaterialName;
        private string _requestMaterialCategory;

        public MasterWarehouseWindowViewModel(Order targetOrder)
        {
            _warehouseRepository = RepositoryManager.Warehouse;
            _warehouseRequestRepository = RepositoryManager.WarehouseRequests;
            _orderRepository = RepositoryManager.Orders;
            _targetOrder = targetOrder;
            _allItems = new List<WarehouseItem>();
            WarehouseItems = new ObservableCollection<WarehouseItem>();

            RefreshCommand = new RelayCommand(LoadData);
            ConsumeWarehouseItemCommand = new RelayCommand(ConsumeWarehouseItem);
            RequestMaterialCommand = new RelayCommand(() => CreateMaterialRequest());

            RebuildSortOptions();
            App.LanguageChanged += OnLanguageChanged;
            LoadData();
        }

        public ObservableCollection<WarehouseItem> WarehouseItems
        {
            get => _warehouseItems;
            set => SetProperty(ref _warehouseItems, value);
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> CategoryFilters { get; } = new ObservableCollection<string>();

        public WarehouseItem SelectedWarehouseItem
        {
            get => _selectedWarehouseItem;
            set
            {
                if (SetProperty(ref _selectedWarehouseItem, value) && value != null)
                {
                    RequestMaterialName = value.Name;
                    RequestMaterialCategory = value.Category;
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (SetProperty(ref _selectedSortOption, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SelectedCategoryFilter
        {
            get => _selectedCategoryFilter;
            set
            {
                if (SetProperty(ref _selectedCategoryFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string MaterialUsageQuantity
        {
            get => _materialUsageQuantity;
            set => SetProperty(ref _materialUsageQuantity, value);
        }

        public string RequestMaterialName
        {
            get => _requestMaterialName;
            set => SetProperty(ref _requestMaterialName, value);
        }

        public string RequestMaterialCategory
        {
            get => _requestMaterialCategory;
            set => SetProperty(ref _requestMaterialCategory, value);
        }

        public string TargetOrderEstimatedTotalText => _targetOrder == null
            ? "0,00 BYN"
            : $"{_targetOrder.EstimatedRepairCost:N2} BYN";

        public string LocalizedTargetOrderText => _targetOrder == null
            ? App.GetString("MaterialRequestNoOrderText", "No order selected. You can only browse the warehouse.")
            : string.Format(
                CultureInfo.CurrentCulture,
                App.GetString("MaterialRequestTargetOrderFormat", "Materials for order No. {0}: {1} {2} {3}"),
                _targetOrder.Id,
                _targetOrder.DeviceType,
                _targetOrder.DeviceBrand,
                _targetOrder.DeviceModel);

        public ICommand RefreshCommand { get; }
        public ICommand ConsumeWarehouseItemCommand { get; }
        public ICommand RequestMaterialCommand { get; }

        private static string AllCategoriesLabel => App.GetString("WarehouseAllCategories", "All categories");

        private void LoadData()
        {
            _allItems = _warehouseRepository.GetAll();

            var selectedCategory = SelectedCategoryFilter;
            CategoryFilters.Clear();
            CategoryFilters.Add(AllCategoriesLabel);

            foreach (var category in _allItems
                .Select(item => item.Category)
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct()
                .OrderBy(category => category))
            {
                CategoryFilters.Add(category);
            }

            if (string.IsNullOrWhiteSpace(selectedCategory) || !CategoryFilters.Contains(selectedCategory))
            {
                SelectedCategoryFilter = CategoryFilters[0];
            }
            else
            {
                _selectedCategoryFilter = selectedCategory;
                OnPropertyChanged(nameof(SelectedCategoryFilter));
            }

            if (string.IsNullOrWhiteSpace(SelectedSortOption) || !SortOptions.Contains(SelectedSortOption))
            {
                SelectedSortOption = SortOptions.FirstOrDefault();
            }
            else
            {
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            IEnumerable<WarehouseItem> items = _allItems ?? Enumerable.Empty<WarehouseItem>();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                items = items.Where(item =>
                    (item.Name?.ToLowerInvariant().Contains(search) ?? false) ||
                    (item.Category?.ToLowerInvariant().Contains(search) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(SelectedCategoryFilter) && SelectedCategoryFilter != AllCategoriesLabel)
            {
                items = items.Where(item => item.Category == SelectedCategoryFilter);
            }

            if (SelectedSortOption == App.GetString("WarehouseSortQuantityAsc", "Lower stock first"))
            {
                items = items.OrderBy(item => item.Quantity).ThenBy(item => item.Name);
            }
            else if (SelectedSortOption == App.GetString("WarehouseSortQuantityDesc", "Higher stock first"))
            {
                items = items.OrderByDescending(item => item.Quantity).ThenBy(item => item.Name);
            }
            else if (SelectedSortOption == App.GetString("WarehouseSortPriceAsc", "Cheaper first"))
            {
                items = items.OrderBy(item => item.UnitPrice).ThenBy(item => item.Name);
            }
            else if (SelectedSortOption == App.GetString("WarehouseSortPriceDesc", "More expensive first"))
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

        public void PrepareMaterialRequest()
        {
            if (SelectedWarehouseItem == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(RequestMaterialName))
            {
                RequestMaterialName = SelectedWarehouseItem.Name;
            }

            if (string.IsNullOrWhiteSpace(RequestMaterialCategory))
            {
                RequestMaterialCategory = SelectedWarehouseItem.Category;
            }
        }

        public bool CreateMaterialRequest()
        {
            if (_targetOrder == null)
            {
                MessageBox.Show("Сначала выберите заявку в панели мастера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(MaterialUsageQuantity, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Укажите корректное количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var requestedName = string.IsNullOrWhiteSpace(RequestMaterialName)
                ? SelectedWarehouseItem?.Name
                : RequestMaterialName.Trim();

            if (string.IsNullOrWhiteSpace(requestedName))
            {
                MessageBox.Show("Напишите, какой материал нужен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (requestedName.Length > 120)
            {
                MessageBox.Show("Название материала должно быть не длиннее 120 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var requestedCategory = string.IsNullOrWhiteSpace(RequestMaterialCategory)
                ? SelectedWarehouseItem?.Category
                : RequestMaterialCategory.Trim();

            if (!string.IsNullOrEmpty(requestedCategory) && requestedCategory.Length > 80)
            {
                MessageBox.Show("Категория должна быть не длиннее 80 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            WarehouseItem actualItem = null;
            if (IsRequestForSelectedWarehouseItem(requestedName, requestedCategory))
            {
                actualItem = _warehouseRepository.GetById(SelectedWarehouseItem.Id);
            }

            var request = new WarehouseRequest
            {
                OrderId = _targetOrder.Id,
                MasterId = SessionManager.CurrentUser?.Id,
                WarehouseItemId = actualItem?.Id,
                RequestedItemName = requestedName,
                RequestedCategory = requestedCategory,
                RequestedQuantity = quantity,
                Status = actualItem == null || actualItem.Quantity < quantity ? "Новая заявка" : "Запрошено",
                CreatedAt = DateTime.Now
            };

            _warehouseRequestRepository.Add(request);
            MoveTargetOrderToWaitingForParts();
            MaterialUsageQuantity = "1";
            RequestMaterialName = string.Empty;
            RequestMaterialCategory = string.Empty;

            MessageBox.Show("Заявка на материал отправлена администратору.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        private bool IsRequestForSelectedWarehouseItem(string requestedName, string requestedCategory)
        {
            if (SelectedWarehouseItem == null)
            {
                return false;
            }

            var selectedCategory = SelectedWarehouseItem.Category ?? string.Empty;
            return string.Equals(SelectedWarehouseItem.Name, requestedName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(selectedCategory, requestedCategory ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private void ConsumeWarehouseItem()
        {
            if (_targetOrder == null)
            {
                MessageBox.Show("Сначала выберите заявку в панели мастера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedWarehouseItem == null)
            {
                MessageBox.Show("Выберите материал для списания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(MaterialUsageQuantity, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Укажите корректное количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var actualItem = _warehouseRepository.GetById(SelectedWarehouseItem.Id);
            if (actualItem == null)
            {
                MessageBox.Show("Материал не найден на складе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (actualItem.Quantity < quantity)
            {
                MessageBox.Show("На складе недостаточно материала. Создайте заявку для администратора.", "Недостаточно материала", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            actualItem.Quantity -= quantity;
            _warehouseRepository.Update(actualItem);
            AddMaterialCostToTargetOrder(actualItem, quantity);
            MoveTargetOrderToInProgress();
            MaterialUsageQuantity = "1";
            LoadData();
            SelectedWarehouseItem = WarehouseItems.FirstOrDefault(item => item.Id == actualItem.Id);

            MessageBox.Show($"Материал списан на заявку №{_targetOrder.Id}.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MoveTargetOrderToWaitingForParts()
        {
            if (_targetOrder == null || IsFinalOrderStatus(_targetOrder.Status))
            {
                return;
            }

            var orderToUpdate = _orderRepository.GetById(_targetOrder.Id);
            if (orderToUpdate == null || IsFinalOrderStatus(orderToUpdate.Status))
            {
                return;
            }

            orderToUpdate.Status = OrderStatus.WaitingForParts;
            _orderRepository.Update(orderToUpdate);
            SynchronizeTargetOrder(orderToUpdate);
        }

        private void MoveTargetOrderToInProgress()
        {
            if (_targetOrder == null ||
                IsFinalOrderStatus(_targetOrder.Status) ||
                _targetOrder.Status == OrderStatus.ReadyForPickup)
            {
                return;
            }

            var orderToUpdate = _orderRepository.GetById(_targetOrder.Id);
            if (orderToUpdate == null ||
                IsFinalOrderStatus(orderToUpdate.Status) ||
                orderToUpdate.Status == OrderStatus.ReadyForPickup)
            {
                return;
            }

            orderToUpdate.Status = OrderStatus.InProgress;
            _orderRepository.Update(orderToUpdate);
            SynchronizeTargetOrder(orderToUpdate);
        }

        private static bool IsFinalOrderStatus(OrderStatus status)
        {
            return status == OrderStatus.Completed || status == OrderStatus.Cancelled;
        }

        private void AddMaterialCostToTargetOrder(WarehouseItem actualItem, int quantity)
        {
            if (_targetOrder == null || actualItem == null || quantity <= 0)
            {
                return;
            }

            var orderToUpdate = _orderRepository.GetById(_targetOrder.Id);
            if (orderToUpdate == null)
            {
                return;
            }

            orderToUpdate.EstimatedPartsCost += actualItem.UnitPrice * quantity;
            orderToUpdate.EstimatedRepairCost = orderToUpdate.EstimatedPartsCost + orderToUpdate.MasterWorkCost;
            _orderRepository.Update(orderToUpdate);
            SynchronizeTargetOrder(orderToUpdate);
        }

        private void SynchronizeTargetOrder(Order sourceOrder)
        {
            if (_targetOrder == null || sourceOrder == null)
            {
                return;
            }

            _targetOrder.Status = sourceOrder.Status;
            _targetOrder.EstimatedPartsCost = sourceOrder.EstimatedPartsCost;
            _targetOrder.MasterWorkCost = sourceOrder.MasterWorkCost;
            _targetOrder.EstimatedRepairCost = sourceOrder.EstimatedRepairCost;
            _targetOrder.CompletedAt = sourceOrder.CompletedAt;
            OnPropertyChanged(nameof(TargetOrderEstimatedTotalText));
            OnPropertyChanged(nameof(LocalizedTargetOrderText));
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            RebuildSortOptions();
            OnPropertyChanged(nameof(LocalizedTargetOrderText));
            OnPropertyChanged(nameof(TargetOrderEstimatedTotalText));
            LoadData();
        }

        private void RebuildSortOptions()
        {
            var previousSelection = _selectedSortOption;
            SortOptions.Clear();
            SortOptions.Add(App.GetString("WarehouseSortByName", "By name"));
            SortOptions.Add(App.GetString("WarehouseSortQuantityAsc", "Lower stock first"));
            SortOptions.Add(App.GetString("WarehouseSortQuantityDesc", "Higher stock first"));
            SortOptions.Add(App.GetString("WarehouseSortPriceAsc", "Cheaper first"));
            SortOptions.Add(App.GetString("WarehouseSortPriceDesc", "More expensive first"));

            if (!string.IsNullOrWhiteSpace(previousSelection) && SortOptions.Contains(previousSelection))
            {
                _selectedSortOption = previousSelection;
                OnPropertyChanged(nameof(SelectedSortOption));
            }
            else if (SortOptions.Count > 0)
            {
                _selectedSortOption = SortOptions[0];
                OnPropertyChanged(nameof(SelectedSortOption));
            }
        }
    }
}
