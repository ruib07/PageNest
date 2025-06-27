using PageNest.Domain.Entities;

namespace PageNest.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetPayments();
    Task<IEnumerable<Payment>> GetPaymentsByOrderId(Guid orderId);
    Task<Payment> GetPaymentById(Guid paymentId);
    Task<Payment> CreatePayment(Payment payment);
    Task UpdatePayment(Payment payment);
    Task DeletePayment(Guid paymentId);
}
