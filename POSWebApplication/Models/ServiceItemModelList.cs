namespace POSWebApplication.Models
{
    public class ServiceItemModelList
    {
        public IEnumerable<ServiceItem> ServiceItems { get; set; }
        public IEnumerable<StockUOM> StockUOMs { get; set; }
        public IEnumerable<StockCategory> StockCategories { get; set; }
        public IEnumerable<StockGroup> StockGroups { get; set; }
        public ServiceItem ServiceItem { get; set; }
    }
}
