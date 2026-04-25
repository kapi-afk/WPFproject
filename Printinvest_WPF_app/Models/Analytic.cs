using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printinvest_WPF_app.Models
{
    public class Analytic
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? ProductId { get; set; }
        public Product Product { get; set; }
        public int? ServiceId { get; set; }
        public Service Service { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
    }
}
