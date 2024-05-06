using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class SupplierItem
    {
        [Key] public int SuppItemId { get; set; }
        public int ApId { get; set; }
        public string ItemId { get; set; }
        public short UserId { get; set; }
        public DateTime RevDteTime { get; set; }
    }
}
