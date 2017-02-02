using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MahApps.Metro.Controls;
using static System.Decimal;
using Path = System.IO.Path;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : MetroWindow
    {
        private readonly SQLiteConnection _con;
        public DateTime SelectedDate = new DateTime();
        private readonly List<Transaction> _transactionList = new List<Transaction>();
        public static List<Category> CategoryList = new List<Category>();
        public static List<MasterCategory> MasterCategoryList { get; set; }
        public List<StatusCategory> StatusCategoryList = new List<StatusCategory>();
        //public List<StatusCategory> GlobalStatusCategoryList = new List<StatusCategory>();
        public static List<Currency> CurrencyList { get; set; }
        public List<ExchangeR> ExchangeRates { get; set; }
        private List<Account> _accountList = new List<Account>(); 

        public static List<Currency> GetCurrencies()
        {
            return CurrencyList;
        }
        public static List<MasterCategory> GetMasterCategories()
        {
            return MasterCategoryList;
        }
        public static List<Category> GetCategories()
        {
            return CategoryList;
        }
        public long PrimaryCurrency;
        public string PrimaryCurrencyName;
        //TODO chagne priamry currency in settings or soemthin
        public string PrimaryCurrencySymbol;
        public string BudgetName;
        public string BudgetPath;
        public MainWindow(string budgetName, string budgetNameWithouExtension, string primaryCurrencyName, List<Currency> currencies,string accountName, string accountInfo,decimal balance, long currencyId, long currencyAccountId)
        {
            InitializeComponent();
            SelectedDate = DateTime.Now;
            TextBlockDate.Text = SelectedDate.ToString("MMMM-yyyy");

            PrimaryCurrencyName = primaryCurrencyName;
            try
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.CreateDirectory("Budgets");
                path = Path.Combine(path,"Budgets");
                path = Path.Combine(path, budgetName);
                SQLiteConnection.CreateFile(path);
                BudgetPath = path;
                BudgetName = budgetName;
            }
            catch (IOException)
            {
                Application.Current.Shutdown();
                MessageBox.Show("There was an error while loading the database");

            }
            _con = new SQLiteConnection($"Data Source={BudgetPath};Version=3;datetimeformat=CurrentCulture;");
            _con.Open();
                using (var com = new SQLiteCommand(_con))
                {
                    using (var transaction = _con.BeginTransaction())
                    {
                        var sqlCreateTable = "CREATE TABLE Category (" +
                                                "ID INTEGER PRIMARY KEY," +
                                                "Name VARCHAR(30)," +
                                                "Info NVARCHAR(255)," +
                                                "CurrencyID INTEGER," +
                                                "MasterCat INTEGER)";
                        com.CommandText = sqlCreateTable;
                        com.ExecuteNonQuery();
                        sqlCreateTable = "CREATE TABLE MasterCategory (" +
                                         "ID INTEGER PRIMARY KEY," +
                                         "Name VARCHAR(250))";
                        com.CommandText = sqlCreateTable;
                        com.ExecuteNonQuery();
                        sqlCreateTable = "CREATE TABLE MonthBudget (" +
                                         "ID INTEGER PRIMARY KEY," +
                                         "Date DATETIME)";
                        com.CommandText = sqlCreateTable;
                        com.ExecuteNonQuery();
                        sqlCreateTable = "CREATE TABLE Transactions (" +
                                         "ID INTEGER PRIMARY KEY," +
                                         "CategoryID INTEGER," +
                                         "IsIncome BOOLEAN," +
                                         "Info NVARCHAR(255)," +
                                         "Payee NVARCHAR(255),"+
                                         "Transactioned DECIMAL(10,5)," +
                                         "Date DATETIME, " +
                                         "CurrencyID INTEGER," +
                                         "AccountID INTEGER," +
                                         "Accounted DECIMAL(10,5)," +
                                         "Attention BOOLEAN)";
                        com.CommandText = sqlCreateTable;
                        com.ExecuteNonQuery();
                        sqlCreateTable = "CREATE TABLE StatusCategory (" +
                                         "ID INTEGER PRIMARY KEY," +
                                         "Budgeted DECIMAL(10,5)," +
                                         "MonthBudgetID INTEGER, " +
                                         "CategoryID INTEGER," +
                                         "CurrencyID INTEGER)";
                        com.CommandText = sqlCreateTable;
                        com.ExecuteNonQuery();
                        sqlCreateTable = "CREATE TABLE Currency (" +
                                         "ID INTEGER PRIMARY KEY," +
                                         "Symbol NVARCHAR(3)," +
                                         "Currency NVARCHAR(5)," +
                                         "IsPrimary BOOLEAN )";
                        com.CommandText = sqlCreateTable;
                        com.ExecuteNonQuery();
                        sqlCreateTable = "CREATE TABLE Account (" +
                                         "ID INTEGER PRIMARY KEY," +
                                         "AccountName NVARCHAR(20)," +
                                         "AccountInfo NVARCHAR(255)," +
                                         "CurrencyID INTEGER)";
                        com.CommandText = sqlCreateTable;
                        com.ExecuteNonQuery();
                        transaction.Commit();

                    }
                    var masterCategories = new List<MasterCategory>
                    {
                        new MasterCategory(0, "Rent"),
                        new MasterCategory(0, "Mortage"),
                        new MasterCategory(0, "Loan"),
                        new MasterCategory(0, "Ocuring expanses"),
                        new MasterCategory(0, "Expanses"),
                        new MasterCategory(0, "Insurance")
                    };
                    var cat = new List<Category>();
                //File.SetLastWriteTime(BudgetPath, DateTime.Now);
                using (var comm = new SQLiteCommand(_con))
                {
                    using (var transaction = _con.BeginTransaction())
                    {
                        foreach (var category in masterCategories)
                        {
                            string sql =
                                $"INSERT INTO MasterCategory VALUES(null, '{category.Name}')";
                            comm.CommandText = sql;
                            comm.ExecuteNonQuery();
                            //if (category.Name.Equals("Inflow")){}

                        }

                        foreach (var currency in currencies)
                        {
                            var primaryValue = 0;
                            if (currency.CurrencyString.Equals(PrimaryCurrencyName))
                            {
                                primaryValue = 1;
                            }
                            string sqlCurrency = $"INSERT INTO Currency VALUES (null, '{currency.Symbol}','{currency.CurrencyString}',{primaryValue})";
                            comm.CommandText = sqlCurrency;
                            comm.ExecuteNonQuery();
                        }
                        LoadCurrency(); //TODO dynamic masterCat
                        #region
                        cat.Add(new Category(0, "Rent", "Monthly rent",   PrimaryCurrency,1));
                        cat.Add(new Category(0, "Mortage", "Mortage",  PrimaryCurrency,2));
                        cat.Add(new Category(0, "Loans", "Monthly loan repayment",   PrimaryCurrency, 3));
                        cat.Add(new Category(0, "TV", "TV fees",   PrimaryCurrency,4));
                        cat.Add(new Category(0, "Phone", "Phone fees",   PrimaryCurrency,4));
                        cat.Add(new Category(0, "Internet", "Internet subscription",   PrimaryCurrency, 4));
                        cat.Add(new Category(0, "Electricity", "Electricity bill",   PrimaryCurrency, 4));
                        cat.Add(new Category(0, "Water", "Water bill",   PrimaryCurrency,4));
                        cat.Add(new Category(0, "Heating/Gas", "Heating/gas bill",   PrimaryCurrency, 4));
                        cat.Add(new Category(0, "Emergrency fund", "In case of emergrency",   PrimaryCurrency,5));
                        cat.Add(new Category(0, "Groceries", "Groceries, shopping",   PrimaryCurrency, 5));
                        cat.Add(new Category(0, "Fun, snacks, drinks", "Streetfood, beers and other",   PrimaryCurrency, 5));
                        cat.Add(new Category(0, "Spending money", "Spending money",   PrimaryCurrency,5));
                        cat.Add(new Category(0, "Subscriptions", "Magazine, software subscriptions",  PrimaryCurrency,4));
                        cat.Add(new Category(0, "Fuel", "Gas",   PrimaryCurrency, 5));
                        cat.Add(new Category(0, "Medical", "Medical expanses",   PrimaryCurrency, 5));
                        cat.Add(new Category(0, "Cloathing", "Upgrade your wardrobe",  PrimaryCurrency, 5));
                        cat.Add(new Category(0, "Household Goods", "Stuff needed around house",  PrimaryCurrency, 5));
                        cat.Add(new Category(0, "Repairs", "Repairs", PrimaryCurrency, 5));
                        cat.Add(new Category(0, "Maintaince", "Maintaince", PrimaryCurrency, 5));
                        cat.Add(new Category(0, "Car Insurance", "Insurance",  PrimaryCurrency,6));
                        cat.Add(new Category(0, "Life Insurance", "Insurance",  PrimaryCurrency, 6));
                        cat.Add(new Category(0, "Health Insurance", "Insurance",  PrimaryCurrency,6));
                        cat.Add(new Category(0, "Gifts", "Gifts for your close ones",   PrimaryCurrency, 6));
                        cat.Add(new Category(0, "Charity", "Charity",  PrimaryCurrency,5));

                        #endregion

                        var sqlA = "";
                        foreach (var category in cat)
                        {
                            string sql =
                                $"INSERT INTO Category  VALUES(null, '{category.Name}', '{category.Info}', '{category.CurrencyId}', '{category.MasterCategoryId}')";
                            comm.CommandText = sql;
                            comm.ExecuteNonQuery();
                            //if (category.Name.Equals("Inflow")){}

                        }

                         sqlA = $"INSERT INTO Account VALUES (null, @name,@info,{currencyAccountId})";
                        comm.CommandText = sqlA;
                        comm.Parameters.AddWithValue("@name", accountName);
                        comm.Parameters.AddWithValue("@info", accountInfo);
                        comm.ExecuteNonQuery();

                        if (balance !=0)
                        {
                            var lastId = _con.LastInsertRowId;
                            if (balance > 0)
                            {
                                string isql =
                                    $"INSERT INTO Transactions  VALUES(null, 0, 1,'Starting balance','Starting balance',{Math.Abs(balance)},'{DateTime.Now}',{currencyAccountId},{lastId},{balance},0)";
                                var icommand = new SQLiteCommand(isql, _con);
                                icommand.ExecuteNonQuery();
                            }
                            else
                            {
                                string isql =
                                    $"INSERT INTO Transactions VALUES(null, 0, 0,'Starting balance','Starting balance',{Math.Abs(balance)},'{DateTime.Now}',{currencyAccountId},{lastId},{balance},0)";
                                var icommand = new SQLiteCommand(isql, _con);
                                icommand.ExecuteNonQuery();
                            }
                           
                        }

                        transaction.Commit();
                    }
                }
            }
            TextBoxMenuTitle.Text = budgetNameWithouExtension;
            CurrencyList = currencies;
            LoadCurrency();
            EnsureCurrencies();
            LoadAccounts();
            LoadCategories();
            LoadTransactions();
            LoadBudget();
            //LoadExchangeRate();
            TotalWorkable();
            ButtonCategories.BorderThickness = new Thickness(6, 0, 0, 0);
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerAsync();

        }

        public MainWindow(string budgetName,string budgetNameWithouExtension, List<Currency> currencies)
        {
            InitializeComponent();
            SelectedDate = DateTime.Now;
            TextBlockDate.Text = SelectedDate.ToString("MMMM-yyyy");
            try
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.CreateDirectory("Budgets");
                path = Path.Combine(path, "Budgets");
                path = Path.Combine(path, budgetName);
                BudgetPath = path;
                BudgetName = budgetName;
                TextBoxMenuTitle.Text = budgetNameWithouExtension;
            }
            catch (IOException)
            {
                Application.Current.Shutdown();
                MessageBox.Show("There was an error while loading the database");

            }
            _con = new SQLiteConnection($"Data Source={BudgetPath};Version=3;datetimeformat=CurrentCulture;");
            _con.Open();
            CurrencyList = currencies;
            LoadCurrency();
            EnsureCurrencies();
            LoadAccounts();
            LoadCategories();
            LoadTransactions();
            LoadBudget();
            TotalWorkable();
            ButtonBudget.BorderThickness = new Thickness(6, 0, 0, 0);
           
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerAsync();

        }

        private static void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //LoadExchangeRate(); //222 ms
        }

        public decimal TotalBalance()
        {
            try
            {
                decimal sum = TotalIncome() - TotalExpanse();
                TextBlockBalance.Text = $"{sum:n} " + PrimaryCurrencySymbol; //$"{sum:0.00}";
                if (sum >= 0)
                {
                    TextBlockBalance.Foreground = Brushes.Green;
                }
                if (sum < 0)
                {
                    TextBlockBalance.Foreground = Brushes.Red;
                }
                return sum;
            }
            catch (OverflowException)
            {
                return MaxValue;
            }
        }

        public decimal TotalExpanse()
        {
            try
            {
                decimal sum =
                    _transactionList.Where(
                        transaction => !transaction.IsIncome && transaction.CurrencyId == PrimaryCurrency)
                        .Sum(transaction => transaction.Transactioned);
                TextBlockExpanse.Text = $"{sum:n} " + PrimaryCurrencySymbol;
                return sum;
            }
            catch (OverflowException)
            {
                return MaxValue;
            }
        }
        public decimal TotalWorkable()
        {
            try
            {
                //var bal = GlobalStatusCategoryList.Where(x => x.CurrencyId == PrimaryCurrency).Sum(x => x.Budgeted);
                string sql = $"SELECT SUM(Budgeted) FROM StatusCategory WHERE CurrencyID=={PrimaryCurrency};";
                var command = new SQLiteCommand(sql, _con);
                var reader = command.ExecuteReader();
                decimal budgeted = 0;
                while (reader.Read())
                {
                    budgeted = reader.GetDecimal(0);
                }
                decimal sum = TotalBalance() -budgeted;
                TextBlockWorkable.Text = $"{sum:n} " + PrimaryCurrencySymbol;
                TotalWarnings();
                return sum;
            }
            catch (OverflowException)
            {
                return MaxValue;
            }
        }

        public decimal TotalIncome()
        {
            try
            {
                decimal sum =
                    _transactionList.Where(
                        transaction => transaction.IsIncome && transaction.CurrencyId == PrimaryCurrency)
                        .Sum(transaction => transaction.Transactioned);
                TextBlockIncome.Text = $"{sum:n} " + PrimaryCurrencySymbol;
                if (sum >= 0)
                {
                    TextBlockIncome.Foreground = Brushes.Green;
                }
                if (sum < 0)
                {
                    TextBlockIncome.Foreground = Brushes.Red;
                }
                return sum;
            }
            catch (OverflowException)
            {
                return MaxValue;
            }
        }

        public string Attention { get; set; }
        public void TotalWarnings()
        {
           
            long sum = _transactionList.Count(transaction => transaction.Attention);
            if (sum > 0)
            {
                Attention = "/Assets/Attention.png";
                textBlockWarnings.Text = sum == 1 ? "One transaction needs your attention" : $"{sum} transactions need your attention";
            }
            else
            {
                Attention = "";
                textBlockWarnings.Text = "";
            }
        }



       /* public void LoadExchangeRate()
        {
            try
            {
                XDocument xdoc = XDocument.Load("https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");
                XNamespace ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";
                var cubes = xdoc.Descendants(ns + "Cube")
                               .Where(x => x.Attribute("currency") != null)
                               .Select(x => new {
                                   Currency = (string)x.Attribute("currency"),
                                   Rate = (decimal)x.Attribute("rate")
                               });
                string sql = "Delete from Exchange";
                SQLiteCommand sqLiteCommand= new SQLiteCommand(sql,_con);
                sqLiteCommand.ExecuteNonQuery();
                ExchangeRates = new List<ExchangeR>();
                using (var com = new SQLiteCommand(_con))
                {
                    using (var transaction = _con.BeginTransaction())
                    {

                        foreach (var resault in cubes)
                        {
                            sql = $"INSERT INTO Exchange VALUES (null, @currency,@rate)";
                            com.CommandText = sql;
                            com.Parameters.AddWithValue("@currency", resault.Currency);
                            com.Parameters.AddWithValue("@rate", resault.Rate);
                            com.ExecuteNonQuery();
                            //sql = //@"select last_insert_rowid()";
                            //sqLiteCommand = new SQLiteCommand(sql, _con);
                            long lastId = _con.LastInsertRowId;//(long) sqLiteCommand.ExecuteScalar();
                            ExchangeRates.Add(new ExchangeR(lastId, resault.Currency, resault.Rate));
                        }
                        ExchangeRates.Add(new ExchangeR(0, "EUR", 1));
                        transaction.Commit();
                    }
                }

            }
            catch (WebException)
            {
                string sql = "SELECT * FROM Exchange;";
                SQLiteCommand command = new SQLiteCommand(sql, _con);
                SQLiteDataReader reader = command.ExecuteReader();
                ExchangeRates = new List<ExchangeR>();
                while (reader.Read())
                {
                    long id = reader.GetInt64(0);
                    string currency = reader.GetString(1);
                    decimal rate = reader.GetDecimal(2);
                    ExchangeRates.Add(new ExchangeR(id, currency,rate));
                }
                //TODO no connection new budget
            }


        }*/

        public void LoadAccounts() //TODO account change currency with transactions
        {
            DataGridAccounts.Columns[0].Width = (DataGridAccounts.Width - 10) / 6;
            DataGridAccounts.Columns[1].Width = (DataGridAccounts.Width - 10) / 6;
            DataGridAccounts.Columns[2].Width = (DataGridAccounts.Width - 10) / 6;
            DataGridAccounts.Columns[3].Width = (DataGridAccounts.Width - 10) / 6;
            DataGridAccounts.Columns[4].Width = (DataGridAccounts.Width - 10) / 6;
            DataGridAccounts.Columns[5].Width = (DataGridAccounts.Width - 10) / 6;

            _accountList.Clear();
            var sql = "SELECT * FROM Account;";
            var command = new SQLiteCommand(sql, _con);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                long id = reader.GetInt64(0);
                sql = $"SELECT * FROM Transactions WHERE AccountID = {id}";
                command = new SQLiteCommand(sql, _con);
                var transReader = command.ExecuteReader();
                decimal income = 0, expanse = 0;
                while (transReader.Read())
                {
                    //TODO trans value
                    if (transReader.GetBoolean(2))
                    {
                        // used to by (5), change back if broken
                        income += transReader.GetDecimal(9);
                    }
                    else
                    {
                        expanse += transReader.GetDecimal(9);
                    }
                }
                var name = reader.GetString(1);
                var info = reader.GetString(2);
                //decimal balance = reader.GetDecimal(3);
                long currency = reader.GetInt64(3);
               var account = new Account(id,name,info,currency, income,expanse);
                _accountList.Add(account);
            }
            DataGridAccounts.ItemsSource = _accountList;
            DataGridAccounts.Items.Refresh();
            DataGridOtherAccounts.Columns[0].Width = 102.5;//(DataGridOtherAccounts.Width -10 ) /2;
            DataGridOtherAccounts.Columns[1].Width = 102.5;//(DataGridOtherAccounts.Width) / 2;
            DataGridOtherAccounts.ItemsSource = _accountList;
            DataGridOtherAccounts.Items.Refresh();
        }
        public void LoadCurrency()
        {
            CurrencyList = new List<Currency>();
            const string sql = "SELECT * FROM Currency;";
            var command = new SQLiteCommand(sql, _con);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                long id = reader.GetInt64(0);
                var symbol = reader.GetString(1);
                var currencyString = reader.GetString(2);
                var primary = reader.GetBoolean(3);
                if (primary)
                {
                    PrimaryCurrency = id;
                    PrimaryCurrencyName = currencyString;
                    PrimaryCurrencySymbol = symbol;
                }
                var currency = new Currency(id,symbol,currencyString,primary);
                CurrencyList.Add(currency);
            }

        }

        public void EnsureCurrencies()
        {
            var databaseCurrencies = new List<Currency>(); 
            const string sql = "SELECT * FROM Currency;";
            var command = new SQLiteCommand(sql, _con);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                long id = reader.GetInt64(0);
                var symbol = reader.GetString(1);
                var currencyString = reader.GetString(2);
                var primary = reader.GetBoolean(3);
                if (primary)
                {
                    PrimaryCurrency = id;
                }
                var currency = new Currency(id, symbol, currencyString, primary);
                databaseCurrencies.Add(currency);
            }
            using (var com = new SQLiteCommand(_con))
            {
                using (var transaction = _con.BeginTransaction())
                {
                    foreach (var currency in CurrencyList)
                    {
                        if (databaseCurrencies.Exists(x => x.CurrencyString.Equals(currency.CurrencyString))) continue;
                        var primaryValue = 0;
                        if (currency.CurrencyString.Equals(PrimaryCurrencyName))
                        {
                            primaryValue = 1;
                        }
                        string sqlCurrency =
                            $"INSERT INTO Currency VALUES (null, '{currency.Symbol}','{currency.CurrencyString}',{primaryValue})";
                        com.CommandText = sqlCurrency;
                        com.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
        public void LoadCategories()
        {
            CategoryList.Clear();
            MasterCategoryList?.Clear();
            DataGridCategories.Columns[0].Width = (DataGridCategories.Width - 8) / 5;
            DataGridCategories.Columns[1].Width = (DataGridCategories.Width - 8) / 5;
            DataGridCategories.Columns[2].Width = (DataGridCategories.Width - 8) / 5;
            DataGridCategories.Columns[3].Width = (DataGridCategories.Width - 8) / 5;
            string sql = $"SELECT * FROM MasterCategory";
            var command = new SQLiteCommand(sql, _con);
            var reader = command.ExecuteReader();
            MasterCategoryList = new List<MasterCategory>();
            while (reader.Read())
            {
                long id = reader.GetInt64(0);
                var name = reader.GetString(1);
                MasterCategoryList.Add(new MasterCategory(id,name));
            }
            sql = $"SELECT * FROM Category";
            command = new SQLiteCommand(sql, _con);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                long id = reader.GetInt64(0);
                var name = reader.GetString(1);
                var info = reader.GetString(2);
                long currencyId = reader.GetInt64(3);
                long masterCategoryId = reader.GetInt64(4);
                var cat = new Category(id, name, info, currencyId, masterCategoryId);
                decimal value = _transactionList.Where(x => x.CategoryId.Equals(id)&&!x.IsIncome).Sum(transaction => transaction.Transactioned);
                cat.TotalValue = value;
                CategoryList.Add(cat);
                //dataGridCategories.Items.Add(cat);
            }

            DataGridCategories.ItemsSource = CategoryList;
            DataGridCategories.Items.Refresh();
        }

        public void LoadTransactions()
        {
            DataGridTransactions.Columns[1].Width = 80;//(DataGridTransactions.Width - 8) /8;
            DataGridTransactions.Columns[2].Width = 150;//(DataGridTransactions.Width - 8) /8;
            DataGridTransactions.Columns[3].Width = 100;//(DataGridTransactions.Width - 8) /8;
            DataGridTransactions.Columns[4].Width = 150;//(DataGridTransactions.Width - 8) /8;
            DataGridTransactions.Columns[5].Width = 150;//(DataGridTransactions.Width - 8) /8;
            DataGridTransactions.Columns[6].Width = 150;//(DataGridTransactions.Width - 8) /8;
            DataGridTransactions.Columns[7].Width = 150;//(DataGridTransactions.Width - 8) /8;
           // DataGridTransactions.Columns[8].Width = 120;//(DataGridTransactions.Width - 8)/8;
            _transactionList.Clear();
            var sql = "SELECT * FROM Transactions ORDER BY Date DESC";
            var command = new SQLiteCommand(sql, _con);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                long catId = reader.GetInt64(1);
                sql = $"SELECT * FROM Category WHERE CATEGORY.ID=={catId}";
                command = new SQLiteCommand(sql, _con);
                var catReader = command.ExecuteReader();
                var category = "Income/Transfer";
                while (catReader.Read())
                {
                    category = catReader.GetString(1);
                    break;
                }

                long id = reader.GetInt64(0);
                var isIncome = reader.GetBoolean(2);
                var info = reader.GetString(3);
                var payee = reader.GetString(4);
                decimal transactioned = reader.GetDecimal(5);
                var date = reader.GetDateTime(6);
                long currencyId = reader.GetInt64(7);
                long accountId = reader.GetInt64(8);
                decimal accounted = reader.GetDecimal(9);
                var attention = reader.GetBoolean(10);
                sql = $"SELECT * FROM Account WHERE ID=={accountId}";
                command = new SQLiteCommand(sql, _con);
                catReader = command.ExecuteReader();
                var accountName = "";
                while (catReader.Read())
                {
                    accountName = catReader.GetString(1);
                    break;
                }
                var transaction = new Transaction(id, catId,category, isIncome, info, payee, transactioned, currencyId,accountId, accountName, accounted, date,
                     attention);
                _transactionList.Add(transaction);
            }
            DataGridTransactions.ItemsSource = _transactionList;
            DataGridTransactions.Items.Refresh();

        }

        public void LoadBudget()
        {
            var selecteDateTime = SelectedDate;
            const string sql = "SELECT * FROM MonthBudget";
            var command = new SQLiteCommand(sql, _con);
            var reader = command.ExecuteReader();
            //string sql = $"SELECT * FROM MonthBudget WHERE strftime('%m', Date) = {selecteDateTime.Month}, strftime('%Y', Date)= {selecteDateTime.Year} ";
            //SELECT * FROM MonthBudget WHERE strftime('%m', Date) = 'JAN'
            var monthBudgetFound = false;
            DataGridBudget.ItemsSource = null;
            StatusCategoryList.Clear();
            while (reader.Read())
            {
                var sqlDateTime = reader.GetDateTime(1);
                if (!sqlDateTime.Month.Equals(selecteDateTime.Month) || !sqlDateTime.Year.Equals(selecteDateTime.Year))
                    continue;
                monthBudgetFound = true;
                FillMonthBudget(reader.GetInt64(0), selecteDateTime);
                break;
            }
            if (monthBudgetFound) return;
            var monthBudgeDateTime = new DateTime(selecteDateTime.Year, selecteDateTime.Month, 1);
            var monthId = CreateMonthBudget(monthBudgeDateTime);
            FillMonthBudget(monthId, selecteDateTime);
        }

        //public void LoadBudget(DateTime budgetDate)
        //{
        // 
        //}
        public long CreateMonthBudget(DateTime month)
        {
            var monthBudgeDateTime = month;
            string insertSql = $"INSERT INTO MonthBudget (ID, Date) VALUES (null, '{monthBudgeDateTime}')";
            var command = new SQLiteCommand(insertSql, _con);
            command.ExecuteNonQuery();
            var monthId = _con.LastInsertRowId;
            const string sql = "SELECT * FROM Category";
            command = new SQLiteCommand(sql, _con);
            var reader = command.ExecuteReader();
            using (var com = new SQLiteCommand(_con))
            { 
                using (var transaction = _con.BeginTransaction())
                {
                    while (reader.Read())
                    {

                            insertSql =
                                $"INSERT INTO StatusCategory VALUES (null, 0,{monthId},{reader.GetInt64(0)},{reader.GetInt64(3)})";
                            com.CommandText = insertSql;
                            com.ExecuteNonQuery();
                        }
                    transaction.Commit();
                    return monthId;
                }
            }
        }

        public void FillMonthBudget(long monthId, DateTime date)
        {
            DataGridBudget.Columns[0].Width = 60;
            DataGridBudget.Columns[1].Width = 200;
            DataGridBudget.Columns[2].Width = 250;
            DataGridBudget.Columns[3].Width = 200;
            DataGridBudget.Columns[4].Width = 200;
            string sql = $"SELECT * FROM StatusCategory WHERE MonthBudgetID={monthId}";
            var command = new SQLiteCommand(sql, _con);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                //TODO is it neccessary select from db? test this out with lists
                long id = reader.GetInt64(0);
                decimal budgeted = reader.GetDecimal(1);
                long categoryId = reader.GetInt64(3);
                var category = "";
                sql = $"SELECT * FROM Category WHERE ID={categoryId}";
                command = new SQLiteCommand(sql, _con);
                var readerCategory = command.ExecuteReader();
                decimal transactioned = 0;
                long currencyId = reader.GetInt64(4);
                long masterCategoryId = 0;
                while (readerCategory.Read())
                {
                    category = readerCategory.GetString(1);
                    masterCategoryId = readerCategory.GetInt64(4);
                    break;
                }
                var currencySymbol = ""; 
                var currencyString = "";
                sql = $"SELECT * FROM Currency WHERE ID = {currencyId}";
                command = new SQLiteCommand(sql, _con);
                var readerCurrency = command.ExecuteReader();
                while (readerCurrency.Read())
                {
                    currencySymbol = readerCurrency.GetString(1);
                    currencyString = readerCurrency.GetString(2);
                }
                sql = $"SELECT * FROM Transactions WHERE CategoryID={categoryId}";
                command = new SQLiteCommand(sql, _con);
                var readerTransactions = command.ExecuteReader();
                while (readerTransactions.Read())
                {
                    var sqlDateTime = readerTransactions.GetDateTime(6);
                    if (sqlDateTime.Month.Equals(date.Month) && sqlDateTime.Year.Equals(date.Year) &&
                        readerTransactions.GetBoolean(2).Equals(false))
                    {
                        transactioned += readerTransactions.GetDecimal(5);
                    }
                }

                var statusCategory = new StatusCategory(id, categoryId, category, budgeted,
                    transactioned, currencyId, currencySymbol, currencyString, monthId,masterCategoryId);
                StatusCategoryList.Add(statusCategory);
            }
            var collectionBudget = new ListCollectionView(StatusCategoryList);
            collectionBudget.GroupDescriptions.Add(new PropertyGroupDescription("MasterCat"));
            DataGridBudget.ItemsSource = collectionBudget;
        }
        public int DeterminStateStatusCategory(decimal budgeted, decimal spent)
        {
            var result = budgeted - spent;
            if (result > 0)
            {
                return 1;
            }
            if (result < 0)
            {
                return -1;
            }
            return 0;
        }

        private void ButtonCategories_Click(object sender, RoutedEventArgs e)
        {
            ButtonCategories.BorderThickness = new Thickness(6, 0, 0, 0);
            ButtonTransactions.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonBudget.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonReview.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonAccounts.BorderThickness = new Thickness(0, 0, 0, 0);
            GridBudget.Visibility = Visibility.Hidden;
            GridCategories.Visibility = Visibility.Visible;
            GridReview.Visibility = Visibility.Hidden;
            GridTransactions.Visibility = Visibility.Hidden;
            GridAccounts.Visibility = Visibility.Hidden;
        }

        private void ButtonTransactions_Click(object sender, RoutedEventArgs e)
        {
            ButtonCategories.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonTransactions.BorderThickness = new Thickness(6, 0, 0, 0);
            ButtonBudget.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonReview.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonAccounts.BorderThickness = new Thickness(0, 0, 0, 0);
            GridBudget.Visibility = Visibility.Hidden;
            GridCategories.Visibility = Visibility.Hidden;
            GridReview.Visibility = Visibility.Hidden;
            GridTransactions.Visibility = Visibility.Visible;
            GridAccounts.Visibility = Visibility.Hidden;
        }

        private void ButtonBudget_Click(object sender, RoutedEventArgs e)
        {
            ButtonCategories.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonTransactions.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonBudget.BorderThickness = new Thickness(6, 0, 0, 0);
            ButtonReview.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonAccounts.BorderThickness = new Thickness(0, 0, 0, 0);
            GridBudget.Visibility = Visibility.Visible;
            GridCategories.Visibility = Visibility.Hidden;
            GridReview.Visibility = Visibility.Hidden;
            GridTransactions.Visibility = Visibility.Hidden;
            GridAccounts.Visibility = Visibility.Hidden;
        }

        private void ButtonReview_Click(object sender, RoutedEventArgs e)
        {
            ButtonCategories.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonTransactions.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonBudget.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonReview.BorderThickness = new Thickness(6, 0, 0, 0);
            ButtonAccounts.BorderThickness = new Thickness(0, 0, 0, 0);
            GridBudget.Visibility = Visibility.Hidden;
            GridCategories.Visibility = Visibility.Hidden;
            GridReview.Visibility = Visibility.Visible;
            GridTransactions.Visibility = Visibility.Hidden;
            GridAccounts.Visibility = Visibility.Hidden;
        }
        private void ButtonAccounts_Click(object sender, RoutedEventArgs e)
        {
            ButtonCategories.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonTransactions.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonBudget.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonReview.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonAccounts.BorderThickness = new Thickness(6, 0, 0, 0);
            GridBudget.Visibility = Visibility.Hidden;
            GridCategories.Visibility = Visibility.Hidden;
            GridReview.Visibility = Visibility.Hidden;
            GridTransactions.Visibility = Visibility.Hidden;
            GridAccounts.Visibility = Visibility.Visible;
        }

        private void TextBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            //ListCollectionView view = (ListCollectionView)DataGridBudget.ItemsSource;
            //StatusCategoryList = (List<StatusCategory>) DataGridBudget.ItemsSource; 
            var row = (DataGridRow) DataGridBudget.ItemContainerGenerator.ContainerFromItem(DataGridBudget.SelectedItem);
            if (row == null) return;
            var textBox = (TextBox)e.Source;
            var rowIndex = row.GetIndex();
            //decimal value = StatusCategoryList[rowIndex].Budgeted;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = 0.00.ToString();
            }
            //decimal textBoxValue = Convert.ToDecimal(textBox.Text);
            //if (textBoxValue != value) //WONT SAVE BECOUSE OF THIS             //TODO loss of focus sometimes doesnt commit value
            //{
            if (textBox.Text.Equals(""))
            {
                textBox.Text = "0.00";
            }
            decimal budgeted = Convert.ToDecimal(textBox.Text);
            //TODO check if StatusCategory List is upodated with changed cat

            var cat = (StatusCategory) row.Item;
            StatusCategoryList.First(x => x.Category == cat.Category && x.MonthId == cat.MonthId).Budgeted = budgeted;
            string sql = $"UPDATE StatusCategory SET Budgeted = {budgeted} WHERE Id = {cat.Id};";//CategoryId = {cat.CategoryId} AND MonthBudgetID = {cat.MonthId};";
            var command = new SQLiteCommand(sql, _con);
            command.ExecuteNonQuery();
            //TODO update workable
            TotalWorkable();
            DataGridBudget.CommitEdit();
            DataGridBudget.CancelEdit(); //TODO prevent unloading, incrases memory usage => faster
            DataGridBudget.Items.Refresh();
            DataGridBudget.UnselectAll();
            //RefreshAll();
            //}
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            ValidateTextDecimal(textBox);

        }
        public void ValidateTextDecimal(TextBox textBox)
        {
            if (textBox == null) return;
            var text = textBox.Text;
            var validatedText = new StringBuilder();
            var dotFound = false;
            var prevChar = '\0';
            for (var i = 0; i < text.Length; i++)
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
            var tb = (sender as TextBox);
            tb?.SelectAll();
        }


        private void SelectivelyIgnoreMousebutton(object sender, MouseButtonEventArgs e)
        {
            var tb = (sender as TextBox);

            if (tb == null) return;
            if (tb.IsKeyboardFocusWithin) return;
            e.Handled = true;
            tb.Focus();
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell == null || cell.IsEditing || cell.IsReadOnly) return;
            if (!cell.IsFocused)
            {
                cell.Focus();
            }
            var dataGrid = FindVisualParent<DataGrid>(cell);
            if (dataGrid == null) return;
            if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
            {
                if (!cell.IsSelected)
                    cell.IsSelected = true;
            }
            else
            {
                var row = FindVisualParent<DataGridRow>(cell);
                if (row != null && !row.IsSelected)
                {
                    row.IsSelected = true;
                }
            }
        }

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        private void TextBox_LostFocus_2(object sender, RoutedEventArgs e)
        {
            CategoryList = (List<Category>)DataGridCategories.ItemsSource;
            var row = (DataGridRow)DataGridCategories.ItemContainerGenerator.ContainerFromItem(DataGridCategories.SelectedItem);
            if (row == null) return;
            var textBox = (TextBox) e.Source;
            if (textBox.Text == CategoryList[row.GetIndex()].Name) return;
            if (textBox.Text.Equals(""))
            {
                textBox.Text = "Category";
            }
            var text = textBox.Text;
            CategoryList[row.GetIndex()].Name = text;
            string sql =
                $"UPDATE Category SET Name = @name WHERE Id = {CategoryList[row.GetIndex()].Id};";
            var command = new SQLiteCommand(sql, _con);
            command.Parameters.AddWithValue("@name", text);
            command.ExecuteNonQuery();
            DataGridCategories.CommitEdit();
            DataGridCategories.Items.Refresh();
            DataGridCategories.UnselectAll();
            RefreshAll();
        }

        private void TextBox_LostFocus_3(object sender, RoutedEventArgs e)
        {
            CategoryList = (List<Category>)DataGridCategories.ItemsSource;
            var row = (DataGridRow)DataGridCategories.ItemContainerGenerator.ContainerFromItem(DataGridCategories.SelectedItem);
            if (row == null) return;
            var textBox = (TextBox) e.Source;
            var text = textBox.Text;
            if (text == CategoryList[row.GetIndex()].Info) return;
            CategoryList[row.GetIndex()].Info = text;
            string sql =
                $"UPDATE Category SET Info = @info WHERE Id = {CategoryList[row.GetIndex()].Id};";
            var command = new SQLiteCommand(sql, _con);
            command.Parameters.AddWithValue("@info", text);
            command.ExecuteNonQuery();
            DataGridCategories.CommitEdit();
            DataGridCategories.Items.Refresh();
            DataGridCategories.UnselectAll();
            RefreshAll();
        }

        public void CheckBoxSavingsChanged(object sender, RoutedEventArgs e)
        {
        }

        private void CurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CategoryList = (List<Category>)DataGridCategories.ItemsSource;
            var row = (DataGridRow)DataGridCategories.ItemContainerGenerator.ContainerFromItem(DataGridCategories.SelectedItem);
            if (row == null) return;
            var comboBox = (ComboBox)e.Source;
            var currency = (Currency) comboBox.SelectedItem;
            var currencyId = currency.Id;
            CategoryList[row.GetIndex()].CurrencyId = currencyId;
            var categoryId = CategoryList[row.GetIndex()].Id;
            string sql =
                $"UPDATE Category SET CurrencyID = @currencyID WHERE Id = {categoryId};";
            var command = new SQLiteCommand(sql, _con);
            command.Parameters.AddWithValue("@currencyID", currencyId);
            command.ExecuteNonQuery();
            sql = $"UPDATE Transactions SET Attention = 1 WHERE CategoryID={categoryId};";
            command = new SQLiteCommand(sql, _con);
            command.ExecuteNonQuery();
            sql = $"UPDATE StatusCategory SET CurrencyID =  @currencyID WHERE CategoryID={categoryId};";
            command = new SQLiteCommand(sql, _con);
            command.Parameters.AddWithValue("@currencyID", currencyId);
            command.ExecuteNonQuery();
            DataGridCategories.CommitEdit();
            DataGridCategories.Items.Refresh();
            DataGridCategories.UnselectAll();
            RefreshAll();
        }

        public void RefreshAll()
        {
            LoadTransactions(); 
            LoadCategories();
            LoadAccounts();
            LoadBudget();
            TotalWorkable();
        }

        public void SyncList()
        {

        }

        private void CurrencyComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e) //TODO transaction, why does this get called on start
        {
            var comboBox = (ComboBox) e.Source;
            _accountList = (List<Account>)DataGridAccounts.ItemsSource;
            var row = (DataGridRow)DataGridAccounts.ItemContainerGenerator.ContainerFromItem(DataGridAccounts.SelectedItem);
            if (row == null || !_userCreated) return;
            var currency = (Currency)comboBox.SelectedItem;
            var currencyId = currency.Id;
            _accountList[row.GetIndex()].CurrencyId = currencyId;
            var accountId = _accountList[row.GetIndex()].Id;
            string sql =
                $"UPDATE Account SET CurrencyID = @currencyID WHERE Id = {accountId};";
            var command = new SQLiteCommand(sql, _con);
            command.Parameters.AddWithValue("@currencyID", currencyId);
            command.ExecuteNonQuery();
            sql = $"UPDATE Transactions SET CurrencyID = @currencyID, Attention = 1 WHERE AccountID = {accountId};";
            command = new SQLiteCommand(sql, _con);
            command.Parameters.AddWithValue("@currencyID", currencyId);
            command.ExecuteNonQuery();
            DataGridCategories.CommitEdit();
            DataGridCategories.Items.Refresh();
            DataGridCategories.UnselectAll();
            RefreshAll();
        }

        private void ButtonAddTransaction_Click(object sender, RoutedEventArgs e)
        {

            var addTransaction = new AddTransaction(CategoryList, _accountList,CurrencyList) {Owner = GetWindow(this)};
            addTransaction.ShowDialog();
            if (!addTransaction.IsSuccesful) return;
            var recivedCategory = addTransaction.Category;
            var recivedAccount = addTransaction.Account;
            decimal valueCat = addTransaction.ValueCat;
            decimal valueAcc = addTransaction.ValueAcc; //TODO - account value
            var isIncome = addTransaction.IsIncome;
            var note = addTransaction.Note;
            var payee = addTransaction.Payee;
            var income = isIncome ? 1 : 0;
            var isql = "";
            isql = isIncome
                ? $"INSERT INTO Transactions VALUES (null, 0,{income},@note,@payee,{valueCat},'{DateTime.Now}',{recivedAccount.CurrencyId},{recivedAccount.Id},{valueAcc},0);"
                : $"INSERT INTO Transactions VALUES(null, {recivedCategory.Id}, {income},@note,@payee,{valueCat},'{DateTime.Now}',{recivedAccount.CurrencyId},{recivedAccount.Id},{valueAcc},0)";
            var icommand = new SQLiteCommand(isql, _con);
            icommand.Parameters.AddWithValue("@note", note);
            icommand.Parameters.AddWithValue("@payee", payee);
            icommand.ExecuteNonQuery();
            RefreshAll();
        }

        private void DataGridTransactions_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Key.Equals(Key.Delete)) return;
            if (DataGridTransactions.SelectedItems.Count.Equals(0)) return;
            long count = DataGridTransactions.SelectedItems.Count;
            var text = "";
            text = count.Equals(1) ? "Are you sure you want to delete this transaction?" : $"Are you sure you want to delete these {count} transactions?";
            var yesNo = new YesNo(text) { Owner = GetWindow(this) };
            yesNo.ShowDialog();
            if (!yesNo.IsSuccesful) return;
            foreach (var item in DataGridTransactions.SelectedItems)
            {
                var transaction = (Transaction)item; //TODO selecteditems
                string sql = $"DELETE FROM Transactions WHERE ID={transaction.Id}";
                var command = new SQLiteCommand(sql, _con);
                command.ExecuteNonQuery();
            }

            RefreshAll();
        }

        private void DataGridTransactions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataGridTransactions.SelectedItem == null) return;
            var transaction = (Transaction) DataGridTransactions.SelectedItem;
            var addTransaction = new AddTransaction(CategoryList, _accountList,
                CurrencyList, transaction.CategoryId, transaction.IsIncome, transaction.Info,
                transaction.Accounted, transaction.AccountId, transaction.Payee, transaction.Transactioned) {Owner = GetWindow(this)};
            addTransaction.ShowDialog();
            if (!addTransaction.IsSuccesful) return;
            var recivedCategory = addTransaction.Category;
            var recivedAccount = addTransaction.Account;
            decimal valueCat = addTransaction.ValueCat;
            decimal valueAcc = addTransaction.ValueAcc; //TODO - account value
            var isIncome = addTransaction.IsIncome;
            var note = addTransaction.Note;
            var payee = addTransaction.Payee;
            var income = isIncome ? 1 : 0;
            var isql = "";
            isql = isIncome ? $"UPDATE Transactions SET CategoryID = 0, IsIncome = {income}, Info = @note, Payee = @payee, Transactioned = {valueCat}, CurrencyID = {recivedAccount.CurrencyId}, AccountID ={recivedAccount.Id}, Accounted={valueAcc}, Attention = 0  WHERE ID = {transaction.Id}" : $"UPDATE Transactions SET CategoryID = {recivedCategory.Id}, IsIncome = {income}, Info = @note, Payee = @payee, Transactioned = {valueCat}, CurrencyID = {recivedCategory.CurrencyId}, AccountID ={recivedAccount.Id}, Accounted={valueAcc},Attention = 0  WHERE ID = {transaction.Id}";   
            var icommand = new SQLiteCommand(isql, _con);
            icommand.Parameters.AddWithValue("@note", note);
            icommand.Parameters.AddWithValue("@payee", payee);
            icommand.ExecuteNonQuery();
            RefreshAll();
        }

        private void ButtonAddAccount_OnClick(object sender, RoutedEventArgs e)
        {
            var addAccount= new AddAccount(CurrencyList) {Owner = GetWindow(this)};
            addAccount.ShowDialog();
            if (!addAccount.IsSuccesful) return;
            using (var com = new SQLiteCommand(_con))
            {
                using (var transaction = _con.BeginTransaction())
                {
                    string sqlA = $"INSERT INTO Account VALUES (null, @name,@info,{addAccount.SelectedCurrency.Id})";
                    com.CommandText = sqlA;
                    com.Parameters.AddWithValue("@name", addAccount.Name);
                    com.Parameters.AddWithValue("@info", addAccount.Info);
                    com.ExecuteNonQuery();

                    if (addAccount.Balance != 0)
                    {
                        //sqlA = @"select last_insert_rowid()";
                        //com.CommandText = sqlA;
                        var lastId = _con.LastInsertRowId;//(long) com.ExecuteScalar();
                        if (addAccount.Balance > 0)
                        {
                            string isql =
                                $"INSERT INTO Transactions  VALUES(null, 0, 1,'Starting balance','Starting balance',{Math.Abs(addAccount.Balance)},'{DateTime.Now}',{addAccount.SelectedCurrency.Id},{lastId},{addAccount.Balance},0)";
                            com.CommandText = isql;
                            com.ExecuteNonQuery();
                        }
                        else
                        {
                            string isql =
                                $"INSERT INTO Transactions VALUES(null, 0, 0,'Starting balance','Starting balance',{Math.Abs(addAccount.Balance)},'{DateTime.Now}',{addAccount.SelectedCurrency.Id},{lastId},{addAccount.Balance},0)";
                            com.CommandText = isql;
                            com.ExecuteNonQuery();
                        }

                    }
                    transaction.Commit();
                }
            }
            RefreshAll();
        }

        private void DataGridCategories_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Key.Equals(Key.Delete)) return;
            if (DataGridCategories.SelectedItems.Count.Equals(0)) return;
            decimal value = DataGridCategories.SelectedItems.Cast<Category>().Sum(item => item.TotalValue);
            long count = DataGridCategories.SelectedItems.Count;
            var text = "";
            text = count.Equals(1) ? $"Are you sure you want to delete this category with total value {value}?" : $"Are you sure you want to delete {count} categories with total value {value}?";
            var yesNo = new YesNoReCategorize(text, CategoryList) { Owner = GetWindow(this) };
            yesNo.ShowDialog();
            if (!yesNo.IsSuccesful) return;
            {
                if (yesNo.Recategorize)
                {
                    foreach (var item in DataGridCategories.SelectedItems)
                    {
                        var category = (Category)item;
                        string sql = $"DELETE FROM StatusCategory WHERE CategoryID = {category.Id}";
                        var command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                        sql = $"UPDATE Transactions  SET CategoryID = {yesNo.Category.Id},  Attention = 1  WHERE CategoryID = {category.Id}";
                        command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                        sql = $"DELETE FROM Category WHERE ID={category.Id}";
                        command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    foreach (var item in DataGridCategories.SelectedItems)
                    {
                        var category = (Category)item;
                        string sql = $"DELETE FROM StatusCategory WHERE CategoryID = {category.Id}";
                        var command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                        sql = $"DELETE FROM Transactions WHERE CategoryID = {category.Id}";
                        command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                        sql = $"DELETE FROM Category WHERE ID={category.Id}";
                        command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                    }
                }

                RefreshAll();
            }
        }

        private void ButtonAddCategory_Click(object sender, RoutedEventArgs e)
        {
            var addCategory = new AddCategory(CurrencyList,PrimaryCurrency) {Owner = GetWindow(this)};
            addCategory.ShowDialog();
            if (!addCategory.IsSuccesful) return;
            var sql = "SELECT * From MonthBudget";
            var command = new SQLiteCommand(sql, _con) {CommandText = sql};
            var reader = command.ExecuteReader();
            using (var com = new SQLiteCommand(_con))
            {
                using (var transaction = _con.BeginTransaction())
                {
                    sql =
                        $"INSERT INTO Category VALUES(null, @name, @info,  {addCategory.SelectedCurrency.Id},'MASTER CAT')"; 
                    com.CommandText = sql;
                    com.Parameters.AddWithValue("@name", addCategory.Name);
                    com.Parameters.AddWithValue("@info", addCategory.Info);
                    com.ExecuteNonQuery();
                    //sql = @"select last_insert_rowid()";
                    //com.CommandText = sql;
                    long lastId = _con.LastInsertRowId;// (long)com.ExecuteScalar();
                    while (reader.Read())
                    {
                        sql =
                            $"INSERT INTO StatusCategory VALUES (null, 0,{reader.GetInt64(0)},{lastId},{addCategory.SelectedCurrency.Id})"; //TODO MASTERCATEGORY
                        com.CommandText = sql;
                        com.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
            RefreshAll();
        }
        //TODO FIX VAR TO DECIMAL!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private void ButtonRemoveCategory_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridCategories.SelectedItems.Count.Equals(0)) return;
            decimal value = DataGridCategories.SelectedItems.Cast<Category>().Sum(item => item.TotalValue);
            long count = DataGridCategories.SelectedItems.Count;
            var text = "";
            var localCategoryLists = DataGridCategories.SelectedItems.Cast<Category>().ToList();
            localCategoryLists = new List<Category>(CategoryList.Except(localCategoryLists));
            text = count.Equals(1) ? $"Are you sure you want to delete this category with total value {value}?" : $"Are you sure you want to delete {count} categories with total value {value}?";
            var yesNo = new YesNoReCategorize(text, localCategoryLists) { Owner = GetWindow(this) };
            yesNo.ShowDialog();
            if (!yesNo.IsSuccesful) return;
            {
                if (yesNo.Recategorize)
                {
                    foreach (var item in DataGridCategories.SelectedItems)
                    {
                        var category = (Category)item;
                        string sql = $"DELETE FROM StatusCategory WHERE CategoryID = {category.Id}";
                        var command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                        //TODO at least one category or master category/ handle not having any.. to be decided
                        sql = $"UPDATE Transactions  SET CategoryID = {yesNo.Category.Id},  Attention = 1  WHERE CategoryID = {category.Id}";
                        command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                        sql = $"DELETE FROM Category WHERE ID={category.Id}";
                        command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    foreach (var item in DataGridCategories.SelectedItems)
                    {
                        var category = (Category)item;
                        string sql = $"DELETE FROM StatusCategory WHERE CategoryID = {category.Id}";
                        var command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                        sql = $"DELETE FROM Transactions WHERE CategoryID = {category.Id}";
                        command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                        sql = $"DELETE FROM Category WHERE ID={category.Id}";
                        command = new SQLiteCommand(sql, _con);
                        command.ExecuteNonQuery();
                    }
                }

                RefreshAll();
            }
        }

        private void ButtonRemoveTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTransactions.SelectedItems.Count.Equals(0)) return;
            long count = DataGridTransactions.SelectedItems.Count;
            var text = "";
            text = count.Equals(1) ? "Are you sure you want to delete this transaction?" : $"Are you sure you want to delete {count} transactions?";
            var yesNo = new YesNo(text) {Owner = GetWindow(this)};
            yesNo.ShowDialog();
            if (!yesNo.IsSuccesful) return;
            foreach (var item in DataGridTransactions.SelectedItems)
            {
                var transaction = (Transaction)item;
                string sql = $"DELETE FROM Transactions WHERE ID={transaction.Id}";
                var command = new SQLiteCommand(sql, _con);
                command.ExecuteNonQuery();
            }

            RefreshAll();
        }

        private void ButtonPrevMonth_Click(object sender, RoutedEventArgs e)
        {
            ButtonPrevMonth.IsEnabled = false;
            if (SelectedDate.Month.Equals(1))
            {
                SelectedDate = SelectedDate.AddYears(-1);
                SelectedDate = SelectedDate = new DateTime(SelectedDate.Year, 12, 1);
            }
            else
            {
                SelectedDate = SelectedDate.AddMonths(-1);
            }
            TextBlockDate.Text = SelectedDate.ToString("MMMM-yyyy");
            LoadBudget();
            ButtonPrevMonth.IsEnabled = true;
        }

        private void ButtonNextMonth_Click(object sender, RoutedEventArgs e)
        {
            ButtonNextMonth.IsEnabled = false;
            if (SelectedDate.Month.Equals(12))
            {
                SelectedDate = SelectedDate.AddYears(1);
                SelectedDate = new DateTime(SelectedDate.Year, 1, 1);
            }
            else
            {
                SelectedDate = SelectedDate.AddMonths(1);
            }
            TextBlockDate.Text = SelectedDate.ToString("MMMM-yyyy");
            LoadBudget();
            ButtonNextMonth.IsEnabled = true;
        }

        private void DataGridAccounts_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Key.Equals(Key.Delete)) return;
            if (DataGridAccounts.SelectedItems.Count.Equals(0)) return;
            long count = DataGridAccounts.SelectedItems.Count;
            var text = "";
            if (count.Equals(1))
            {
                var account = (Account) DataGridAccounts.SelectedItems[0];
                text = $"Are you sure you want to delete account \"{account.Name}\"?";
            }
            else
            {
                text = $"Are you sure you want to delete {count} accounts?";
            }
            var yesNo = new YesNo(text) { Owner = GetWindow(this) };
            yesNo.ShowDialog();
            if (!yesNo.IsSuccesful) return;
            foreach (var item in DataGridAccounts.SelectedItems) //118 ms
            {
                var transaction = (Account) item;
                string sql = $"DELETE FROM Transactions Where AccountId ={transaction.Id}";
                var command = new SQLiteCommand(sql, _con);
                command.ExecuteNonQuery();
                sql = $"DELETE FROM Account WHERE ID={transaction.Id}";
                command = new SQLiteCommand(sql,_con);
                command.ExecuteNonQuery();
            }
            RefreshAll();
        }

        private void ButtonRemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridAccounts.SelectedItems.Count.Equals(0)) return;
            long count = DataGridAccounts.SelectedItems.Count;
            var text = "";
            if (count.Equals(1))
            {
                var account = (Account)DataGridAccounts.SelectedItems[0];
                text = $"Are you sure you want to delete account \"{account.Name}\"?";
            }
            else
            {
                text = $"Are you sure you want to delete {count} accounts?";
            }
            var yesNo = new YesNo(text) { Owner = GetWindow(this) };
            yesNo.ShowDialog();
            if (!yesNo.IsSuccesful) return;
            foreach (var item in DataGridAccounts.SelectedItems)
            {
                var transaction = (Account)item;
                string sql = $"DELETE FROM Transactions Where AccountId ={transaction.Id}";
                var command = new SQLiteCommand(sql, _con);
                command.ExecuteNonQuery();
                sql = $"DELETE FROM Account WHERE ID={transaction.Id}";
                command = new SQLiteCommand(sql, _con);
                command.ExecuteNonQuery();
            }
            RefreshAll();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _accountList = (List<Account>)DataGridAccounts.ItemsSource;
            var row = (DataGridRow)DataGridAccounts.ItemContainerGenerator.ContainerFromItem(DataGridAccounts.SelectedItem);
            if (row == null) return;
            var textBox = (TextBox)e.Source;
            var text = textBox.Text;
            if (text == _accountList[row.GetIndex()].Name) return;
            _accountList[row.GetIndex()].Name = text;
            string sql =
                $"UPDATE Account SET AccountName = @name WHERE Id = {_accountList[row.GetIndex()].Id};";
            var command = new SQLiteCommand(sql, _con);
            command.Parameters.AddWithValue("@name", text);
            command.ExecuteNonQuery();
            DataGridCategories.CommitEdit();
            DataGridCategories.Items.Refresh();
            DataGridCategories.UnselectAll();
            RefreshAll();
        }

        private void TextBox_LostFocus_4(object sender, RoutedEventArgs e)
        {
            _accountList = (List<Account>)DataGridAccounts.ItemsSource;
            var row = (DataGridRow)DataGridAccounts.ItemContainerGenerator.ContainerFromItem(DataGridAccounts.SelectedItem);
            if (row == null) return;
            var textBox = (TextBox)e.Source;
            var text = textBox.Text;
            if (text == _accountList[row.GetIndex()].Info) return;
            _accountList[row.GetIndex()].Info = text;
            string sql =
                $"UPDATE Account SET AccountInfo = @info WHERE Id = {_accountList[row.GetIndex()].Id};";
            var command = new SQLiteCommand(sql, _con);
            command.Parameters.AddWithValue("@info", text);
            command.ExecuteNonQuery();
            DataGridCategories.CommitEdit();
            DataGridCategories.Items.Refresh();
            DataGridCategories.UnselectAll();
            RefreshAll();
        }

        private static readonly string[] Scopes = { DriveService.Scope.DriveAppdata };
        private const string ApplicationName = "QuickBudget";
        private string _path;

        private void UploadAll()
        {
            try
            {
                _con.Close();


                _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                _path = Path.Combine(_path, "Budgets");
                UserCredential credential;

                using (var stream =
                    new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    var credPath = System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Define parameters of request.
                var listRequest = service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name)";
                var request = service.Files.List();
                request.Spaces = "appDataFolder";
                request.Fields = "nextPageToken, files(id, name)";
                request.PageSize = 10;
                var result = request.Execute();
                var driveFiles = result.Files.ToList();
                Directory.CreateDirectory("Budgets");
                var localFiles = Directory.EnumerateFiles(_path, "*.sqlite").ToList();
                foreach (var file in localFiles)
                {
                    var fileName = Path.GetFileName(file);
                    if (driveFiles.Any(x => x.Name.Equals(fileName) && File.GetLastWriteTime(file) > x.ModifiedTime))
                    {
                        //UPDATE EXISTING ON DRIVE


                        var body = new Google.Apis.Drive.v3.Data.File
                        {
                            Name = Path.GetFileName(file),
                            Description = "QuickBudget Database",
                            MimeType = ".sqlite database",
                            ModifiedTime = File.GetLastWriteTime(file)
                        };
                        var byteArray = File.ReadAllBytes(file);
                        var stream = new MemoryStream(byteArray);
                        var id = result.Files.First(x => x.Name.Equals(body.Name)).Id;
                        var req = service.Files.Update(body, id, stream, GetMimeType(Path.GetFileName(file)));
                        req.Fields = "id";
                        req.Upload();
                        var progress = req.GetProgress();

                    }
                    else if (!driveFiles.Any(x => x.Name.Equals(fileName)))
                    {
                        //UPLOAD NON-EXISTING TO DRIVE
                        var yesNo = new YesNo($"Budget {BudgetName} is currently not on Google Drive, do you wish to upload it?") { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                        yesNo.ShowDialog();
                        if (!yesNo.IsSuccesful) continue;
                        var body = new Google.Apis.Drive.v3.Data.File
                        {
                            Name = Path.GetFileName(file),
                            Parents = new List<string>() {"appDataFolder"},
                            Description = "QuickBudget Database",
                            MimeType = ".sqlite database",
                            ModifiedTime = File.GetLastWriteTime(file)
                        };
                        var byteArray = File.ReadAllBytes(file);
                        var stream = new MemoryStream(byteArray);

                        var req = service.Files.Create(body, stream,
                            GetMimeType(Path.GetFileName(file)));
                        req.Fields = "id";
                        req.Upload();
                        var progress = req.GetProgress();
                    }
                }
            }
            catch (Exception exception) when (exception is Google.GoogleApiException || exception is HttpRequestException)
            {
                var retry = false;
                Dispatcher.Invoke((Action)(() =>
                {
                    var yesNo = new YesNo("Quickbudget failed to connect to Google Drive. Do you want to try again?") {  WindowStartupLocation = WindowStartupLocation.CenterScreen};
                    yesNo.ShowDialog();
                    if (yesNo.IsSuccesful)
                    {
                        retry = true;
                    }
                }));
                if (retry)
                {
                    UploadAll();
                }
                else
                {
                    //TODO load local
                }


            }
        }
        private static string GetMimeType(string fileName)
        {
            var mimeType = "application/unknown";
            var extension = Path.GetExtension(fileName);
            if (extension == null) return mimeType;
            var ext = extension.ToLower();
            var regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey?.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {

        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            UploadAll();
        }

        private bool _userCreated = false;
        private void CurrencyComboBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        { 
            _userCreated = true;
        }

        private void textBlockWarnings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonCategories.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonTransactions.BorderThickness = new Thickness(6, 0, 0, 0);
            ButtonBudget.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonReview.BorderThickness = new Thickness(0, 0, 0, 0);
            ButtonAccounts.BorderThickness = new Thickness(0, 0, 0, 0);
            GridBudget.Visibility = Visibility.Hidden;
            GridCategories.Visibility = Visibility.Hidden;
            GridReview.Visibility = Visibility.Hidden;
            GridTransactions.Visibility = Visibility.Visible;
            GridAccounts.Visibility = Visibility.Hidden;
        }

        private void TextBlockChange_MouseDown(object sender, MouseButtonEventArgs e)
        {

            var chooseBudget = new ChooseBudget();
            chooseBudget.Show();
            Close();
        }

        private void buttonUsePrevMonthBudget_Click(object sender, RoutedEventArgs e)
        {
            var localDate = SelectedDate;
            if (localDate.Month.Equals(1))
            {
                localDate = localDate.AddYears(-1);
                localDate = new DateTime(localDate.Year, 12, 1);
            }
            else
            {
                localDate = localDate.AddMonths(-1);
                localDate = new DateTime(localDate.Year, localDate.Month, 1);
            }
            //TODO TEST THIS OUT!!!
            var sql = $"SELECT * FROM MonthBudget WHERE Date = '{localDate.ToShortDateString() + " 00:00:00"}'";
            var command = new SQLiteCommand(sql, _con);
            var reader = command.ExecuteReader();
            long previousMonthBudgetId = 0;
            long monthBudgetId = 0;
            while (reader.Read())
            {
                previousMonthBudgetId = reader.GetInt64(0);
            }
            var localSelectedDate = new DateTime(SelectedDate.Year,SelectedDate.Month,1);
            sql = $"SELECT * FROM MonthBudget WHERE Date = '{localSelectedDate.ToShortDateString() + " 00:00:00"}'";
            command = new SQLiteCommand(sql, _con);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                monthBudgetId = reader.GetInt64(0);
            }
            sql = $"SELECT * FROM StatusCategory WHERE MonthBudgetID={previousMonthBudgetId}";
            command = new SQLiteCommand(sql, _con);
            reader = command.ExecuteReader();
            using (var com = new SQLiteCommand(_con))
            {
                using (var transaction = _con.BeginTransaction())
                {
                    while (reader.Read())
                    {
                        decimal budgeted = reader.GetDecimal(1);
                        long categoryId = reader.GetInt64(3);
                        StatusCategoryList.First(x => x.CategoryId == categoryId && x.MonthId == monthBudgetId).Budgeted = budgeted;
                        com.CommandText = $"UPDATE StatusCategory SET Budgeted= {budgeted} WHERE CategoryId = {categoryId} AND MonthBudgetId = {monthBudgetId};";
                        com.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
            var collectionBudget = new ListCollectionView(StatusCategoryList);
            collectionBudget.GroupDescriptions.Add(new PropertyGroupDescription("MasterCat"));
            DataGridBudget.ItemsSource = collectionBudget;
            TotalWorkable();

        }

        private void buttonApplyBudgetNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlockSettings_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var settings = new Settings() { Owner = GetWindow(this) };
            settings.ShowDialog();
        }

        private void ButtonTransferAccount_OnClick(object sender, RoutedEventArgs e)
        {
            var transfer = new Transfer(_accountList) { Owner = GetWindow(this) };
            transfer.ShowDialog();
            if (!transfer.IsSuccesful) return;
            var accountFrom = transfer.AccountFrom;
            var accountTo= transfer.AccountTo;
            decimal amountFrom = transfer.AmountFrom;
            decimal amountTo= transfer.AmountTo;
            string sql =$"INSERT INTO Transactions VALUES (null, 0,0,'Transfer',@payee,{amountFrom},'{DateTime.Now}',{accountFrom.CurrencyId},{accountFrom.Id},{amountFrom},0);";
            var icommand = new SQLiteCommand(sql, _con);
            icommand.Parameters.AddWithValue("@note", "Transfer");
            icommand.Parameters.AddWithValue("@payee", accountTo.Name);
            icommand.ExecuteNonQuery();
            sql = $"INSERT INTO Transactions VALUES(null, {0}, 1,@note,@payee,{amountTo},'{DateTime.Now}',{accountTo.CurrencyId},{accountTo.Id},{amountTo},0)";
            icommand = new SQLiteCommand(sql, _con);
            icommand.Parameters.AddWithValue("@note", "Transfer");
            icommand.Parameters.AddWithValue("@payee", accountFrom.Name);
            icommand.ExecuteNonQuery();
            RefreshAll();
        }

        private void MasterCategoryComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)e.Source;
            CategoryList = (List<Category>)DataGridCategories.ItemsSource;
            var row = (DataGridRow)DataGridCategories.ItemContainerGenerator.ContainerFromItem(DataGridCategories.SelectedItem);
            if (row == null || !_userCreatedMasterCategory) return;
            var masterCategory = (MasterCategory)comboBox.SelectedItem;
            var masterCategoryId = masterCategory.Id;
            CategoryList[row.GetIndex()].MasterCategoryId = masterCategoryId;
            var categoryId  = CategoryList[row.GetIndex()].Id;
            string sql =
                $"UPDATE Category SET MasterCat= @masterCategoryId WHERE Id = {categoryId};";
            var command = new SQLiteCommand(sql, _con);
            command.Parameters.AddWithValue("@masterCategoryId", masterCategoryId);
            command.ExecuteNonQuery();
            //sql = $"UPDATE Transactions SET CurrencyID = @currencyID, Attention = 1 WHERE AccountID = {accountId};";
            //command = new SQLiteCommand(sql, _con);
            //command.Parameters.AddWithValue("@currencyID", currencyId);
            //command.ExecuteNonQuery();
            DataGridCategories.CommitEdit();
            DataGridCategories.Items.Refresh();
            DataGridCategories.UnselectAll();
            RefreshAll();
        }
        //TODO test _usercreated, using this just to be safe, might be able tu use usercreated
        private bool _userCreatedMasterCategory = false;
        private void MasterCategoryComboBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _userCreatedMasterCategory = true;
        }

        private void ButtonManageMasterCategory_OnClick(object sender, RoutedEventArgs e)
        {
            var  manageMasterCategory = new ManageMasterCategory(MasterCategoryList) { Owner = GetWindow(this) };
            manageMasterCategory.ShowDialog();
        }

        //TODO hotkeys, enter to confirm, new, delete ...
    }
}
