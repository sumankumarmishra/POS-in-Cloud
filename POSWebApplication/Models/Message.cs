namespace POSWebApplication.Models
{
    public class Message
    {
        //Common
        public const string SESSION_EXPIRED_MESSAGE = "Session expired!";
        public const string REQUIRED_FIELDS_MESSAGE = "Required fields must be filled";
        public const string CREATED_MESSAGE = " is successfully created.";
        public const string UPDATED_MESSAGE = " is successfully updated.";
        public const string DELETED_MESSAGE = " is successfully deleted.";

        //StockController
        public const string ALREADY_EXISTS_MESSAGE = " already exists. Please enter a different one.";
        public const string IMAGE_SIZE_MESSAGE = "Image size needs to be less than 500KB.";
        public const string NOT_IMAGE_MESSAGE = "Uploaded file is not an image.";

        //UserController
        public static string PASSWORD_NOT_SAME_MESSAGE = "Password and confirm password are not the same";

        //LogInController
        public static string AUTHENTICATION_FAILED_MESSAGE = "Authentication failed. Please check your credentials.";




    }
}
