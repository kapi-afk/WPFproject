using Printinvest_WPF_app.ViewModels;
using System.Windows;

namespace Printinvest_WPF_app.Views
{
    public partial class AdminOrderCreateWindow : Window
    {
        public AdminOrderCreateWindow(ServiceAdminPanelViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ServiceAdminPanelViewModel;
            if (viewModel == null)
            {
                return;
            }

            var hasRequiredFields = !string.IsNullOrWhiteSpace(viewModel.NewOrderClientName) &&
                                    !string.IsNullOrWhiteSpace(viewModel.NewOrderClientEmail) &&
                                    !string.IsNullOrWhiteSpace(viewModel.NewOrderContactPhone) &&
                                    !string.IsNullOrWhiteSpace(viewModel.NewOrderDeviceType) &&
                                    !string.IsNullOrWhiteSpace(viewModel.NewOrderDeviceBrand) &&
                                    !string.IsNullOrWhiteSpace(viewModel.NewOrderDeviceModel) &&
                                    !string.IsNullOrWhiteSpace(viewModel.NewOrderProblemDescription) &&
                                    !string.IsNullOrWhiteSpace(viewModel.NewOrderDeliveryMethod) &&
                                    (!viewModel.IsCourierDeliverySelected ||
                                     !string.IsNullOrWhiteSpace(viewModel.NewOrderDeliveryAddress));

            viewModel.CreateAdminOrderCommand.Execute(null);

            if (hasRequiredFields &&
                string.IsNullOrWhiteSpace(viewModel.NewOrderClientName) &&
                string.IsNullOrWhiteSpace(viewModel.NewOrderClientEmail) &&
                string.IsNullOrWhiteSpace(viewModel.NewOrderContactPhone))
            {
                DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
