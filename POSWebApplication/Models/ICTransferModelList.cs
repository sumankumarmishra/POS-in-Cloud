namespace POSWebApplication.Models
{
    public class ICTransferModelList
    {
        public IEnumerable<InventoryMoveBillH> ICTransferList { get; set; }
        public IEnumerable<ICReason> ICReasonList { get; set; }
        public InventoryMoveBillH ICTransfer { get; set; }
        public InventoryBillD ICTransferDetails { get; set; }
        public string RefNo { get; set; }
    }
}
