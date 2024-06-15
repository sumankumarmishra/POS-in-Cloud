using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class YearlySales
    {
    }

    public class YearlySalesDataModel // for yearly sales form
    {
        public IEnumerable<YearlySalesModel> YearlySales { get; set; } = new List<YearlySalesModel>();

        public string? Branch { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")] public decimal? TotalAmount { get; set; }

    }

    [Keyless]
    public class YearlySalesSearch // for search form
    {
        public short CmpyId { get; set; }
        public int Year { get; set; } = DateTime.Now.Year; //default
        public int NextYear { get; set; } = 1; //default
    }

    [Keyless]
    public class YearlySalesModel // for yearly sales bar chart and donut chart
    {
        public short CmpyId { get; set; }
        public string? Branch { get; set; }
        public int? YearNo { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? Amount { get; set; }
        [NotMapped]
        public int? No { get; set; }

        [NotMapped]
        public int? ProgressBar { get; set; }
    }

    public class YearlyCatgWiseSalesModel
    {
        public string? CatgCde { get; set; }

        public decimal? SaleAmount { get; set; }

    }


}


