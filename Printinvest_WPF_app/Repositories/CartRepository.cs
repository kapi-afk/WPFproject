using Microsoft.EntityFrameworkCore;
using Printinvest_WPF_app.Contex;
using Printinvest_WPF_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printinvest_WPF_app.Repositories
{
    public class CartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public Cart GetByUserId(int userId)
        {
            return _context.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId);
        }

        public void Add(Cart cart)
        {
            _context.Carts.Add(cart);
            _context.SaveChanges();
        }

        public void Update(Cart cart)
        {
            _context.Carts.Update(cart);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var cart = _context.Carts.Find(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                _context.SaveChanges();
            }
        }

        public void AddItemToCart(int cartId, CartItem item)
        {
            var cart = _context.Carts.Find(cartId);
            if (cart != null)
            {
                cart.Items.Add(item);
                RepositoryManager.Analytics.Add(
    new Analytic
    {
        ProductId = item.ProductId,
        ServiceId = item.ServiceId,
        Timestamp = DateTime.UtcNow,
        Action = "Cart",
        UserId = SessionManager.CurrentUser.Id,
    }
    );
                _context.SaveChanges();
            }
        }

        public void RemoveItemFromCart(int cartId, int itemId)
        {
            var cart = _context.Carts.Include(c => c.Items).FirstOrDefault(c => c.Id == cartId);
            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
                if (item != null)
                {
                    cart.Items.Remove(item);
                    _context.SaveChanges();
                }
            }
        }
    }

}
