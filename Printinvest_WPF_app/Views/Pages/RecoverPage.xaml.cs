using System.Windows;
using System.Windows.Controls;

namespace Printinvest_WPF_app.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для RecoverPage.xaml
    /// </summary>
    public partial class RecoverPage : Page
    {
        public RecoverPage()
        {
            InitializeComponent();
            NewPasswordBox.PasswordChanged += NewPasswordBox_PasswordChanged;
            ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.RecoverViewModel viewModel)
            {
                viewModel.NewPassword = NewPasswordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.RecoverViewModel viewModel)
            {
                viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        }
    }
}
