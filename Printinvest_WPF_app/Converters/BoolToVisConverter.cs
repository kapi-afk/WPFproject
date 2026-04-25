using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Printinvest_WPF_app.Converters
{
    public class ImagePathToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
                return new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (value is bool flag && flag);
            if (parameter?.ToString() == "Inverse")
                b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }


    public static class FileHelper
    {
        public static string SaveImage(byte[] imageBytes, string fileName)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fullPath = Path.Combine(dir, fileName);
            File.WriteAllBytes(fullPath, imageBytes);
            return fullPath;
        }

        public static byte[] LoadImage(string filePath)
        {
            return File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;
        }
    }
    public static class AppUtils
    {
        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch { }
        }
    }
}