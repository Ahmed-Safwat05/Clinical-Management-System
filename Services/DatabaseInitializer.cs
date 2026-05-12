using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Services;

public class DatabaseInitializer
{
    public static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        // Check if users already exist
        if (await context.AppUsers.AnyAsync())
        {
            return;
        }

        var passwordHasher = new PasswordHasher<AppUser>();

        var adminUser = new AppUser
        {
            Username = "admin",
            DisplayName = "Administrator",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "admin1");

        var receptionUser = new AppUser
        {
            Username = "reception",
            DisplayName = "Receptionist",
            Role = UserRole.Receptionist,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        receptionUser.PasswordHash = passwordHasher.HashPassword(receptionUser, "reception1");

        context.AppUsers.AddRange(adminUser, receptionUser);
        await context.SaveChangesAsync();
    }
}
