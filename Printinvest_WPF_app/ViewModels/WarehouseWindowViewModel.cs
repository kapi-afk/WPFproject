using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class WarehouseWindowViewModel : BaseViewModel
    {
        private readonly WarehouseRepository _warehouseRepository;
        private readonly Order _targetOrder;
        private List<WarehouseItem> _allItems;
        private ObservableCollection<WarehouseItem> _warehouseItems;
        private WarehouseItem _selectedWarehouseItem;
        private string _searchText;
        private string _selectedSortOption;
        private string _selectedCategoryFilter;
        private string _materialUsageQuantity = "1";

        public ObservableCollection<WarehouseItem> WarehouseItems
        {
            get => _warehouseItems;
            set => SetProperty(ref _warehouseItems, value);
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "По названию",
            "По остатку: меньше сначала",
            "По остатку: больше сначала",
            "По цене: дешевле сначала",
            "По цене: дороже сначала"
        };

        public ObservableCollection<string> CategoryFilters { get; } = new ObservableCollection<string>();

        public WarehouseItem SelectedWarehouseItem
        {
            get => _selectedWarehouseItem;
            set => SetProperty(ref _selectedWarehouseItem, value);
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

        public string TargetOrderText => _targetOrder == null
            ? "Заявка не выбрана. Можно искать и просматривать материалы."
            : $"Материалы для заявки №{_targetOrder.Id}: {_targetOrder.DeviceType} {_targetOrder.DeviceBrand} {_targetOrder.DeviceModel}";

        public ICommand RefreshCommand { get; }
        public ICommand ConsumeWarehouseItemCommand { get; }

        public WarehouseWindowViewModel(Order targetOrder)
        {
            _warehouseRepository = RepositoryManager.Warehouse;
            _targetOrder = targetOrder;
            _allItems = new List<WarehouseItem>();
            WarehouseItems = new ObservableCollection<WarehouseItem>();

            RefreshCommand = new RelayCommand(LoadData);
            ConsumeWarehouseItemCommand = new RelayCommand(ConsumeWarehouseItem);

            LoadData();
        }

        private void LoadData()
        {
            _allItems = _warehouseRepository.GetAll();

            var selectedCategory = SelectedCategoryFilter;
            CategoryFilters.Clear();
            CategoryFilters.Add("Все категории");

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

            if (string.IsNullOrWhiteSpace(SelectedSortOption))
            {
                SelectedSortOption = SortOptions[0];
            }
            else
            {
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            IEnumerable<WarehouseItem> items = _allItems;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                items = items.Where(item =>
                    (item.Name?.ToLowerInvariant().Contains(search) ?? false) ||
                    (item.Category?.ToLowerInvariant().Contains(search) ?? false) ||
                    (item.Notes?.ToLowerInvariant().Contains(search) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(SelectedCategoryFilter) && SelectedCategoryFilter != "Все категории")
            {
                items = items.Where(item => item.Category == SelectedCategoryFilter);
            }

            switch (SelectedSortOption)
            {
                case "По остатку: меньше сначала":
                    items = items.OrderBy(item => item.Quantity).ThenBy(item => item.Name);
                    break;
                case "По остатку: больше сначала":
                    items = items.OrderByDescending(item => item.Quantity).ThenBy(item => item.Name);
                    break;
                case "По цене: дешевле сначала":
                    items = items.OrderBy(item => item.UnitPrice).ThenBy(item => item.Name);
                    break;
                case "По цене: дороже сначала":
                    items = items.OrderByDescending(item => item.UnitPrice).ThenBy(item => item.Name);
                    break;
                default:
                    items = items.OrderBy(item => item.Name);
                    break;
            }

            var selectedId = SelectedWarehouseItem?.Id;
            WarehouseItems = new ObservableCollection<WarehouseItem>(items.ToList());
            if (selectedId.HasValue)
            {
                SelectedWarehouseItem = WarehouseItems.FirstOrDefault(item => item.Id == selectedId.Value);
            }
        }

        private void ConsumeWarehouseItem()
        {
            if (_targetOrder == null)
            {
                MessageBox.Show("Сначала выберите заявку в панели мастера, а затем откройте склад.", "Заявка не выбрана", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedWarehouseItem == null)
            {
                MessageBox.Show("Выберите материал для списания.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(MaterialUsageQuantity, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Укажите корректное количество.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var actualItem = _warehouseRepository.GetById(SelectedWarehouseItem.Id);
            if (actualItem == null)
            {
                MessageBox.Show("Позиция склада не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (actualItem.Quantity < quantity)
            {
                MessageBox.Show("На складе недостаточно материала.", "Недостаточно остатка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            actualItem.Quantity -= quantity;
            _warehouseRepository.Update(actualItem);
            MaterialUsageQuantity = "1";
            LoadData();
            SelectedWarehouseItem = WarehouseItems.FirstOrDefault(item => item.Id == actualItem.Id);

            MessageBox.Show($"Материал списан на заявку №{_targetOrder.Id}.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
