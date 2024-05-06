using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class InventoryBillH
    {
        [Key] public int ArapId { get; set; }
        public short IcTranTypId { get; set; }
        [StringLength(5)] public string IcTranTypCde { get; set; } = string.Empty;
        [StringLength(50)] public string IcRefNo { get; set; } = string.Empty;
        [StringLength(50)] public string RefNo2 { get; set; } = string.Empty;
        public int ArId { get; set; } = 0; // Check again
        public int ApId { get; set; }
        public DateTime TranDte { get; set; }
        [StringLength(100)] public string ArapDesc { get; set; } = string.Empty;
        public decimal BillAmt { get; set; }
        public decimal BillDiscAmt { get; set; }
        public decimal BillTaxAmt { get; set; }
        public decimal OtherChrgAmt { get; set; }
        public decimal DepositAmt { get; set; }
        public DateTime DepositDte { get; set; }
        [StringLength(5)] public string TradeCurrCde { get; set; } = string.Empty;
        [StringLength(5)] public string TenderCde { get; set; } = string.Empty;
        public decimal CurrRate { get; set; } = 1; // Check again
        [StringLength(50)] public string BillTerm { get; set; } = string.Empty;
        public short BillTermDay { get; set; }
        public short DeliDay { get; set; } = 0; // Check again
        [StringLength(100)] public string ToShipNme { get; set; } = string.Empty;
        public string ToShipAddr { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public bool CancelFlg { get; set; } = false;
        public short CreateUserId { get; set; }
        public DateTime CreateDteTime { get; set; }
        public short RevUserId { get; set; }
        public DateTime RevDteTime { get; set; }
        [NotMapped] public string StringTranDate { get; set; } = string.Empty;
        [NotMapped] public int No { get; set; }
        [NotMapped] public string Supplier { get; set; } = string.Empty;
        [NotMapped] public string FromLoc { get; set; } = string.Empty;

    }
}
