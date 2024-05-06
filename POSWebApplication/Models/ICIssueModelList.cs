namespace POSWebApplication.Models
{
    public class ICIssueModelList
    {
        public IEnumerable<InventoryMoveBillH> ICIssueList { get; set; }
        public IEnumerable<ICReason> ICReasonList { get; set; }
        public InventoryMoveBillH ICIssue { get; set; }
        public InventoryBillD IcIssueDetails { get; set; }
        public string RefNo { get; set; }
    }
}
