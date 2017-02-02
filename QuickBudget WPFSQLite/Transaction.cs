using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickBudget_WPFSQLite
{
    class Transaction
    {
        public Transaction(long id, long categoryId,string category, bool isIncome, string info,string payee, decimal transactioned,long currencyId,long accountId,string accountName, decimal accounted, DateTime date, bool attention)
        {
            Id = id;
            CategoryId = categoryId;
            Category = category;
            IsIncome = isIncome;
            Info = info;
            Transactioned = transactioned;
            Date = date;
            CurrencyId = currencyId;
            AccountId = accountId;
            Accounted = accounted;
            AccountName = accountName;

            Payee = payee;
            Attention = attention;
        }

        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string Category { get; set; }
        public bool IsIncome { get; set; }
        public string Info { get; set; }
        public decimal Transactioned { get; set; }
        public long CurrencyId { get; set; }
        public DateTime Date { get; set; }
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public string Payee { get; set; }
        public decimal Accounted { get; set; }
        public bool Attention { get; set; }

        public string CurrencySymbol
        {
            get
            {
                List<Currency> currencies = MainWindow.GetCurrencies();
                return currencies.Where(x => x.Id.Equals(CurrencyId)).ToList()[0].Symbol;
            }
        }

        public string AttentionImage => Attention ? "/Assets/Attention.png" : "";

        public string StatusImage
        {
            get
            {
                if (IsIncome)
                {
                    return "/Assets/IncomeIconT.png";
                }
                if (IsIncome==false)
                {
                    return "/Assets/ExpanseIconT.png";
                }
                return "/Assets/ExpanseIconT.png";
            }
        }

    }
}
