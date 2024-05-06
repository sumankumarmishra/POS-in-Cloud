using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class AutoNumber
    {
        [DisplayName("Auto Number ID")][Key] public short AutoNoId { get; set; }
        [DisplayName("Bill Type Code")][StringLength(1)] public string BillTypCde { get; set; }
        [DisplayName("Bill Prefix")][StringLength(5)] public string BillPrefix { get; set; } = string.Empty;
        [DisplayName("Zero Leading")] public bool ZeroLeading { get; set; }
        [DisplayName("Running Number")] public short RunningNo { get; set; }
        [DisplayName("Last Used Number")] public long LastUsedNo { get; set; }
        [DisplayName("Last Generated Date")] public DateTime LastGenerateDte { get; set; }
        [DisplayName("Bill Nature")][StringLength(5)] public string BillNature { get; set; } = string.Empty;
        [DisplayName("Reset Every")][StringLength(1)] public string ResetEvery { get; set; }
        [DisplayName("Company ID")] public short CmpyId { get; set; }
        [DisplayName("POS ID")] public string PosId { get; set; } = string.Empty;
        [DisplayName("Def-Location")][StringLength(24)] public string PosDefLoc { get; set; } = string.Empty;
        [DisplayName("Biz Date")] public DateTime BizDte { get; set; }
        [NotMapped] public string BizDteString { get; set; } = string.Empty;
        [DisplayName("No. of Shift")] public byte NoOfShift { get; set; }
        [DisplayName("Current Shift")] public byte CurShift { get; set; }
        [DisplayName("Def-Sale Type")][StringLength(5)] public string PosDefSaleTyp { get; set; } = string.Empty;
        [DisplayName("Allow Access Room")] public bool AllowAccessRoom { get; set; }
    }
}
