using Microsoft.Extensions.DependencyInjection;
using PageNest.Application.Helpers;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using PageNest.IntegrationTests.Helpers;
using PageNest.TestUtils.Builders;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace PageNest.IntegrationTests.Tests;

public class AuthenticationsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private const string _baseURL = "/api/v1/auth";
    private const string _signupEndpoint = "signup";
    private const string _signinEndpoint = "signin";
    private const string _refreshEndpoint = "refresh";
    private const string _logoutEndpoint = "logout";
    private const string _recoverPasswordEndpoint = "recover-password";

    public AuthenticationsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
        _serviceProvider = _factory.Services;
    }

    [Fact]
    public async Task SignUp_ShouldReturnCreatedResult_WhenUserIsCreated()
    {
        var user = UsersBuilder.CreateUsers().First();
        var request = new AuthenticationDTO.SignUp.Request(user.Name, user.Email, "User@Password-123");

        var response = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_signupEndpoint}", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ResponsesDTO.Creation>();

        Assert.Equal("User created successfully.", content.Message);
    }

    [Fact]
    public async Task SignIn_ShouldReturnCreatedResult_WhenCredentialsAreValid()
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

        var signinRequest = new AuthenticationDTO.SignIn.Request(user.Email, "User@Password-123");
        var signinResponse = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_signinEndpoint}", signinRequest);
        Assert.Equal(HttpStatusCode.OK, signinResponse.StatusCode);
        var content = await signinResponse.Content.ReadFromJsonAsync<AuthenticationDTO.SignIn.Response>();

        Assert.NotNull(content);
        Assert.Multiple(() =>
        {
            Assert.NotNull(content.AccessToken);
            Assert.NotNull(content.RefreshToken);
            Assert.Equal("Bearer", content.TokenType);
            Assert.True(content.ExpiresAt > DateTime.UtcNow);
        });
    }

    [Fact]
    public async Task Refresh_ShouldReturnOkResult_WhenAccessTokenIsRefreshed()
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

        var signInRequest = new AuthenticationDTO.SignIn.Request(user.Email, "User@Password-123");
        var signInResponse = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_signinEndpoint}", signInRequest);
        Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);

        var signInContent = await signInResponse.Content.ReadFromJsonAsync<AuthenticationDTO.SignIn.Response>();

        var refreshRequest = new AuthenticationDTO.SignIn.RefreshTokenRequest(signInContent.RefreshToken);
        var refreshResponse = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_refreshEndpoint}", refreshRequest);
        Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);

        var refreshContent = await refreshResponse.Content.ReadFromJsonAsync<AuthenticationDTO.SignIn.Response>();

        Assert.NotNull(refreshContent);
        Assert.Multiple(() =>
        {
            Assert.NotNull(refreshContent.AccessToken);
            Assert.Equal("Bearer", refreshContent.TokenType);
            Assert.NotNull(refreshContent.RefreshToken);
            Assert.True(refreshContent.ExpiresAt > DateTime.UtcNow);
        });
    }

    [Fact]
    public async Task Refresh_ShouldReturnBadRequestResult_WhenRefreshTokenIsEmpty()
    {
        var request = new AuthenticationDTO.SignIn.RefreshTokenRequest(string.Empty);
        var response = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_refreshEndpoint}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorizedResult_WhenRefreshTokenIsInvalid()
    {
        var invalidToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var request = new AuthenticationDTO.SignIn.RefreshTokenRequest(invalidToken);

        var response = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_refreshEndpoint}", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorizedResult_WhenRefreshTokenIsExpired()
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

        var expiredToken = new RefreshToken()
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            IsRevoked = false
        };

        context.Users.Add(user);
        context.RefreshTokens.Add(expiredToken);
        await context.SaveChangesAsync();

        var request = new AuthenticationDTO.SignIn.RefreshTokenRequest(expiredToken.Token);
        var response = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_refreshEndpoint}", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ShouldReturnOkResult_WhenRefreshTokenIsRevoked()
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

        var signInRequest = new AuthenticationDTO.SignIn.Request(user.Email, "User@Password-123");
        var signInResponse = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_signinEndpoint}", signInRequest);
        Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);

        var signInContent = await signInResponse.Content.ReadFromJsonAsync<AuthenticationDTO.SignIn.Response>();

        var refreshRequest = new AuthenticationDTO.SignIn.RefreshTokenRequest(signInContent.RefreshToken);
        await _httpClient.PostAsJsonAsync($"{_baseURL}/{_logoutEndpoint}", refreshRequest);

        Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_ShouldReturnNotFoundResult_WhenRefreshTokenDoNotExist()
    {
        await AuthHelper.AuthenticateUser(_httpClient, _serviceProvider);

        var invalidToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var request = new AuthenticationDTO.SignIn.RefreshTokenRequest(invalidToken);

        var response = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_logoutEndpoint}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RecoverPasswordSendEmail_ShouldReturnOkResult_WhenUserExists()
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

        var request = new { user.Email };

        var response = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_recoverPasswordEndpoint}", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RecoverPasswordSendEmail_ShouldReturnOkResult_WhenUserDoesNotExist()
    {
        var request = new { Email = "nonexistentuser@email.com" };

        var response = await _httpClient.PostAsJsonAsync($"{_baseURL}/{_recoverPasswordEndpoint}", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePassword_ShouldReturnOkResult_WhenTokenAndPasswordAreValid()
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

        var token = await _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IUsersService>()
                                           .GeneratePasswordResetToken(user.Email);

        var updatePassword = UsersBuilder.UpdatePasswordCreation(token, "New@Password-123", "New@Password-123");

        var response = await _httpClient.PutAsJsonAsync($"{_baseURL}/update-password", updatePassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
