using ServiceCenter.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ServiceCenter.Utilities
{
    public static class OrderPublicNumberService
    {
        private const string Prefix = "SC";
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string GetOrCreate(Order order)
        {
            if (order == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(order.PublicNumber))
            {
                return order.PublicNumber.Trim().ToUpperInvariant();
            }

            if (order.Id <= 0)
            {
                return string.Empty;
            }

            var createdAt = order.CreatedAt == default(DateTime)
                ? DateTime.MinValue
                : order.CreatedAt.ToUniversalTime();
            var seed = $"{order.Id}|{order.UserId}|{createdAt:O}|ServiceCenter";
            byte[] hash;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(seed));
            }
            var shortCode = new string(hash.Take(8)
                .Select(value => Alphabet[value % Alphabet.Length])
                .ToArray());

            return $"{Prefix}-{shortCode}";
        }
    }
}
