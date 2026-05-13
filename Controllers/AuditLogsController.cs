namespace ClinicManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AuditLogsController : Controller
{
    private const int PageSize = 20;
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? keyword = null)
    {
        page = Math.Max(page, 1);

        var auditLogs = await _auditService.GetAuditLogsAsync(
            page,
            PageSize,
            username,
            actionType,
            startDate,
            endDate,
            keyword);

        var totalCount = await _auditService.GetAuditLogsCountAsync(
            username,
            actionType,
            startDate,
            endDate,
            keyword);

        var model = new AuditLogIndexViewModel
        {
            AuditLogs = auditLogs.Select(x => new AuditLogItemDto
            {
                Id = x.Id,
                Username = x.Username,
                ActionType = x.ActionType,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Description = x.Description,
                CreatedAt = x.CreatedAt,
                IpAddress = x.IpAddress
            }).ToList(),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize),
            PageSize = PageSize,
            TotalCount = totalCount,
            UsernameFilter = username,
            ActionTypeFilter = actionType,
            StartDateFilter = startDate,
            EndDateFilter = endDate,
            KeywordFilter = keyword
        };

        ViewData["Title"] = "سجل العمليات";
        return View(model);
    }
}
