using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ServiceCenter.Converters
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
                case "РќРѕСѓС‚Р±СѓРє":
                    return "DeviceTypeLaptop";
                case "Стационарный ПК":
                case "РЎС‚Р°С†РёРѕРЅР°СЂРЅС‹Р№ РџРљ":
                    return "DeviceTypeDesktopPc";
                case "Моноблок":
                case "РњРѕРЅРѕР±Р»РѕРє":
                    return "DeviceTypeAllInOne";
                case "Монитор":
                case "РњРѕРЅРёС‚РѕСЂ":
                    return "DeviceTypeMonitor";
                case "Принтер":
                case "РџСЂРёРЅС‚РµСЂ":
                    return "DeviceTypePrinter";
                case "Другое":
                case "Р”СЂСѓРіРѕРµ":
                case "Other":
                    return "OtherOption";
                case "Не включается":
                case "РќРµ РІРєР»СЋС‡Р°РµС‚СЃСЏ":
                    return "ProblemNoPower";
                case "Сильно греется":
                case "РЎРёР»СЊРЅРѕ РіСЂРµРµС‚СЃСЏ":
                    return "ProblemOverheats";
                case "Шумит":
                case "РЁСѓРјРёС‚":
                    return "ProblemNoisy";
                case "Не заряжается":
                case "РќРµ Р·Р°СЂСЏР¶Р°РµС‚СЃСЏ":
                    return "ProblemNotCharging";
                case "Разбит экран":
                case "Р Р°Р·Р±РёС‚ СЌРєСЂР°РЅ":
                    return "ProblemBrokenScreen";
                case "Тормозит":
                case "РўРѕСЂРјРѕР·РёС‚":
                    return "ProblemSlow";
                case "Перезагружается":
                case "РџРµСЂРµР·Р°РіСЂСѓР¶Р°РµС‚СЃСЏ":
                    return "ProblemRestarts";
                case "Нет изображения":
                case "РќРµС‚ РёР·РѕР±СЂР°Р¶РµРЅРёСЏ":
                    return "ProblemNoImage";
                case "Не видит диск":
                case "РќРµ РІРёРґРёС‚ РґРёСЃРє":
                    return "ProblemDriveNotDetected";
                case "Не работает сенсор":
                case "РќРµ СЂР°Р±РѕС‚Р°РµС‚ СЃРµРЅСЃРѕСЂ":
                    return "ProblemTouchNotWorking";
                case "Мерцает экран":
                case "РњРµСЂС†Р°РµС‚ СЌРєСЂР°РЅ":
                    return "ProblemFlickeringScreen";
                case "Полосы на экране":
                case "РџРѕР»РѕСЃС‹ РЅР° СЌРєСЂР°РЅРµ":
                    return "ProblemLinesOnScreen";
                case "Не работает подсветка":
                case "РќРµ СЂР°Р±РѕС‚Р°РµС‚ РїРѕРґСЃРІРµС‚РєР°":
                    return "ProblemBacklightNotWorking";
                case "Не печатает":
                case "РќРµ РїРµС‡Р°С‚Р°РµС‚":
                    return "ProblemNotPrinting";
                case "Зажевывает бумагу":
                case "Р—Р°Р¶РµРІС‹РІР°РµС‚ Р±СѓРјР°РіСѓ":
                    return "ProblemPaperJam";
                case "Полосы при печати":
                case "РџРѕР»РѕСЃС‹ РїСЂРё РїРµС‡Р°С‚Рё":
                    return "ProblemPrintLines";
                case "Ошибка картриджа":
                case "РћС€РёР±РєР° РєР°СЂС‚СЂРёРґР¶Р°":
                    return "ProblemCartridgeError";
                case "Не подключается":
                case "РќРµ РїРѕРґРєР»СЋС‡Р°РµС‚СЃСЏ":
                    return "ProblemNotConnecting";
                case "Работает нестабильно":
                case "Р Р°Р±РѕС‚Р°РµС‚ РЅРµСЃС‚Р°Р±РёР»СЊРЅРѕ":
                    return "ProblemUnstableOperation";
                case "Проблема с экраном":
                case "РџСЂРѕР±Р»РµРјР° СЃ СЌРєСЂР°РЅРѕРј":
                    return "ProblemScreenIssue";
                case "Проблема с подключением":
                case "РџСЂРѕР±Р»РµРјР° СЃ РїРѕРґРєР»СЋС‡РµРЅРёРµРј":
                    return "ProblemConnectionIssue";
                default:
                    return null;
            }
        }
    }
}
