using ServiceCenter.Models;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace ServiceCenter.Views
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

            WindowTitleText.Text = App.GetString("WarehouseItemEditTitle", "Edit item");
            Title = App.GetString("WarehouseItemEditTitle", "Edit item");
            NameTextBox.Text = item.Name;
            CategoryTextBox.Text = item.Category;
            QuantityTextBox.Text = item.Quantity.ToString();
            UnitPriceTextBox.Text = item.UnitPrice.ToString(CultureInfo.CurrentCulture);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemName))
            {
                MessageBox.Show(
                    App.GetString("WarehouseNameRequiredError", "Enter the item name."),
                    App.GetString("WarehouseValidationTitle", "Validation error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                NameTextBox.Focus();
                NameTextBox.SelectAll();
                return;
            }

            if (!ContainsLetter(ItemName))
            {
                MessageBox.Show(
                    App.GetString("WarehouseNameLettersError", "The name must contain letters, not only digits or symbols."),
                    App.GetString("WarehouseValidationTitle", "Validation error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                NameTextBox.Focus();
                NameTextBox.SelectAll();
                return;
            }

            if (string.IsNullOrWhiteSpace(ItemCategory))
            {
                MessageBox.Show(
                    App.GetString("WarehouseCategoryRequiredError", "Enter the item category."),
                    App.GetString("WarehouseValidationTitle", "Validation error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CategoryTextBox.Focus();
                CategoryTextBox.SelectAll();
                return;
            }

            if (!ContainsLetter(ItemCategory))
            {
                MessageBox.Show(
                    App.GetString("WarehouseCategoryLettersError", "The category must contain letters, not only digits or symbols."),
                    App.GetString("WarehouseValidationTitle", "Validation error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CategoryTextBox.Focus();
                CategoryTextBox.SelectAll();
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out var quantity) || quantity <= 0)
            {
                MessageBox.Show(
                    App.GetString("WarehouseQuantityError", "Quantity must be a whole number greater than zero."),
                    App.GetString("WarehouseValidationTitle", "Validation error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                QuantityTextBox.SelectAll();
                return;
            }

            if (!TryParseUnitPrice(UnitPriceTextBox.Text, out var unitPrice) || unitPrice <= 0)
            {
                MessageBox.Show(
                    App.GetString("WarehouseUnitPriceError", "Price must be a number greater than zero."),
                    App.GetString("WarehouseValidationTitle", "Validation error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
            return !string.IsNullOrWhiteSpace(value) && value.Any(char.IsLetter);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
