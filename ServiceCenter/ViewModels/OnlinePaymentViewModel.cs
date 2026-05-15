using ServiceCenter.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServiceCenter.ViewModels
{
    public class OnlinePaymentViewModel : BaseViewModel
    {
        private readonly Order _order;
        private string _cardholderName;
        private string _cardNumber;
        private string _expiryDate;
        private string _cvv;
        private bool _showValidationErrors;

        public OnlinePaymentViewModel(Order order)
        {
            _order = order;
        }

        public string OrderNumber => _order?.DisplayNumber ?? string.Empty;
        public decimal AmountToPay => _order?.EstimatedRepairCost ?? 0;
        public string DeviceDisplay => string.Join(" ", new[] { _order?.DeviceBrand, _order?.DeviceModel }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

        public string CardholderName
        {
            get => _cardholderName;
            set
            {
                var normalizedValue = NormalizeCardholderName(value);
                if (SetProperty(ref _cardholderName, normalizedValue))
                {
                    NotifyValidationStateChanged();
                }
            }
        }

        public string CardNumber
        {
            get => _cardNumber;
            set
            {
                var normalizedValue = FormatCardNumber(value);
                if (SetProperty(ref _cardNumber, normalizedValue))
                {
                    NotifyValidationStateChanged();
                }
            }
        }

        public string ExpiryDate
        {
            get => _expiryDate;
            set
            {
                var normalizedValue = FormatExpiryDate(value);
                if (SetProperty(ref _expiryDate, normalizedValue))
                {
                    NotifyValidationStateChanged();
                }
            }
        }

        public string Cvv
        {
            get => _cvv;
            set
            {
                var normalizedValue = FormatCvv(value);
                if (SetProperty(ref _cvv, normalizedValue))
                {
                    NotifyValidationStateChanged();
                }
            }
        }

        public bool ShowValidationErrors
        {
            get => _showValidationErrors;
            private set
            {
                if (SetProperty(ref _showValidationErrors, value))
                {
                    NotifyValidationStateChanged();
                }
            }
        }

        public bool IsCardholderNameInvalid => ShowValidationErrors && !IsValidCardholderName(CardholderName);
        public bool IsCardNumberInvalid => ShowValidationErrors && !IsValidCardNumber(CardNumber);
        public bool IsExpiryDateInvalid => ShowValidationErrors && !IsValidExpiryDate(ExpiryDate);
        public bool IsCvvInvalid => ShowValidationErrors && !IsValidCvv(Cvv);
        public bool HasValidationErrors =>
            IsCardholderNameInvalid ||
            IsCardNumberInvalid ||
            IsExpiryDateInvalid ||
            IsCvvInvalid;

        public bool Validate()
        {
            ShowValidationErrors = true;
            return !HasValidationErrors;
        }

        private void NotifyValidationStateChanged()
        {
            OnPropertyChanged(nameof(IsCardholderNameInvalid));
            OnPropertyChanged(nameof(IsCardNumberInvalid));
            OnPropertyChanged(nameof(IsExpiryDateInvalid));
            OnPropertyChanged(nameof(IsCvvInvalid));
            OnPropertyChanged(nameof(HasValidationErrors));
        }

        private static bool IsValidCardholderName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalizedValue = value.Trim();
            if (normalizedValue.Length < 2)
            {
                return false;
            }

            return Regex.IsMatch(normalizedValue, @"^[A-ZА-ЯЁ\s\-']+$", RegexOptions.IgnoreCase);
        }

        private static bool IsValidCardNumber(string value)
        {
            var digits = GetDigits(value);
            return digits.Length == 16;
        }

        private static bool IsValidExpiryDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var parts = value.Trim().Split('/');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var year))
            {
                return false;
            }

            if (month < 1 || month > 12)
            {
                return false;
            }

            var fullYear = year < 100 ? 2000 + year : year;

            try
            {
                var expirationBoundary = new DateTime(fullYear, month, 1).AddMonths(1);
                return expirationBoundary > DateTime.Now;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidCvv(string value)
        {
            var digits = GetDigits(value);
            return digits.Length == 3;
        }

        private static string NormalizeCardholderName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.ToUpperInvariant();
        }

        private static string FormatCardNumber(string value)
        {
            var digits = GetDigits(value);
            if (digits.Length > 16)
            {
                digits = digits.Substring(0, 16);
            }

            return string.Join(" ",
                Enumerable.Range(0, (digits.Length + 3) / 4)
                    .Select(index => digits.Substring(index * 4, Math.Min(4, digits.Length - index * 4))));
        }

        private static string FormatExpiryDate(string value)
        {
            var digits = GetDigits(value);
            if (digits.Length > 4)
            {
                digits = digits.Substring(0, 4);
            }

            if (digits.Length <= 2)
            {
                return digits;
            }

            return $"{digits.Substring(0, 2)}/{digits.Substring(2)}";
        }

        private static string FormatCvv(string value)
        {
            var digits = GetDigits(value);
            return digits.Length <= 3 ? digits : digits.Substring(0, 3);
        }

        private static string GetDigits(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(value.Where(char.IsDigit).ToArray());
        }
    }
}
