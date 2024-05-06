using System.Collections;
using System.Security.Permissions;

namespace POSWebApplication.Models
{
    public class StockModelList
    {
        public Stock Stock { get; set; }
        public StockUOM StockUOM { get; set; }
        public  IEnumerable<Stock> StockList { get; set;}
        public  IEnumerable<StockUOM> StockUOMs { get; set; }
        public IEnumerable<StockCategory> StockCategories { get; set; }
        public IEnumerable<StockGroup> StockGroups { get; set; }
    }
}
