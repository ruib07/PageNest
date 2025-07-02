using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.API.Controllers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Controllers;

public class UsersControllerTests : TestBase
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UsersService _usersService;
    private readonly UsersController _usersController;

    public UsersControllerTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _usersService = new UsersService(_userRepositoryMock.Object);
        _usersController = new UsersController(_usersService);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOkResult_WithAllUsers()
    {
        var users = UsersBuilder.CreateUsers();

        _userRepositoryMock.Setup(repo => repo.GetUsers()).ReturnsAsync(users);

        var result = await _usersController.GetUsers();
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as IEnumerable<User>;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(users.Count, response.Count());
            Assert.Equal(users.First().Id, response.First().Id);
            Assert.Equal(users.Last().Id, response.Last().Id);
        });
    }

    [Fact]
    public async Task GetUserById_ShouldReturnOkResult_WithUser()
    {
        var user = UsersBuilder.CreateUsers().First();

        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _usersController.GetUserById(user.Id);
        var okResult = result as OkObjectResult;
        var response = okResult.Value as User;

        Assert.NotNull(response);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(user.Id, response.Id);
            Assert.Equal(user.Name, response.Name);
            Assert.Equal(user.Email, response.Email);
            Assert.Equal(user.PasswordHash, response.PasswordHash);
            Assert.Equal(user.Role, response.Role);
        });
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFoundResult_WhenUserDoesNotExist()
    {
        var result = await _usersController.GetUserById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("User not found.", error.Message);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnOkResult_WhenUserInfoIsUpdated()
    {
        var user = UsersBuilder.CreateUsers().First();
        var updatedUser = UsersBuilder.UpdateUser(user.Id);

        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _usersController.UpdateUser(user.Id, updatedUser);
        var okResult = result as OkObjectResult;

        Assert.NotNull(okResult);
        Assert.Multiple(() =>
        {
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("User updated successfully.", okResult.Value);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequestResult_WhenUserNameIsEmpty()
    {
        var user = UsersBuilder.CreateUsers().First();
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation(string.Empty, "user@email.com", "User@Password-123");

        var result = await _usersController.UpdateUser(user.Id, invalidUpdatedUser);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Name is required.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserEmailIsEmpty()
    {
        var user = UsersBuilder.CreateUsers().First();
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", string.Empty, "User@Password-123");

        var result = await _usersController.UpdateUser(user.Id, invalidUpdatedUser);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Valid email is required.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserEmailIsInvalid()
    {
        var user = UsersBuilder.CreateUsers().First();
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", "useremail.com", "User@Password-123");

        var result = await _usersController.UpdateUser(user.Id, invalidUpdatedUser);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("Valid email is required.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnConflict_WhenUserEmailAlreadyExists()
    {
        var user = UsersBuilder.CreateUsers().First();
        var existingUserWithSameEmail = UsersBuilder.InvalidUserCreation("Other User", user.Email, "User@Password-123");
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", user.Email, "User@Password-123");

        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(user.Email)).ReturnsAsync(existingUserWithSameEmail);

        var result = await _usersController.UpdateUser(user.Id, invalidUpdatedUser);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal("User with the same email already exists.", error.Message);
        Assert.Equal(409, error.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserPasswordDoesNotMeetTheRequirements()
    {
        var user = UsersBuilder.CreateUsers().First();
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", "user@email.com", "Password");

        var result = await _usersController.UpdateUser(user.Id, invalidUpdatedUser);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);

        var error = Assert.IsType<ResponsesDTO.Error>(objectResult.Value);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Password must be at least 8 characters long and contain at least one uppercase letter, " +
                            "one lowercase letter, one number, and one special character.", error.Message);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNoContentResult_WhenUserIsDeleted()
    {
        var user = UsersBuilder.CreateUsers().First();

        _userRepositoryMock.Setup(repo => repo.DeleteUser(user.Id)).Returns(Task.CompletedTask);

        var result = await _usersController.DeleteUser(user.Id);
        var noContentResult = result as NoContentResult;

        Assert.Equal(204, noContentResult.StatusCode);
    }
}
