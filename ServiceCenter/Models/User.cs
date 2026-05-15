using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Models
{
    public enum UserRole
    {
        Admin,
        Client,
        Master
    }

    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string HashPassword { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string MasterSpecializations { get; set; }
        public byte[] Photo { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Order> AssignedOrders { get; set; } = new List<Order>();

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }

            if (!string.IsNullOrWhiteSpace(Login))
            {
                return Login;
            }

            return $"User #{Id}";
        }
    }
}
