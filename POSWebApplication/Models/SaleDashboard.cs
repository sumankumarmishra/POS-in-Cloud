using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class SaleDashboard
    {

    }
    [Keyless]
    public class SaleDashboardDataModel // for sale dashboard form
    {
        public IEnumerable<SaleDashboardListModel> SaleDashboardList { get; set; } = new List<SaleDashboardListModel>();

        public IEnumerable<SaleDashboardCurrListModel> SaleCurrList { get; set; } = new List<SaleDashboardCurrListModel>();

        public SaleDashboardSearchModel SaleDashboardSearch { get; set; } = new SaleDashboardSearchModel();

        [DisplayFormat(DataFormatString = "{0:N0}")] public decimal? TotalAmt { get; set; }
    }

    [Keyless]
    public class SaleDashboardSearchModel // for search form
    {
        public short CmpyId { get; set; }
        public DateTime FromDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime ToDate { get; set; } = DateTime.Now;
    }

    [Keyless]
    public class SaleDashboardListModel // for sale dashboard list
    {
        public short CmpyId { get; set; }
        public string? CompanyName { get; set; }
        public decimal SaleAmount { get; set; }
        [NotMapped]
        public int No { get; set; }
    }

    [Keyless]
    public class SaleDashboardCurrListModel // for sale dashboard currency list
    {
        public string? CurrCode { get; set; }
        public decimal SaleAmount { get; set; }
        [NotMapped]
        public int No { get; set; }
    }

    [Keyless]
    public class DateWiseSaleListModel // for date wise sale list
    {
        public DateTime Date { get; set; }
        public decimal SaleAmount { get; set; }
        [NotMapped]
        public int No { get; set; }
    }

    [Keyless]
    public class ItemWiseSaleListModel // for item wise sale list
    {

        public string? Item { get; set; }
        public decimal Qty { get; set; }
        [NotMapped]
        public int? No { get; set; }
    }

}
