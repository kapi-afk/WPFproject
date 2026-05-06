using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.ViewModels;
using System.Windows;

namespace Printinvest_WPF_app.Views
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
            var viewModel = DataContext as OnlinePaymentViewModel;
            if (viewModel == null)
            {
                return;
            }

            if (!viewModel.Validate())
            {
                MessageBox.Show(
                    "Проверьте данные карты. Заполните все обязательные поля в корректном формате.",
                    "Ошибка проверки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
