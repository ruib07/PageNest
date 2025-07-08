using Microsoft.Extensions.DependencyInjection;
using Moq;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using Stripe;
using System.Net;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Tests;

public class PaymentsTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/payments";

    public PaymentsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
        _serviceProvider = _factory.Services;
    }

    public async Task InitializeAsync()
    {
        await AuthHelper.AuthenticateUser(_httpClient, _serviceProvider);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetPayments_ShouldReturnOkResult_WithAllPayments()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var payments = PaymentsBuilder.CreatePayments();
        context.Payments.AddRange(payments);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPaymentsByOrderId_ShouldReturnOkResult_WithAllPayments_WhenOrderExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var payments = PaymentsBuilder.CreatePayments();
        context.Payments.AddRange(payments);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/order/{payments[0].OrderId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnOkResult_WithPayment()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var payment = PaymentsBuilder.CreatePayments().First();
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/{payment.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<Payment>();

        Assert.NotNull(responseContent);
        Assert.Equal(payment.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnNotFoundResult_WhenPaymentDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Payment not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturnCreatedResult_WhenPaymentIsCreated()
    {
        var newPayment = PaymentsBuilder.CreatePayments().First();
        var paymentIntent = new PaymentIntent()
        {
            Id = "pi_mocked_123",
            Amount = (long)newPayment.Amount
        };

        _factory.StripeServiceMock.Setup(repo => repo.CreatePaymentIntent(newPayment.Amount))
                                                     .ReturnsAsync(paymentIntent);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, newPayment);
        var content = await response.Content.ReadFromJsonAsync<ResponsesDTO.PaymentResponse>();

        Assert.NotNull(content);
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Payment created successfully.", content.Message);
            Assert.NotEqual(newPayment.Id, content.PaymentId);
            Assert.False(string.IsNullOrWhiteSpace(content.StripePaymentIntentId));
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

        var response = await _httpClient.PostAsJsonAsync(_baseURL, invalidPayment);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal(expectedError, error.Message);
    }

    [Fact]
    public async Task UpdatePayment_ShouldReturnOkResult_WhenPaymentIsUpdated()
    {
        var payment = PaymentsBuilder.CreatePayments().First();
        var paymentIntent = new PaymentIntent()
        {
            Id = "pi_mocked_123",
            Amount = (long)payment.Amount
        };

        _factory.StripeServiceMock.Setup(repo => repo.CreatePaymentIntent(payment.Amount))
                                                     .ReturnsAsync(paymentIntent);

        var createdResponse = await _httpClient.PostAsJsonAsync(_baseURL, payment);
        Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
        var createdContent = await createdResponse.Content.ReadFromJsonAsync<ResponsesDTO.PaymentResponse>();
        Assert.NotNull(createdContent);

        var updatePayment = PaymentsBuilder.UpdatePayment(payment.Id, payment.OrderId);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{createdContent.PaymentId}", updatePayment);
        var responseMessage = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Payment updated successfully.", responseMessage);
        });
    }

    [Fact]
    public async Task UpdatePayment_ShouldReturnBadRequest_WhenInvalid()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var existingPayment = PaymentsBuilder.CreatePayments().First();
        await context.Payments.AddAsync(existingPayment);
        await context.SaveChangesAsync();

        var invalidPayment = PaymentsBuilder.InvalidPaymentCreation(-5, "pi_456", PaymentStatus.Completed);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{existingPayment.Id}", invalidPayment);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Amount cannot be negative.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task DeletePayment_ShouldReturnNoContentResult_WhenPaymentIsDeleted()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var payment = PaymentsBuilder.CreatePayments().First();
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var response = await _httpClient.DeleteAsync($"{_baseURL}/{payment.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
