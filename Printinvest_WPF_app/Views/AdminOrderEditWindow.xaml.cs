using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.ViewModels;
using System.Windows;

namespace Printinvest_WPF_app.Views
{
    public partial class AdminOrderEditWindow : Window
    {
        public AdminOrderEditWindow(ServiceAdminPanelViewModel viewModel)
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

            var canSave = viewModel.SelectedOrder != null;

            viewModel.SaveOrderCommand.Execute(null);

            if (canSave)
            {
                DialogResult = true;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ServiceAdminPanelViewModel;
            if (viewModel == null || viewModel.SelectedOrder == null)
            {
                return;
            }

            viewModel.DeleteOrderCommand.Execute(null);
            DialogResult = true;
        }
    }
}
