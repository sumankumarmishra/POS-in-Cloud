using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class ServiceItem
    {
        [Key][DisplayName("Service ID")] public int SrvcId { get; set; }
        [StringLength(24)][Required][DisplayName("Item ID")] public string SrvcItemId { get; set; }
        [StringLength(100)][Required][DisplayName("Description")] public string SrvcDesc { get; set; }
        [StringLength(8)][Required][DisplayName("Category")] public string CategoryId { get; set; }
        [StringLength(8)][Required][DisplayName("GroupCode")] public string GrpCde { get; set; }
        [StringLength(8)][Required][DisplayName("UOM")] public string BaseUnit { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")][Required][DisplayName("Amount")] public decimal SrvcAmt { get; set; }
        [StringLength(8)][Required][DisplayName("Acc Code")] public string AccCde { get; set; }
        [DisplayName("Created Datetime")] public DateTime? CreatedDtetime { get; set; }
        [DisplayName("UserId")] public short? UserId { get; set; }
    }
}
