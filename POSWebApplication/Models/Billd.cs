using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class Billd
    {
        [Key][DisplayName("Billd ID")] public long BilldId { get; set; }
        [DisplayName("Billh ID")] public int BillhId { get; set; }
        [DisplayName("Order No.")] public short OrdNo { get; set; }
        [DisplayName("Item")][StringLength(24)] public string ItemID { get; set; }
        [DisplayName("Base Unit")][StringLength(8)] public string UOMCde { get; set; }
        [DisplayName("UOM Rate")] public decimal UOMRate { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")][DisplayName("Qty")] public decimal Qty { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")][DisplayName("Price")] public decimal Price { get; set; }
        [DisplayName("Spec Instr")][StringLength(100)] public string SpecInstr { get; set; } = string.Empty;
        [DisplayFormat(DataFormatString = "{0:N0}")][DisplayName("Discount")] public decimal DiscAmt { get; set; }
        [DisplayName("Served By")] public short ServedById { get; set; }
        [DisplayName("Remark")][StringLength(255)] public string Remark { get; set; } = string.Empty;
        [DisplayName("Void Flg")] public bool VoidFlg { get; set; }
        [DisplayName("Void DteTime")] public DateTime? VoidDteTime { get; set; }
        [NotMapped] public string ItemDesc { get; set; } = string.Empty;
        [NotMapped] public int No { get; set; }

    }
}
