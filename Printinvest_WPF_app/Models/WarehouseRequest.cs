using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printinvest_WPF_app.Models
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
                if (string.Equals(Status, "Нужно", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Status, "Запрошено", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Status, "Новая заявка", StringComparison.OrdinalIgnoreCase))
                {
                    return App.GetString("WarehouseRequestStatusPending", "Requested");
                }

                if (string.Equals(Status, "Обработано", StringComparison.OrdinalIgnoreCase))
                {
                    return App.GetString("WarehouseRequestStatusProcessed", "Processed");
                }

                return Status;
            }
        }
    }
}
