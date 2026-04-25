using System;
using System.Globalization;
using System.Windows.Data;
using Printinvest_WPF_app.Models;

namespace Printinvest_WPF_app.Converters
{
    public class CartItemTotalPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CartItem item)
            {
                decimal price = item.Product?.Price ?? item.Service?.Price ?? 0;
                return $"{(price * item.Quantity):F2} BYN";
            }
            return "0.00 BYN";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Printinvest_WPF_app.Converters
{
    public class CartItemPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CartItem item)
            {
                decimal price = item.Product?.Price ?? item.Service?.Price ?? 0;
                Console.WriteLine($"CartItemPriceConverter: Price={price}, ItemId={item.Id}");
                return $"{price:F2} BYN";
            }
            Console.WriteLine("CartItemPriceConverter: No CartItem");
            return "0.00 BYN";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}