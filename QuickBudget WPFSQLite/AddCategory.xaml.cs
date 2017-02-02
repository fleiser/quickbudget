using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for AddCategory.xaml
    /// </summary>
    public partial class AddCategory : MetroWindow
    {
        public AddCategory(List<Currency> currencies, long priamryCurrency)
        {
            InitializeComponent();
            ComboBoxCurrency.ItemsSource = currencies;
            if (currencies.Any())
            {
                ComboBoxCurrency.SelectedIndex = (int) priamryCurrency;
            }
        }

        public bool IsSuccesful = false;
        //private bool IsSuccesful;
        public new string Name;
        public string Info;
        public Currency SelectedCurrency;

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            IsSuccesful = true;
            Name = TextBoxName.Text;
            Info = TextBoxInfo.Text;
            SelectedCurrency = (Currency) ComboBoxCurrency.SelectionBoxItem;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxName.Text)&&ComboBoxCurrency.SelectedItem!=null)
            {
                ButtonAdd.IsEnabled = true;
            }
            else
            {
                ButtonAdd.IsEnabled = false;
            }
        }

        private void ComboBoxCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxName.Text) && ComboBoxCurrency.SelectedItem != null)
            {
                ButtonAdd.IsEnabled = true;
            }
            else
            {
                ButtonAdd.IsEnabled = false;
            }
        }
    }
}
