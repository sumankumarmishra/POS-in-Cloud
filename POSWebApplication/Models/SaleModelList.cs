using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class SaleModelList
    {
        public IEnumerable<Stock> Stocks { get; set; }
        public IEnumerable<StockCategory> StockCategories { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        public AutoNumber AutoNumber { get; set; }
        public Stock Stock { get; set; }
        public Billh BillH { get; set; }
        public Billp BillP { get; set; }
        [StringLength(20)] public string BillNo { get; set; } = string.Empty;
    }
}
