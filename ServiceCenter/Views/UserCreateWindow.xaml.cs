using ServiceCenter.ViewModels;
using System.Windows;

namespace ServiceCenter.Views
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

            viewModel.CreateUserCommand.Execute(null);

            if (viewModel.WasLastUserCreateSuccessful)
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
