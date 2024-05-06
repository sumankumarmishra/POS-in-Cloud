using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class InventoryMoveBillH
    {
        [Key] public int IcMoveId { get; set; }
        public short IcTranTypId { get; set; }
        [StringLength(5)] public string IcTranTypCde { get; set; } = string.Empty;
        [StringLength(50)] public string IcRefNo { get; set; } = string.Empty;
        public DateTime TranDte { get; set; }
        [StringLength(50)] public string IcRefNo2 { get; set; } = string.Empty;
        public short ReasonId { get; set; }
        public string Remark { get; set; } = string.Empty;
        public bool CancelFlg { get; set; } = false;
        public bool AsmFlg { get; set; } = false;
        public short RevUserId { get; set; }
        public DateTime RevDteTime { get; set; }
        [NotMapped] public string StringTranDate { get; set; } = string.Empty;
        [NotMapped] public int No { get; set; }
        [NotMapped] public string FromLoc { get; set; } = string.Empty;
    }
}
