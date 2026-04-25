using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.ViewModels;
using System.Windows;

namespace Printinvest_WPF_app.Views
{
    public partial class WarehouseWindow : Window
    {
        public WarehouseWindow(Order targetOrder)
        {
            InitializeComponent();
            DataContext = new MasterWarehouseWindowViewModel(targetOrder);
        }

        private void CreateRequest_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MasterWarehouseWindowViewModel;
            if (viewModel == null)
            {
                return;
            }

            viewModel.PrepareMaterialRequest();

            var requestWindow = new MaterialRequestWindow(viewModel)
            {
                Owner = this
            };

            requestWindow.ShowDialog();
        }
    }
}
