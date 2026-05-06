using Printinvest_WPF_app.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Printinvest_WPF_app.Converters
{
    public class OrderStatusForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is OrderStatus status))
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"));
            }

            switch (status)
            {
                case OrderStatus.Completed:
                case OrderStatus.ReadyForPickup:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#15803D"));
                case OrderStatus.Cancelled:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B91C1C"));
                case OrderStatus.WaitingForParts:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B45309"));
                case OrderStatus.InProgress:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D4ED8"));
                case OrderStatus.Assigned:
                case OrderStatus.Diagnosing:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4338CA"));
                case OrderStatus.Created:
                default:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
