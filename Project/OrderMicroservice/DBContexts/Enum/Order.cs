namespace Order.DBContexts.Enum
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
}
