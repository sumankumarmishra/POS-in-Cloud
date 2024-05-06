namespace POSWebApplication.Models
{
    public class GoodReceiveModelList
    {
        public IEnumerable<InventoryBillH> GoodReceiveList { get; set; }
        public IEnumerable<Supplier> Suppliers { get; set; }
        public InventoryBillH GoodReceive { get; set; }
        public InventoryBillD GoodReceiveDetails { get; set; }
        public string RefNo { get; set; }

        

    }
}
