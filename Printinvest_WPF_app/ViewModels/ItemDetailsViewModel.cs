using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Repositories;
using Printinvest_WPF_app.Utilities;
using Printinvest_WPF_app.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Printinvest_WPF_app.ViewModels
{
    public class ItemDetailsViewModel : BaseViewModel
    {
        private Product _product;
        private ObservableCollection<Comment> _comments;
        private string _newComment;
        private readonly CommentRepository _commentRepository;
        private readonly CartRepository _cartRepository;

        public string Price => Product != null ? $"{Product.Price:F2} BYN" : string.Empty;

        public Product Product
        {
            get => _product;
            set => SetProperty(ref _product, value);
        }

        public ObservableCollection<Comment> Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        public string NewComment
        {
            get => _newComment;
            set => SetProperty(ref _newComment, value);
        }

        public bool IsProduct => true; // Всегда Product, так как Comment ссылается на Product

        public string Characteristics => Product?.Characteristics;

        public ICommand AddToCartCommand { get; }
        public ICommand AddCommentCommand { get; }

        public ItemDetailsViewModel()
        {
            Product = new Product { Name = "Sample", Description = "Sample Description" };
            Comments = new ObservableCollection<Comment>();
            _commentRepository = RepositoryManager.Comments;
            _cartRepository = RepositoryManager.Carts;
            AddToCartCommand = new RelayCommandSec(ExecuteAddToCart);
            AddCommentCommand = new RelayCommandSec(ExecuteAddComment);
        }

        public ItemDetailsViewModel(Product product) : this()
        {
            Product = product;
            if (SessionManager.IsAuthenticated)
            {
                RepositoryManager.Analytics.Add(
                    new Analytic
                    {
                        ProductId = product.Id,
                        Timestamp = DateTime.UtcNow,
                        Action = "Visit",
                        UserId = SessionManager.CurrentUser.Id,
                    }
                );
            }

        }

        public void Initialize()
        {
            LoadComments();
        }

        private void LoadComments()
        {
            try
            {
                var comments = _commentRepository.GetByProductId(Product.Id);
                Comments = new ObservableCollection<Comment>(comments);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка загрузки комментариев: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddToCart(object parameter)
        {
            try
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

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == Product.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = Product.Id,
                        Product = Product,
                        Quantity = 1
                    };
                    _cartRepository.AddItemToCart(cart.Id, cartItem);
                }

                _cartRepository.Update(cart);
                MessageBox.Show($"Товар '{Product.Name}' добавлен в корзину!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddComment(object parameter)
        {
            try
            {
                if (SessionManager.CurrentUser == null)
                {
                    MessageBox.Show("Пожалуйста, войдите в систему для добавления комментария.", "Авторизация требуется", MessageBoxButton.OK, MessageBoxImage.Warning);
                    var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
                    mainViewModel.CurrentPage = new LoginPage();
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewComment))
                {
                    MessageBox.Show("Комментарий не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var comment = new Comment
                {
                    ProductId = Product.Id,
                    UserId = SessionManager.CurrentUser.Id,
                    User = SessionManager.CurrentUser,
                    Text = NewComment,
                    Timestamp = DateTime.Now
                };

                _commentRepository.Add(comment);
                Comments.Add(comment);
                NewComment = string.Empty;
                //MessageBox.Show("Комментарий добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении комментария: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}