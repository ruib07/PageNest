namespace PageNest.Application.Helpers;

public static class PasswordPolicyHelper
{
    private const string SpecialChars = "!@#$%^&*()_+-=[]{};':,.<>?";

    public static bool IsValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsLower)) return false;
        if (!password.Any(char.IsDigit)) return false;
        if (!password.Any(ch => SpecialChars.Contains(ch))) return false;

        return true;
    }
}
