using Printinvest_WPF_app.Models;
using System.Globalization;
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
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out var quantity) || quantity < 0)
            {
                MessageBox.Show("Укажите корректное количество.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(UnitPriceTextBox.Text, out var unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Укажите корректную цену.", "Ошибка проверки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ItemQuantity = quantity;
            ItemUnitPrice = unitPrice;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
