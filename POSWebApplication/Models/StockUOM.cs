using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class StockUOM
    {
        [DisplayName("UOM")][Key]public int UOMId { get; set; }
        [DisplayName("Item ID")][StringLength(24)]public string ItemId { get; set; }
        [DisplayName("Order No.")]public byte OrdNo { get; set; }
        [DisplayName("UOM Code")][StringLength(8)] public string UOMCde { get; set; } = string.Empty;
        [DisplayName("Rate")]public decimal UOMRate { get; set; }
        [DisplayName("UnitCost")]public decimal UnitCost { get; set; }
        [DisplayName("Price")]public decimal SellingPrice { get; set; }
    }
}
