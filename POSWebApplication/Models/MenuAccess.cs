using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class MenuAccess
    {
        [DisplayName("Access ID")][Key] public int AccessId { get; set; }
        [DisplayName("Group ID")] public short MnuGrpId { get; set; }
        [DisplayName("Account Level")] public byte AccLevel { get; set; }
        [DisplayName("Button Name")] public string BtnNme { get; set; } = string.Empty;
        [DisplayName("Datetime")] public DateTime RevDtetime { get; set; }
        [DisplayName("By User ID")] public short ByUserID { get; set; }
        [NotMapped][DisplayName("Menu Group")] public string MnuGrpNme { get; set; } = string.Empty;
        [NotMapped][DisplayName("Account Level")] public string AccountLevel { get; set; } = string.Empty;
    }
}
