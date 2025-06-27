using Stripe;

namespace PageNest.Application.Interfaces.Services;

public interface IStripeService
{
    Task<PaymentIntent> CreatePaymentIntent(decimal amount);
}
