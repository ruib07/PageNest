using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Services;

public interface IPaymentsService
{
    Task<IEnumerable<Payment>> GetPayments();
    Task<IEnumerable<Payment>> GetPaymentsByOrderId(Guid orderId);
    Task<Result<Payment>> GetPaymentById(Guid paymentId);
    Task<Result<Payment>> CreatePayment(Payment payment);
    Task<Result<Payment>> UpdatePayment(Guid paymentId, Payment updatePayment);
    Task DeletePayment(Guid paymentId);
}
