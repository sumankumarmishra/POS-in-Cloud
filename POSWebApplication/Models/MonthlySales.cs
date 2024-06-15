using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class MonthlySales
    {

    }

    public class MonthlySalesDataModel // for monthly sales form
    {
        public IEnumerable<MonthlySalesModel> MonthlySales { get; set; } = new List<MonthlySalesModel>();

        public string? Branch { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")] public decimal? TotalAmount { get; set; }

    }

    [Keyless]
    public class MonthlySalesSearch // for search form
    {
        public short CmpyId { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Year { get; set; } = DateTime.Now.Year; //default
    }

    [Keyless]
    public class MonthlySalesModel // for monthly sales bar chart and donut chart
    {
        public short CmpyId { get; set; }
        public string? Branch { get; set; }
        public int? MonthNo { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? Amount { get; set; }
        [NotMapped]
        public int? No { get; set; }
        [NotMapped]
        public string? Month { get; set; }
        [NotMapped]
        public int? ProgressBar { get; set; }

    }

    [Keyless]
    public class CategorySalesModel // for category sales bar chart
    {
        public string? Category { get; set; }
        public decimal? SaleAmount { get; set; }

    }


}



