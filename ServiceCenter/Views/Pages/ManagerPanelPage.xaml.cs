using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ServiceCenter.Views.Pages
{
    /// <summary>
    /// Р›РѕРіРёРєР° РІР·Р°РёРјРѕРґРµР№СЃС‚РІРёСЏ РґР»СЏ ManagerPanelPage.xaml
    /// </summary>
    public partial class ManagerPanelPage : Page
    {
        public ManagerPanelPage()
        {
            InitializeComponent();
        }

        private void ProblemPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element) || !(element.Tag is byte[] imageBytes) || imageBytes.Length == 0)
            {
                return;
            }

            BitmapImage bitmap;
            try
            {
                bitmap = new BitmapImage();
                using (var stream = new MemoryStream(imageBytes))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }
            catch
            {
                MessageBox.Show(
                    "РќРµ СѓРґР°Р»РѕСЃСЊ РѕС‚РєСЂС‹С‚СЊ С„РѕС‚Рѕ РЅРµРёСЃРїСЂР°РІРЅРѕСЃС‚Рё. Р’РѕР·РјРѕР¶РЅРѕ, РёР·РѕР±СЂР°Р¶РµРЅРёРµ РїРѕРІСЂРµР¶РґРµРЅРѕ.",
                    "РћС€РёР±РєР°",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var owner = Window.GetWindow(this);
            var contentBackground = Application.Current.TryFindResource("ContentBackgroundBrush") as Brush ?? Brushes.White;
            var cardBackground = Application.Current.TryFindResource("CardBackgroundBrush") as Brush ?? Brushes.White;
            var previewWindow = new Window
            {
                Title = App.GetString("PhotoPreviewTitle", "Problem photo"),
                Owner = owner,
                Width = 760,
                Height = 760,
                MinWidth = 420,
                MinHeight = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = contentBackground,
                Content = new Border
                {
                    Padding = new Thickness(16),
                    Background = cardBackground,
                    Child = new ScrollViewer
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Content = new Image
                        {
                            Source = bitmap,
                            Stretch = Stretch.Uniform
                        }
                    }
                }
            };

            previewWindow.ShowDialog();
        }
    }
}
