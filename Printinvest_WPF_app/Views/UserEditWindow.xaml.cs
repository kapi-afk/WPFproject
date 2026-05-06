using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.ViewModels;
using System.Windows;

namespace Printinvest_WPF_app.Views
{
    public partial class UserEditWindow : Window
    {
        public UserEditWindow(ServiceAdminPanelViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ServiceAdminPanelViewModel;
            if (viewModel == null)
            {
                return;
            }

            var canSave = viewModel.SelectedUser != null &&
                          !string.IsNullOrWhiteSpace(viewModel.SelectedUser.Login);

            if (canSave && viewModel.SelectedUserRole == UserRole.Client)
            {
                canSave =
                    !string.IsNullOrWhiteSpace(viewModel.SelectedUserLastName) &&
                    !string.IsNullOrWhiteSpace(viewModel.SelectedUserFirstName) &&
                    !string.IsNullOrWhiteSpace(viewModel.SelectedUserEmail);
            }
            else if (canSave)
            {
                canSave = !string.IsNullOrWhiteSpace(viewModel.SelectedUser.Name);
            }

            viewModel.SaveUserRoleCommand.Execute(null);

            if (canSave && !viewModel.IsUserEditPanelVisible)
            {
                DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ServiceAdminPanelViewModel;
            viewModel?.CancelEditUserCommand.Execute(null);
            DialogResult = false;
        }
    }
}
