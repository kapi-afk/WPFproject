using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Printinvest_WPF_app.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ManagerPanelPage.xaml
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
                    "Не удалось открыть фото неисправности. Возможно, изображение повреждено.",
                    "Ошибка",
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
