using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.API.Controllers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;
using PageNest.TestUtils.Helpers;
using System.Security.Cryptography;

namespace PageNest.UnitTests.Controllers;

public class AuthenticationsControllerTests : TestBase
{
    private readonly JwtDTO _jwt;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly UsersService _usersService;
    private readonly AuthenticationsController _authenticationsController;

    public AuthenticationsControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        var key = RandomKeyGenerator.GenerateRandomKey();
        _jwt = new JwtDTO("testIssuer", "testAudience", key);
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _usersService = new UsersService(_userRepositoryMock.Object);
        _authenticationsController = new AuthenticationsController(_context, _usersService, _emailServiceMock.Object, _jwt);
    }

    [Fact]
    public async Task SignUp_ShouldReturnCreatedResult_WhenUserIsCreated()
    {
        var user = UsersBuilder.CreateUsers().First();
        var request = new AuthenticationDTO.SignUp.Request(user.Name, user.Email, user.PasswordHash);

        _userRepositoryMock.Setup(repo => repo.CreateUser(It.IsAny<User>())).ReturnsAsync(user);

        var result = await _authenticationsController.SignUp(request);
        var createdResult = result as CreatedAtActionResult;
        var signupResponse = createdResult.Value as ResponsesDTO.Creation;

        Assert.NotNull(createdResult);
        Assert.NotNull(signupResponse);
        Assert.Multiple(() =>
        {
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("User created successfully.", signupResponse.Message);
            Assert.Equal(user.Id, signupResponse.Id);
        });
    }

    [Fact]
    public async Task SignUp_ShouldReturnBadRequestResult_WhenRequestIsNull()
    {
        var result = await _authenticationsController.SignUp(null);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.NotNull(badRequestResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("All fields are required.", badRequestResult.Value);
        });
    }

    [Fact]
    public async Task SignUp_ShouldReturnBadRequestResult_WhenNameIsNull()
    {
        var user = UsersBuilder.CreateUsers().First();
        var request = new AuthenticationDTO.SignUp.Request(string.Empty, user.Email, user.PasswordHash);

        var result = await _authenticationsController.SignUp(request);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.NotNull(badRequestResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid input.", badRequestResult.Value);
        });
    }

    [Fact]
    public async Task SignUp_ShouldReturnBadRequestResult_WhenEmailIsNull()
    {
        var user = UsersBuilder.CreateUsers().First();
        var request = new AuthenticationDTO.SignUp.Request(user.Name, string.Empty, user.PasswordHash);

        var result = await _authenticationsController.SignUp(request);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.NotNull(badRequestResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid input.", badRequestResult.Value);
        });
    }

    [Fact]
    public async Task SignUp_ShouldReturnBadRequestResult_WhenPasswordIsNull()
    {
        var user = UsersBuilder.CreateUsers().First();
        var request = new AuthenticationDTO.SignUp.Request(user.Name, user.Email, string.Empty);

        var result = await _authenticationsController.SignUp(request);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.NotNull(badRequestResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid input.", badRequestResult.Value);
        });
    }

    [Fact]
    public async Task SignUp_ShouldReturnConflictResult_WhenEmailAlreadyExists()
    {
        var existingUser = UsersBuilder.CreateUsers().First();
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new AuthenticationDTO.SignUp.Request("New User", existingUser.Email, "New@User-123");

        var result = await _authenticationsController.SignUp(request);
        var conflictResult = result as ConflictObjectResult;

        Assert.NotNull(conflictResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(409, conflictResult.StatusCode);
            Assert.Equal("Email already in use.", conflictResult.Value);
        });
    }

    [Fact]
    public async Task SignIn_ShouldReturnOkResult_WhenCredentialsAreValid()
    {
        var user = UsersBuilder.CreateUsers().First();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new AuthenticationDTO.SignIn.Request(user.Email, "User@Password-123");

        var result = await _authenticationsController.SignIn(request);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as AuthenticationDTO.SignIn.Response;

        Assert.NotNull(response);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task SignIn_ShouldReturnBadRequestResult_WhenRequestIsNull()
    {
        var result = await _authenticationsController.SignIn(null);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.NotNull(badRequestResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Email and password are mandatory.", badRequestResult.Value);
        });
    }

    [Fact]
    public async Task SignIn_ShouldReturnBadRequestResult_WhenEmailIsNull()
    {
        var request = new AuthenticationDTO.SignIn.Request(string.Empty, "User@Password-123");

        var result = await _authenticationsController.SignIn(request);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.NotNull(badRequestResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Email is required.", badRequestResult.Value);
        });
    }

    [Fact]
    public async Task SignIn_ShouldReturnBadRequestResult_WhenPasswordlIsNull()
    {
        var request = new AuthenticationDTO.SignIn.Request("usertest@email.com", string.Empty);

        var result = await _authenticationsController.SignIn(request);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.NotNull(badRequestResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Password is required.", badRequestResult.Value);
        });
    }

    [Fact]
    public async Task SignIn_ShouldReturnUnauthorizedResult_WhenPasswordIsIncorrect()
    {
        var user = UsersBuilder.CreateUsers().First();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new AuthenticationDTO.SignIn.Request(user.Email, "Wrong@Password-123");

        var result = await _authenticationsController.SignIn(request);
        var unauthorizedResult = result as UnauthorizedObjectResult;

        Assert.NotNull(unauthorizedResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(401, unauthorizedResult.StatusCode);
            Assert.Equal("Incorrect password.", unauthorizedResult.Value);
        });
    }

    [Fact]
    public async Task Refresh_ShouldReturnOkResult_WhenAccessTokenIsRefreshed()
    {
        var user = UsersBuilder.CreateUsers().First();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var signInRequest = new AuthenticationDTO.SignIn.Request(user.Email, "User@Password-123");

        var signInResult = await _authenticationsController.SignIn(signInRequest);
        var okSignInResult = signInResult as OkObjectResult;
        var signInResponse = okSignInResult.Value as AuthenticationDTO.SignIn.Response;

        var refreshRequest = new AuthenticationDTO.SignIn.RefreshTokenRequest(signInResponse.RefreshToken);

        var refreshResult = await _authenticationsController.Refresh(refreshRequest);
        var okRefreshResult = refreshResult as OkObjectResult;
        var refreshResponse = okRefreshResult.Value as AuthenticationDTO.SignIn.Response;

        Assert.NotNull(refreshResponse);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okRefreshResult.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(refreshResponse.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshResponse.RefreshToken));
            Assert.Equal("Bearer", refreshResponse.TokenType);
            Assert.True(refreshResponse.ExpiresAt > DateTime.UtcNow);
        });
    }

    [Fact]
    public async Task Refresh_ShouldReturnBadRequestResult_WhenRefreshTokenIsEmpty()
    {
        var request = new AuthenticationDTO.SignIn.RefreshTokenRequest(string.Empty);

        var result = await _authenticationsController.Refresh(request);
        var badRequestResult = result as BadRequestObjectResult;

        Assert.NotNull(badRequestResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Refresh token is required.", badRequestResult.Value);
        });
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorizedResult_WhenRefreshTokenIsInvalid()
    {
        var invalidToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var request = new AuthenticationDTO.SignIn.RefreshTokenRequest(invalidToken);

        var result = await _authenticationsController.Refresh(request);
        var unauthorizedResult = result as UnauthorizedObjectResult;

        Assert.NotNull(unauthorizedResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(401, unauthorizedResult.StatusCode);
            Assert.Equal("Invalid or expired refresh token.", unauthorizedResult.Value);
        });
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorizedResult_WhenRefreshTokenIsExpired()
    {
        var user = UsersBuilder.CreateUsers().First();
        var expiredToken = new RefreshToken()
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            IsRevoked = false
        };

        _context.Users.Add(user);
        _context.RefreshTokens.Add(expiredToken);
        await _context.SaveChangesAsync();

        var request = new AuthenticationDTO.SignIn.RefreshTokenRequest(expiredToken.Token);

        var result = await _authenticationsController.Refresh(request);
        var unauthorizedResult = result as UnauthorizedObjectResult;

        Assert.NotNull(unauthorizedResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(401, unauthorizedResult.StatusCode);
            Assert.Equal("Invalid or expired refresh token.", unauthorizedResult.Value);
        });
    }

    [Fact]
    public async Task Logout_ShouldReturnOkResult_WhenRefreshTokenIsRevoked()
    {
        var user = UsersBuilder.CreateUsers().First();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var signInRequest = new AuthenticationDTO.SignIn.Request(user.Email, "User@Password-123");

        var signInResult = await _authenticationsController.SignIn(signInRequest);
        var okSignInResult = signInResult as OkObjectResult;
        var signInResponse = okSignInResult.Value as AuthenticationDTO.SignIn.Response;

        var refreshRequest = new AuthenticationDTO.SignIn.RefreshTokenRequest(signInResponse.RefreshToken);
        var logoutResult = await _authenticationsController.Logout(refreshRequest);
        var okResult = logoutResult as OkObjectResult;

        Assert.NotNull(okResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Logged out.", okResult.Value);
        });
    }

    [Fact]
    public async Task Logout_ShouldReturnNotFoundResult_WhenRefreshTokenDoNotExist()
    {
        var invalidToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var request = new AuthenticationDTO.SignIn.RefreshTokenRequest(invalidToken);

        var result = await _authenticationsController.Logout(request);
        var notFoundResult = result as NotFoundObjectResult;

        Assert.NotNull(notFoundResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Token not found.", notFoundResult.Value);
        });
    }

    [Fact]
    public async Task RecoverPasswordSendEmail_ShouldReturnOkResult_WhenTokenIsGeneratedAndEmailIsSent()
    {
        var user = UsersBuilder.CreateUsers().First();
        var token = "generated_token";

        _userRepositoryMock.Setup(u => u.GetUserByEmail(user.Email)).ReturnsAsync(user);
        _userRepositoryMock.Setup(u => u.GeneratePasswordResetToken(user.Id)).ReturnsAsync(token);
        _emailServiceMock.Setup(u => u.SendPasswordResetEmail(user.Email, token)).Returns(Task.CompletedTask);

        var request = new PasswordResetDTO.Request(user.Email);
        var result = await _authenticationsController.RecoverPasswordSendEmail(request);
        var response = result as OkResult;

        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task RecoverPasswordSendEmail_ShouldReturnOkResult_WhenEmailIsInvalid()
    {
        var request = new PasswordResetDTO.Request("testuser@gmail.com");
        var token = "";

        _userRepositoryMock.Setup(u => u.GetUserByEmail(request.Email)).ReturnsAsync((User)null);
        _emailServiceMock.Setup(e => e.SendPasswordResetEmail(request.Email, token)).Returns(Task.CompletedTask);

        var result = await _authenticationsController.RecoverPasswordSendEmail(request);
        var okResult = result as OkResult;

        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdatePassword_ShouldReturnOkResult_WhenPasswordIsUpdated()
    {
        var request = UsersBuilder.UpdatePasswordCreation("validToken", "New@Password-123", "New@Password-123");
        var user = UsersBuilder.CreateUsers().First();
        var token = UsersBuilder.CreateToken(request.Token, user);

        _userRepositoryMock.Setup(repo => repo.GetPasswordResetToken(request.Token)).ReturnsAsync(token);
        _userRepositoryMock.Setup(repo => repo.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.RemovePasswordResetToken(token)).Returns(Task.CompletedTask);

        var result = await _authenticationsController.UpdatePassword(request);
        var okResult = result as OkObjectResult;

        Assert.NotNull(okResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Password updated successfully.", okResult.Value);
        });
    }
}
