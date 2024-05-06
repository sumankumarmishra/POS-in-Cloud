namespace POSWebApplication.Models
{
    public class UserModelList
    {
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<UserMenuGroup> UserMenuGroupList { get; set; }
        public IEnumerable<String> POSList { get; set; }
        public User User { get; set; }
        
    }
}
