using BCrypt.Net;
using System;

namespace Printinvest_WPF_app.Utilities
{
    public static class HashHelper
    {
        /// <summary>
        /// Создаёт хеш пароля с использованием BCrypt.
        /// </summary>
        /// <param name="password">Пароль для хеширования.</param>
        /// <returns>Хеш пароля.</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Проверяет, соответствует ли пароль указанному хешу.
        /// </summary>
        /// <param name="password">Пароль для проверки.</param>
        /// <param name="hashedPassword">Хеш пароля для сравнения.</param>
        /// <returns>True, если пароль соответствует хешу; иначе False.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));
            }

            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                throw new ArgumentException("Хеш пароля не может быть пустым.", nameof(hashedPassword));
            }

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}