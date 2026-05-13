using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagementSystem.Models;

[Table("AuditLogs")]
public class AuditLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    public AuditActionType ActionType { get; set; }

    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = null!;

    public int? EntityId { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(45)]
    public string? IpAddress { get; set; }
}
