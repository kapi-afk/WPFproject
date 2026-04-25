using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class ManagerPanelViewModel : BaseViewModel
    {
        private readonly AnalyticRepository _analyticRepository;
        private readonly OrderRepository _orderRepository;
        private readonly CommentRepository _commentRepository;
        private ObservableCollection<Analytic> _analytics;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<Comment> _comments;
        private Order _selectedOrder;
        private Comment _selectedComment;
        private OrderStatus _selectedOrderStatus;
        private string _analyticsSearchQuery;
        private string _selectedAnalyticsFilter;
        private DateTime? _analyticsDateFilter;

        public ObservableCollection<Analytic> Analytics
        {
            get => _analytics;
            set => SetProperty(ref _analytics, value);
        }

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public ObservableCollection<Comment> Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);
                SelectedOrderStatus = value?.Status ?? OrderStatus.Pending;
            }
        }

        public Comment SelectedComment
        {
            get => _selectedComment;
            set => SetProperty(ref _selectedComment, value);
        }

        public OrderStatus SelectedOrderStatus
        {
            get => _selectedOrderStatus;
            set => SetProperty(ref _selectedOrderStatus, value);
        }

        public string AnalyticsSearchQuery
        {
            get => _analyticsSearchQuery;
            set
            {
                SetProperty(ref _analyticsSearchQuery, value);
                LoadAnalytics();
            }
        }

        public string SelectedAnalyticsFilter
        {
            get => _selectedAnalyticsFilter;
            set
            {
                SetProperty(ref _selectedAnalyticsFilter, value);
                LoadAnalytics();
            }
        }

        public DateTime? AnalyticsDateFilter
        {
            get => _analyticsDateFilter;
            set
            {
                SetProperty(ref _analyticsDateFilter, value);
                LoadAnalytics();
            }
        }

        public ObservableCollection<OrderStatus> OrderStatuses { get; } = new ObservableCollection<OrderStatus>
        {
            OrderStatus.Pending,
            OrderStatus.Shipped,
            OrderStatus.Completed,
            OrderStatus.Cancelled
        };

        public ObservableCollection<string> AnalyticsFilters { get; } = new ObservableCollection<string>
        {
            "Все", "Пользователь", "Продукт", "Услуга", "Действие"
        };

        public ICommand LoadDataCommand { get; }
        public ICommand UpdateOrderStatusCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand DeleteCommentCommand { get; }
        public ICommand ClearAnalyticsSearchCommand { get; }

        public ManagerPanelViewModel()
        {
            _analyticRepository = RepositoryManager.Analytics;
            _orderRepository = RepositoryManager.Orders;
            _commentRepository = RepositoryManager.Comments;

            Analytics = new ObservableCollection<Analytic>();
            Orders = new ObservableCollection<Order>();
            Comments = new ObservableCollection<Comment>();
            SelectedAnalyticsFilter = AnalyticsFilters.First();

            LoadDataCommand = new RelayCommand(LoadData);
            UpdateOrderStatusCommand = new RelayCommand(UpdateOrderStatus);
            DeleteOrderCommand = new RelayCommand(DeleteOrder);
            DeleteCommentCommand = new RelayCommand(DeleteComment);
            ClearAnalyticsSearchCommand = new RelayCommand(() => AnalyticsSearchQuery = string.Empty);

            LoadDataCommand.Execute(null);
        }

        private void LoadData()
        {
            try
            {
                Orders.Clear();
                var orders = _orderRepository.GetAll();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                Comments.Clear();
                var comments = _commentRepository.GetAll();
                foreach (var comment in comments)
                {
                    Comments.Add(comment);
                }

                LoadAnalytics();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAnalytics()
        {
            try
            {
                Analytics.Clear();
                var analytics = _analyticRepository.GetAll();
                var searchLower = string.IsNullOrWhiteSpace(_analyticsSearchQuery) ? null : _analyticsSearchQuery.ToLower();

                var filteredAnalytics = analytics.Where(a =>
                {
                    bool matchesSearch = true;
                    bool matchesDate = _analyticsDateFilter == null || a.Timestamp.Date == _analyticsDateFilter.Value.Date;

                    if (searchLower != null)
                    {
                        if (_selectedAnalyticsFilter == "Пользователь")
                            matchesSearch = a.User?.Name?.ToLower().Contains(searchLower) ?? false;
                        else if (_selectedAnalyticsFilter == "Продукт")
                            matchesSearch = a.Product?.Name?.ToLower().Contains(searchLower) ?? false;
                        else if (_selectedAnalyticsFilter == "Услуга")
                            matchesSearch = a.Service?.Name?.ToLower().Contains(searchLower) ?? false;
                        else if (_selectedAnalyticsFilter == "Действие")
                            matchesSearch = a.Action?.ToLower().Contains(searchLower) ?? false;
                        else
                            matchesSearch = (a.User?.Name?.ToLower().Contains(searchLower) ?? false) ||
                                            (a.Product?.Name?.ToLower().Contains(searchLower) ?? false) ||
                                            (a.Service?.Name?.ToLower().Contains(searchLower) ?? false) ||
                                            (a.Action?.ToLower().Contains(searchLower) ?? false);
                    }

                    return matchesSearch && matchesDate;
                });

                foreach (var item in filteredAnalytics)
                {
                    Analytics.Add(item);
                }
                Console.WriteLine($"Загружено записей аналитики: {Analytics.Count}");
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка загрузки аналитики: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateOrderStatus()
        {
            try
            {
                if (SelectedOrder == null)
                {
                    //MessageBox.Show("Выберите заказ для обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedOrder.Status = SelectedOrderStatus;
                _orderRepository.Update(SelectedOrder);
                //MessageBox.Show("Статус заказа обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка обновления статуса заказа: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteOrder()
        {
            try
            {
                if (SelectedOrder == null)
                {
                    //MessageBox.Show("Выберите заказ для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _orderRepository.Delete(SelectedOrder.Id);
                Orders.Remove(SelectedOrder);
                SelectedOrder = null;
                //MessageBox.Show("Заказ удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка удаления заказа: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteComment()
        {
            try
            {
                if (SelectedComment == null)
                {
                    //MessageBox.Show("Выберите комментарий для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _commentRepository.Delete(SelectedComment.Id);
                Comments.Remove(SelectedComment);
                SelectedComment = null;
                //MessageBox.Show("Комментарий удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка удаления комментария: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}