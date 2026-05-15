using ServiceCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceCenter.Utilities
{
    public static class MasterAssignmentService
    {
        public const string LaptopSpecialization = "РќРѕСѓС‚Р±СѓРєРё";
        public const string ComputerSpecialization = "РџРљ";
        public const string OfficeEquipmentSpecialization = "РћСЂРіС‚РµС…РЅРёРєР°";

        public static User FindBestMaster(string deviceType, IEnumerable<User> masters, IEnumerable<Order> orders)
        {
            var requiredSpecialization = GetRequiredSpecialization(deviceType);
            var availableMasters = (masters ?? Enumerable.Empty<User>())
                .Where(master => master.Role == UserRole.Master);

            if (!string.IsNullOrWhiteSpace(requiredSpecialization))
            {
                availableMasters = availableMasters
                    .Where(master => HasSpecialization(master, requiredSpecialization));
            }

            return availableMasters
                .Select(master => new
                {
                    Master = master,
                    ActiveOrderCount = (orders ?? Enumerable.Empty<Order>()).Count(order =>
                        order.AssignedMasterId == master.Id &&
                        order.Status != OrderStatus.Completed &&
                        order.Status != OrderStatus.Cancelled)
                })
                .OrderBy(item => item.ActiveOrderCount)
                .ThenBy(item => item.Master.Id)
                .Select(item => item.Master)
                .FirstOrDefault();
        }

        public static string BuildSpecializations(bool laptops, bool computers, bool officeEquipment)
        {
            var values = new List<string>();

            if (laptops)
            {
                values.Add(LaptopSpecialization);
            }

            if (computers)
            {
                values.Add(ComputerSpecialization);
            }

            if (officeEquipment)
            {
                values.Add(OfficeEquipmentSpecialization);
            }

            return string.Join(";", values);
        }

        public static bool HasSpecialization(User master, string specialization)
        {
            var normalizedSpecialization = Normalize(specialization);
            return GetSpecializations(master)
                .Any(item => Normalize(item) == normalizedSpecialization ||
                             Normalize(item).Contains(normalizedSpecialization) ||
                             normalizedSpecialization.Contains(Normalize(item)));
        }

        private static IEnumerable<string> GetSpecializations(User master)
        {
            return (master?.MasterSpecializations ?? string.Empty)
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim());
        }

        private static string GetRequiredSpecialization(string deviceType)
        {
            var normalizedDeviceType = Normalize(deviceType);

            if (normalizedDeviceType.Contains("РЅРѕСѓС‚") ||
                normalizedDeviceType.Contains("laptop"))
            {
                return LaptopSpecialization;
            }

            if (normalizedDeviceType.Contains("РїРє") ||
                normalizedDeviceType.Contains("РєРѕРјРїСЊСЋС‚") ||
                normalizedDeviceType.Contains("СЃРёСЃС‚РµРј") ||
                normalizedDeviceType.Contains("РјРѕРЅРѕР±Р»РѕРє") ||
                normalizedDeviceType.Contains("desktop") ||
                normalizedDeviceType.Contains("allinone"))
            {
                return ComputerSpecialization;
            }

            if (normalizedDeviceType.Contains("РјРѕРЅРёС‚РѕСЂ") ||
                normalizedDeviceType.Contains("РїСЂРёРЅС‚РµСЂ") ||
                normalizedDeviceType.Contains("РјС„Сѓ") ||
                normalizedDeviceType.Contains("РїРµС‡Р°С‚СЊ") ||
                normalizedDeviceType.Contains("monitor") ||
                normalizedDeviceType.Contains("printer"))
            {
                return OfficeEquipmentSpecialization;
            }

            switch (deviceType)
            {
                case "РќРѕСѓС‚Р±СѓРє":
                case "Laptop":
                    return LaptopSpecialization;
                case "РЎС‚Р°С†РёРѕРЅР°СЂРЅС‹Р№ РџРљ":
                case "Desktop PC":
                case "РњРѕРЅРѕР±Р»РѕРє":
                case "All-in-one":
                    return ComputerSpecialization;
                case "РњРѕРЅРёС‚РѕСЂ":
                case "Monitor":
                case "РџСЂРёРЅС‚РµСЂ":
                case "Printer":
                    return OfficeEquipmentSpecialization;
                default:
                    return null;
            }
        }

        private static string Normalize(string value)
        {
            return new string((value ?? string.Empty)
                .ToLowerInvariant()
                .Where(ch => !char.IsWhiteSpace(ch) && ch != '-' && ch != '_' && ch != ';' && ch != ',')
                .ToArray());
        }
    }
}
