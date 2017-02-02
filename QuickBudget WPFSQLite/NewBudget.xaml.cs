using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Path = System.IO.Path;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for NewBudget.xaml
    /// </summary>
    public partial class NewBudget : MetroWindow
    {
        public NewBudget(List<Currency> currencies)
        {
            InitializeComponent();
            _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _path = Path.Combine(_path, "Budgets");
            _currencies = currencies;
            ComboBoxCurrency.ItemsSource = _currencies;
            ComboBoxCurrencyCopy.ItemsSource = _currencies;
        }

        private string _path;
        private bool existing = false;
        readonly List<Currency> _currencies = new List<Currency>();
        private void button_Click(object sender, RoutedEventArgs e)
        {
            string name = TextBoxBudgetName.Text;
            string nameWithExtension = name + ".sqlite";
            Currency selectedCurrency = (Currency)ComboBoxCurrency.SelectedItem;
            Currency accountCurrency = (Currency)ComboBoxCurrencyCopy.SelectedItem;
            decimal balance;
            try
            {
                 balance = Convert.ToDecimal(TextBoxBalance.Text);
            }
            catch (OverflowException)
            {
                balance = decimal.MaxValue;
            }
            string info = TextBoxInfo.Text;
            string nameAcc = TextBoxAccName.Text;
            MainWindow mainWindow = new MainWindow(nameWithExtension, name, selectedCurrency.CurrencyString, _currencies, nameAcc, info, balance, _currencies.IndexOf(selectedCurrency) +1, _currencies.IndexOf(accountCurrency)+1); //TODO prasarna misto ID, index +1 ewwwww
            Close();
            mainWindow.Show();
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
                else if (c == '-' && i == 0)
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
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            GridOne.Visibility = Visibility.Hidden;
            GridTwo.Visibility = Visibility.Visible;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            GridOne.Visibility = Visibility.Visible;
            GridTwo.Visibility = Visibility.Hidden;
        }

        private void ComboBoxCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxCurrencyCopy.SelectedItem = ComboBoxCurrency.SelectedItem;
            ButtonConfirm.IsEnabled = IsValidFilename(TextBoxBudgetName.Text) && !existing && !string.IsNullOrWhiteSpace(TextBoxAccName.Text) && !string.IsNullOrWhiteSpace(TextBoxBudgetName.Text) && !string.IsNullOrEmpty(TextBoxBalance.Text) && ComboBoxCurrency.SelectedItem != null && ComboBoxCurrencyCopy.SelectedItem != null;

        }

        private void TextBoxBalance_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateTextDecimal(sender as TextBox);
            ButtonConfirm.IsEnabled = IsValidFilename(TextBoxBudgetName.Text) && !existing && !string.IsNullOrWhiteSpace(TextBoxAccName.Text) && !string.IsNullOrWhiteSpace(TextBoxBudgetName.Text) && !string.IsNullOrEmpty(TextBoxBalance.Text) && ComboBoxCurrency.SelectedItem != null && ComboBoxCurrencyCopy.SelectedItem != null;

        }

        public bool IsValidFilename(string testName)
        {
            var localFiles = Directory.EnumerateFiles(_path, "*.sqlite").ToList();
            var exists = localFiles.Any(x => Path.GetFileNameWithoutExtension(x).Equals(TextBoxBudgetName.Text));
            var chars = System.IO.Path.GetInvalidFileNameChars();
            var match = testName.IndexOfAny(chars) != -1;
            if (exists)
            {
                TextBlockInvalid.Text = "Budget with this name already exists";
                
                return false;
            }
            if (match)
            {
                TextBlockInvalid.Text = "Name contains invalid characters";
                return false;
            }
            return true;
            
        }
        private void TextBoxBudgetName_TextChanged(object sender, TextChangedEventArgs e)
        {


            ButtonConfirm.IsEnabled = IsValidFilename(TextBoxBudgetName.Text) && !string.IsNullOrWhiteSpace(TextBoxAccName.Text) && !string.IsNullOrWhiteSpace(TextBoxBudgetName.Text) && !string.IsNullOrEmpty(TextBoxBalance.Text) && ComboBoxCurrency.SelectedItem != null && ComboBoxCurrencyCopy.SelectedItem != null;

        }

        private void TextBoxAccName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonConfirm.IsEnabled = IsValidFilename(TextBoxBudgetName.Text) && !existing && !string.IsNullOrWhiteSpace(TextBoxAccName.Text) && !string.IsNullOrWhiteSpace(TextBoxBudgetName.Text) && !string.IsNullOrEmpty(TextBoxBalance.Text) && ComboBoxCurrency.SelectedItem != null && ComboBoxCurrencyCopy.SelectedItem != null;

        }

        private void ComboBoxCurrencyCopy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonConfirm.IsEnabled = IsValidFilename(TextBoxBudgetName.Text) && !existing && !string.IsNullOrWhiteSpace(TextBoxAccName.Text) && !string.IsNullOrWhiteSpace(TextBoxBudgetName.Text) && !string.IsNullOrEmpty(TextBoxBalance.Text) && ComboBoxCurrency.SelectedItem != null && ComboBoxCurrencyCopy.SelectedItem != null;
        }
    }
}