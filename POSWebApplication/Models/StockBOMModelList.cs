namespace POSWebApplication.Models
{
    public class StockBOMModelList
    {
        public IEnumerable<Stock> Stocks { get; set; }
        public IEnumerable<StockBOM> UnionStockBOMs { get; set; }
        public IEnumerable<StockBOM> ThisStockBOMs { get; set; }
        public IEnumerable<StockUOM> ThisStockUOMs { get; set; }
        public Stock Stock { get; set; }
        public StockBOM StockBOM { get; set; }
        public Boolean BaseUnitFlg { get; set; }
    }
}
