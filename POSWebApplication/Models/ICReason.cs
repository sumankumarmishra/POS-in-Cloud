using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class ICReason
    {
        [Key]public short ICReasonId { get; set; }
        [StringLength(50)]public string ICReasonCde { get; set; }
    }
}
