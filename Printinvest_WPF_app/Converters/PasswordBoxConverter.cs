using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Printinvest_WPF_app.Converters
{
    public class PasswordBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PasswordBox passwordBox)
            {
                return passwordBox.Password;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}