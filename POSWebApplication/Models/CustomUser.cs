using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace POSWebApplication.Models
{
    public class CustomUser
    {
        [Key] public short UserId { get; set; }
        [DisplayName("User code")][StringLength(20)] public string UserCde { get; set; } = string.Empty;
        [DisplayName("User name")][StringLength(50)] public string UserNme { get; set; } = string.Empty;
        [DisplayName("Password")][StringLength(20)] public string Pwd { get; set; } = string.Empty;
        [DisplayName("New Password")][StringLength(20)] public string NewPwd { get; set; } = string.Empty;
        [NotMapped][DisplayName("Confirm Password")] public string ConfirmPwd { get; set; } = string.Empty;
        [NotMapped][DisplayName("Menu Group")] public string? MnuGrp { get; set; } 
    }
}
