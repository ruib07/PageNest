using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;
using Stripe;

namespace PageNest.UnitTests.Services;

public class PaymentsServiceTests : TestBase
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly PaymentsService _paymentsService;

    public PaymentsServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _stripeServiceMock = new Mock<IStripeService>();
        _paymentsService = new PaymentsService(_paymentRepositoryMock.Object, _stripeServiceMock.Object);
    }

    [Fact]
    public async Task GetPayments_ShouldReturnAllPayments()
    {
        var payments = PaymentsBuilder.CreatePayments();

        _paymentRepositoryMock.Setup(repo => repo.GetPayments()).ReturnsAsync(payments);

        var result = await _paymentsService.GetPayments();

        Assert.NotNull(result);
        Assert.Equal(payments.Count, result.Count());
    }

    [Fact]
    public async Task GetPaymentsByOrderId_ShouldReturnPayments_WhenOrderIdExists()
    {
        var payments = PaymentsBuilder.CreatePayments();
        var orderId = payments.First().OrderId;

        _paymentRepositoryMock.Setup(repo => repo.GetPaymentsByOrderId(orderId))
                              .ReturnsAsync(payments.Where(p => p.OrderId == orderId).ToList());

        var result = await _paymentsService.GetPaymentsByOrderId(orderId);

        Assert.NotNull(result);
        Assert.All(result, p => Assert.Equal(orderId, p.OrderId));
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnPayment_WhenExists()
    {
        var payment = PaymentsBuilder.CreatePayments().First();

        _paymentRepositoryMock.Setup(repo => repo.GetPaymentById(payment.Id))
                              .ReturnsAsync(payment);

        var result = await _paymentsService.GetPaymentById(payment.Id);

        Assert.NotNull(result.Data);
        Assert.Equal(payment.Id, result.Data.Id);
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        _paymentRepositoryMock.Setup(repo => repo.GetPaymentById(It.IsAny<Guid>()))
                              .ReturnsAsync((Payment)null);

        var result = await _paymentsService.GetPaymentById(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error.StatusCode);
        Assert.Equal("Payment not found.", result.Error.Message);
    }

    [Fact]
    public async Task CreatePayment_ShouldCreateSuccessfully()
    {
        var payment = PaymentsBuilder.CreatePayments().First();

        _stripeServiceMock.Setup(s => s.CreatePaymentIntent(payment.Amount))
                                       .ReturnsAsync(new PaymentIntent { Id = "pi_123456" });

        _paymentRepositoryMock.Setup(repo => repo.CreatePayment(It.IsAny<Payment>()))
                                                 .ReturnsAsync((Payment p) => p);

        var result = await _paymentsService.CreatePayment(payment);

        Assert.True(result.IsSuccess);
        Assert.Equal("Payment created successfully.", result.Message);
        Assert.Equal(payment.Amount, result.Data.Amount);
        Assert.Equal(PaymentStatus.Pending, result.Data.Status);
    }

    [Theory]
    [InlineData(-10, "pi_123", PaymentStatus.Pending, "Amount cannot be negative.")]
    [InlineData(100, "", PaymentStatus.Pending, "Stripe Payment Intent ID cannot be empty.")]
    [InlineData(100, "pi_123", (PaymentStatus)999, "Invalid status.")]
    public async Task CreatePayment_ShouldReturnBadRequest_WhenInvalid(
        decimal amount, string stripePaymentIntentId, PaymentStatus status, string expectedError)
    {
        var invalidPayment = PaymentsBuilder.InvalidPaymentCreation(amount, stripePaymentIntentId, status);

        var result = await _paymentsService.CreatePayment(invalidPayment);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.StatusCode);
        Assert.Equal(expectedError, result.Error.Message);
    }

    [Fact]
    public async Task UpdatePayment_ShouldUpdateSuccessfully()
    {
        var existingPayment = PaymentsBuilder.CreatePayments().First();
        var updatedPayment = PaymentsBuilder.UpdatePayment(existingPayment.Id, existingPayment.OrderId);

        _paymentRepositoryMock.Setup(repo => repo.GetPaymentById(existingPayment.Id))
                              .ReturnsAsync(existingPayment);

        _paymentRepositoryMock.Setup(repo => repo.UpdatePayment(It.IsAny<Payment>()))
                              .Returns(Task.CompletedTask);

        var result = await _paymentsService.UpdatePayment(existingPayment.Id, updatedPayment);

        Assert.True(result.IsSuccess);
        Assert.Equal("Payment updated successfully.", result.Message);
        Assert.Equal(updatedPayment.Amount, result.Data.Amount);
        Assert.Equal(updatedPayment.Status, result.Data.Status);
    }

    [Fact]
    public async Task UpdatePayment_ShouldReturnBadRequest_WhenInvalid()
    {
        var existingPayment = PaymentsBuilder.CreatePayments().First();
        var invalidPayment = PaymentsBuilder.InvalidPaymentCreation(-5, "pi_456", PaymentStatus.Completed);

        _paymentRepositoryMock.Setup(repo => repo.GetPaymentById(existingPayment.Id))
                              .ReturnsAsync(existingPayment);

        var result = await _paymentsService.UpdatePayment(existingPayment.Id, invalidPayment);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.StatusCode);
        Assert.Equal("Amount cannot be negative.", result.Error.Message);
    }

    [Fact]
    public async Task DeletePayment_ShouldCallRepositoryDelete()
    {
        var paymentId = Guid.NewGuid();

        _paymentRepositoryMock.Setup(repo => repo.DeletePayment(paymentId)).Returns(Task.CompletedTask);

        await _paymentsService.DeletePayment(paymentId);

        _paymentRepositoryMock.Verify(repo => repo.DeletePayment(paymentId), Times.Once);
    }
}
