using Microsoft.EntityFrameworkCore;
using Printinvest_WPF_app.Contex;
using Printinvest_WPF_app.Models;
using Printinvest_WPF_app.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printinvest_WPF_app.Repositories
{
    public class OrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Order> GetAll()
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.AssignedMaster)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
        }

        public Order GetById(int id)
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.AssignedMaster)
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id);
        }

        public List<Order> GetByUserId(int userId)
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.AssignedMaster)
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
        }

        public List<Order> GetByAssignedMasterId(int masterId)
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.AssignedMaster)
                .Include(o => o.Items)
                .Where(o => o.AssignedMasterId == masterId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
        }

        public void Add(Order order)
        {
            AssignBestMasterIfNeeded(order);
            order.UpdatedAt = order.CreatedAt;
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        public void Update(Order order)
        {
            var previousStatus = _context.Orders
                .AsNoTracking()
                .Where(item => item.Id == order.Id)
                .Select(item => item.Status)
                .FirstOrDefault();

            order.UpdatedAt = DateTime.Now;
            _context.Orders.Update(order);
            _context.SaveChanges();

            if (previousStatus != order.Status)
            {
                var updatedOrder = GetById(order.Id);
                OrderEmailService.TrySendOrderStatusChangedEmail(updatedOrder, previousStatus);
            }
        }

        public void Delete(int id)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }
        }

        private void AssignBestMasterIfNeeded(Order order)
        {
            if (order == null ||
                order.AssignedMasterId.HasValue ||
                order.Status != OrderStatus.Created ||
                string.IsNullOrWhiteSpace(order.DeviceType))
            {
                return;
            }

            var master = MasterAssignmentService.FindBestMaster(
                order.DeviceType,
                _context.Users.AsNoTracking().Where(user => user.Role == UserRole.Master).ToList(),
                _context.Orders.AsNoTracking().ToList());

            if (master == null)
            {
                return;
            }

            order.AssignedMasterId = master.Id;
            order.Status = OrderStatus.Assigned;
        }
    }

    // Статический менеджер репозиториев
}
