namespace POSWebApplication.Models
{
    public class ICAssemblyModelList
    {
        public IEnumerable<InventoryMoveBillH> ICAssemblyList { get; set; }
        public IEnumerable<ICReason> ICReasonList { get; set; }
        public InventoryMoveBillH ICAssembly { get; set; }
        public InventoryBillD ICAssemblyDetails { get; set; }
        public string RefNo { get; set; }
    }
}
