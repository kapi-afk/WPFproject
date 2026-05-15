using Microsoft.Win32;
using ServiceCenter.ViewModels;
using ServiceCenter.Utilities;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ServiceCenter.Views
{
    public partial class AdminOrderCreateWindow : Window
    {
        private readonly Dictionary<string, string[]> _brandCatalog = OrderCatalogData.BrandCatalog;
        private readonly Dictionary<string, string[]> _defaultModelCatalog = OrderCatalogData.DefaultModelCatalog;
        private readonly Dictionary<string, Dictionary<string, string[]>> _brandModelCatalog = OrderCatalogData.BrandModelCatalog;
        private readonly Dictionary<string, string[]> _problemCatalog = OrderCatalogData.ProblemCatalog;

        public AdminOrderCreateWindow(ServiceAdminPanelViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            InitializeFormOptions();
            SyncPhotoState();
        }

        private ServiceAdminPanelViewModel ViewModel => DataContext as ServiceAdminPanelViewModel;

        private static string OtherOption => OrderCatalogData.OtherOption;

        private void InitializeFormOptions()
        {
            if (ViewModel == null)
            {
                return;
            }

            DeviceTypeComboBox.SelectedItem = ViewModel.NewOrderDeviceType;
            SyncCustomEntryFlags();
            RefreshBrandOptions();
            SyncPhotoState();
        }

        private void DeviceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomDeviceTypeTextBox.Visibility = IsOtherSelected(DeviceTypeComboBox) ? Visibility.Visible : Visibility.Collapsed;
            SyncCustomEntryFlags();
            RefreshBrandOptions();
            RefreshProblemOptions();
        }

        private void BrandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomBrandTextBox.Visibility = IsOtherSelected(BrandComboBox) ? Visibility.Visible : Visibility.Collapsed;
            SyncCustomEntryFlags();
            RefreshModelOptions();
        }

        private void ModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomModelTextBox.Visibility = IsOtherSelected(ModelComboBox) ? Visibility.Visible : Visibility.Collapsed;
            SyncCustomEntryFlags();
        }

        private void ProblemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomProblemTextBox.Visibility = IsOtherSelected(ProblemComboBox) ? Visibility.Visible : Visibility.Collapsed;
            SyncCustomEntryFlags();
        }

        private void SelectProblemPhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true || ViewModel == null)
            {
                return;
            }

            ViewModel.NewOrderProblemPhoto = File.ReadAllBytes(openFileDialog.FileName);
            ViewModel.NewOrderProblemPhotoName = Path.GetFileName(openFileDialog.FileName);
            SyncPhotoState();
        }

        private void RemoveProblemPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            ViewModel.NewOrderProblemPhoto = null;
            ViewModel.NewOrderProblemPhotoName = string.Empty;
            SyncPhotoState();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            ViewModel.NewOrderDeviceType = ResolveOptionValue(DeviceTypeComboBox, CustomDeviceTypeTextBox);
            ViewModel.NewOrderDeviceBrand = ResolveOptionValue(BrandComboBox, CustomBrandTextBox);
            ViewModel.NewOrderDeviceModel = ResolveOptionValue(ModelComboBox, CustomModelTextBox);
            ViewModel.NewOrderProblemDescription = ResolveOptionValue(ProblemComboBox, CustomProblemTextBox);
            SyncCustomEntryFlags();

            ViewModel.CreateAdminOrderCommand.Execute(null);

            if (ViewModel.WasLastAdminOrderCreationSuccessful)
            {
                DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void RefreshBrandOptions()
        {
            var deviceType = DeviceTypeComboBox.SelectedItem as string;
            PopulateComboBox(BrandComboBox, GetOptionsForDevice(_brandCatalog, deviceType));
            CustomBrandTextBox.Visibility = Visibility.Collapsed;
            CustomBrandTextBox.Text = string.Empty;
            SyncCustomEntryFlags();
            RefreshModelOptions();
        }

        private void RefreshModelOptions()
        {
            var deviceType = DeviceTypeComboBox.SelectedItem as string;
            var brand = BrandComboBox.SelectedItem as string;
            IEnumerable<string> values;

            if (!string.IsNullOrWhiteSpace(deviceType) &&
                !string.IsNullOrWhiteSpace(brand) &&
                !IsOtherValue(brand) &&
                _brandModelCatalog.TryGetValue(deviceType, out var brandModels) &&
                brandModels.TryGetValue(brand, out var specificModels))
            {
                values = specificModels;
            }
            else
            {
                values = GetOptionsForDevice(_defaultModelCatalog, deviceType);
            }

            PopulateComboBox(ModelComboBox, values);
            CustomModelTextBox.Visibility = Visibility.Collapsed;
            CustomModelTextBox.Text = string.Empty;
            SyncCustomEntryFlags();
        }

        private void RefreshProblemOptions()
        {
            var deviceType = DeviceTypeComboBox.SelectedItem as string;
            PopulateComboBox(ProblemComboBox, GetOptionsForDevice(_problemCatalog, deviceType));
            CustomProblemTextBox.Visibility = Visibility.Collapsed;
            CustomProblemTextBox.Text = string.Empty;
            SyncCustomEntryFlags();
        }

        private void PopulateComboBox(ComboBox comboBox, IEnumerable<string> values)
        {
            comboBox.ItemsSource = values.Concat(new[] { OtherOption }).ToList();
            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
        }

        private IEnumerable<string> GetOptionsForDevice(Dictionary<string, string[]> catalog, string deviceType)
        {
            if (!string.IsNullOrWhiteSpace(deviceType) && catalog.TryGetValue(deviceType, out var values))
            {
                return values;
            }

            return System.Array.Empty<string>();
        }

        private static bool IsOtherSelected(ComboBox comboBox)
        {
            return IsOtherValue(comboBox.SelectedItem as string);
        }

        private static string ResolveOptionValue(ComboBox comboBox, TextBox customTextBox)
        {
            var selectedValue = comboBox.SelectedItem as string;
            if (IsOtherValue(selectedValue))
            {
                return string.IsNullOrWhiteSpace(customTextBox.Text) ? string.Empty : customTextBox.Text.Trim();
            }

            return string.IsNullOrWhiteSpace(selectedValue) ? string.Empty : selectedValue.Trim();
        }

        private static bool IsOtherValue(string value)
        {
            return string.Equals(value, OtherOption);
        }

        private void SyncCustomEntryFlags()
        {
            if (ViewModel == null)
            {
                return;
            }

            ViewModel.IsNewOrderDeviceTypeCustomEntry = IsOtherSelected(DeviceTypeComboBox);
            ViewModel.IsNewOrderDeviceBrandCustomEntry = IsOtherSelected(BrandComboBox);
            ViewModel.IsNewOrderDeviceModelCustomEntry = IsOtherSelected(ModelComboBox);
            ViewModel.IsNewOrderProblemCustomEntry = IsOtherSelected(ProblemComboBox);
        }

        private void SyncPhotoState()
        {
            if (ViewModel?.NewOrderProblemPhoto == null || ViewModel.NewOrderProblemPhoto.Length == 0)
            {
                ProblemPhotoStatusTextBlock.Text = App.GetString("ProblemPhotoNotAdded", "No photo added");
                ProblemPhotoPreviewBorder.Visibility = Visibility.Collapsed;
                RemovePhotoButton.Visibility = Visibility.Collapsed;
                ProblemPhotoPreview.Source = null;
                return;
            }

            ProblemPhotoStatusTextBlock.Text = string.Format(
                CultureInfo.CurrentCulture,
                App.GetString("ProblemPhotoSelectedFormat", "Selected photo: {0}"),
                ViewModel.NewOrderProblemPhotoName);
            ProblemPhotoPreviewBorder.Visibility = Visibility.Visible;
            RemovePhotoButton.Visibility = Visibility.Visible;
            ProblemPhotoPreview.Source = LoadImage(ViewModel.NewOrderProblemPhoto);
        }

        private static BitmapImage LoadImage(byte[] bytes)
        {
            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
