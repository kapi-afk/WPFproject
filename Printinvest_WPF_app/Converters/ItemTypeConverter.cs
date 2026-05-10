using Printinvest_WPF_app.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Printinvest_WPF_app.Converters
{
    public class ItemTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product)
                return App.GetString("ItemTypeProduct", "Product");
            if (value is Service)
                return App.GetString("ItemTypeService", "Service");
            return App.GetString("ItemTypeUnknown", "Unknown");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
