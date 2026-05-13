namespace ClinicManagementSystem.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository auditLogRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        AuditActionType actionType,
        string entityName,
        string description,
        int? entityId = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                ActionType = actionType,
                EntityName = TruncateRequired(entityName, 100),
                EntityId = entityId,
                Description = TruncateRequired(description, 500),
                Username = TruncateRequired(GetCurrentUsername(), 100),
                UserId = GetCurrentUserId(),
                IpAddress = TruncateOptional(GetClientIpAddress(), 45),
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit logging failed for {ActionType} on {EntityName}.", actionType, entityName);
        }
    }

    public async Task LogLoginAsync(string username)
    {
        await LogAsync(AuditActionType.Login, "Authentication", $"Login: {username}");
    }

    public async Task LogLogoutAsync(string username)
    {
        await LogAsync(AuditActionType.Logout, "Authentication", $"Logout: {username}");
    }

    public async Task<IReadOnlyList<AuditLog>> GetAuditLogsAsync(
        int pageNumber,
        int pageSize,
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? keyword = null)
    {
        return await _auditLogRepository.GetPagedAsync(pageNumber, pageSize, username, actionType, startDate, endDate, keyword);
    }

    public async Task<int> GetAuditLogsCountAsync(
        string? username = null,
        AuditActionType? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? keyword = null)
    {
        return await _auditLogRepository.CountAsync(username, actionType, startDate, endDate, keyword);
    }

    private string GetCurrentUsername()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (!string.IsNullOrWhiteSpace(httpContext?.User?.Identity?.Name))
        {
            return httpContext.User.Identity.Name;
        }

        return "System";
    }

    private int? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    private string? GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ipAddress = forwardedFor.ToString().Split(',').FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                return ipAddress.Trim();
            }
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static string TruncateRequired(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string? TruncateOptional(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
