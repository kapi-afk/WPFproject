using ServiceCenter.Contex;
using ServiceCenter.Models;
using System.Collections.Generic;
using System.Linq;

namespace ServiceCenter.Repositories
{
    public class WarehouseRepository
    {
        private readonly AppDbContext _context;

        public WarehouseRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<WarehouseItem> GetAll()
        {
            return _context.WarehouseItems
                .OrderBy(item => item.Name)
                .ToList();
        }

        public WarehouseItem GetById(int id)
        {
            return _context.WarehouseItems.Find(id);
        }

        public void Add(WarehouseItem item)
        {
            _context.WarehouseItems.Add(item);
            _context.SaveChanges();
        }

        public void Update(WarehouseItem item)
        {
            _context.WarehouseItems.Update(item);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var item = _context.WarehouseItems.Find(id);
            if (item != null)
            {
                _context.WarehouseItems.Remove(item);
                _context.SaveChanges();
            }
        }
    }
}
