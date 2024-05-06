namespace POSWebApplication.Models
{
    public class ICAdjustModelList
    {
        public IEnumerable<InventoryMoveBillH> ICAdjustList { get; set; }
        public IEnumerable<ICReason> ICReasonList { get; set; }
        public InventoryMoveBillH ICAdjust { get; set; }
        public InventoryBillD ICAdjustDetails { get; set; }
        public string RefNo { get; set; }
    }
}
