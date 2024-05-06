using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class Location
    {
        [Key] [DisplayName("Location Code")][StringLength(24)] public string LocCde { get; set; }
        [DisplayName("Location Description")][StringLength(40)] public string LocDesc { get; set; }
        [DisplayName("Is Outlet")] public bool IsOutlet { get; set; }
    }
}
