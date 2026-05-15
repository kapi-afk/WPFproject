using ServiceCenter.Models;
using ServiceCenter.ViewModels;
using System;
using System.Windows;

namespace ServiceCenter.Views
{
    public partial class OnlinePaymentWindow : Window
    {
        public OnlinePaymentWindow(Order order)
        {
            InitializeComponent();
            DataContext = new OnlinePaymentViewModel(order);
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewModel = DataContext as OnlinePaymentViewModel;
                if (viewModel == null)
                {
                    return;
                }

                if (!viewModel.Validate())
                {
                    MessageBox.Show(
                        App.GetString("PaymentValidationMessage", "Check the card details. Fill in all required fields in the correct format."),
                        App.GetString("PaymentValidationTitle", "Validation error"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось подтвердить оплату.{Environment.NewLine}{ex.Message}",
                    "Ошибка оплаты",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
