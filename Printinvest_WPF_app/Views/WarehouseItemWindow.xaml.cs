using Printinvest_WPF_app.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;

namespace Printinvest_WPF_app.Views
{
    public partial class WarehouseItemWindow : Window
    {
        public string ItemName => NameTextBox.Text?.Trim();
        public string ItemCategory => string.IsNullOrWhiteSpace(CategoryTextBox.Text) ? null : CategoryTextBox.Text.Trim();
        public int ItemQuantity { get; private set; }
        public decimal ItemUnitPrice { get; private set; }

        public WarehouseItemWindow(WarehouseItem item = null)
        {
            InitializeComponent();

            if (item == null)
            {
                QuantityTextBox.Text = "0";
                UnitPriceTextBox.Text = "0";
                return;
            }

            WindowTitleText.Text = "Изменить позицию";
            Title = "Изменить позицию";
            NameTextBox.Text = item.Name;
            CategoryTextBox.Text = item.Category;
            QuantityTextBox.Text = item.Quantity.ToString();
            UnitPriceTextBox.Text = item.UnitPrice.ToString(CultureInfo.CurrentCulture);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemName))
            {
                MessageBox.Show("Укажите название позиции.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                NameTextBox.SelectAll();
                return;
            }

            if (!ContainsLetter(ItemName))
            {
                MessageBox.Show("Название должно содержать буквы, а не только цифры или символы.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                NameTextBox.SelectAll();
                return;
            }

            if (string.IsNullOrWhiteSpace(ItemCategory))
            {
                MessageBox.Show("Укажите категорию позиции.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryTextBox.Focus();
                CategoryTextBox.SelectAll();
                return;
            }

            if (!ContainsLetter(ItemCategory))
            {
                MessageBox.Show("Категория должна содержать буквы, а не только цифры или символы.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryTextBox.Focus();
                CategoryTextBox.SelectAll();
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Количество должно быть целым числом больше нуля.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                QuantityTextBox.SelectAll();
                return;
            }

            if (!TryParseUnitPrice(UnitPriceTextBox.Text, out var unitPrice) || unitPrice <= 0)
            {
                MessageBox.Show("Цена должна быть числом больше нуля.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                UnitPriceTextBox.Focus();
                UnitPriceTextBox.SelectAll();
                return;
            }

            ItemQuantity = quantity;
            ItemUnitPrice = unitPrice;
            DialogResult = true;
        }

        private static bool TryParseUnitPrice(string rawValue, out decimal unitPrice)
        {
            var normalizedValue = rawValue?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                unitPrice = 0;
                return false;
            }

            if (decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.CurrentCulture, out unitPrice))
            {
                return true;
            }

            normalizedValue = normalizedValue.Replace(',', '.');
            return decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out unitPrice);
        }

        private static bool ContainsLetter(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, "[A-Za-zА-Яа-яЁё]");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
