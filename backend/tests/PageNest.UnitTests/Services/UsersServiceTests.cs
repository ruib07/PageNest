using Microsoft.EntityFrameworkCore;
using Moq;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Services;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Services;

public class UsersServiceTests : TestBase
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UsersService _usersService;

    public UsersServiceTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _usersService = new UsersService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnAllUsers()
    {
        var users = UsersBuilder.CreateUsers();
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        _userRepositoryMock.Setup(repo => repo.GetUsers()).ReturnsAsync(users);

        var result = await _usersService.GetUsers();

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(users.Count, result.Count());
            Assert.Equal(users.First().Id, result.First().Id);
            Assert.Equal(users.Last().Id, result.Last().Id);
        });
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        var user = UsersBuilder.CreateUsers().First();

        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _usersService.GetUserById(user.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(user.Id, result.Data.Id);
            Assert.Equal(user.Name, result.Data.Name);
            Assert.Equal(user.Email, result.Data.Email);
            Assert.Equal(user.PasswordHash, result.Data.PasswordHash);
            Assert.Equal(user.Role, result.Data.Role);
        });
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _userRepositoryMock.Setup(repo => repo.GetUserById(It.IsAny<Guid>())).ReturnsAsync((User)null);

        var result = await _usersService.GetUserById(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("User not found.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateUser_ShouldCreateUserSuccessfully()
    {
        var user = UsersBuilder.CreateUsers().First();

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);

        var result = await _usersService.CreateUser(user);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(user.Id, result.Data.Id);
            Assert.Equal(user.Name, result.Data.Name);
            Assert.Equal(user.Email, result.Data.Email);
            Assert.Equal(user.PasswordHash, result.Data.PasswordHash);
            Assert.Equal(user.Role, result.Data.Role);
            Assert.Equal("User created successfully.", result.Message);
        });
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenUserNameIsEmpty()
    {
        var user = UsersBuilder.InvalidUserCreation("", "user@email.com", "User@Password-123", Roles.User);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);

        var result = await _usersService.CreateUser(user);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Name is required.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenUserEmailIsEmpty()
    {
        var user = UsersBuilder.InvalidUserCreation("User Name", "", "User@Password-123", Roles.User);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);

        var result = await _usersService.CreateUser(user);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Valid email is required.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenUserEmailIsInvalid()
    {
        var user = UsersBuilder.InvalidUserCreation("User Name", "useremail.com", "User@Password-123", Roles.User);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);

        var result = await _usersService.CreateUser(user);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Valid email is required.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateUser_ShouldReturnConflict_WhenUserEmailAlreadyExists()
    {
        var user = UsersBuilder.CreateUsers().First();

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(user.Email)).ReturnsAsync(user);

        var result = await _usersService.CreateUser(user);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.Error.StatusCode);
            Assert.Equal("User with the same email already exists.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenUserPasswordDoesNotMeetTheRequirements()
    {
        var user = UsersBuilder.InvalidUserCreation("User Name", "user@email.com", "WrongPassword", Roles.User);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);

        var result = await _usersService.CreateUser(user);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Password must be at least 8 characters long and contain at least one uppercase letter, " +
                            "one lowercase letter, one number, and one special character.", result.Error.Message);
        });
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenUserRoleIsInvalid()
    {
        var user = UsersBuilder.InvalidUserCreation("User Name", "user@email.com", "User@Password-123", (Roles)999);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);

        var result = await _usersService.CreateUser(user);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Invalid role.", result.Error.Message);
        });
    }

    [Fact]
    public async Task GeneratePasswordResetToken_ShouldReturnToken()
    {
        var user = UsersBuilder.CreateUsers().First();

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(user.Email)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.GeneratePasswordResetToken(user.Id)).ReturnsAsync("token");

        var result = await _usersService.GeneratePasswordResetToken(user.Email);

        Assert.NotNull(result);
        Assert.Equal("token", result);
    }

    [Fact]
    public async Task UpdatePassword_ShouldUpdatePasswordSuccessfully()
    {
        var user = UsersBuilder.CreateUsers().First();
        var updatePassword = UsersBuilder.UpdatePasswordCreation("valid-token", "New@Password-123", "New@Password-123");
        var passwordResetToken = new PasswordReset() { Token = updatePassword.Token, User = user };

        _userRepositoryMock.Setup(u => u.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(u => u.GetPasswordResetToken(updatePassword.Token)).ReturnsAsync(passwordResetToken);
        _userRepositoryMock.Setup(u => u.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(u => u.RemovePasswordResetToken(passwordResetToken)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(u => u.GetUserById(user.Id)).ReturnsAsync(user);

        await _usersService.UpdatePassword(updatePassword.Token, updatePassword.NewPassword, updatePassword.ConfirmNewPassword);
        var result = await _usersService.GetUserById(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.PasswordHash, result.Data.PasswordHash);
    }

    [Fact]
    public async Task UpdatePassword_ShouldReturnsBadRequest_WhenPasswordFieldsAreEmpty()
    {
        var updatePassword = UsersBuilder.UpdatePasswordCreation("valid-token", string.Empty, string.Empty);

        _userRepositoryMock.Setup(u => u.GetPasswordResetToken(updatePassword.Token)).ReturnsAsync((PasswordReset)null);
        _userRepositoryMock.Setup(u => u.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _usersService.UpdatePassword(updatePassword.Token, updatePassword.NewPassword, updatePassword.ConfirmNewPassword);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Password fields cannot be empty.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdatePassword_ShouldReturnsBadRequest_WhenPasswordsDoNotMatch()
    {
        var updatePassword = UsersBuilder.UpdatePasswordCreation("valid-token", "NewPassword123!", "DifferentPassword123!");

        _userRepositoryMock.Setup(u => u.GetPasswordResetToken(updatePassword.Token)).ReturnsAsync((PasswordReset)null);
        _userRepositoryMock.Setup(u => u.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _usersService.UpdatePassword(updatePassword.Token, updatePassword.NewPassword, updatePassword.ConfirmNewPassword);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Passwords do not match.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdatePassword_ShouldReturnsBadRequest_WhenTokenIsInvalid()
    {
        var updatePassword = UsersBuilder.UpdatePasswordCreation("invalid-token", "NewPassword123!", "NewPassword123!");

        _userRepositoryMock.Setup(u => u.GetPasswordResetToken(updatePassword.Token)).ReturnsAsync((PasswordReset)null);
        _userRepositoryMock.Setup(u => u.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _usersService.UpdatePassword(updatePassword.Token, updatePassword.NewPassword, updatePassword.ConfirmNewPassword);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Invalid or expired token.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdatePassword_ShouldReturnsBadRequest_WhenPasswordDoesNotMeetRequirements()
    {
        var user = UsersBuilder.CreateUsers().First();
        var updatePassword = UsersBuilder.UpdatePasswordCreation("valid-token", "password", "password");
        var resetToken = UsersBuilder.CreateToken(updatePassword.Token, user);

        _userRepositoryMock.Setup(u => u.GetPasswordResetToken(updatePassword.Token)).ReturnsAsync(resetToken);
        _userRepositoryMock.Setup(u => u.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _usersService.UpdatePassword(updatePassword.Token, updatePassword.NewPassword, updatePassword.ConfirmNewPassword);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Password must be at least 8 characters long and contain at least one uppercase letter, " +
                            "one lowercase letter, one number, and one special character.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldUpdateUserSuccessfully()
    {
        var user = UsersBuilder.CreateUsers().First();
        var updatedUser = UsersBuilder.UpdateUser(user.Id);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUser(updatedUser)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        var updatedResult = await _usersService.UpdateUser(user.Id, updatedUser);
        var result = await _usersService.GetUserById(user.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(updatedUser.Id, result.Data.Id);
            Assert.Equal(updatedUser.Name, result.Data.Name);
            Assert.Equal("User updated successfully.", updatedResult.Message);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserNameIsEmpty()
    {
        var user = UsersBuilder.CreateUsers().First();
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("", "user@email.com", "User@Password-123", Roles.User);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUser(invalidUpdatedUser)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _usersService.UpdateUser(user.Id, invalidUpdatedUser);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Name is required.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserEmailIsEmpty()
    {
        var user = UsersBuilder.CreateUsers().First();
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", "", "User@Password-123", Roles.User);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUser(invalidUpdatedUser)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _usersService.UpdateUser(user.Id, invalidUpdatedUser);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Valid email is required.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserEmailIsInvalid()
    {
        var user = UsersBuilder.CreateUsers().First();
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", "useremail.com", "User@Password-123", Roles.User);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUser(invalidUpdatedUser)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _usersService.UpdateUser(user.Id, invalidUpdatedUser);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Valid email is required.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnConflict_WhenUserEmailAlreadyExists()
    {
        var user = UsersBuilder.CreateUsers().First();
        var existingUserWithSameEmail = UsersBuilder.InvalidUserCreation("Other User", user.Email, "User@Password-123", Roles.User);
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", user.Email, "User@Password-123", Roles.User);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUser(invalidUpdatedUser)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(user.Email)).ReturnsAsync(existingUserWithSameEmail);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _usersService.UpdateUser(user.Id, invalidUpdatedUser);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.Error.StatusCode);
            Assert.Equal("User with the same email already exists.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserPasswordDoesNotMeetTheRequirements()
    {
        var user = UsersBuilder.CreateUsers().First();
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", "user@email.com", "Password", Roles.User);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUser(invalidUpdatedUser)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _usersService.UpdateUser(user.Id, invalidUpdatedUser);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Password must be at least 8 characters long and contain at least one uppercase letter, " +
                            "one lowercase letter, one number, and one special character.", result.Error.Message);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserRoleIsInvalid()
    {
        var user = UsersBuilder.CreateUsers().First();
        var invalidUpdatedUser = UsersBuilder.InvalidUserCreation("User Updated", "user@email.com", "User@Password-123", (Roles)999);

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUser(invalidUpdatedUser)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync(user);

        var result = await _usersService.UpdateUser(user.Id, invalidUpdatedUser);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.StatusCode);
            Assert.Equal("Invalid role.", result.Error.Message);
        });
    }

    [Fact]
    public async Task DeleteUser_ShouldDeleteUserSuccessfully()
    {
        var user = UsersBuilder.CreateUsers().First();

        _userRepositoryMock.Setup(repo => repo.CreateUser(user)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.DeleteUser(user.Id)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.GetUserById(user.Id)).ReturnsAsync((User)null);

        await _usersService.CreateUser(user);
        await _usersService.DeleteUser(user.Id);

        var result = await _usersService.GetUserById(user.Id);

        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.StatusCode);
            Assert.Equal("User not found.", result.Error.Message);
        });
    }
}
