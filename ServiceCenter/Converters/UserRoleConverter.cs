using ServiceCenter.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ServiceCenter.Converters
{
    public class UserRoleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is UserRole role))
            {
                return value?.ToString();
            }

            string resourceKey;
            string fallback;

            switch (role)
            {
                case UserRole.Admin:
                    resourceKey = "UserRoleAdmin";
                    fallback = "Administrator";
                    break;
                case UserRole.Client:
                    resourceKey = "UserRoleClient";
                    fallback = "Client";
                    break;
                case UserRole.Master:
                    resourceKey = "UserRoleMaster";
                    fallback = "Master";
                    break;
                default:
                    return role.ToString();
            }

            return Application.Current.TryFindResource(resourceKey)?.ToString() ?? fallback;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
