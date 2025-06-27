using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;

namespace PageNest.Infrastructure.Services;

public class PaymentsService : IPaymentsService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStripeService _stripeService;

    public PaymentsService(IPaymentRepository paymentRepository, IStripeService stripeService)
    {
        _paymentRepository = paymentRepository;
        _stripeService = stripeService;
    }

    public async Task<IEnumerable<Payment>> GetPayments()
    {
        return await _paymentRepository.GetPayments();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByOrderId(Guid orderId)
    {
        return await _paymentRepository.GetPaymentsByOrderId(orderId);
    }

    public async Task<Result<Payment>> GetPaymentById(Guid paymentId)
    {
        var payment = await _paymentRepository.GetPaymentById(paymentId);

        if (payment == null) return Result<Payment>.Fail("Payment not found.", 404);

        return Result<Payment>.Success(payment);
    }

    public async Task<Result<Payment>> CreatePayment(Payment payment)
    {
        var validation = ValidatePaymentFields(payment);

        if (!validation.IsSuccess)
            return Result<Payment>.Fail(validation.Error.Message, validation.Error.StatusCode);

        var paymentIntent = await _stripeService.CreatePaymentIntent(payment.Amount);

        var newPayment = new Payment()
        {
            Id = Guid.NewGuid(),
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            StripePaymentIntentId = paymentIntent.Id,
            Status = PaymentStatus.Pending
        };

        var createdPayment = await _paymentRepository.CreatePayment(newPayment);

        return Result<Payment>.Success(createdPayment, "Payment created successfully.");
    }

    public async Task<Result<Payment>> UpdatePayment(Guid paymentId, Payment updatePayment)
    {
        var currentPayment = await _paymentRepository.GetPaymentById(paymentId);

        var validation = ValidatePaymentFields(updatePayment);

        if (!validation.IsSuccess)
            return Result<Payment>.Fail(validation.Error.Message, validation.Error.StatusCode);

        currentPayment.Amount = updatePayment.Amount;
        currentPayment.Status = updatePayment.Status;

        await _paymentRepository.UpdatePayment(currentPayment);

        return Result<Payment>.Success(currentPayment, "Payment updated successfully.");
    }

    public async Task DeletePayment(Guid paymentId)
    {
        await _paymentRepository.DeletePayment(paymentId);
    }

    #region Private Methods

    private static Result<bool> ValidatePaymentFields(Payment payment)
    {
        if (!Enum.IsDefined(typeof(PaymentStatus), payment.Status))
            return Result<bool>.Fail("Invalid status.", 400);

        if (string.IsNullOrWhiteSpace(payment.StripePaymentIntentId))
            return Result<bool>.Fail("Stripe Payment Intent ID cannot be empty.", 400);

        if (payment.Amount < 0)
            return Result<bool>.Fail("Amount cannot be negative.", 400);

        return Result<bool>.Success(true);
    }

    #endregion
}
