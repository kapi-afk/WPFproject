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
            NormalizeEstimatedCosts(order);
            NormalizePaymentState(order);
            if (order.Status == OrderStatus.Completed)
            {
                order.CompletedAt = order.CompletedAt ?? order.CreatedAt;
            }
            else
            {
                order.CompletedAt = null;
            }
            order.UpdatedAt = order.CreatedAt;
            _context.Orders.Add(order);
            _context.SaveChanges();

            if (string.IsNullOrWhiteSpace(order.PublicNumber))
            {
                order.PublicNumber = OrderPublicNumberService.GetOrCreate(order);
                _context.SaveChanges();
            }
        }

        public void Update(Order order)
        {
            var previousStatus = _context.Orders
                .AsNoTracking()
                .Where(item => item.Id == order.Id)
                .Select(item => item.Status)
                .FirstOrDefault();

            NormalizeEstimatedCosts(order);
            NormalizePaymentState(order);
            if (order.Status == OrderStatus.Completed)
            {
                order.CompletedAt = order.CompletedAt ?? DateTime.Now;
            }
            else
            {
                order.CompletedAt = null;
            }
            order.UpdatedAt = DateTime.Now;
            _context.Orders.Update(order);
            _context.SaveChanges();

            if (previousStatus != order.Status)
            {
                var updatedOrder = GetById(order.Id);
                var emailSnapshot = CreateEmailSnapshot(updatedOrder);
                Task.Run(() => OrderEmailService.TrySendOrderStatusChangedEmail(emailSnapshot, previousStatus));
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

        public void EnsurePublicNumbers()
        {
            var ordersToUpdate = _context.Orders
                .Where(order => string.IsNullOrWhiteSpace(order.PublicNumber))
                .ToList();

            if (!ordersToUpdate.Any())
            {
                return;
            }

            foreach (var order in ordersToUpdate)
            {
                order.PublicNumber = OrderPublicNumberService.GetOrCreate(order);
            }

            _context.SaveChanges();
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

        private static void NormalizeEstimatedCosts(Order order)
        {
            if (order == null)
            {
                return;
            }

            order.EstimatedPartsCost = order.EstimatedPartsCost < 0 ? 0 : order.EstimatedPartsCost;
            order.MasterWorkCost = order.MasterWorkCost < 0 ? 0 : order.MasterWorkCost;

            if (order.EstimatedPartsCost == 0 &&
                order.MasterWorkCost == 0 &&
                order.EstimatedRepairCost > 0)
            {
                order.EstimatedPartsCost = order.EstimatedRepairCost;
            }

            order.EstimatedRepairCost = order.EstimatedPartsCost + order.MasterWorkCost;
        }

        private static void NormalizePaymentState(Order order)
        {
            if (order == null)
            {
                return;
            }

            order.PaymentMethod = string.IsNullOrWhiteSpace(order.PaymentMethod)
                ? Order.OnSitePaymentMethod
                : order.PaymentMethod.Trim();

            if (!order.IsOnlinePayment)
            {
                order.IsOnlinePaymentCompleted = false;
                order.OnlinePaymentPaidAt = null;
                return;
            }

            if (order.OnlinePaymentPaidAt.HasValue)
            {
                order.IsOnlinePaymentCompleted = true;
            }

            if (order.IsOnlinePaymentCompleted)
            {
                order.OnlinePaymentPaidAt = order.OnlinePaymentPaidAt ?? DateTime.Now;
            }
            else
            {
                order.OnlinePaymentPaidAt = null;
            }
        }

        private static Order CreateEmailSnapshot(Order source)
        {
            if (source == null)
            {
                return null;
            }

            return new Order
            {
                Id = source.Id,
                PublicNumber = source.PublicNumber,
                DeviceType = source.DeviceType,
                DeviceBrand = source.DeviceBrand,
                DeviceModel = source.DeviceModel,
                Status = source.Status,
                PaymentMethod = source.PaymentMethod,
                IsOnlinePaymentCompleted = source.IsOnlinePaymentCompleted,
                OnlinePaymentPaidAt = source.OnlinePaymentPaidAt,
                EstimatedPartsCost = source.EstimatedPartsCost,
                MasterWorkCost = source.MasterWorkCost,
                EstimatedRepairCost = source.EstimatedRepairCost,
                User = source.User == null
                    ? null
                    : new User
                    {
                        Id = source.User.Id,
                        Name = source.User.Name,
                        Email = source.User.Email
                    },
                AssignedMaster = source.AssignedMaster == null
                    ? null
                    : new User
                    {
                        Id = source.AssignedMaster.Id,
                        Name = source.AssignedMaster.Name,
                        Email = source.AssignedMaster.Email
                    }
            };
        }
    }

    // Статический менеджер репозиториев
}
