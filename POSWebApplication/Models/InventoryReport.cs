using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class InventoryReport
    {
        public DateTime tranDte { get; set; }
        public string fromloc { get; set; }
        public string toloc { get; set; }
        public string itemid { get; set; }
        public string uom { get; set; }
        public decimal uomrate { get; set; }
        public decimal qty { get; set; }
        public decimal unitcost { get; set; }
        public decimal discamt { get; set; }
        public string trandte { get; set; }
        public string icrefno { get; set; }
        public short billtermday { get; set; }
        public string apid { get; set; }
        public string tradecurrcde { get; set; }
        public string tendercde { get; set; }
        public decimal billdiscamt { get; set; }
        public string reasonid { get; set; }
        public int revuserid { get; set; }
        public string remark { get; set; }
    }
}
