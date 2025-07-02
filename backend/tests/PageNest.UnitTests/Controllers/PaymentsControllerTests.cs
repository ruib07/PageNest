using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.API.Controllers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;
using Stripe;

namespace PageNest.UnitTests.Controllers;

public class PaymentsControllerTests : TestBase
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly PaymentsService _paymentsService;
    private readonly PaymentsController _paymentsController;

    public PaymentsControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _stripeServiceMock = new Mock<IStripeService>();
        _paymentsService = new PaymentsService(_paymentRepositoryMock.Object, _stripeServiceMock.Object);
        _paymentsController = new PaymentsController(_paymentsService);
    }

    [Fact]
    public async Task GetPayments_ShouldReturnOkResult_WithAllPayments()
    {
        var payments = PaymentsBuilder.CreatePayments();

        _paymentRepositoryMock.Setup(repo => repo.GetPayments()).ReturnsAsync(payments);

        var result = await _paymentsController.GetPayments();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Payment>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(payments.Count, response.Count());
        });
    }

    [Fact]
    public async Task GetPaymentsByOrderId_ShouldReturnOkResult_WithAllPayments_WhenOrderExists()
    {
        var payments = PaymentsBuilder.CreatePayments();
        var paymentsByOrderList = payments.Where(p => p.OrderId == payments[0].OrderId).ToList();

        _paymentRepositoryMock.Setup(repo => repo.GetPaymentsByOrderId(payments[0].OrderId))
                                                 .ReturnsAsync(paymentsByOrderList);

        var result = await _paymentsController.GetPaymentsByOrderId(payments[0].OrderId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<Payment>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(payments[0].OrderId, response.First().OrderId);
        });
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnOkResult_WithPayment()
    {
        var payment = PaymentsBuilder.CreatePayments().First();

        _paymentRepositoryMock.Setup(repo => repo.GetPaymentById(payment.Id)).ReturnsAsync(payment);

        var result = await _paymentsController.GetPaymentById(payment.Id);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as Payment;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(payment.Id, response.Id);
            Assert.Equal(payment.OrderId, response.OrderId);
            Assert.Equal(payment.Amount, response.Amount);
            Assert.Equal(payment.StripePaymentIntentId, response.StripePaymentIntentId);
            Assert.Equal(payment.Status, response.Status);
        });
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnNotFoundResult_WhenPaymentDoesNotExist()
    {
        var result = await _paymentsController.GetPaymentById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Payment not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturnCreatedResult_WhenPaymentIsCreated()
    {
        var newPayment = PaymentsBuilder.CreatePayments().First();
        var paymentIntent = new PaymentIntent() { Id = "pi_test_123" };

        _stripeServiceMock.Setup(s => s.CreatePaymentIntent(newPayment.Amount))
                                       .ReturnsAsync(paymentIntent);

        _paymentRepositoryMock.Setup(repo => repo.CreatePayment(It.IsAny<Payment>()))
                                                 .ReturnsAsync((Payment p) => p);

        var paymentsServiceMock = new Mock<IPaymentsService>();
        paymentsServiceMock.Setup(s => s.CreatePayment(It.IsAny<Payment>()))
                                        .ReturnsAsync(Result<Payment>
                                        .Success(newPayment, "Payment created successfully."));

        var controller = new PaymentsController(paymentsServiceMock.Object);

        var result = await controller.CreatePayment(newPayment);
        var createdResult = result as OkObjectResult;
        Assert.NotNull(createdResult);

        var response = createdResult.Value as ResponsesDTO.PaymentResponse;
        Assert.NotNull(response);

        Assert.Multiple(() =>
        {
            Assert.Equal(200, createdResult.StatusCode);
            Assert.Equal("Payment created successfully.", response.Message);
            Assert.Equal(newPayment.Id, response.PaymentId);
            Assert.Equal(newPayment.StripePaymentIntentId, response.StripePaymentIntentId);
        });
    }

    [Theory]
    [InlineData(-10, "pi_123", PaymentStatus.Pending, "Amount cannot be negative.")]
    [InlineData(100, "", PaymentStatus.Pending, "Stripe Payment Intent ID cannot be empty.")]
    [InlineData(100, "pi_123", (PaymentStatus)999, "Invalid status.")]
    public async Task CreatePayment_ShouldReturnBadRequest_WhenInvalid(
        decimal amount, string stripePaymentIntentId, PaymentStatus status, string expectedError)
    {
        var invalidPayment = PaymentsBuilder.InvalidPaymentCreation(amount, stripePaymentIntentId, status);

        var result = await _paymentsController.CreatePayment(invalidPayment);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal(expectedError, error.Message);
    }

    [Fact]
    public async Task UpdatePayment_ShouldReturnOkResult_WhenPaymentIsUpdated()
    {
        var payment = PaymentsBuilder.CreatePayments().First();
        var updatedPayment = PaymentsBuilder.UpdatePayment(payment.Id, payment.OrderId);

        _paymentRepositoryMock.Setup(repo => repo.GetPaymentById(payment.Id)).ReturnsAsync(payment);
        _paymentRepositoryMock.Setup(repo => repo.UpdatePayment(It.IsAny<Payment>())).Returns(Task.CompletedTask);

        var result = await _paymentsController.UpdatePayment(payment.Id, updatedPayment);
        var okResult = result as OkObjectResult;

        Assert.NotNull(okResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Payment updated successfully.", okResult.Value);
        });
    }

    [Fact]
    public async Task UpdatePayment_ShouldReturnBadRequest_WhenInvalid()
    {
        var existingPayment = PaymentsBuilder.CreatePayments().First();
        var invalidPayment = PaymentsBuilder.InvalidPaymentCreation(-5, "pi_456", PaymentStatus.Completed);

        _paymentRepositoryMock.Setup(repo => repo.GetPaymentById(existingPayment.Id))
                                                 .ReturnsAsync(existingPayment);

        var result = await _paymentsController.UpdatePayment(existingPayment.Id, invalidPayment);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Amount cannot be negative.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task DeletePayment_ShouldReturnNoContentResult_WhenPaymentIsDeleted()
    {
        var payment = PaymentsBuilder.CreatePayments().First();

        _paymentRepositoryMock.Setup(repo => repo.DeletePayment(payment.Id)).Returns(Task.CompletedTask);

        var result = await _paymentsController.DeletePayment(payment.Id);
        var noContentResult = result as NoContentResult;

        Assert.Equal(204, noContentResult.StatusCode);
    }
}
