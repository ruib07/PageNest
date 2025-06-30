using Microsoft.EntityFrameworkCore;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class PaymentRepositoryTests : TestBase
{
    private readonly PaymentRepository _paymentRepository;

    public PaymentRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _paymentRepository = new PaymentRepository(_context);
    }

    [Fact]
    public async Task GetPayments_ShouldReturnAllPayments()
    {
        var payments = PaymentsBuilder.CreatePayments();
        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        var result = await _paymentRepository.GetPayments();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(payments.Count, result.Count());
            Assert.Equal(payments.First().Id, result.First().Id);
            Assert.Equal(payments.First().OrderId, result.First().OrderId);
            Assert.Equal(payments.First().Amount, result.First().Amount);
            Assert.Equal(payments.First().StripePaymentIntentId, result.First().StripePaymentIntentId);
            Assert.Equal(payments.First().Status, result.First().Status);
            Assert.Equal(payments.Last().Id, result.Last().Id);
            Assert.Equal(payments.Last().OrderId, result.Last().OrderId);
            Assert.Equal(payments.Last().Amount, result.Last().Amount);
            Assert.Equal(payments.Last().StripePaymentIntentId, result.Last().StripePaymentIntentId);
            Assert.Equal(payments.Last().Status, result.Last().Status);
        });
    }

    [Fact]
    public async Task GetPaymentsByOrderId_ShouldReturnAllPayments_WhenOrderExists()
    {
        var payments = PaymentsBuilder.CreatePayments();
        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        var result = await _paymentRepository.GetPaymentsByOrderId(payments.First().OrderId);

        Assert.NotNull(result);
        Assert.Equal(payments.First().Id, result.First().Id);
        Assert.Equal(payments.First().OrderId, result.First().OrderId);
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnPayment_WhenPaymentExists()
    {
        var payment = PaymentsBuilder.CreatePayments().First();
        await _paymentRepository.CreatePayment(payment);

        var result = await _paymentRepository.GetPaymentById(payment.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(payment.Id, result.Id);
            Assert.Equal(payment.OrderId, result.OrderId);
            Assert.Equal(payment.Amount, result.Amount);
            Assert.Equal(payment.StripePaymentIntentId, result.StripePaymentIntentId);
            Assert.Equal(payment.Status, result.Status);
        });
    }

    [Fact]
    public async Task CreatePayment_ShouldCreatePayment()
    {
        var newPayment = PaymentsBuilder.CreatePayments().First();
        await _paymentRepository.CreatePayment(newPayment);

        var result = await _paymentRepository.GetPaymentById(newPayment.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(newPayment.Id, result.Id);
            Assert.Equal(newPayment.OrderId, result.OrderId);
            Assert.Equal(newPayment.Amount, result.Amount);
            Assert.Equal(newPayment.StripePaymentIntentId, result.StripePaymentIntentId);
            Assert.Equal(newPayment.Status, result.Status);
        });
    }

    [Fact]
    public async Task UpdatePayment_ShouldUpdatePayment()
    {
        var createPayment = PaymentsBuilder.CreatePayments().First();
        await _paymentRepository.CreatePayment(createPayment);

        _context.Entry(createPayment).State = EntityState.Detached;

        var updatedPayment = PaymentsBuilder.UpdatePayment(createPayment.Id, createPayment.OrderId);
        await _paymentRepository.UpdatePayment(updatedPayment);

        var result = await _paymentRepository.GetPaymentById(createPayment.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(updatedPayment.Id, result.Id);
            Assert.Equal(updatedPayment.OrderId, result.OrderId);
            Assert.Equal(updatedPayment.Amount, result.Amount);
            Assert.Equal(updatedPayment.StripePaymentIntentId, result.StripePaymentIntentId);
            Assert.Equal(updatedPayment.Status, result.Status);
        });
    }

    [Fact]
    public async Task DeletePayment_ShouldDeletePayment()
    {
        var payment = PaymentsBuilder.CreatePayments().First();

        await _paymentRepository.CreatePayment(payment);
        await _paymentRepository.DeletePayment(payment.Id);

        var result = await _paymentRepository.GetPaymentById(payment.Id);

        Assert.Null(result);
    }
}
