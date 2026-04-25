using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views.Pages;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class RepairOrderViewModel : BaseViewModel
    {
        private readonly OrderRepository _orderRepository;
        private string _deviceType;
        private string _deviceBrand;
        private string _deviceModel;
        private string _problemDescription;
        private string _contactPhone;
        private string _clientComment;

        public ObservableCollection<string> DeviceTypes { get; } = new ObservableCollection<string>
        {
            "Laptop",
            "Desktop PC",
            "All-in-one",
            "Monitor",
            "Printer",
            "Other"
        };

        public string DeviceType
        {
            get => _deviceType;
            set => SetProperty(ref _deviceType, value);
        }

        public string DeviceBrand
        {
            get => _deviceBrand;
            set => SetProperty(ref _deviceBrand, value);
        }

        public string DeviceModel
        {
            get => _deviceModel;
            set => SetProperty(ref _deviceModel, value);
        }

        public string ProblemDescription
        {
            get => _problemDescription;
            set => SetProperty(ref _problemDescription, value);
        }

        public string ContactPhone
        {
            get => _contactPhone;
            set => SetProperty(ref _contactPhone, value);
        }

        public string ClientComment
        {
            get => _clientComment;
            set => SetProperty(ref _clientComment, value);
        }

        public ICommand CreateOrderCommand { get; }

        public RepairOrderViewModel()
        {
            _orderRepository = RepositoryManager.Orders;
            DeviceType = DeviceTypes[0];
            CreateOrderCommand = new RelayCommand(CreateOrder);
        }

        private void CreateOrder()
        {
            try
            {
                if (!SessionManager.IsAuthenticated)
                {
                    MessageBox.Show("Sign in to create a repair order.", "Authorization required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
                    if (mainViewModel != null)
                    {
                        mainViewModel.CurrentPage = new LoginPage();
                    }
                    return;
                }

                if (string.IsNullOrWhiteSpace(DeviceType) ||
                    string.IsNullOrWhiteSpace(DeviceBrand) ||
                    string.IsNullOrWhiteSpace(DeviceModel) ||
                    string.IsNullOrWhiteSpace(ProblemDescription) ||
                    string.IsNullOrWhiteSpace(ContactPhone))
                {
                    MessageBox.Show("Fill in device type, brand, model, problem description, and phone number.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var order = new Order
                {
                    UserId = SessionManager.CurrentUser.Id,
                    User = SessionManager.CurrentUser,
                    DeviceType = DeviceType,
                    DeviceBrand = DeviceBrand,
                    DeviceModel = DeviceModel,
                    ProblemDescription = ProblemDescription,
                    ContactPhone = ContactPhone,
                    ClientComment = ClientComment,
                    Status = OrderStatus.Created,
                    CreatedAt = DateTime.Now
                };

                AssignBestMaster(order);
                _orderRepository.Add(order);
                ResetForm();
                MessageBox.Show("Repair order created.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create repair order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            DeviceType = DeviceTypes[0];
            DeviceBrand = string.Empty;
            DeviceModel = string.Empty;
            ProblemDescription = string.Empty;
            ContactPhone = string.Empty;
            ClientComment = string.Empty;
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
    }
}
