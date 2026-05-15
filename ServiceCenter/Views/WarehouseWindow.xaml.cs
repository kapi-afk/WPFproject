using ServiceCenter.Models;
using ServiceCenter.ViewModels;
using System.Windows;

namespace ServiceCenter.Views
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
                    $"РќРµ СѓРґР°Р»РѕСЃСЊ РѕС‚РєСЂС‹С‚СЊ РѕРєРЅРѕ Р·Р°СЏРІРєРё РЅР° РјР°С‚РµСЂРёР°Р».{System.Environment.NewLine}{ex.Message}",
                    "РћС€РёР±РєР°",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
