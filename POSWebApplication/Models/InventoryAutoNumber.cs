using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class InventoryAutoNumber
    {
        [Key] public short IcTranTypId { get; set; }
        [StringLength(20)] public string IcTranTypCde { get; set; } = string.Empty;
        [StringLength(12)] public string IcTranPrefix { get; set; } = string.Empty;
        public bool ZeroLeading { get; set; }
        public short RunningNo { get; set; }
        public long LastUsedNo { get; set; }
        [StringLength(1)] public string ResetEvery { get; set; } = string.Empty;
        [StringLength(24)] public string DefLoc { get; set; } = string.Empty;
        public DateTime CreatedDteTime { get; set; }
        public short UserId { get; set; }
    }
}
