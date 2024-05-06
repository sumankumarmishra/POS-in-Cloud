namespace POSWebApplication.Models
{
    public class Home
    {


        public IEnumerable<Billh> BillHList { get; set; }

        public IEnumerable<Billd> BillDList { get; set; }

        public IEnumerable<Location> Locations { get; set; }

        public IEnumerable<Currency> SaleTypes { get; set; }

        public AutoNumber AutoNumber { get; set; }

        public Billh BillH { get; set; }

        public decimal SalesTotal { get; set; }
        public decimal ReturnTotal { get; set; }
        public decimal VoidTotal { get; set; }
    }
}
