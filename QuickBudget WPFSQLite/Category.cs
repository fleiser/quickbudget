using System.Collections.Generic;
using System.Linq;
namespace QuickBudget_WPFSQLite
{
    public class Category
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long MasterCategoryId { get; set; }
        public string MasterCategoryName { get; set; }
        public string Info { get; set; }
        //public int Popularity { get; set; }
        public long CurrencyId { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencString { get; set; }
        public string CurrencyDisplay => CurrencySymbol + " " + CurrencySymbol;
        public decimal TotalValue { get; set; }
        public int CurrencyIndex
        {
            get
            {
                List<Currency> currencies = MainWindow.GetCurrencies();
                int selectedCurrency = currencies.FindIndex(x => x.Id.Equals(CurrencyId));
                    CurrencySymbol = currencies.Where(x => x.Id.Equals(CurrencyId)).ToList()[0].Symbol;
                    CurrencString = currencies.Where(x => x.Id.Equals(CurrencyId)).ToList()[0].CurrencyString;
                return selectedCurrency;
            }
        }
        public int MasterCategoryIndex
        {
            get
            {
                var masterCategories = MainWindow.GetMasterCategories();
                var selectedCurrency = masterCategories.FindIndex(x => x.Id.Equals(MasterCategoryId));
                MasterCategoryName = masterCategories.Where(x => x.Id.Equals(MasterCategoryId)).ToList()[0].Name;
                return selectedCurrency;
            }
        }

        public Category(long id, string name, string info ,long currencyId, long masterCategoryId)
        {
            Id = id;
            Name = name;
            Info = info;
            CurrencyId = currencyId;
            MasterCategoryId = masterCategoryId;
        }
    }

}
