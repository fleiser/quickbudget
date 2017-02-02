using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for Transfer.xaml
    /// </summary>
    public partial class Transfer : MetroWindow
    {
        public Transfer(List<Account> accounts)
        {
            InitializeComponent();
            ComboBoxFrom.ItemsSource = accounts;
            ComboBoxTo.ItemsSource = accounts;
            if (accounts.Any())
            {
                ComboBoxFrom.SelectedIndex = 0;
                ComboBoxTo.SelectedIndex = 0;
            }
            IsSuccesful = false;
            _initialized = true;
            ComboBoxFrom.Focus();
        }
        private readonly bool _initialized = false;

        public bool IsSuccesful;
        public Account AccountFrom { get; set; }
        public Account AccountTo { get; set; }
        public decimal AmountFrom { get; set; }
        public decimal AmountTo { get; set; }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            IsSuccesful = true;
            AmountFrom = Convert.ToDecimal(TextBoxFrom.Text);
            AmountTo = Convert.ToDecimal(TextBoxTo.Text);
            if (ComboBoxFrom.SelectedItem != null)
            {
                AccountFrom = (Account)ComboBoxFrom.SelectionBoxItem;
            }
            if (ComboBoxTo.SelectedItem != null)
            {
                AccountTo= (Account)ComboBoxTo.SelectionBoxItem;
            }
            Close();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ComboBoxTo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            CheckRequirements();
            //AccountTo = (Account)ComboBoxTo.SelectionBoxItem;
        }

        private void ComboBoxFrom_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            CheckRequirements();
            //AccountFrom = (Account)ComboBoxFrom.SelectionBoxItem;
        }


        public void ValidateTextDecimal(TextBox textBox)
        {
            if (textBox == null) return;
            var text = textBox.Text;
            var validatedText = new StringBuilder();
            var dotFound = false;
            char prevChar = '\0';
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (i > 0)
                {
                    prevChar = text[i - 1];
                }
                if (char.IsDigit(c))
                {
                    validatedText.Append(c);
                }
                //TODO check - across app
                else if (c != '-' && i == 0)
                {
                    validatedText.Append(c);
                }
                else if (!dotFound && c == '.' && prevChar != '.' && prevChar != ',' && i > 0)
                {
                    validatedText.Append(c);
                    dotFound = true;
                }
                else if (c == ',' && prevChar != '.' && prevChar != ',' && i > 0)
                {
                    validatedText.Append(c);
                }
            }
            var newText = validatedText.ToString();
            textBox.Text = newText;
            textBox.CaretIndex = newText.Length;
        }

        private void SelectText(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb?.SelectAll();
        }
        private void SelectivelyIgnoreMousebutton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBox);

            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }

        private void TextBoxTo_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateTextDecimal(sender as TextBox);
            CheckRequirements();
            var textBox = sender as TextBox;
            if (textBox == null) return;


            try
            {
                AmountTo = string.IsNullOrEmpty(TextBoxTo.Text) ? 0 : Convert.ToDecimal(TextBoxTo.Text);
            }
            catch (OverflowException)
            {

                AmountTo = Decimal.MaxValue;
            }
        }

        private void TextBoxFrom_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateTextDecimal(sender as TextBox);
            CheckRequirements();
            var textBox = sender as TextBox;
            if (textBox == null) return;


            try
            {
                AmountFrom = string.IsNullOrEmpty(TextBoxFrom.Text) ? 0 : Convert.ToDecimal(TextBoxFrom.Text);
            }
            catch (OverflowException)
            {

                AmountFrom = Decimal.MaxValue;
            }
        }

        private void CheckRequirements()
        {
            if (ComboBoxFrom.SelectedItem != null && ComboBoxTo.SelectedItem != null && !string.IsNullOrEmpty(TextBoxFrom.Text) && !string.IsNullOrEmpty(TextBoxTo.Text) && AmountFrom >= 0 && AmountTo >=0)
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
