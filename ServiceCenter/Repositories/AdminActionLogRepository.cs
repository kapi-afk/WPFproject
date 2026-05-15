using ServiceCenter.Contex;
using ServiceCenter.Models;
using System.Collections.Generic;
using System.Linq;

namespace ServiceCenter.Repositories
{
    public class AdminActionLogRepository
    {
        private readonly AppDbContext _context;

        public AdminActionLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<AdminActionLog> GetAll()
        {
            return _context.Set<AdminActionLog>()
                .OrderByDescending(item => item.CreatedAt)
                .ThenByDescending(item => item.Id)
                .ToList();
        }

        public void Add(AdminActionLog log)
        {
            _context.Set<AdminActionLog>().Add(log);
            _context.SaveChanges();
        }
    }
}
