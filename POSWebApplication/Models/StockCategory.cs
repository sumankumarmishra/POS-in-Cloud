using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace POSWebApplication.Models
{
    public class StockCategory
    {
        [Key][DisplayName("Id")] public int CatgId { get;set; }
        [StringLength(8)][DisplayName("Category ID")][NotNull] public string CategoryId { get;set; }
        [StringLength(40)][DisplayName("Category Description")] public string? CategoryDesc { get; set; }
        public bool DefPrintFlg { get;set; }
    }
}
