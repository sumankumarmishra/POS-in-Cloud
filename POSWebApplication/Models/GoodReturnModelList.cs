namespace POSWebApplication.Models
{
    public class GoodReturnModelList
    {
        public IEnumerable<InventoryBillH> GoodReturnList { get; set; }
        public IEnumerable<Supplier> Suppliers { get; set; }
        public InventoryBillH GoodReturn { get; set; }
        public InventoryBillD GoodReturnDetails { get; set; }
        public string RefNo { get; set; }
    }
}
