namespace OrderMicroservice.DBContexts.Enum
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipping,
        Delivered,
        Cancelled
    }
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed
    }
    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        NetBanking,
        UPI,
        Wallet
    }
}
