using ServiceCenter.Models;
using ServiceCenter.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ServiceCenter.Views
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

        private void ProblemPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element) || !(element.Tag is byte[] imageBytes) || imageBytes.Length == 0)
            {
                return;
            }

            ShowImagePreview(imageBytes, App.GetString("PhotoPreviewTitle", "Problem photo"));
        }

        private void ShowImagePreview(byte[] imageBytes, string title)
        {
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
                    App.GetString("ErrorTitle", "Ошибка"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var contentBackground = Application.Current.TryFindResource("ContentBackgroundBrush") as Brush ?? Brushes.White;
            var cardBackground = Application.Current.TryFindResource("CardBackgroundBrush") as Brush ?? Brushes.White;
            var previewWindow = new Window
            {
                Title = title,
                Owner = this,
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
