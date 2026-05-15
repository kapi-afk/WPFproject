namespace ServiceCenter.Models
{
    public class WarehouseItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public int MinimumQuantity { get; set; }
        public string Notes { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
