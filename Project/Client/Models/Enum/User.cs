namespace Client.Models.Enum
{
    public class User
    {
        public enum UserStatus
        {
            Active,
            Inactive,
            Block,
            Deleted
        }
        public enum EmailStatus
        {
            Unconfirmed,
            Confirmed
        }
        public enum UserRole
        {
            User,
            Admin,
            Developer
        }
    }
}
