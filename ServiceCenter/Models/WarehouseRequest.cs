using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Models
{
    public class WarehouseRequest
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int? MasterId { get; set; }
        public User Master { get; set; }
        public int? WarehouseItemId { get; set; }
        public WarehouseItem WarehouseItem { get; set; }
        public string RequestedItemName { get; set; }
        public string RequestedCategory { get; set; }
        public int RequestedQuantity { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public string OrderDisplayNumber => Order?.DisplayNumber ?? $"{App.GetString("OrderNumberShort", "No.")} {OrderId}";

        [NotMapped]
        public string DisplayStatus
        {
            get
            {
                if (MatchesStatus("Нужно", "РќСѓР¶РЅРѕ") ||
                    MatchesStatus("Запрошено", "Р—Р°РїСЂРѕС€РµРЅРѕ") ||
                    MatchesStatus("Новая заявка", "РќРѕРІР°СЏ Р·Р°СЏРІРєР°"))
                {
                    return App.GetString("WarehouseRequestStatusPending", "Requested");
                }

                if (MatchesStatus("Обработано", "РћР±СЂР°Р±РѕС‚Р°РЅРѕ"))
                {
                    return App.GetString("WarehouseRequestStatusProcessed", "Processed");
                }

                return Status;
            }
        }

        private bool MatchesStatus(string expected, string legacyExpected)
        {
            return string.Equals(Status, expected, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(Status, legacyExpected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
