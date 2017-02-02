using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickBudget_WPFSQLite
{
    public class Account
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public long CurrencyId { get; set; }
        public decimal Income { get; set; }
        public decimal Expanse { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal Balance => Income - Expanse;
        public string StatusColor
        {
            get
            {
                decimal result = Income - Expanse;
                if (result > 0)
                {
                    return "#4CAF50";
                }
                if (result < 0)
                {
                    return "#DD2C00";
                }
                return "#9E9E9E";
            }
        }
        public int CurrencyIndex
        {
            get
            {
                List<Currency> currencies = MainWindow.GetCurrencies();
                int selectedCurrency = currencies.FindIndex(x => x.Id.Equals(CurrencyId));
                CurrencySymbol = currencies.Where(x => x.Id.Equals(CurrencyId)).ToList()[0].Symbol;
                return selectedCurrency;
            }
        }
        public string Display => Name + " (" + CurrencySymbol+")";
        public Account(long id, string name, string info,long currencyId, decimal income,decimal expanse)
        {
            Id = id;
            Name = name;
            Info = info;
            CurrencyId = currencyId;
            Income = income;
            Expanse = expanse;
            var j = CurrencyIndex;
        }
    }
}
