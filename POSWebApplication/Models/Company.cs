using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class Company
    {
        [Key]
        public int CmpyId { get; set; }
        public string CmpyNme { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string POSPkgNme { get; set; } = string.Empty;
        public DateTime RevDteTime { get; set; }
    }
}
