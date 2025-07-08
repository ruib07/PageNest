using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Tests;

public class CartItemsTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/cartitems";

    public CartItemsTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetCartItems_ShouldReturnOkResult_WithAllCartItems()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cartItems = CartItemsBuilder.CreateCartItems();
        context.CartItems.AddRange(cartItems);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCartItemsByUserId_ShouldReturnOkResult_WithAllCartItems_WhenUserExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cartItems = CartItemsBuilder.CreateCartItems();
        context.CartItems.AddRange(cartItems);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/user/{cartItems[0].UserId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCartItemById_ShouldReturnOkResult_WithCartItem()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cartItem = CartItemsBuilder.CreateCartItems().First();
        await context.CartItems.AddAsync(cartItem);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/{cartItem.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<CartItem>();

        Assert.NotNull(responseContent);
        Assert.Equal(cartItem.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetCartItemById_ShouldReturnNotFoundResult_WhenCartItemDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Cart item not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnCreatedResult_WhenCartItemIsCreated()
    {
        var newCartItem = CartItemsBuilder.CreateCartItems().First();

        var response = await _httpClient.PostAsJsonAsync(_baseURL, newCartItem);
        var content = await response.Content.ReadFromJsonAsync<ResponsesDTO.Creation>();

        Assert.NotNull(content);
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("Cart item created successfully.", content.Message);
            Assert.Equal(newCartItem.Id, content.Id);
        });
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenQuantityIsNegative()
    {
        var cartItem = CartItemsBuilder.InvalidCartItemCreation(-1);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, cartItem);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Quantity must be greater than zero.", error.Message);
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenQuantityIsZero()
    {
        var cartItem = CartItemsBuilder.InvalidCartItemCreation(0);

        var response = await _httpClient.PostAsJsonAsync(_baseURL, cartItem);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Quantity must be greater than zero.", error.Message);
    }

    [Fact]
    public async Task CreateCartItem_ShouldReturnBadRequest_WhenCartItemHasDuplicateItems()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var existingCartItem = new CartItem()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            Quantity = 1
        };

        await context.CartItems.AddAsync(existingCartItem);
        await context.SaveChangesAsync();

        var duplicateCartItem = new CartItem()
        {
            Id = Guid.NewGuid(),
            UserId = existingCartItem.UserId,
            BookId = existingCartItem.BookId,
            Quantity = 2
        };

        var response = await _httpClient.PostAsJsonAsync(_baseURL, duplicateCartItem);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.Equal(409, error.StatusCode);
        Assert.Equal("Book already exists in the cart.", error.Message);
    }

    [Fact]
    public async Task UpdateCartItem_ShouldReturnOkResult_WhenCartItemIsUpdated()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();

        var createdResponse = await _httpClient.PostAsJsonAsync(_baseURL, cartItem);
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var createdCartItem = await createdResponse.Content.ReadFromJsonAsync<CartItem>();

        var updateCartItem = CartItemsBuilder.UpdateCartItem(createdCartItem.Id, cartItem.UserId, cartItem.BookId);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{createdCartItem.Id}", updateCartItem);
        var responseMessage = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Cart item updated successfully.", responseMessage);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task UpdateCartItem_ShouldReturnBadRequest_WhenQuantityIsInvalid(int invalidQuantity)
    {
        var existingCartItem = CartItemsBuilder.CreateCartItems().First();

        var invalidCartItem = new CartItem()
        {
            Id = existingCartItem.Id,
            UserId = existingCartItem.UserId,
            BookId = existingCartItem.BookId,
            Quantity = invalidQuantity
        };

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{existingCartItem.Id}", invalidCartItem);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();
        Assert.NotNull(error);
        Assert.Equal("Quantity must be greater than zero.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task DeleteCartItem_ShouldReturnNoContentResult_WhenCartItemIsDeleted()
    {
        var cartItem = CartItemsBuilder.CreateCartItems().First();

        var createdResponse = await _httpClient.PostAsJsonAsync(_baseURL, cartItem);
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var createdCartItem = await createdResponse.Content.ReadFromJsonAsync<CartItem>();

        var response = await _httpClient.DeleteAsync($"{_baseURL}/{createdCartItem.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
