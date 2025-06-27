using PageNest.Domain.Enums;

namespace PageNest.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    public decimal Amount { get; set; }
    public string StripePaymentIntentId { get; set; }
    public PaymentStatus Status { get; set; }
}
