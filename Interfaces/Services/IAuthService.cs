namespace ClinicManagementSystem.Interfaces.Services;

public interface IAuthService
{
    Task<bool> ValidateUserAsync(string username, string password);
    Task<ClaimsPrincipal?> CreatePrincipalAsync(string username);
    string HashPassword(AppUser user, string password);
}
