using Microsoft.Identity.Client;
using System.ComponentModel;

namespace POSWebApplication.Models
{
    public class CategorySaleReport
    {
        [DisplayName("From Date.")] public DateTime FromDateTime { get; set; }

        [DisplayName("To Date.")] public DateTime ToDateTime { get; set; }

        [DisplayName("Location")] public string? Location { get; set; }

        [DisplayName("POS ID")] public string? POSId { get; set; }

        [DisplayName("Shift #")] public byte? ShiftNo { get; set; }

        [DisplayName("Category")] public List<string> CategoryCode { get; set; }
    }
}
