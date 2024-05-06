namespace POSWebApplication.Models
{
    public class MenuAccessModelList
    {
        public MenuAccess MenuAccess { get; set; }
        public IEnumerable<UserMenuGroup> UserMenuGroupList { get; set; }
    }
}
