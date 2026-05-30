namespace ClinicManagementSystem.Models;

public class Payment
{
    public int Id { get; set; }

    public int VisitId { get; set; }
    public Visit? Visit { get; set; }

    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? CreatedBy { get; set; }
}
