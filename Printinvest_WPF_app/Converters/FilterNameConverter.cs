using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Printinvest_WPF_app.Converters
{
    public class FilterNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filter)
            {
                if (filter == "All")
                    return Application.Current.Resources["FilterAll"];
                if (filter == "Products")
                    return Application.Current.Resources["FilterProducts"];
                if (filter == "Services")
                    return Application.Current.Resources["FilterServices"];
                return filter;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}