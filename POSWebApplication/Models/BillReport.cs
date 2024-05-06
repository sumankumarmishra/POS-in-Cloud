using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class BillReport
    {
        [DisplayName("Item")][StringLength(24)] public string itemid { get; set; }
        [DisplayName("Qty")] public decimal qty { get; set; }
        [DisplayName("Price")] public decimal price { get; set; }
        [DisplayName("Discount")] public decimal discamt { get; set; }
        public string posid { get; set; }
        public string billno { get; set; }
        public string userid { get; set; }
        public string bizdte { get; set; }
        public decimal billdiscount { get; set; }
        public short shiftno { get; set; }
        public string billtypcde { get; set; }
        public string remark { get; set; }
    }
}
