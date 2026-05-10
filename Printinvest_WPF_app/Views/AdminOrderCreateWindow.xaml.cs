using Microsoft.Win32;
using Printinvest_WPF_app.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Printinvest_WPF_app.Views
{
    public partial class AdminOrderCreateWindow : Window
    {
        private readonly Dictionary<string, string[]> _brandCatalog = new Dictionary<string, string[]>
        {
            ["Ноутбук"] = new[] { "Lenovo", "ASUS", "HP", "Acer", "Dell", "Apple", "MSI" },
            ["Стационарный ПК"] = new[] { "Dell", "HP", "Lenovo", "ASUS", "MSI", "Acer" },
            ["Моноблок"] = new[] { "Lenovo", "HP", "Apple", "Acer", "ASUS" },
            ["Монитор"] = new[] { "Samsung", "LG", "AOC", "Philips", "Dell", "BenQ" },
            ["Принтер"] = new[] { "HP", "Canon", "Epson", "Brother", "Xerox" },
            ["Другое"] = new string[0]
        };

        private readonly Dictionary<string, string[]> _defaultModelCatalog = new Dictionary<string, string[]>
        {
            ["Ноутбук"] = new[] { "IdeaPad", "ThinkPad", "VivoBook", "Pavilion", "Aspire", "MacBook" },
            ["Стационарный ПК"] = new[] { "OptiPlex", "ProDesk", "ThinkCentre", "ROG", "MAG", "Nitro" },
            ["Моноблок"] = new[] { "iMac", "IdeaCentre AIO", "Aspire C", "Zen AiO", "ProOne" },
            ["Монитор"] = new[] { "Odyssey", "UltraGear", "ThinkVision", "P-series", "GW", "24MK" },
            ["Принтер"] = new[] { "LaserJet", "DeskJet", "PIXMA", "EcoTank", "HL-L", "WorkCentre" },
            ["Другое"] = new string[0]
        };

        private readonly Dictionary<string, Dictionary<string, string[]>> _brandModelCatalog = new Dictionary<string, Dictionary<string, string[]>>
        {
            ["Ноутбук"] = new Dictionary<string, string[]>
            {
                ["Lenovo"] = new[] { "IdeaPad", "ThinkPad", "Legion", "Yoga" },
                ["ASUS"] = new[] { "VivoBook", "Zenbook", "ROG", "TUF" },
                ["HP"] = new[] { "Pavilion", "Victus", "ProBook", "EliteBook" },
                ["Acer"] = new[] { "Aspire", "Nitro", "Swift", "Predator" },
                ["Dell"] = new[] { "Inspiron", "Latitude", "Vostro", "XPS" },
                ["Apple"] = new[] { "MacBook Air", "MacBook Pro" },
                ["MSI"] = new[] { "Modern", "Katana", "Prestige", "Stealth" }
            },
            ["Стационарный ПК"] = new Dictionary<string, string[]>
            {
                ["Dell"] = new[] { "OptiPlex", "Precision", "Inspiron" },
                ["HP"] = new[] { "ProDesk", "EliteDesk", "Pavilion" },
                ["Lenovo"] = new[] { "ThinkCentre", "IdeaCentre", "Legion" },
                ["ASUS"] = new[] { "ROG", "ExpertCenter", "ProArt" },
                ["MSI"] = new[] { "MAG", "Aegis", "Trident" },
                ["Acer"] = new[] { "Aspire", "Veriton", "Predator" }
            },
            ["Моноблок"] = new Dictionary<string, string[]>
            {
                ["Lenovo"] = new[] { "IdeaCentre AIO", "Yoga AIO" },
                ["HP"] = new[] { "All-in-One", "ProOne" },
                ["Apple"] = new[] { "iMac" },
                ["Acer"] = new[] { "Aspire C" },
                ["ASUS"] = new[] { "Zen AiO", "Vivo AiO" }
            },
            ["Монитор"] = new Dictionary<string, string[]>
            {
                ["Samsung"] = new[] { "Odyssey", "ViewFinity", "S24" },
                ["LG"] = new[] { "UltraGear", "UltraWide", "24MK" },
                ["AOC"] = new[] { "Gaming", "Value Line", "Professional" },
                ["Philips"] = new[] { "P-line", "V-line", "Momentum" },
                ["Dell"] = new[] { "P-series", "S-series", "UltraSharp" },
                ["BenQ"] = new[] { "GW", "EX", "PD" }
            },
            ["Принтер"] = new Dictionary<string, string[]>
            {
                ["HP"] = new[] { "LaserJet", "DeskJet", "OfficeJet" },
                ["Canon"] = new[] { "PIXMA", "i-SENSYS", "MAXIFY" },
                ["Epson"] = new[] { "EcoTank", "WorkForce", "L-series" },
                ["Brother"] = new[] { "HL-L", "DCP", "MFC" },
                ["Xerox"] = new[] { "Phaser", "VersaLink", "WorkCentre" }
            }
        };

        private readonly Dictionary<string, string[]> _problemCatalog = new Dictionary<string, string[]>
        {
            ["Ноутбук"] = new[] { "Не включается", "Сильно греется", "Шумит", "Не заряжается", "Разбит экран", "Тормозит" },
            ["Стационарный ПК"] = new[] { "Не включается", "Перезагружается", "Шумит", "Нет изображения", "Тормозит", "Не видит диск" },
            ["Моноблок"] = new[] { "Не включается", "Нет изображения", "Сильно греется", "Тормозит", "Не работает сенсор" },
            ["Монитор"] = new[] { "Нет изображения", "Мерцает экран", "Полосы на экране", "Разбит экран", "Не работает подсветка" },
            ["Принтер"] = new[] { "Не печатает", "Зажевывает бумагу", "Полосы при печати", "Ошибка картриджа", "Не подключается" },
            ["Другое"] = new[] { "Не включается", "Работает нестабильно", "Проблема с экраном", "Проблема с подключением" }
        };

        public AdminOrderCreateWindow(ServiceAdminPanelViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            InitializeFormOptions();
            SyncPhotoState();
        }

        private ServiceAdminPanelViewModel ViewModel => DataContext as ServiceAdminPanelViewModel;

        private static string OtherOption => App.GetString("OtherOption", "Other");

        private void InitializeFormOptions()
        {
            if (ViewModel == null)
            {
                return;
            }

            DeviceTypeComboBox.SelectedItem = ViewModel.NewOrderDeviceType;
            RefreshBrandOptions();
            SyncPhotoState();
        }

        private void DeviceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomDeviceTypeTextBox.Visibility = IsOtherSelected(DeviceTypeComboBox) ? Visibility.Visible : Visibility.Collapsed;
            RefreshBrandOptions();
            RefreshProblemOptions();
        }

        private void BrandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomBrandTextBox.Visibility = IsOtherSelected(BrandComboBox) ? Visibility.Visible : Visibility.Collapsed;
            RefreshModelOptions();
        }

        private void ModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomModelTextBox.Visibility = IsOtherSelected(ModelComboBox) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ProblemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomProblemTextBox.Visibility = IsOtherSelected(ProblemComboBox) ? Visibility.Visible : Visibility.Collapsed;
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
        }

        private void RefreshProblemOptions()
        {
            var deviceType = DeviceTypeComboBox.SelectedItem as string;
            PopulateComboBox(ProblemComboBox, GetOptionsForDevice(_problemCatalog, deviceType));
            CustomProblemTextBox.Visibility = Visibility.Collapsed;
            CustomProblemTextBox.Text = string.Empty;
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

            return new string[0];
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
            return string.Equals(value, OtherOption) || string.Equals(value, "Другое");
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
