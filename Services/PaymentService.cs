using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentRepository _payments;
    private readonly IVisitRepository _visits;
    private readonly IAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ApplicationDbContext context,
        IPaymentRepository payments,
        IVisitRepository visits,
        IAuditService auditService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _payments = payments;
        _visits = visits;
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<PaymentCreateViewModel?> BuildCreateModelAsync(int visitId)
    {
        var visit = await _visits.GetDetailsAsync(visitId);
        if (visit is null)
        {
            return null;
        }

        return BuildCreateModel(visit);
    }

    public async Task AddPaymentAsync(PaymentCreateViewModel model)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var visit = await _visits.GetDetailsAsync(model.VisitId);
            if (visit is null)
            {
                throw new ValidationException("الزيارة غير موجودة.");
            }

            ValidatePaymentAmount(model.Amount, visit.RemainingBalance);

            var payment = new Payment
            {
                VisitId = visit.Id,
                Amount = model.Amount,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUsername()
            };

            await _payments.AddAsync(payment);

            visit.PaidAmount += model.Amount;
            visit.PaidAmount = Math.Min(visit.PaidAmount, visit.TotalPrice);
            visit.Paid = visit.RemainingBalance <= 0m;

            await _payments.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditService.LogAsync(
                AuditActionType.Create,
                nameof(Payment),
                $"Added payment of {payment.Amount:N2} EGP to Visit #{visit.Id}",
                payment.Id);
        }
        catch (ValidationException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to add payment to visit {VisitId}.", model.VisitId);
            throw;
        }
    }

    private static PaymentCreateViewModel BuildCreateModel(Visit visit)
    {
        return new PaymentCreateViewModel
        {
            VisitId = visit.Id,
            VisitTotalPrice = visit.TotalPrice,
            TotalPaid = visit.TotalPaid,
            RemainingBalance = visit.RemainingBalance,
            PatientName = visit.Patient?.Name ?? string.Empty,
            DoctorName = visit.Doctor?.Name ?? string.Empty
        };
    }

    private static void ValidatePaymentAmount(decimal amount, decimal remainingBalance)
    {
        if (amount <= 0)
        {
            throw new ValidationException("قيمة الدفعة يجب أن تكون أكبر من صفر.");
        }

        if (amount > remainingBalance)
        {
            throw new ValidationException($"لا يمكن إضافة دفعة أكبر من الرصيد المتبقي ({remainingBalance:N2} ج.م).");
        }
    }

    private string? GetCurrentUsername()
    {
        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        return string.IsNullOrWhiteSpace(username) ? null : username;
    }
}
