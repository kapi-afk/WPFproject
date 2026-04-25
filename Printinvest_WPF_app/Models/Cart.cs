using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Printinvest_WPF_app.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;

        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; }
        public int? ProductId { get; set; }
        public Product Product { get; set; }
        public int? ServiceId { get; set; }
        public Service Service { get; set; }



        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ItemName => Product?.Name ?? Service?.Name ?? "Неизвестный элемент";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
