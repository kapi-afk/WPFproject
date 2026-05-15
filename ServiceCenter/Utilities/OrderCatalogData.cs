using System.Collections.Generic;

namespace ServiceCenter.Utilities
{
    public static class OrderCatalogData
    {
        public const string LaptopDeviceType = "Ноутбук";
        public const string DesktopPcDeviceType = "Стационарный ПК";
        public const string AllInOneDeviceType = "Моноблок";
        public const string MonitorDeviceType = "Монитор";
        public const string PrinterDeviceType = "Принтер";
        public const string OtherOption = "Другое";
        public const string PickupDeliveryMethod = "Самовывоз";
        public const string CourierDeliveryMethod = "Курьер";

        public static readonly string[] DeviceTypes =
        {
            LaptopDeviceType,
            DesktopPcDeviceType,
            AllInOneDeviceType,
            MonitorDeviceType,
            PrinterDeviceType,
            OtherOption
        };

        public static readonly string[] DeliveryMethods =
        {
            PickupDeliveryMethod,
            CourierDeliveryMethod
        };

        public static readonly Dictionary<string, string[]> BrandCatalog = new Dictionary<string, string[]>
        {
            [LaptopDeviceType] = new[] { "Lenovo", "ASUS", "HP", "Acer", "Dell", "Apple", "MSI" },
            [DesktopPcDeviceType] = new[] { "Dell", "HP", "Lenovo", "ASUS", "MSI", "Acer" },
            [AllInOneDeviceType] = new[] { "Lenovo", "HP", "Apple", "Acer", "ASUS" },
            [MonitorDeviceType] = new[] { "Samsung", "LG", "AOC", "Philips", "Dell", "BenQ" },
            [PrinterDeviceType] = new[] { "HP", "Canon", "Epson", "Brother", "Xerox" },
            [OtherOption] = System.Array.Empty<string>()
        };

        public static readonly Dictionary<string, string[]> DefaultModelCatalog = new Dictionary<string, string[]>
        {
            [LaptopDeviceType] = new[] { "IdeaPad", "ThinkPad", "VivoBook", "Pavilion", "Aspire", "MacBook" },
            [DesktopPcDeviceType] = new[] { "OptiPlex", "ProDesk", "ThinkCentre", "ROG", "MAG", "Nitro" },
            [AllInOneDeviceType] = new[] { "iMac", "IdeaCentre AIO", "Aspire C", "Zen AiO", "ProOne" },
            [MonitorDeviceType] = new[] { "Odyssey", "UltraGear", "ThinkVision", "P-series", "GW", "24MK" },
            [PrinterDeviceType] = new[] { "LaserJet", "DeskJet", "PIXMA", "EcoTank", "HL-L", "WorkCentre" },
            [OtherOption] = System.Array.Empty<string>()
        };

        public static readonly Dictionary<string, Dictionary<string, string[]>> BrandModelCatalog =
            new Dictionary<string, Dictionary<string, string[]>>
            {
                [LaptopDeviceType] = new Dictionary<string, string[]>
                {
                    ["Lenovo"] = new[] { "IdeaPad", "ThinkPad", "Legion", "Yoga" },
                    ["ASUS"] = new[] { "VivoBook", "Zenbook", "ROG", "TUF" },
                    ["HP"] = new[] { "Pavilion", "Victus", "ProBook", "EliteBook" },
                    ["Acer"] = new[] { "Aspire", "Nitro", "Swift", "Predator" },
                    ["Dell"] = new[] { "Inspiron", "Latitude", "Vostro", "XPS" },
                    ["Apple"] = new[] { "MacBook Air", "MacBook Pro" },
                    ["MSI"] = new[] { "Modern", "Katana", "Prestige", "Stealth" }
                },
                [DesktopPcDeviceType] = new Dictionary<string, string[]>
                {
                    ["Dell"] = new[] { "OptiPlex", "Precision", "Inspiron" },
                    ["HP"] = new[] { "ProDesk", "EliteDesk", "Pavilion" },
                    ["Lenovo"] = new[] { "ThinkCentre", "IdeaCentre", "Legion" },
                    ["ASUS"] = new[] { "ROG", "ExpertCenter", "ProArt" },
                    ["MSI"] = new[] { "MAG", "Aegis", "Trident" },
                    ["Acer"] = new[] { "Aspire", "Veriton", "Predator" }
                },
                [AllInOneDeviceType] = new Dictionary<string, string[]>
                {
                    ["Lenovo"] = new[] { "IdeaCentre AIO", "Yoga AIO" },
                    ["HP"] = new[] { "All-in-One", "ProOne" },
                    ["Apple"] = new[] { "iMac" },
                    ["Acer"] = new[] { "Aspire C" },
                    ["ASUS"] = new[] { "Zen AiO", "Vivo AiO" }
                },
                [MonitorDeviceType] = new Dictionary<string, string[]>
                {
                    ["Samsung"] = new[] { "Odyssey", "ViewFinity", "S24" },
                    ["LG"] = new[] { "UltraGear", "UltraWide", "24MK" },
                    ["AOC"] = new[] { "Gaming", "Value Line", "Professional" },
                    ["Philips"] = new[] { "P-line", "V-line", "Momentum" },
                    ["Dell"] = new[] { "P-series", "S-series", "UltraSharp" },
                    ["BenQ"] = new[] { "GW", "EX", "PD" }
                },
                [PrinterDeviceType] = new Dictionary<string, string[]>
                {
                    ["HP"] = new[] { "LaserJet", "DeskJet", "OfficeJet" },
                    ["Canon"] = new[] { "PIXMA", "i-SENSYS", "MAXIFY" },
                    ["Epson"] = new[] { "EcoTank", "WorkForce", "L-series" },
                    ["Brother"] = new[] { "HL-L", "DCP", "MFC" },
                    ["Xerox"] = new[] { "Phaser", "VersaLink", "WorkCentre" }
                }
            };

        public static readonly Dictionary<string, string[]> ProblemCatalog = new Dictionary<string, string[]>
        {
            [LaptopDeviceType] = new[] { "Не включается", "Сильно греется", "Шумит", "Не заряжается", "Разбит экран", "Тормозит" },
            [DesktopPcDeviceType] = new[] { "Не включается", "Перезагружается", "Шумит", "Нет изображения", "Тормозит", "Не видит диск" },
            [AllInOneDeviceType] = new[] { "Не включается", "Нет изображения", "Сильно греется", "Тормозит", "Не работает сенсор" },
            [MonitorDeviceType] = new[] { "Нет изображения", "Мерцает экран", "Полосы на экране", "Разбит экран", "Не работает подсветка" },
            [PrinterDeviceType] = new[] { "Не печатает", "Зажевывает бумагу", "Полосы при печати", "Ошибка картриджа", "Не подключается" },
            [OtherOption] = new[] { "Не включается", "Работает нестабильно", "Проблема с экраном", "Проблема с подключением" }
        };
    }
}
