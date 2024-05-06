using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class Billp
    {
        [DisplayName("Billp ID")][Key] public int BillpId { get; set; }
        [DisplayName("Billh ID")] public int BillhID { get; set; }
        [DisplayName("Currency Type")][StringLength(20)] public string CurrTyp { get; set; }
        [DisplayName("Currency Code")][StringLength(5)] public string CurrCde { get; set; }
        [DisplayName("Currency Rate")] public decimal CurrRate { get; set; }
        [DisplayName("Paid Amount")] public decimal PaidAmt { get; set; }
        [DisplayName("Local Amount")] public decimal LocalAmt { get; set; }
        [DisplayName("Change Amount")] public decimal ChangeAmt { get; set; }
        [DisplayName("Pay Datetime")] public DateTime PayDteTime { get; set; }

    }
}
