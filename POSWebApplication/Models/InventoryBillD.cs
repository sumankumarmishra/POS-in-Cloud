using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class InventoryBillD
    {
        [Key] public long ArapDId { get; set; }
        public int ArapId { get; set; }
        public int? IcMoveId { get; set; }
        [StringLength(50)] public string IcRefNo { get; set; } = string.Empty;
        [DisplayName("Location")][StringLength(24)] public string FromLoc { get; set; } = string.Empty;
        [StringLength(24)] public string ToLoc { get; set; } = string.Empty;
        public short OrdNo { get; set; }
        [DisplayName("Item ID")][StringLength(24)] public string ItemId { get; set; } = string.Empty;
        [DisplayName("Item Description")][StringLength(100)] public string? ItemDesc { get; set; }
        [StringLength(8)] public string UOM { get; set; } = string.Empty;
        [DisplayName("Rate")] public decimal UOMRate { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitCost { get; set; }
        [DisplayName("Discount")] public decimal DiscAmt { get; set; }
        public decimal Price { get; set; }
        public bool VoidFlg { get; set; } = false;
        public short VoidUserId { get; set; }
        public DateTime VoidDteTime { get; set; } = DateTime.Parse("1990-01-01");
        public short RevUserId { get; set; }
        public DateTime RevDteTime { get; set; }

    }
}
