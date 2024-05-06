using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class StockGroup
    {
        [Key][DisplayName("Group ID")] public short StkGrpId { get; set; }
        [StringLength(8)][DisplayName("Group Code")] public string GrpCde { get; set; }
        [StringLength(40)][DisplayName("Group Desc")] public string GrpDesc { get; set; }
    }
}
