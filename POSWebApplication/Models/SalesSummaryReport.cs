using Microsoft.CodeAnalysis.Editing;
using Microsoft.Identity.Client;
using System.ComponentModel;

namespace POSWebApplication.Models
{
    public class SalesSummaryReport
    {
        [DisplayName("From Date.")] public DateTime FromDateTime { get; set; }

        [DisplayName("To Date.")] public DateTime ToDateTime { get; set; }

        [DisplayName("Location")] public string? Location { get; set; }

        [DisplayName("POS ID")] public string? POSId { get; set; }

        [DisplayName("Shift #")] public byte? ShiftNo { get; set; }

        [DisplayName("ItemID")] public string? ItemID { get; set; }

        [DisplayName("View Report")] public string ReportView { get; set; }
    }

    public class SalesSummaryDbReport
    {
        public string bizdte { get; set; }

        public string billno { get; set; }

        public string loccde { get; set; }

        public string posid { get; set; }

        public int shiftno { get; set; }

        public string guestnme { get; set; }

        public string remark { get; set; }

        public decimal cmpyid { get; set; }

        public decimal srvcchrgamt { get; set; }

        public decimal billdiscount { get; set; }

        public string tableno { get; set; }

    }
}
