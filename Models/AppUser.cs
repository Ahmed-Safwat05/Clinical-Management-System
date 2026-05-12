using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagementSystem.Models;

[Table("AppUsers")]
public class AppUser
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
