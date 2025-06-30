using Microsoft.EntityFrameworkCore;
using PageNest.Infrastructure.Data.Context;
using PageNest.Infrastructure.Data.Repositories;
using PageNest.TestUtils.Base;
using PageNest.TestUtils.Builders;

namespace PageNest.UnitTests.Repositories;

public class UserRepositoryTests : TestBase
{
    private readonly UserRepository _userRepository;

    public UserRepositoryTests() : base(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options))
    {
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnAllUsers()
    {
        var users = UsersBuilder.CreateUsers();
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        var result = await _userRepository.GetUsers();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        var user = UsersBuilder.CreateUsers().First();
        await _userRepository.CreateUser(user);

        var result = await _userRepository.GetUserById(user.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.PasswordHash, result.PasswordHash);
            Assert.Equal(user.Role, result.Role);
        });
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnUser_WhenUserExists()
    {
        var user = UsersBuilder.CreateUsers().First();
        await _userRepository.CreateUser(user);

        var result = await _userRepository.GetUserByEmail(user.Email);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.PasswordHash, result.PasswordHash);
            Assert.Equal(user.Role, result.Role);
        });
    }

    [Fact]
    public async Task CreateUser_ShouldCreateUser()
    {
        var newPlayer = UsersBuilder.CreateUsers().First();
        await _userRepository.CreateUser(newPlayer);

        var result = await _userRepository.GetUserById(newPlayer.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(newPlayer.Id, result.Id);
            Assert.Equal(newPlayer.Name, result.Name);
            Assert.Equal(newPlayer.Email, result.Email);
            Assert.Equal(newPlayer.PasswordHash, result.PasswordHash);
            Assert.Equal(newPlayer.Role, result.Role);
        });
    }

    [Fact]
    public async Task GetPasswordHashResetToken_ShouldReturnToken()
    {
        var existingUser = UsersBuilder.CreateUsers().First();
        await _userRepository.CreateUser(existingUser);

        var token = await _userRepository.GeneratePasswordResetToken(existingUser.Id);
        var savedToken = await _userRepository.GetPasswordResetToken(token);

        Assert.Multiple(() =>
        {
            Assert.Equal(token, savedToken.Token);
            Assert.True(savedToken.ExpirationDate > DateTime.UtcNow);
            Assert.Equal(existingUser.Id, savedToken.UserId);
        });
    }

    [Fact]
    public async Task GeneratePasswordHashResetToken_ShouldCreateToken()
    {
        var existingUser = UsersBuilder.CreateUsers().First();
        await _userRepository.CreateUser(existingUser);

        var token = await _userRepository.GeneratePasswordResetToken(existingUser.Id);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task RemovePasswordHashResetToken_ShouldRemoveToken()
    {
        var existingUser = UsersBuilder.CreateUsers().First();
        await _userRepository.CreateUser(existingUser);

        var token = await _userRepository.GeneratePasswordResetToken(existingUser.Id);
        var savedToken = await _userRepository.GetPasswordResetToken(token);
        await _userRepository.RemovePasswordResetToken(savedToken);

        var deletedToken = await _userRepository.GetPasswordResetToken(token);

        Assert.Multiple(() =>
        {
            Assert.NotNull(savedToken);
            Assert.Null(deletedToken);
        });
    }

    [Fact]
    public async Task UpdateUser_ShouldUpdateUser()
    {
        var createUser = UsersBuilder.CreateUsers().First();
        await _userRepository.CreateUser(createUser);

        _context.Entry(createUser).State = EntityState.Detached;

        var updatedUser = UsersBuilder.UpdateUser(createUser.Id);
        await _userRepository.UpdateUser(updatedUser);

        var result = await _userRepository.GetUserById(createUser.Id);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(updatedUser.Id, result.Id);
            Assert.Equal(updatedUser.Name, result.Name);
            Assert.Equal(updatedUser.Email, result.Email);
            Assert.Equal(updatedUser.PasswordHash, result.PasswordHash);
            Assert.Equal(updatedUser.Role, result.Role);
        });
    }

    [Fact]
    public async Task DeleteUser_ShouldDeleteUser()
    {
        var user = UsersBuilder.CreateUsers().First();

        await _userRepository.CreateUser(user);
        await _userRepository.DeleteUser(user.Id);

        var result = await _userRepository.GetUserById(user.Id);

        Assert.Null(result);
    }
}