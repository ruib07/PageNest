using System.Security.Cryptography;

namespace PageNest.TestUtils.Helpers;

public static class RandomKeyGenerator
{
    public static string GenerateRandomKey()
    {
        byte[] keyBytes = new byte[32];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(keyBytes);

        string base64Key = Convert.ToBase64String(keyBytes);
        base64Key = base64Key.Replace('_', '/').Replace('-', '+');

        return base64Key;
    }
}
