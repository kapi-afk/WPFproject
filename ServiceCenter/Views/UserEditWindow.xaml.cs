using ServiceCenter.Models;
using ServiceCenter.ViewModels;
using System.Windows;

namespace ServiceCenter.Views
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

            viewModel.SaveUserRoleCommand.Execute(null);

            if (viewModel.WasLastUserEditSuccessful)
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
