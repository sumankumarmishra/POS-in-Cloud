using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class Customer
    {
        [Key] public int ArId { get; set; }
        [DisplayName("Acc-Code")][StringLength(9)] public string ArAcCde { get; set; } = string.Empty;
        [StringLength(100)] public string ArNme { get; set; } = string.Empty;
        [DisplayName("Address")]public string? Addr { get; set; } = string.Empty;
        [StringLength(100)] public string? Phone { get; set; } = string.Empty;
        [StringLength(100)] public string? Email { get; set; } = string.Empty;
        [DisplayName("Payment Term")][StringLength(50)] public string? PTerm { get; set; } = string.Empty;
        [DisplayName("Term Day")] public short? PTermDay { get; set; }
        [DisplayName("Credit Allow")] public bool CreditFlg { get; set; }
        [DisplayName("Credit Limit")] public decimal? CreditLimit { get; set; }
        [DisplayName("Discount (%)")] public decimal? DiscPerc { get; set; }
        [DisplayName("Trade Currency")][StringLength(5)] public string TradeTenderCde { get; set; } = string.Empty;
        [DisplayName("Bad Status")] public bool BadStatus { get; set; }
        [DisplayName("Active")] public bool ActiveFlg { get; set; }
        public short UserId { get; set; }
        public DateTime RevDteTime { get; set; }


    }
}
