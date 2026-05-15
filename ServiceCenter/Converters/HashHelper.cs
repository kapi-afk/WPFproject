using BCrypt.Net;
using System;
using System.Linq;

using System.Windows;

namespace ServiceCenter.Utilities
{
    public static class HashHelper
    {
        private const int MinimumPasswordLength = 8;
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

        /// <summary>
        /// Проверяет пароль на соответствие политике сложности.
        /// </summary>
        /// <param name="password">Пароль для проверки.</param>
        /// <returns>Текст ошибки, если пароль не подходит; иначе пустую строку.</returns>
        public static string GetPasswordValidationError(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return GetString("PasswordRequiredMessage", "Enter a password.");
            }

            if (password.Length < MinimumPasswordLength)
            {
                return string.Format(
                    GetString("PasswordMinLengthFormat", "Password must contain at least {0} characters."),
                    MinimumPasswordLength);
            }

            var hasLetter = password.Any(char.IsLetter);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecialCharacter = password.Any(character =>
                !char.IsLetterOrDigit(character) &&
                !char.IsWhiteSpace(character));

            if (!hasLetter || !hasDigit || !hasSpecialCharacter)
            {
                return GetString("PasswordComplexityMessage", "Password must contain letters, digits, and special characters.");
            }

            return string.Empty;
        }

        private static string GetString(string key, string fallback)
        {
            return Application.Current?.TryFindResource(key)?.ToString() ?? fallback;
        }
    }
}
