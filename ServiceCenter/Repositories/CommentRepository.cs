using Microsoft.EntityFrameworkCore;
using ServiceCenter.Contex;
using ServiceCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Repositories
{
    public class CommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Comment> GetAll()
        {
            return _context.Comments
                .Include(c => c.User)
                .Include(c => c.Product)
                .OrderByDescending(c => c.Timestamp)
                .ToList();
        }

        public Comment GetById(int id)
        {
            return _context.Comments.Find(id);
        }

        public List<Comment> GetByProductId(int productId)
        {
            return _context.Comments.Include(c => c.User)
                .Where(c => c.ProductId == productId)
                .OrderByDescending(c => c.Timestamp)
                .Take(50)
                .ToList();
        }

        public List<Comment> GetPublicReviews()
        {
            return _context.Comments
                .Include(c => c.User)
                .Where(c => c.ProductId == null)
                .OrderByDescending(c => c.Timestamp)
                .Take(50)
                .ToList();
        }

        public void Add(Comment comment)
        {
            _context.Comments.Add(comment);
            _context.SaveChanges();
        }

        public void Update(Comment comment)
        {
            _context.Comments.Update(comment);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var comment = _context.Comments.Find(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                _context.SaveChanges();
            }
        }
    }
}
