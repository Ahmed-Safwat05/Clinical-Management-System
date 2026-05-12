using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Utilities;

/// <summary>
/// Utility class for password hashing and verification.
/// Use this to generate password hashes for appsettings.json configuration.
/// </summary>
public static class PasswordHashingUtility
{
    private static readonly PasswordHasher<string> Hasher = new();

    /// <summary>
    /// Generates a secure hash of the provided password.
    /// Use the output to configure ReceptionistAccount:PasswordHash in appsettings.json
    /// </summary>
    /// <param name="username">The username (e.g., "receptionist")</param>
    /// <param name="password">The plain-text password</param>
    /// <returns>A hashed password string suitable for configuration</returns>
    public static string GenerateHash(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return Hasher.HashPassword(username, password);
    }

    /// <summary>
    /// Verifies if a plain-text password matches the stored hash.
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="hash">The stored password hash</param>
    /// <param name="password">The plain-text password to verify</param>
    /// <returns>True if password matches, false otherwise</returns>
    public static bool VerifyPassword(string username, string hash, string password)
    {
        var result = Hasher.VerifyHashedPassword(username, hash, password);
        return result == PasswordVerificationResult.Success;
    }
}

/// <summary>
/// Example usage in Program.cs or during development:
/// 
/// var hash = PasswordHashingUtility.GenerateHash("receptionist", "MySecurePassword123!");
/// Console.WriteLine($"PasswordHash: {hash}");
/// 
/// Then copy the output to appsettings.json:
/// "ReceptionistAccount": {
///   "Username": "receptionist",
///   "PasswordHash": "AQAAAAIAAYagAAAAE..."
/// }
/// </summary>
