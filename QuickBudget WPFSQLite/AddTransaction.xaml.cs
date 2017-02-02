using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;
using static System.Decimal;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for AddTransaction.xaml
    /// </summary>
    public partial class AddTransaction : MetroWindow
    {
        public decimal ValueAcc { get; set; }
        public  decimal ValueCat { get; set; }
        private decimal Rate { get; set; } = 1;
        public Account Account { get; set; }
        public Category Category { get; set; }
        public bool IsIncome { get; set; }
        public string Note { get; set; }
        public string Payee { get; set; }
        public bool IsSuccesful { get; set; }
        //private readonly List<ExchangeR> _curremcyRs;
        private readonly List<Currency> _currencyList;
        private readonly bool _initialized = false;
/*
        private bool IncomeCategory;
*/

        public AddTransaction(List<Category> categories, List<Account> accounts,
            List<Currency> currencies)
        {
            InitializeComponent();
            ComboBoxAccount.ItemsSource = accounts;
            ComboBoxCategory.ItemsSource = categories;
            if (accounts.Any())
            {
                ComboBoxAccount.SelectedIndex = 0;
            }
            if (categories.Any())
            {
                ComboBoxCategory.SelectedIndex = 0;
            }

            //_curremcyRs = currencyExchangeRs;
            _currencyList = currencies;
            ButtonExpanse.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 209, 43));
            ButtonIncome.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 209, 43)); ;
            IsSuccesful = false;
            _initialized = true;
            IsIncome = false;
            ValidateCurrencyVal();
            TextBoxValue.Focus();
            if (TextBoxValueCat.Text == "0" || TextBoxValueCat.Text == "0.00")
            {
                TextBoxValueCat.BorderBrush = Brushes.Red;
            }
            else
            {
                TextBoxValueCat.BorderBrush = Brushes.Black;
            }
        }

        public AddTransaction(List<Category> categories, List<Account> accounts,
            List<Currency> currencies, long categoryId, bool isIncome, string info, decimal value, long accountId, string payee, decimal transValue)
        {
            InitializeComponent();
            ComboBoxAccount.ItemsSource = accounts;
            ComboBoxCategory.ItemsSource = categories;
            ComboBoxAccount.SelectedIndex = accounts.FindIndex(x => x.Id.Equals(accountId));
            ComboBoxCategory.SelectedIndex = categories.FindIndex(x => x.Id.Equals(categoryId));
            TextBoxPayee.Text = payee;
            TextBoxNote.Text = info;
            ValueCat = transValue;
            TextBoxValue.Text = value.ToString(CultureInfo.CurrentCulture);
            TextBoxValueCat.Text = ValueCat.ToString(CultureInfo.CurrentCulture);
            //_curremcyRs = currencyExchangeRs;
            _currencyList = currencies;
            IsIncome = isIncome;
            if (IsIncome)
            {
                ButtonExpanse.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 209, 43)); ;
                ButtonIncome.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255,209,43));
                ComboBoxCategory.IsEnabled = false;
                LabelCategory.IsEnabled = false;
            }
            else
            {
                ButtonExpanse.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 209, 43));
                ButtonIncome.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 209, 43)); ;
            }
            ValidateCurrencyVal();
            _initialized = true;
            TextBoxValue.Focus();
            if (TextBoxValueCat.Text == "0" || TextBoxValueCat.Text == "0.00")
            {
                TextBoxValueCat.BorderBrush = Brushes.Red;
            }
            else
            {
                TextBoxValueCat.BorderBrush = Brushes.Black;
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            IsSuccesful = true;
            ValueAcc = Convert.ToDecimal(TextBoxValue.Text);
            if (ComboBoxAccount.SelectedItem != null)
            {
                Account = (Account) ComboBoxAccount.SelectionBoxItem;
            }
            if (ComboBoxCategory.SelectedItem != null)
            {
                Category = (Category) ComboBoxCategory.SelectionBoxItem;
            }
            Note = TextBoxNote.Text;
            Payee = TextBoxPayee.Text;
            if (string.IsNullOrEmpty(TextBoxValueCat.Text))
            {
                TextBoxValueCat.Text = TextBoxValue.Text;
            }
            ValueCat = Convert.ToDecimal(TextBoxValueCat.Text);
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            IsSuccesful = false;
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
                else if (!dotFound && c == '.' && prevChar != '.' && prevChar != ',' && i >0)
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

        private void CheckRequirements()
        {
            if (!string.IsNullOrEmpty(TextBoxValue.Text) && ValueAcc * Rate < MaxValue && TextBoxValue.Text != 0.ToString() && TextBoxValueCat.Text != 0.ToString() && !string.IsNullOrEmpty(TextBoxValueCat.Text) && ComboBoxAccount.SelectedItem != null && (ComboBoxCategory != null || IsIncome))
            {
                ButtonAdd.IsEnabled = true;
            }
            else
            {
                ButtonAdd.IsEnabled = false;
            }
        }

        private void TextBoxValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_initialized) return;
            ValidateTextDecimal(sender as TextBox);
            CheckRequirements();
            var textBox = sender as TextBox;
            if (textBox == null) return;


            try
            {
                ValueAcc = string.IsNullOrEmpty(textBox.Text) ? 0 : Convert.ToDecimal(textBox.Text);
            }
            catch (OverflowException)
            {

                ValueAcc = MaxValue;
            }

            if (ValueAcc * Rate < MaxValue)
            {
                ValueCat = ValueAcc * Rate;
                TextBoxValueCat.Text = ValueCat.ToString(CultureInfo.CurrentCulture);
            }


            /*if (_initialized)
            {


                ValidateTextDecimal(sender as TextBox);

                if (ComboBoxCategory.SelectedItem != null || IsIncome)
                {
                    IncomeCategory = true;
                }
                else
                {
                    IncomeCategory = false;
                }
                if (!string.IsNullOrEmpty(TextBoxValue.Text) && !string.IsNullOrEmpty(TextBoxValueCat.Text) &&
                    ComboBoxAccount.SelectedItem != null && IncomeCategory)
                {
                    ValueAcc = Convert.ToDecimal(TextBoxValue.Text);
                    ValueAcc = Convert.ToDecimal(TextBoxValue.Text);
                    ButtonAdd.IsEnabled = true;
                }
                else
                {
                    ButtonAdd.IsEnabled = false;
                }
                ValidateCurrencyVal();
            }*/

        }

        private void ComboBoxAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_initialized)
            {
               
                CheckRequirements();
                ValidateCurrencyVal();
            }
        }

        private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_initialized)
            {
                //ValidateTextDecimal(sender as TextBox);
                CheckRequirements();
                ValidateCurrencyVal();
            }
        }

        public void ValidateCurrencyVal()
        {
            Account account = null;
            Category category = null;
            var clear = true;
            if (ComboBoxAccount.SelectedItem != null)
            {
                account = (Account)ComboBoxAccount.SelectedItem;
            }
            else
            {
                clear = false;
            }
            if (ComboBoxCategory.SelectedItem != null)
            {
                category = (Category)ComboBoxCategory.SelectedItem; //TODO check if income clear = false;
            } 
            else
            {
                clear = false;
            }
            if (clear && !account.CurrencyId.Equals(category.CurrencyId))
            {
                LabelRate.IsEnabled = true;
                LabelTrans.IsEnabled = true;
                TextBoxValueCat.IsEnabled = true;
                TextBlockRate.IsEnabled = true;
                /*
                Currency currency = _currencyList.Where(x => x.Id.Equals(account.CurrencyId)).ToList()[0];
                if (currency.CurrencyString.Equals("EUR"))
                {
                    //long id = currency.Id;
                    Rate = _curremcyRs.Where(x => x.Currency.Equals(currency.CurrencyString)).ToList()[0].Rate;
                    TextBlockRate.Text = Rate.ToString("N2");
                    decimal value = ValueAcc * Rate;
                    TextBoxValueCat.Text = value.ToString("N2");
                    var currencyAcc = _currencyList.Where(x => x.Id.Equals(account.CurrencyId)).ToList();
                    var rateAcc = _curremcyRs.Where(x => x.Currency.Equals(currencyAcc[0].CurrencyString)).ToList();
                    var currencyCat = _currencyList.Where(x => x.Id.Equals(category.CurrencyId)).ToList();
                    var rateCat = _curremcyRs.Where(x => x.Currency.Equals(currencyCat[0].CurrencyString)).ToList();
                    Rate = rateCat[0].Rate / rateAcc[0].Rate;
                    TextBlockRate.Text = Rate.ToString("N2");
                    decimal value = ValueAcc * Rate;
                    TextBoxValueCat.Text = value.ToString("N2");
                }
                else
                {

                }*/
                //var currencyAcc = _currencyList.Where(x => x.Id.Equals(account.CurrencyId)).ToList();
                //var rateAcc = _curremcyRs.Where(x => x.Currency.Equals(currencyAcc[0].CurrencyString)).ToList();
                //var currencyCat = _currencyList.Where(x => x.Id.Equals(category.CurrencyId)).ToList();
                //var rateCat = _curremcyRs.Where(x => x.Currency.Equals(currencyCat[0].CurrencyString)).ToList();
                //Rate = rateCat[0].Rate / rateAcc[0].Rate;
                TextBlockRate.Text = Rate.ToString("N2");
                //decimal value = ValueAcc * Rate;
                //TextBoxValueCat.Text = value.ToString("N2");
                TextBlockCurrency.Text = $"The currency used in \"{category.Name}\"({category.CurrencySymbol}) is different than in \"{account.Name}\"({account.CurrencySymbol}) ";
            }
            else
            {
                LabelRate.IsEnabled = false;
                LabelTrans.IsEnabled = false;
                TextBoxValueCat.IsEnabled = false;
                TextBlockRate.IsEnabled = false;
                TextBlockRate.Text = "1";
                Rate = 1;
                TextBlockCurrency.Text = "";
                TextBoxValueCat.Text = TextBoxValue.Text;
            }
            /* Account account = null;
             Category category = null;
             bool clear = true;
             if (ComboBoxAccount.SelectedItem != null)
             {
                 account = (Account) ComboBoxAccount.SelectedItem;
             }
             else
             {
                 clear = false;
             }
             if (ComboBoxCategory.SelectedItem != null)
             {
                 category = (Category) ComboBoxCategory.SelectedItem; //TODO check if income clear = false;
             }
             else
             {
                 clear = false;
             }
             TextBlockCurrency.Text = "";
             if (clear && !account.CurrencyId.Equals(category.CurrencyId))
             {
                 LabelRate.IsEnabled = true;
                 LabelTrans.IsEnabled = true;
                 TextBoxValueCat.IsEnabled = true;
                 TextBoxRate.IsEnabled = true;
                 TextBlockCurrency.Text =
                     $"The currency used in '{account.Name}'({account.CurrencySymbol}) is different than in category '{category.Name}'({category.CurrencySymbol}), please specify the exchange rate or value to be transactioned";
                 TextBlockCurrency.Foreground = Brushes.Red;
                 if (_curremcyRs != null && _currencyList != null)
                 {
                     var currencyAcc = _currencyList.Where(x => x.Id.Equals(account.CurrencyId)).ToList();
                     var rateAcc = _curremcyRs.Where(x => x.Currency.Equals(currencyAcc[0].CurrencyString)).ToList();
                     if (!category.Id.Equals(0))
                     {
                         var currencyCat = _currencyList.Where(x => x.Id.Equals(category.CurrencyId)).ToList();
                         var rateCat = _curremcyRs.Where(x => x.Currency.Equals(currencyCat[0].CurrencyString)).ToList();
                         Rate = rateCat[0].Rate / rateAcc[0].Rate;
                     }
                     else
                     {
                         Rate = 1;
                     }
                    /* if (currencyAcc[0].CurrencyString.Equals("EUR") && !rateAcc.Exists(x => x.Currency.Equals("EUR")))
                     {
                         rateAcc.Add(new ExchangeR(0,"EUR",1));
                     }
                     TextBoxRate.Text = Rate.ToString("0.###");//$"{Rate:0.000}";
                     decimal valueCat = Rate*ValueAcc;
                     TextBoxValueCat.Text = valueCat.ToString("0.##"); //$"{Rate*ValueAcc:0.00}";
                 }
             }
             else
             {
                 LabelRate.IsEnabled = false;
                 LabelTrans.IsEnabled = false;
                 TextBoxValueCat.IsEnabled = false;
                 TextBoxRate.IsEnabled = false;
                 TextBoxRate.Text = "1";
                 TextBoxValueCat.Text = TextBoxValue.Text;
             }*/
        }

        private void TextBoxValueCat_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_initialized)
            {
                ValidateTextDecimal(sender as TextBox);
                CheckRequirements();
                decimal value;
                decimal transValue;
                if (!string.IsNullOrEmpty(TextBoxValueCat.Text))
                {
                    TextBoxValueCat.BorderBrush = Brushes.Black;
                    try
                    {
                    value = Convert.ToDecimal(TextBoxValueCat.Text);
                    }
                    catch (Exception)
                    {
                        value = MaxValue;
                    }
                }
                else
                {
                    value = 0;
                    ValueAcc = 0;
                    TextBoxValueCat.Text = 0.00.ToString(CultureInfo.CurrentCulture);
                }
                if (TextBoxValueCat.Text == "0" || TextBoxValueCat.Text == "0.00")
                {
                    TextBoxValueCat.BorderBrush = Brushes.Red;
                }
                else
                {
                    TextBoxValueCat.BorderBrush = Brushes.Black;
                }
                if (!string.IsNullOrEmpty(TextBoxValue.Text))
                {
                    try
                    {
                        transValue = Convert.ToDecimal(TextBoxValue.Text);
                    }
                    catch (OverflowException)
                    {

                        transValue = MaxValue;
                    }
                }
                else
                {
                    transValue = 0;
                    ValueCat = 0;
                    //TextBoxValue.Text = 0.00.ToString(CultureInfo.CurrentCulture);
                }
                if (value!=0)
                {
                    //TODO EDIT BY ZERO EXCEPTION
                    TextBlockRate.Text = (value/ transValue).ToString("N2");
                }
            }
        }

        private void TextBoxRate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_initialized)
            {
                ValidateTextDecimal(sender as TextBox);
                CheckRequirements();
            }
        }

        private void ButtonIncome_Click(object sender, RoutedEventArgs e)
        {
            ButtonExpanse.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 209, 43)); ;
            ButtonIncome.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 209, 43));
            IsIncome = true;
            ComboBoxCategory.IsEnabled = false;
            LabelCategory.IsEnabled = false;
            TextBoxValueCat.IsEnabled = false;
            TextBlockRate.IsEnabled = false;
            LabelRate.IsEnabled = false;
            LabelTrans.IsEnabled = false;
            ComboBoxCategory.SelectedItem = null;
        }

        private void ButtonExpanse_Click(object sender, RoutedEventArgs e)
        {
            ButtonExpanse.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 209, 43)); ;
            ButtonIncome.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 209, 43));
            IsIncome = false;
            ComboBoxCategory.IsEnabled = true;
            LabelCategory.IsEnabled = true;
            TextBoxValueCat.IsEnabled = true;
            TextBlockRate.IsEnabled = true;
            LabelRate.IsEnabled = true;
            LabelTrans.IsEnabled = true;
            ComboBoxCategory.SelectedIndex = 0;
        }

        private void TextBoxValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            var textBox = sender as TextBox;
            if (textBox == null) return;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                ValueAcc = 0;
                textBox.Text = 0.00.ToString(CultureInfo.CurrentCulture);
            }
            //TODO FROMATN2
            // string value = TextBoxValue.Text;
            //TextBoxValue.Text = $"{value:0.00}";
        }

        private void TextBoxValueCat_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            var textBox = sender as TextBox;
            if (textBox == null) return;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                ValueCat = 0;
                textBox.Text = 0.00.ToString(CultureInfo.CurrentCulture);
            }
            /*decimal value = Convert.ToDecimal(TextBoxValueCat.Text);
            decimal transValue = Convert.ToDecimal(TextBoxValue.Text);
            if (transValue != 0)
            {
                TextBoxRate.Text = (value/transValue).ToString(CultureInfo.CurrentCulture);
            }*/

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            TextBoxValueCat.Text = (Convert.ToDecimal(TextBoxValue.Text) * Rate).ToString();
        }
    }
}
