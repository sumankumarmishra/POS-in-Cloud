using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace POSWebApplication.Models
{
    public class UserPOS
    {
        [DisplayName("User POS Id")] public int UserPOSid { get; set; }
        [DisplayName("User Id")] public short UserId { get; set; }
        [DisplayName("POS Id")] public string POSid { get; set; } = string.Empty;
        [DisplayName("User Code")][NotMapped] public string UserCode { get; set; } = string.Empty;
      
    }
}
