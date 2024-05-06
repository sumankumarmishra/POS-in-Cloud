using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace POSWebApplication.Models
{
    public class StockPkgH
    {
        [Key][DisplayName("ID")] public int PkgHId { get; set; }
        [NotNull][DisplayName("Package Name")][StringLength(50)] public string PkgNme { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")][NotNull][DisplayName("Price")] public decimal SellingPrice { get; set; }
        [DisplayName("Image")] public byte[]? Image { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")][DisplayName("UnitCost")] public decimal UnitCost { get; set; }
        [DisplayName("Barcode")][StringLength(100)] public string Barcode { get; set; } = null!;
        [DisplayName("RevDatetime")] public DateTime RevDtetime { get; set; }
        [DisplayName("UserID")] public short UserId { get; set; }
        [NotMapped][DisplayName("User Code")] public string? UserCde { get; set; } = string.Empty;
        [NotMapped] public IFormFile? ImageFile { get; set; } // for image
        [NotMapped] public string Base64Image { get; set; } = string.Empty; // for image

    }
}
