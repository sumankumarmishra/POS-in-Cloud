using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace POSWebApplication.Models
{
    public class StockPkgD
    {
        [Key] public int PkgDId { get; set; }
        [DisplayName("Package ID")] public int PkgHId { get; set; }
        [DisplayName("ItemID")][StringLength(24)] public string ItemId { get; set; }
        [DisplayName("Item Desc")][StringLength(100)] public string? ItemDesc { get; set; } = string.Empty;
        [DisplayName("UOM")][StringLength(8)] public string? BaseUnit { get; set; } = null!;
        [DisplayName("Qty")] public decimal Qty { get; set; }
        [DisplayName("RevDatetime")] public DateTime RevDtetime { get; set; }
        [DisplayName("UserID")] public short UserId { get; set; }

    }
}
