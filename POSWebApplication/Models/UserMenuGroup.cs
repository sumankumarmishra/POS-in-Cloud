using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class UserMenuGroup
    {
        [DisplayName("Group ID")][Key] public short MnuGrpId { get; set; }
        [DisplayName("Menu Group")][StringLength(50)]public String MnuGrpNme { get; set; } = string.Empty;
    }
}
