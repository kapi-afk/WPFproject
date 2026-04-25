using Printinvest_WPF_app.ViewModels;
using System.Windows;

namespace Printinvest_WPF_app.Views
{
    public partial class WarehouseRequestsWindow : Window
    {
        public WarehouseRequestsWindow(ServiceAdminPanelViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
