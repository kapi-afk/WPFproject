using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printinvest_WPF_app.Models
{
    public interface IItem
    {
        int Id { get; }
        string Name { get; }
        string Description { get; }
        byte[] Photo { get; }
    }
    public class Product : IItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Characteristics { get; set; }
        public decimal Price { get; set; } // Стоимость в BYN
        public byte[] Photo { get; set; }
    }

    public class Service : IItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Characteristics { get; set; }
        public decimal Price { get; set; } // Стоимость в BYN
        public byte[] Photo { get; set; }
    }

}
