using Printinvest_WPF_app.ViewModels;
using System.Windows;

namespace Printinvest_WPF_app.Views
{
    public partial class MaterialRequestWindow : Window
    {
        public MaterialRequestWindow(MasterWarehouseWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MasterWarehouseWindowViewModel;
            if (viewModel != null && viewModel.CreateMaterialRequest())
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
