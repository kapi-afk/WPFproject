using ServiceCenter.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ServiceCenter.Converters
{
    public class OrderStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                string resourceKey;
                switch (status)
                {
                    case OrderStatus.Created:
                        resourceKey = "OrderStatusCreated";
                        break;
                    case OrderStatus.Assigned:
                        resourceKey = "OrderStatusAssigned";
                        break;
                    case OrderStatus.Diagnosing:
                        resourceKey = "OrderStatusDiagnosing";
                        break;
                    case OrderStatus.WaitingForParts:
                        resourceKey = "OrderStatusWaitingForParts";
                        break;
                    case OrderStatus.InProgress:
                        resourceKey = "OrderStatusInProgress";
                        break;
                    case OrderStatus.ReadyForPickup:
                        resourceKey = "OrderStatusReadyForPickup";
                        break;
                    case OrderStatus.Completed:
                        resourceKey = "OrderStatusCompleted";
                        break;
                    case OrderStatus.Cancelled:
                        resourceKey = "OrderStatusCancelled";
                        break;
                    default:
                        resourceKey = string.Empty;
                        break;
                }
                return Application.Current.TryFindResource(resourceKey)?.ToString() ?? status.ToString();
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
