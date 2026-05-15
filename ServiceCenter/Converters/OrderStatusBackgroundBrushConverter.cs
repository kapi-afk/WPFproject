using ServiceCenter.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ServiceCenter.Converters
{
    public class OrderStatusBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is OrderStatus status))
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            }

            switch (status)
            {
                case OrderStatus.Completed:
                case OrderStatus.ReadyForPickup:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"));
                case OrderStatus.Cancelled:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                case OrderStatus.WaitingForParts:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));
                case OrderStatus.InProgress:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
                case OrderStatus.Assigned:
                case OrderStatus.Diagnosing:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E7FF"));
                case OrderStatus.Created:
                default:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
