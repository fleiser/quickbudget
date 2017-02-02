

namespace QuickBudget_WPFSQLite
{
    public class ExchangeR
    {
        public ExchangeR(long id, string currency, decimal rate)
        {
            Id = id;
            Currency = currency;
            Rate = rate;
        }

        public long Id { get; set; }
        public string Currency { get; set; }
        public decimal Rate { get; set; }
    }
}
