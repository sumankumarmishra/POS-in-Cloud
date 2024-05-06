namespace POSWebApplication.Models
{
    public class GoodReceiveDetailModelList
    {
        public IEnumerable<Location> Locations { get; set; }
        public IEnumerable<Stock> Stocks { get; set; }
        public IEnumerable<StockUOM> StockUOMs { get; set; }
    }
}
