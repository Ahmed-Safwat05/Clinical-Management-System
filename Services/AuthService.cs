using System.Security.Claims;
using ClinicManagementSystem.Interfaces.Repositories;
using ClinicManagementSystem.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Services;

public class AuthService : IAuthService
{
    private readonly IAppUserRepository _appUserRepository;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public AuthService(IAppUserRepository appUserRepository)
    {
        _appUserRepository = appUserRepository;
    }

    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        var user = await _appUserRepository.GetByUsernameAsync(username);

        if (user == null || !user.IsActive)
        {
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }

    public async Task<ClaimsPrincipal?> CreatePrincipalAsync(string username)
    {
        var user = await _appUserRepository.GetByUsernameAsync(username);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("DisplayName", user.DisplayName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    public string HashPassword(AppUser user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }
}
