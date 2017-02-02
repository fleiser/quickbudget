using System.Collections.Generic;
using System.Linq;

namespace QuickBudget_WPFSQLite
{
    public class StatusCategory
    {
        public long Id { get; set; }
        public long State { get; set; }
        public string Category { get; set; }

        public string MasterCat
        {
            get
            {
                var categories = MainWindow.GetMasterCategories();
                return categories.Count==0 ? null : categories.First(x => x.Id.Equals(MasterCategoryId)).Name;
            }
        }
        public long MasterCategoryId { get; set; }
        public long CategoryId { get; set; }
        public long MonthId { get; set; }
        public decimal Budgeted { get; set; } //TODO validate text breaks N2 format
        public decimal Balance => Budgeted - Transactioned;
        public decimal Transactioned { get; set; }
        public decimal CurrentState => Transactioned;
        public long CurrencyId { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyString { get; set; }

        public string StatusImage
        {
            get
            {
                decimal result = Budgeted - CurrentState;
                if (result > 0)
                {
                    return "/Assets/IncomeIconT.png";
                }
                if (result < 0)
                {
                    return "/Assets/ExpanseIconT.png";
                }
                return "/Assets/EqualsIcon.png";
            }
        }

        public string StatusColor {
            get
            {
                decimal result = Budgeted - CurrentState;
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


        public StatusCategory(long id, long categoryId , string category, decimal budgeted,  decimal transactioned, long currencyId, string currencySymbol, string currencyString, long monthId, long masterCategoryId)
        {
            Id = id;
            Category = category;
            Budgeted = budgeted;
            Transactioned = transactioned;
            CurrencyId = currencyId;
            CategoryId = categoryId;
            MonthId = monthId;
            CurrencyString = currencyString;
            CurrencySymbol = currencySymbol;
            MasterCategoryId = masterCategoryId;
        }
    }
}
