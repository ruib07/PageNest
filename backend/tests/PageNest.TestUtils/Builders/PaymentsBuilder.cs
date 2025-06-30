using PageNest.Domain.Entities;
using PageNest.Domain.Enums;

namespace PageNest.TestUtils.Builders;

public class PaymentsBuilder
{
    private static int _counter = 2;

    public static List<Payment> CreatePayments(int quantity = 2)
    {
        var payments = new List<Payment>();

        for (int i = 0; i < quantity; i++)
        {
            payments.Add(new Payment()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Amount = 100.00m + _counter,
                StripePaymentIntentId = "po_3RglTvCByFDvESD21VOl87wJ",
                Status = PaymentStatus.Pending
            });

            _counter++;
        }

        return payments;
    }

    public static Payment InvalidPaymentCreation(decimal amount, string stripePaymentIntentId, PaymentStatus status)
    {
        return new Payment()
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Amount = amount,
            StripePaymentIntentId = stripePaymentIntentId,
            Status = status
        };
    }

    public static Payment UpdatePayment(Guid id, Guid orderId)
    {
        return new Payment()
        {
            Id = id,
            OrderId = orderId,
            Amount = 150.00m,
            StripePaymentIntentId = "pi_3RglTvCByFDvESD21VOl87wJ",
            Status = PaymentStatus.Completed
        };
    }
}
