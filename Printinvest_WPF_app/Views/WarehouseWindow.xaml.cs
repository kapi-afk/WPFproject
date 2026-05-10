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

            try
            {
                var requestWindow = new MaterialRequestWindow(viewModel)
                {
                    Owner = this
                };

                requestWindow.ShowDialog();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось открыть окно заявки на материал.{System.Environment.NewLine}{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
