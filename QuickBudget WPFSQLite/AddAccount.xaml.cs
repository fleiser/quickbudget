using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for AddAccount.xaml
    /// </summary>
    public partial class AddAccount : MetroWindow
    {
        public AddAccount(List<Currency> currencies)
        {
            InitializeComponent();
            ComboBoxCurrency.ItemsSource = currencies;
            if (currencies.Any())
            {
                ComboBoxCurrency.SelectedIndex = 0;
            }
            IsSuccesful = false;
        }

        public new string Name;
        public string Info;
        public Currency SelectedCurrency;
        public decimal Balance;
        public bool IsSuccesful;

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            IsSuccesful = true;
            Balance = Convert.ToDecimal(TextBoxBalance.Text);
            if (ComboBoxCurrency.SelectedItem != null)
            {
                SelectedCurrency = (Currency)ComboBoxCurrency.SelectionBoxItem;
            }
            else
            {
                IsSuccesful = false;
            }

            Name = textBoxAccName.Text;
            Info = textBoxInfo.Text;
            if (string.IsNullOrEmpty(textBoxAccName.Text))
            {
                IsSuccesful = false;
            }
            if (string.IsNullOrEmpty(TextBoxBalance.Text))
            {
                TextBoxBalance.Text = "0";
                Balance = 0;
                // IsSuccesful = false;
            }
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
                else if(c == '-' && i == 0)
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

        private void textBoxAccName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonAdd.IsEnabled = !string.IsNullOrEmpty(TextBoxBalance.Text) &&
                                                  !string.IsNullOrEmpty(textBoxAccName.Text)&&ComboBoxCurrency.SelectedItem!=null;
        }

        private void textBoxBalance_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateTextDecimal(sender as TextBox);
            ButtonAdd.IsEnabled = !string.IsNullOrEmpty(TextBoxBalance.Text) &&
                                                  !string.IsNullOrEmpty(textBoxAccName.Text) && ComboBoxCurrency.SelectedItem != null;
        }
        private void ComboBoxCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonAdd.IsEnabled = !string.IsNullOrEmpty(TextBoxBalance.Text) &&
                                      !string.IsNullOrEmpty(textBoxAccName.Text) && ComboBoxCurrency.SelectedItem != null;
        }
    }
}
