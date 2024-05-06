using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class StockBOM
    {
        [Key][DisplayName("ID")] public int StkBOMId { get; set; }
        [StringLength(24)][DisplayName("Assembly ID")] public string BOMId { get; set; }
        [StringLength(24)][DisplayName("Item ID")] public string ItemId { get; set; }
        [DisplayName("Order")] public short OrdId { get; set; }
        [DisplayName("Quantity")] public decimal Qty { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")][StringLength(8)][DisplayName("Base Unit")] public string BaseUnit { get; set; }
        [NotMapped][DisplayName("UOMRate")] public decimal UOMRate { get; set; }
        [NotMapped][DisplayName("Datetime")] public DateTime RevDteTime { get; set; }
        [NotMapped][DisplayName("UserId")] public short UserId { get; set; }
        [NotMapped][DisplayName("Stock Flag")] public char StockFlg { get; set; }
    }
}
