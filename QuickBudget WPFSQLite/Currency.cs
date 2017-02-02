
namespace QuickBudget_WPFSQLite
{
    public class Currency
    {
        public Currency(long id, string symbol, string currencyString, bool primary)
        {
            Id = id;
            Symbol = symbol;
            CurrencyString = currencyString;
            Primary = primary;
        }

        public long Id { get; set; }
        public string Symbol { get; set; }
        public string CurrencyString { get; set; }
        public bool Primary { get; set; }
        public string Display => CurrencyString+ " (" + Symbol+")";
    }
}
