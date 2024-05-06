namespace POSWebApplication.Models
{
    public class InventoryJSView
    {
        public List<List<string>> TableData { get; set; }
        public Dictionary<string, string> FormData { get; set; }
        public int ArapId { get; set; }
        public int IcMoveId { get; set; }
        public string ASMRefNo { get; set; }

    }
}
