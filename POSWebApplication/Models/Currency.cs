using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace POSWebApplication.Models
{
    public class Currency
    {
        [Key][DisplayName("ID")] public int CurrId { get; set; }
        [StringLength(20)][Required][DisplayName("Currency Type")] public string CurrTyp { get; set; }
        [StringLength(8)][Required][DisplayName("Currency Code")] public string CurrCde { get; set; }
        [Required][DisplayName("Currency Rate")] public decimal CurrRate { get; set; }
    }
}
