
using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Tests;

public class OrderItemsTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/orderitems";

    public OrderItemsTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetOrderItems_ShouldReturnOkResult_WithAllOrderItems()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        context.OrderItems.AddRange(orderItems);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderItemsByOrderId_ShouldReturnOkResult_WithAllOrderItems_WhenOrderExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        context.OrderItems.AddRange(orderItems);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/order/{orderItems[0].OrderId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderItemsByBookId_ShouldReturnOkResult_WithAllOrderItems_WhenBookExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orderItems = OrderItemsBuilder.CreateOrderItems();
        context.OrderItems.AddRange(orderItems);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/book/{orderItems[0].BookId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderItemById_ShouldReturnOkResult_WithOrderItem()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orderItem = OrderItemsBuilder.CreateOrderItems().First();
        await context.OrderItems.AddAsync(orderItem);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/{orderItem.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<OrderItem>();

        Assert.NotNull(responseContent);
        Assert.Equal(orderItem.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetOrderItemById_ShouldReturnNotFoundResult_WhenOrderItemDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Order item not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }
}
