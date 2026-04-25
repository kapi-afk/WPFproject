using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace Printinvest_WPF_app.ViewModels
{
    public class AdminPanelViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly ProductRepository _productRepository;
        private readonly ServiceRepository _serviceRepository;

        private ObservableCollection<User> _users;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Service> _services;
        private User _selectedUser;
        private Product _selectedProduct;
        private Service _selectedService;
        private bool _isAddingProduct;
        private bool _isAddingService;

        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<Service> Services
        {
            get => _services;
            set => SetProperty(ref _services, value);
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public Service SelectedService
        {
            get => _selectedService;
            set => SetProperty(ref _selectedService, value);
        }

        public bool IsAddingProduct
        {
            get => _isAddingProduct;
            set => SetProperty(ref _isAddingProduct, value);
        }

        public bool IsAddingService
        {
            get => _isAddingService;
            set => SetProperty(ref _isAddingService, value);
        }

        public ICommand LoadDataCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand DeleteServiceCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand AddServiceCommand { get; }
        public ICommand SaveProductCommand { get; }
        public ICommand SaveServiceCommand { get; }
        public ICommand CancelAddCommand { get; }
        public ICommand LoadProductPhotoCommand { get; }
        public ICommand LoadServicePhotoCommand { get; }

        public AdminPanelViewModel()
        {
            _userRepository = RepositoryManager.Users;
            _productRepository = RepositoryManager.Products;
            _serviceRepository = RepositoryManager.Services;

            Users = new ObservableCollection<User>();
            Products = new ObservableCollection<Product>();
            Services = new ObservableCollection<Service>();

            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            DeleteUserCommand = new RelayCommand(async () => await DeleteUserAsync());
            DeleteProductCommand = new RelayCommand(async () => await DeleteProductAsync());
            DeleteServiceCommand = new RelayCommand(async () => await DeleteServiceAsync());
            AddProductCommand = new RelayCommand(() => { IsAddingProduct = true; SelectedProduct = new Product(); });
            AddServiceCommand = new RelayCommand(() => { IsAddingService = true; SelectedService = new Service(); });
            SaveProductCommand = new RelayCommand(async () => await SaveProductAsync());
            SaveServiceCommand = new RelayCommand(async () => await SaveServiceAsync());
            CancelAddCommand = new RelayCommand(() => { IsAddingProduct = false; IsAddingService = false; SelectedProduct = null; SelectedService = null; });
            LoadProductPhotoCommand = new RelayCommandSec(LoadProductPhoto);
            LoadServicePhotoCommand = new RelayCommandSec(LoadServicePhoto);

            LoadDataCommand.Execute(null);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Users.Clear();
                Products.Clear();
                Services.Clear();

                var users = _userRepository.GetAll();
                var products = _productRepository.GetAll();
                var services = _serviceRepository.GetAll();

                foreach (var user in users) Users.Add(user);
                foreach (var product in products) Products.Add(product);
                foreach (var service in services) Services.Add(service);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                _userRepository.Delete(SelectedUser.Id);
                Users.Remove(SelectedUser);
                SelectedUser = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Выберите товар для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                _productRepository.Delete(SelectedProduct.Id);
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления товара: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteServiceAsync()
        {
            if (SelectedService == null)
            {
                MessageBox.Show("Выберите услугу для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                _serviceRepository.Delete(SelectedService.Id);
                Services.Remove(SelectedService);
                SelectedService = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления услуги: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveProductAsync()
        {
            if (SelectedProduct == null ||
                string.IsNullOrWhiteSpace(SelectedProduct.Name) ||
                string.IsNullOrWhiteSpace(SelectedProduct.Description) ||
                string.IsNullOrWhiteSpace(SelectedProduct.Characteristics) ||
                SelectedProduct.Price < 0)
            {
                MessageBox.Show("Все поля (Название, Описание, Характеристики, Цена) должны быть заполнены, а цена должна быть неотрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (SelectedProduct.Id == 0)
                {
                    _productRepository.Add(SelectedProduct);
                    Products.Add(SelectedProduct);
                }
                else
                {
                    _productRepository.Update(SelectedProduct);
                }
                IsAddingProduct = false;
                SelectedProduct = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения товара: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveServiceAsync()
        {
            if (SelectedService == null ||
                string.IsNullOrWhiteSpace(SelectedService.Name) ||
                string.IsNullOrWhiteSpace(SelectedService.Description) ||
                string.IsNullOrWhiteSpace(SelectedService.Characteristics) ||
                SelectedService.Price < 0)
            {
                MessageBox.Show("Все поля (Название, Описание, Характеристики, Цена) должны быть заполнены, а цена должна быть неотрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (SelectedService.Id == 0)
                {
                    _serviceRepository.Add(SelectedService);
                    Services.Add(SelectedService);
                }
                else
                {
                    _serviceRepository.Update(SelectedService);
                }
                IsAddingService = false;
                SelectedService = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения услуги: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductPhoto(object parameter)
        {
            if (SelectedProduct == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    SelectedProduct.Photo = File.ReadAllBytes(openFileDialog.FileName);
                    OnPropertyChanged(nameof(SelectedProduct));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadServicePhoto(object parameter)
        {
            if (SelectedService == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    SelectedService.Photo = File.ReadAllBytes(openFileDialog.FileName);
                    OnPropertyChanged(nameof(SelectedService));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}