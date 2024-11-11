﻿namespace OrderMicroservice.DBContexts.Enum
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Paid,
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
        Pending,
        CreditCard,
        DebitCard,
        NetBanking,
        UPI,
        Wallet
    }
}
