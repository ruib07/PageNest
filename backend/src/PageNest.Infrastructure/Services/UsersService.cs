using PageNest.Application.Helpers;
using PageNest.Application.Interfaces.Repositories;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.Common;
using PageNest.Domain.Entities;
using PageNest.Domain.Enums;

namespace PageNest.Infrastructure.Services;

public class UsersService : IUsersService
{
    private readonly IUserRepository _userRepository;

    public UsersService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<User>> GetUsers()
    {
        return await _userRepository.GetUsers();
    }

    public async Task<Result<User>> GetUserById(Guid userId)
    {
        var user = await _userRepository.GetUserById(userId);

        if (user == null) return Result<User>.Fail("User not found.", 404);

        return Result<User>.Success(user);
    }

    public async Task<Result<User>> CreateUser(User user)
    {
        var validation = ValidateUserFields(user);
        if (!validation.IsSuccess) return Result<User>.Fail(validation.Error.Message, validation.Error.StatusCode);

        var emailCheck = await ValidateExistingEmail(user);
        if (!emailCheck.IsSuccess) return Result<User>.Fail(emailCheck.Error.Message, emailCheck.Error.StatusCode);

        user.PasswordHash = PasswordHasherHelper.HashPassword(user.PasswordHash);

        var createdUser = await _userRepository.CreateUser(user);

        return Result<User>.Success(createdUser, "User created successfully.");
    }

    public async Task<string> GeneratePasswordResetToken(string email)
    {
        var user = await _userRepository.GetUserByEmail(email);

        if (user == null) return null;

        return await _userRepository.GeneratePasswordResetToken(user.Id);
    }

    public async Task<Result<User>> UpdatePassword(string token, string newPassword, string confirmNewPassword)
    {
        var validation = ValidateUpdatePasswordFields(newPassword, confirmNewPassword);
        if (!validation.IsSuccess) return Result<User>.Fail(validation.Error.Message, validation.Error.StatusCode);

        var passwordResetToken = await _userRepository.GetPasswordResetToken(token);
        if (passwordResetToken == null) return Result<User>.Fail("Invalid or expired token.", 400);

        var user = passwordResetToken.User;
        user.PasswordHash = PasswordHasherHelper.HashPassword(newPassword);

        await _userRepository.UpdateUser(user);
        await _userRepository.RemovePasswordResetToken(passwordResetToken);

        return Result<User>.Success(user, "Password updated successfully.");
    }

    public async Task<Result<User>> UpdateUser(Guid userId, User updateUser)
    {
        var currentUser = await _userRepository.GetUserById(userId);

        var skipPassword = string.IsNullOrWhiteSpace(updateUser.PasswordHash);
        var validation = ValidateUserFields(updateUser, skipPassword);
        if (!validation.IsSuccess) return Result<User>.Fail(validation.Error.Message, validation.Error.StatusCode);

        var emailCheck = await ValidateExistingEmail(updateUser, userId);
        if (!emailCheck.IsSuccess) return Result<User>.Fail(emailCheck.Error.Message, emailCheck.Error.StatusCode);

        currentUser.Name = updateUser.Name;
        currentUser.Email = updateUser.Email;
        if (!skipPassword)
        {
            currentUser.PasswordHash = PasswordHasherHelper.HashPassword(updateUser.PasswordHash);
        }
        currentUser.Role = updateUser.Role;

        await _userRepository.UpdateUser(currentUser);

        return Result<User>.Success(currentUser, "User updated successfully.");
    }

    public async Task DeleteUser(Guid userId)
    {
        await _userRepository.DeleteUser(userId);
    }

    #region Private Methods

    private static Result<bool> ValidateUserFields(User user, bool skipPassword = false)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
            return Result<bool>.Fail("Name is required.", 400);

        if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains('@'))
            return Result<bool>.Fail("Valid email is required.", 400);

        if (!skipPassword)
        {
            if (!PasswordPolicyHelper.IsValid(user.PasswordHash))
                return Result<bool>.Fail("Password must be at least 8 characters long and contain at least one uppercase letter, " +
                                            "one lowercase letter, one number, and one special character.", 400);
        }

        if (!Enum.IsDefined(typeof(Roles), user.Role)) return Result<bool>.Fail("Invalid role.", 400);

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ValidateExistingEmail(User user, Guid? currentUserId = null)
    {
        var existing = await _userRepository.GetUserByEmail(user.Email);

        if (existing != null && existing.Id != currentUserId)
            return Result<bool>.Fail("User with the same email already exists.", 409);

        return Result<bool>.Success(true);
    }

    private static Result<bool> ValidateUpdatePasswordFields(string newPassword, string confirmNewPassword)
    {
        if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmNewPassword))
            return Result<bool>.Fail("Password fields cannot be empty.", 400);

        if (newPassword != confirmNewPassword) return Result<bool>.Fail("Passwords do not match.", 400);

        if (!PasswordPolicyHelper.IsValid(newPassword))
            return Result<bool>.Fail("Password must be at least 8 characters long and contain at least one uppercase letter, " +
                                        "one lowercase letter, one number, and one special character.", 400);

        return Result<bool>.Success(true);
    }

    #endregion
}
