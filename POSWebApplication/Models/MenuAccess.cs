using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class MenuAccess
    {
        [DisplayName("Access ID")][Key] public int AccessId { get; set; }
        [DisplayName("Group ID")] public short MnuGrpId { get; set; }
        [DisplayName("Account Level")] public byte AccLevel { get; set; }
        [DisplayName("Button Name")] public String BtnNme { get; set; } = string.Empty;
        [DisplayName("Datetime")] public DateTime RevDtetime { get; set; }
        [DisplayName("By User ID")] public short ByUserID { get; set; }
    }
}
