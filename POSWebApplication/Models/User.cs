using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace POSWebApplication.Models
{
    public class User
    {
        public short UserId { get; set; }
        [DisplayName("User code")][StringLength(20)] public string UserCde { get; set; } = string.Empty;
        [DisplayName("User name")][StringLength(50)] public string UserNme { get; set; } = string.Empty;
        [DisplayName("Password")][StringLength(20)] public string Pwd { get; set; } = string.Empty;
        [DisplayName("Menu group Id")] public short MnuGrpId { get; set; }
        [DisplayName("Created Datetime")][AllowNull] public DateTime CreateDtetime { get; set; } = DateTime.Now;
        [DisplayName("Company Id")] public short CmpyId { get; set; }
        [NotMapped][DisplayName("Confirm Password")] public string ConfirmPwd { get; set; } = string.Empty;
        [NotMapped][DisplayName("POS Id")] public string POSid { get; set; } = string.Empty;
    }
}
