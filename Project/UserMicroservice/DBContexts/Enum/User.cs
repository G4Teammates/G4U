namespace UserMicroservice.DBContexts.Enum
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
    public enum UserLoginType
    {
        Local,
        Google,
        Other
    }
}
