using System;

namespace ServiceCenter.Models
{
    public class AdminActionLog
    {
        public int Id { get; set; }
        public string AdminLogin { get; set; }
        public string ActionType { get; set; }
        public string EntityType { get; set; }
        public int? EntityId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
