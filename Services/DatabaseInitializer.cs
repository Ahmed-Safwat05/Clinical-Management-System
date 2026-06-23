using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Services;

public static class DatabaseInitializer
{
    public static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        var passwordHasher = new PasswordHasher<AppUser>();

        // Admin
        //if (!await context.AppUsers.AnyAsync(x => x.Username == "admin"))
        //{
        //    var adminUser = new AppUser
        //    {
        //        Username = "admin",
        //        DisplayName = "Administrator",
        //        Role = UserRole.Admin,
        //        IsActive = true,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    adminUser.PasswordHash =
        //        passwordHasher.HashPassword(adminUser, "admin1");

        //    context.AppUsers.Add(adminUser);
        //}

        //// Reception
        //if (!await context.AppUsers.AnyAsync(x => x.Username == "reception"))
        //{
        //    var receptionUser = new AppUser
        //    {
        //        Username = "reception",
        //        DisplayName = "Receptionist",
        //        Role = UserRole.Receptionist,
        //        IsActive = true,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    receptionUser.PasswordHash =
        //        passwordHasher.HashPassword(receptionUser, "reception1");

        //    context.AppUsers.Add(receptionUser);
        //}

        await context.SaveChangesAsync();
    }
}