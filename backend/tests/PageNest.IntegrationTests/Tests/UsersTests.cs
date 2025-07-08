using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Helpers;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;
using System.Net.Http.Json;

namespace PageNest.IntegrationTests.Tests;

public class UsersTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/users";

    public UsersTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetUsers_ShouldReturnOkResult_WithAllUsers()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = UsersBuilder.CreateUsers();
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync(_baseURL);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnOkResult_WithUser()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User()
        {
            Id = Guid.NewGuid(),
            Name = "UserTest",
            Email = "user@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.Admin
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"{_baseURL}/{user.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<User>();

        Assert.NotNull(responseContent);
        Assert.Equal(user.Id, responseContent.Id);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFoundResult_WhenUserDoesNotExist()
    {
        var response = await _httpClient.GetAsync($"{_baseURL}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("User not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnOkResult_WhenUserIsUpdated()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User()
        {
            Id = Guid.NewGuid(),
            Name = "UserTest",
            Email = "user@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.User
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var updateUser = UsersBuilder.UpdateUser(user.Id);

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{user.Id}", updateUser);
        var responseMessage = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("User updated successfully.", responseMessage);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequestResult_WhenUserNameIsEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User()
        {
            Id = Guid.NewGuid(),
            Name = "UserTest",
            Email = "user@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.User
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("", "user@email.com", "User@Password-123");

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{user.Id}", invalidUpdatedUser);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Name is required.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserEmailIsEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User()
        {
            Id = Guid.NewGuid(),
            Name = "UserTest",
            Email = "user@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.User
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", "", "User@Password-123");

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{user.Id}", invalidUpdatedUser);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Valid email is required.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserEmailIsInvalid()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User()
        {
            Id = Guid.NewGuid(),
            Name = "UserTest",
            Email = "user@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.User
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", "useremail.com", "User@Password-123");

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{user.Id}", invalidUpdatedUser);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("Valid email is required.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnConflict_WhenUserEmailAlreadyExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var existingUser = new User()
        {
            Id = Guid.NewGuid(),
            Name = "Existing User",
            Email = "existing@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.User
        };

        var userToUpdate = new User()
        {
            Id = Guid.NewGuid(),
            Name = "User To Update",
            Email = "update@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.User
        };

        context.Users.Add(existingUser);
        context.Users.Add(userToUpdate);
        await context.SaveChangesAsync();

        var conflictingUpdate = UsersBuilder.InvalidUserCreation("User Updated", existingUser.Email, "User@Password-123");

        var updateResponse = await _httpClient.PutAsJsonAsync($"{_baseURL}/{userToUpdate.Id}", conflictingUpdate);
        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);

        var error = await updateResponse.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal("User with the same email already exists.", error.Message);
        Assert.Equal(409, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserPasswordDoesNotMeetTheRequirements()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User()
        {
            Id = Guid.NewGuid(),
            Name = "UserTest",
            Email = "user@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.User
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", "user@email.com", "Password");

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/{user.Id}", invalidUpdatedUser);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ResponsesDTO.Error>();

        Assert.NotNull(error);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Password must be at least 8 characters long and contain at least one uppercase letter, " +
                            "one lowercase letter, one number, and one special character.", error.Message);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNoContentResult_WhenUserIsDeleted()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User()
        {
            Id = Guid.NewGuid(),
            Name = "UserTest",
            Email = "user@email.com",
            PasswordHash = PasswordHasherHelper.HashPassword("User@Password-123"),
            Role = Roles.User
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var response = await _httpClient.DeleteAsync($"{_baseURL}/{user.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
