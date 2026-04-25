using Printinvest_WPF_app.Contex;
using Printinvest_WPF_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printinvest_WPF_app.Repositories
{
    public class AnalyticRepository
    {
        private readonly AppDbContext _context;

        public AnalyticRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Analytic> GetAll()
        {
            return _context.Analytics.ToList();
        }

        public Analytic GetById(int id)
        {
            return _context.Analytics.Find(id);
        }

        public List<Analytic> GetByUserId(int userId)
        {
            return _context.Analytics.Where(a => a.UserId == userId).ToList();
        }

        public List<Analytic> GetByProductId(int productId)
        {
            return _context.Analytics.Where(a => a.ProductId == productId).ToList();
        }

        public List<Analytic> GetByServiceId(int serviceId)
        {
            return _context.Analytics.Where(a => a.ServiceId == serviceId).ToList();
        }

        public void Add(Analytic analytic)
        {
            _context.Analytics.Add(analytic);
            _context.SaveChanges();
        }

        public void Update(Analytic analytic)
        {
            _context.Analytics.Update(analytic);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var analytic = _context.Analytics.Find(id);
            if (analytic != null)
            {
                _context.Analytics.Remove(analytic);
                _context.SaveChanges();
            }
        }
    }
}
