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
                case "РќРѕСѓС‚Р±СѓРє":
                    return "DeviceTypeLaptop";
                case "РЎС‚Р°С†РёРѕРЅР°СЂРЅС‹Р№ РџРљ":
                    return "DeviceTypeDesktopPc";
                case "РњРѕРЅРѕР±Р»РѕРє":
                    return "DeviceTypeAllInOne";
                case "РњРѕРЅРёС‚РѕСЂ":
                    return "DeviceTypeMonitor";
                case "РџСЂРёРЅС‚РµСЂ":
                    return "DeviceTypePrinter";
                case "Р”СЂСѓРіРѕРµ":
                case "Other":
                    return "OtherOption";
                case "РќРµ РІРєР»СЋС‡Р°РµС‚СЃСЏ":
                    return "ProblemNoPower";
                case "РЎРёР»СЊРЅРѕ РіСЂРµРµС‚СЃСЏ":
                    return "ProblemOverheats";
                case "РЁСѓРјРёС‚":
                    return "ProblemNoisy";
                case "РќРµ Р·Р°СЂСЏР¶Р°РµС‚СЃСЏ":
                    return "ProblemNotCharging";
                case "Р Р°Р·Р±РёС‚ СЌРєСЂР°РЅ":
                    return "ProblemBrokenScreen";
                case "РўРѕСЂРјРѕР·РёС‚":
                    return "ProblemSlow";
                case "РџРµСЂРµР·Р°РіСЂСѓР¶Р°РµС‚СЃСЏ":
                    return "ProblemRestarts";
                case "РќРµС‚ РёР·РѕР±СЂР°Р¶РµРЅРёСЏ":
                    return "ProblemNoImage";
                case "РќРµ РІРёРґРёС‚ РґРёСЃРє":
                    return "ProblemDriveNotDetected";
                case "РќРµ СЂР°Р±РѕС‚Р°РµС‚ СЃРµРЅСЃРѕСЂ":
                    return "ProblemTouchNotWorking";
                case "РњРµСЂС†Р°РµС‚ СЌРєСЂР°РЅ":
                    return "ProblemFlickeringScreen";
                case "РџРѕР»РѕСЃС‹ РЅР° СЌРєСЂР°РЅРµ":
                    return "ProblemLinesOnScreen";
                case "РќРµ СЂР°Р±РѕС‚Р°РµС‚ РїРѕРґСЃРІРµС‚РєР°":
                    return "ProblemBacklightNotWorking";
                case "РќРµ РїРµС‡Р°С‚Р°РµС‚":
                    return "ProblemNotPrinting";
                case "Р—Р°Р¶РµРІС‹РІР°РµС‚ Р±СѓРјР°РіСѓ":
                    return "ProblemPaperJam";
                case "РџРѕР»РѕСЃС‹ РїСЂРё РїРµС‡Р°С‚Рё":
                    return "ProblemPrintLines";
                case "РћС€РёР±РєР° РєР°СЂС‚СЂРёРґР¶Р°":
                    return "ProblemCartridgeError";
                case "РќРµ РїРѕРґРєР»СЋС‡Р°РµС‚СЃСЏ":
                    return "ProblemNotConnecting";
                case "Р Р°Р±РѕС‚Р°РµС‚ РЅРµСЃС‚Р°Р±РёР»СЊРЅРѕ":
                    return "ProblemUnstableOperation";
                case "РџСЂРѕР±Р»РµРјР° СЃ СЌРєСЂР°РЅРѕРј":
                    return "ProblemScreenIssue";
                case "РџСЂРѕР±Р»РµРјР° СЃ РїРѕРґРєР»СЋС‡РµРЅРёРµРј":
                    return "ProblemConnectionIssue";
                default:
                    return null;
            }
        }
    }
}
