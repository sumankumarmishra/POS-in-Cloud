using System.ComponentModel;

namespace POSWebApplication.Models
{
    public class SupplierSalesReport
    {
        [DisplayName("From Date.")] public DateTime FromDateTime { get; set; }

        [DisplayName("To Date.")] public DateTime ToDateTime { get; set; }

        [DisplayName("Supplier")] public string? Supplier { get; set; }


    }
}
