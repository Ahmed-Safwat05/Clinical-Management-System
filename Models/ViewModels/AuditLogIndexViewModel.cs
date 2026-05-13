namespace ClinicManagementSystem.Models.ViewModels;

public class AuditLogIndexViewModel
{
    public List<AuditLogItemDto> AuditLogs { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }

    // Filters
    public string? UsernameFilter { get; set; }
    public AuditActionType? ActionTypeFilter { get; set; }
    public DateTime? StartDateFilter { get; set; }
    public DateTime? EndDateFilter { get; set; }
    public string? KeywordFilter { get; set; }
}

public class AuditLogItemDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public AuditActionType ActionType { get; set; }
    public string EntityName { get; set; } = null!;
    public int? EntityId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? IpAddress { get; set; }
}
