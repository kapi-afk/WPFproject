using Printinvest_WPF_app.ViewModels;
using System.Windows;

namespace Printinvest_WPF_app.Views
{
    public partial class UserCreateWindow : Window
    {
        public UserCreateWindow(ServiceAdminPanelViewModel viewModel)
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

            var hasRequiredFields = !string.IsNullOrWhiteSpace(viewModel.NewUserName) &&
                                    !string.IsNullOrWhiteSpace(viewModel.NewUserLogin) &&
                                    !string.IsNullOrWhiteSpace(viewModel.NewUserPassword);

            viewModel.CreateUserCommand.Execute(null);

            if (hasRequiredFields &&
                string.IsNullOrWhiteSpace(viewModel.NewUserName) &&
                string.IsNullOrWhiteSpace(viewModel.NewUserLogin) &&
                string.IsNullOrWhiteSpace(viewModel.NewUserPassword))
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
