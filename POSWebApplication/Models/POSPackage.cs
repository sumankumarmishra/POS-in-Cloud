using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class POSPackage
    {
        [Key]
        public string Package { get; set; } = string.Empty;
        public int ItemLimit { get; set; }
        public decimal SaleLimit { get; set; }
        public short CashierAcc { get; set; }
        public short ManagementAcc { get; set; }
    }
}
