using System;

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
    }
}
