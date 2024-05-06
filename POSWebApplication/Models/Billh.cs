using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class Billh
    {
        [DisplayName("ID")][Key] public int BillhId { get; set; }
        [DisplayName("Date")] public DateTime BizDte { get; set; }
        [DisplayName("Bill No.")][StringLength(20)] public string BillNo { get; set; }
        [DisplayName("Bill Type Code")][StringLength(1)] public string BillTypCde { get; set; } = "I";
        [DisplayName("Location")][StringLength(24)] public string LocCde { get; set; } = string.Empty;
        [DisplayName("Shift")] public Byte ShiftNo { get; set; }
        [DisplayName("Table No.")][StringLength(20)] public string? TableNo { get; set; } = string.Empty;
        [DisplayName("Pax No.")] public Byte PaxNo { get; set; }
        [DisplayName("Charge Acc Code")][StringLength(8)] public string? ChrgAccCde { get; set; } = string.Empty;
        [DisplayName("Guest ID")] public int GuestId { get; set; }
        [DisplayName("Guest Name")][StringLength(50)] public string? GuestNme { get; set; } = string.Empty;
        [DisplayName("Room No.")][StringLength(20)] public string? RoomNo { get; set; } = string.Empty;
        [DisplayName("Filio No.")][StringLength(20)] public string? FilioNo { get; set; } = string.Empty;
        [DisplayName("Service Charge Amount")] public decimal SrvcChrgAmt { get; set; }
        [DisplayName("Table Charge Amount")] public decimal TableChrgAmt { get; set; }
        [DisplayName("Tax Amount")] public decimal TaxAmt { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")][DisplayName("Bill Discount")] public decimal BillDiscount { get; set; }
        [DisplayName("Remark")][StringLength(255)] public string? Remark { get; set; } = string.Empty;
        [DisplayName("Status")] public char Status { get; set; }
        [DisplayName("Bill Opened Datetime")] public DateTime BillOpenDteTime { get; set; }
        [DisplayName("Collect Flg")] public bool CollectFlg { get; set; }
        [DisplayName("Collect Datetime")] public DateTime? CollectDteTime { get; set; }
        [DisplayName("POS ID")][StringLength(24)] public string? POSId { get; set; }
        [DisplayName("Cmpy ID")] public short CmpyId { get; set; }
        [DisplayName("User ID")] public short UserId { get; set; }
        [DisplayName("RevDteTime")] public DateTime RevDteTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")][NotMapped] public decimal TotalAmount { get; set; }
        [NotMapped] public string Action { get; set; } = string.Empty; // Bill Vouncher 
        [NotMapped] public string SaleType { get; set; } = string.Empty; // Bill Vouncher 
        [NotMapped] public string User { get; set; } = string.Empty; // Bill Vouncher 

        [NotMapped] public bool VoidFlg { get; set; }

    }
}
