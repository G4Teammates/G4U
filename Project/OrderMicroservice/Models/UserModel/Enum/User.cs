namespace OrderMicroservice.Models.UserModel.Enum
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
    public enum BankName
    {
        Unknown,
        Vietcombank,
        VietinBank,
        Techcombank,
        BIDV,
        MBBank,
        VPBank,
        ACB,
        TPBank,
        VIB,
        Agribank
    }
}
