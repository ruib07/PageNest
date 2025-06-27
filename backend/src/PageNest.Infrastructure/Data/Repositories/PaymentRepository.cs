using Microsoft.EntityFrameworkCore;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Infrastructure.Data.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;
    public DbSet<Payment> Payments => _context.Payments;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payment>> GetPayments()
    {
        return await Payments.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByOrderId(Guid orderId)
    {
        return await Payments.AsNoTracking().Where(p => p.OrderId == orderId).ToListAsync();
    }

    public async Task<Payment> GetPaymentById(Guid paymentId)
    {
        return await Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Payment> CreatePayment(Payment payment)
    {
        await Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    public async Task UpdatePayment(Payment payment)
    {
        Payments.Update(payment);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePayment(Guid paymentId)
    {
        var payment = await GetPaymentById(paymentId);

        Payments.Remove(payment);
        await _context.SaveChangesAsync();
    }
}
