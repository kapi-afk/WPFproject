using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Printinvest_WPF_app.ViewModels
{
    public class CatalogViewModel : BaseViewModel
    {
        private readonly ProductRepository _productRepository;
        private readonly ServiceRepository _serviceRepository;
        private readonly CartRepository _cartRepository;
        private ObservableCollection<IItem> _items;
        private string _searchQuery;
        private string _selectedFilter;

        public ObservableCollection<IItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                SetProperty(ref _searchQuery, value);
                LoadItemsCommand?.Execute(null);
            }
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                SetProperty(ref _selectedFilter, value);
                LoadItemsCommand?.Execute(null);
            }
        }

        public ObservableCollection<string> Filters { get; } = new ObservableCollection<string>
        {
            "All", "Products", "Services"
        };

        public ICommand LoadItemsCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand ViewItemDetailsCommand { get; }

        public CatalogViewModel()
        {
            _productRepository = RepositoryManager.Products;
            _serviceRepository = RepositoryManager.Services;
            _cartRepository = RepositoryManager.Carts;

            Items = new ObservableCollection<IItem>();
            LoadItemsCommand = new RelayCommand(LoadItems);
            ClearSearchCommand = new RelayCommand(() => SearchQuery = string.Empty);
            AddToCartCommand = new RelayCommandSec(ExecuteAddToCart);
            ViewItemDetailsCommand = new RelayCommandSec(ExecuteViewItemDetails);
            SelectedFilter = "All";
        }

        public void Initialize()
        {
            LoadItems();
        }

        private void LoadItems()
        {
            try
            {
                if (_productRepository == null || _serviceRepository == null)
                {
                    //MessageBox.Show("Репозитории не инициализированы. Пожалуйста, перезапустите приложение.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Items.Clear();
                var searchLower = string.IsNullOrWhiteSpace(_searchQuery) ? null : _searchQuery.ToLower();

                if (_selectedFilter == "All" || _selectedFilter == "Products")
                {
                    var products = _productRepository.GetAll();
                    foreach (var product in products)
                    {
                        if (searchLower == null || product.Name.ToLower().Contains(searchLower))
                        {
                            Items.Add(product);
                        }
                    }
                }

                if (_selectedFilter == "All" || _selectedFilter == "Services")
                {
                    var services = _serviceRepository.GetAll();
                    foreach (var service in services)
                    {
                        if (searchLower == null || service.Name.ToLower().Contains(searchLower))
                        {
                            Items.Add(service);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка загрузки каталога: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddToCart(object parameter)
        {
            try
            {
                if (parameter is IItem item)
                {
                    if (SessionManager.CurrentUser == null)
                    {
                        MessageBox.Show("Пожалуйста, войдите в систему для добавления в корзину.", "Авторизация требуется", MessageBoxButton.OK, MessageBoxImage.Warning);
                        var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
                        mainViewModel.CurrentPage = new LoginPage();
                        return;
                    }

                    var cart = _cartRepository.GetByUserId(SessionManager.CurrentUser.Id);
                    if (cart == null)
                    {
                        cart = new Cart
                        {
                            UserId = SessionManager.CurrentUser.Id,
                            User = SessionManager.CurrentUser,
                            Items = new List<CartItem>()
                        };
                        _cartRepository.Add(cart);
                    }

                    var existingItem = cart.Items.FirstOrDefault(i =>
                        (item is Product && i.ProductId == item.Id) ||
                        (item is Service && i.ServiceId == item.Id));

                    if (existingItem != null)
                    {
                        existingItem.Quantity++;
                    }
                    else
                    {
                        CartItem cartItem;
                        if (item is Product)
                        {
                            cartItem = new CartItem
                            {
                                CartId = cart.Id,
                                ProductId = item.Id ,
                               
                                Product = item is Product ? (Product)item : null,
                                Service = item is Service ? (Service)item : null,
                                Quantity = 1
                            };
                        }
                        else
                        {
                            cartItem = new CartItem
                            {
                                CartId = cart.Id,
                                
                                ServiceId = item.Id,
                                Product = item is Product ? (Product)item : null,
                                Service = item is Service ? (Service)item : null,
                                Quantity = 1
                            };
                        }
                        _cartRepository.AddItemToCart(cart.Id, cartItem);
                    }

                    _cartRepository.Update(cart);
                    MessageBox.Show($"Товар '{item.Name}' добавлен в корзину!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteViewItemDetails(object parameter)
        {
            try
            {
                if (parameter is System.Windows.Input.MouseButtonEventArgs e)
                {
                    var listViewItem = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
                    if (listViewItem == null)
                    {
                        MessageBox.Show("Не удалось найти элемент списка.", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    if (listViewItem.DataContext is Product product)
                    {
                        var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
                        if (mainViewModel == null)
                        {
                            MessageBox.Show("Главная модель представления недоступна.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var viewModel = new ItemDetailsViewModel(product);
                        var page = new ItemDetailsPage { DataContext = viewModel };
                        viewModel.Initialize(); // Синхронная инициализация
                        mainViewModel.CurrentPage = page;
                    }
                    else
                    {
                        MessageBox.Show("Детали доступны только для товаров.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Некорректный параметр события.", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии страницы товара: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null && !(current is T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }
    }
}