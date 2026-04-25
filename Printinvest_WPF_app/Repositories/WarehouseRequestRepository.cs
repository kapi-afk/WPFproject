using Microsoft.EntityFrameworkCore;
using Printinvest_WPF_app.Contex;
using Printinvest_WPF_app.Models;
using System.Collections.Generic;
using System.Linq;

namespace Printinvest_WPF_app.Repositories
{
    public class WarehouseRequestRepository
    {
        private readonly AppDbContext _context;

        public WarehouseRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<WarehouseRequest> GetAll()
        {
            return _context.Set<WarehouseRequest>()
                .Include(request => request.Order)
                .ThenInclude(order => order.User)
                .Include(request => request.Master)
                .Include(request => request.WarehouseItem)
                .OrderByDescending(request => request.CreatedAt)
                .ToList();
        }

        public List<WarehouseRequest> GetByOrderId(int orderId)
        {
            return GetAll()
                .Where(request => request.OrderId == orderId)
                .ToList();
        }

        public void Add(WarehouseRequest request)
        {
            _context.Set<WarehouseRequest>().Add(request);
            _context.SaveChanges();
        }

        public void Update(WarehouseRequest request)
        {
            _context.Set<WarehouseRequest>().Update(request);
            _context.SaveChanges();
        }
    }
}
