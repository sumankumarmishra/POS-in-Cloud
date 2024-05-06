using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace POSWebApplication.Models
{
    public class Stock
    {
        [DisplayName("ItemID")][StringLength(24)][Key] public string ItemId { get; set; }
        [DisplayName("Item Desc")][StringLength(100)] public string? ItemDesc { get; set; } = string.Empty;
        [DisplayName("ShortDescription")][StringLength(100)] public string? ShortDesc { get; set; } = string.Empty;
        [DisplayName("Barcode")][StringLength(100)] public string? Barcode { get; set; } = string.Empty;
        [DisplayName("Category")][StringLength(8)] public string CatgCde { get; set; } = string.Empty;
        [DisplayName("GroupCode")][StringLength(8)] public string? GrpCde { get; set; } = string.Empty;
        [DisplayName("ShelfLiveDay")] public short? ShelfLiveDay { get; set; }
        [DisplayName("UOM")][StringLength(8)] public string? BaseUnit { get; set; } = string.Empty;
        [DisplayFormat(DataFormatString = "{0:N0}")][DisplayName("UnitCost")] public decimal UnitCost { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")][DisplayName("Price")] public decimal SellingPrice { get; set; }
        [DisplayName("Image")] public byte[]? Image { get; set; }
        [DisplayName("FinishGood")] public bool FinishGoodFlg { get; set; }
        [DisplayName("CreateDatetime")] public DateTime? CreateDtetime { get; set; }
        [DisplayName("UserID")] public short? UserId { get; set; }
        [NotMapped][DisplayName("User Code")] public string? UserCde { get; set; } = string.Empty;
        [NotMapped][DisplayName("Qty")] public int? Quantity { get; set; }
        [NotMapped] public IFormFile? ImageFile { get; set; }
        [NotMapped] public string Base64Image { get; set; } = string.Empty;
        [NotMapped] public decimal? amount { get; set; }
        [NotMapped] public int PkgHId { get; set; }

        public const int MaxImageSize = 1024 * 500;

    }
}
