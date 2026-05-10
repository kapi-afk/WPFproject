using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Printinvest_WPF_app.Converters
{
    public class CatalogOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                return value;
            }

            var resourceKey = GetResourceKey(text);
            return resourceKey == null
                ? text
                : Application.Current.TryFindResource(resourceKey)?.ToString() ?? text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetResourceKey(string value)
        {
            switch (value)
            {
                case "Ноутбук":
                    return "DeviceTypeLaptop";
                case "Стационарный ПК":
                    return "DeviceTypeDesktopPc";
                case "Моноблок":
                    return "DeviceTypeAllInOne";
                case "Монитор":
                    return "DeviceTypeMonitor";
                case "Принтер":
                    return "DeviceTypePrinter";
                case "Другое":
                case "Other":
                    return "OtherOption";
                case "Не включается":
                    return "ProblemNoPower";
                case "Сильно греется":
                    return "ProblemOverheats";
                case "Шумит":
                    return "ProblemNoisy";
                case "Не заряжается":
                    return "ProblemNotCharging";
                case "Разбит экран":
                    return "ProblemBrokenScreen";
                case "Тормозит":
                    return "ProblemSlow";
                case "Перезагружается":
                    return "ProblemRestarts";
                case "Нет изображения":
                    return "ProblemNoImage";
                case "Не видит диск":
                    return "ProblemDriveNotDetected";
                case "Не работает сенсор":
                    return "ProblemTouchNotWorking";
                case "Мерцает экран":
                    return "ProblemFlickeringScreen";
                case "Полосы на экране":
                    return "ProblemLinesOnScreen";
                case "Не работает подсветка":
                    return "ProblemBacklightNotWorking";
                case "Не печатает":
                    return "ProblemNotPrinting";
                case "Зажевывает бумагу":
                    return "ProblemPaperJam";
                case "Полосы при печати":
                    return "ProblemPrintLines";
                case "Ошибка картриджа":
                    return "ProblemCartridgeError";
                case "Не подключается":
                    return "ProblemNotConnecting";
                case "Работает нестабильно":
                    return "ProblemUnstableOperation";
                case "Проблема с экраном":
                    return "ProblemScreenIssue";
                case "Проблема с подключением":
                    return "ProblemConnectionIssue";
                default:
                    return null;
            }
        }
    }
}
