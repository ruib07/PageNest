using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Tests;

public class OrdersTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/orders";

    public OrdersTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetOrders_ShouldReturnOkResult_WithAllOrders()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orders = OrdersBuilder.CreateOrders();
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOrdersByUserId_ShouldReturnOkResult_WithAllOrders_WhenUserExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orders = OrdersBuilder.CreateOrders();
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/user/{orders[0].UserId}");
        var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<Order>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.All(responseContent, o => Assert.Equal(orders[0].UserId, o.UserId));
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnOkResult_WithOrder()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = OrdersBuilder.CreateOrders().First();
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/{order.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<Order>();

        Assert.NotNull(responseContent);
        Assert.Equal(order.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNotFoundResult_WhenOrderDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Order not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnCreatedResult_WhenOrderIsCreated()
    {
        var newOrder = new Order()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            Total = 100m,
            OrderItems = new List<OrderItem>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    BookId = Guid.NewGuid(),
                    Quantity = 2,
                    PriceAtPurchase = 50m
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(_baseURL, newOrder);
        var content = await response.Content.ReadFromJsonAsync<ResponsesDTO.Creation>();

        Assert.NotNull(content);
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("Order created successfully.", content.Message);
            Assert.Equal(newOrder.Id, content.Id);
        });
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenOrderIsInvalid()
    {
        var invalidOrder = OrdersBuilder.InvalidOrderCreation(0, OrderStatus.Shipped);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, invalidOrder);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Order must have at least one item.", error.Message);
    }

    //[Fact]
    //public async Task UpdateOrder_ShouldUpdateSuccessfully_WhenOrderIsValid()
    //{
    //    var order = new Order()
    //    {
    //        Id = Guid.NewGuid(),
    //        UserId = Guid.NewGuid(),
    //        Status = OrderStatus.Pending,
    //        Total = 100m,
    //        OrderItems = new List<OrderItem>()
    //        {
    //            new()
    //            {
    //                Id = Guid.NewGuid(),
    //                BookId = Guid.NewGuid(),
    //                Quantity = 2,
    //                PriceAtPurchase = 50m
    //            }
    //        }
    //    };

    //    var createdResponse = await _httpClient.PostAsJsonAsync(_baseURL, order);
    //    Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
    //    var createdOrder = await createdResponse.Content.ReadFromJsonAsync<Order>();

    //    var updateOrder = new Order()
    //    {
    //        Id = order.Id,
    //        UserId = order.UserId,
    //        Status = OrderStatus.Delivered,
    //        Total = 200m,
    //        OrderItems = new List<OrderItem>()
    //        {
    //            new() { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Quantity = 2, PriceAtPurchase = 100m }
    //        }
    //    };

    //    var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{createdOrder.Id}", updateOrder);
    //    var responseMessage = await response.Content.ReadAsStringAsync();

    //    Assert.Multiple(() =>
    //    {
    //        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //        Assert.Equal("Order updated successfully.", responseMessage);
    //    });
    //}

    [Fact]
    public async Task UpdateOrder_ShouldReturnBadRequest_WhenValidationFails()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var existingOrder = OrdersBuilder.CreateOrders(1).First();
        await context.Orders.AddAsync(existingOrder);
        await context.SaveChangesAsync();
        var invalidUpdate = OrdersBuilder.InvalidOrderCreation(-10, OrderStatus.Cancelled);
        invalidUpdate.OrderItems = new List<OrderItem>()
        {
            new() { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Quantity = -1, PriceAtPurchase = -50 }
        };

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{existingOrder.Id}", invalidUpdate);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Each item must have quantity greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task DeleteOrder_ShouldReturnNoContentResult_WhenOrderIsDeleted()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = OrdersBuilder.CreateOrders().First();
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var response = await _httpClient.DeleteAsync($"{_baseURL}/{order.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
