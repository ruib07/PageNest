using Microsoft.Extensions.Options;
using PageNest.Application.Interfaces.Services;
using PageNest.Infrastructure.Settings;
using Stripe;

namespace PageNest.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly StripeSettings _stripeSettings;

    public StripeService(IOptions<StripeSettings> stripeSettings)
    {
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<PaymentIntent> CreatePaymentIntent(decimal amount)
    {
        var options = new PaymentIntentCreateOptions()
        {
            Amount = (long)(amount * 100),
            Currency = "eur",
            PaymentMethodTypes = new List<string> { "card" }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        return intent;
    }
}
