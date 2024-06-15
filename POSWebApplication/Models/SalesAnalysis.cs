using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSWebApplication.Models
{
    public class SalesAnalysis
    {

    }

    public class SalesAnalysisSearch // for search form
    {
        public string BranchName { get; set; } = string.Empty;

        public DateTime FromDate { get; set; } = DateTime.Now.Date;//default

        public int NextDay { get; set; } = 7; //default

        public int TopBottom { get; set; } = 3; // default
    }


    [Keyless]
    public class SalesAnalysisModel
    {
        public string? ItemDesc { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? Qty { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? SaleAmount { get; set; }
        [NotMapped]
        public int? No { get; set; }
    }


}
