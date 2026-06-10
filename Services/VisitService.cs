namespace ClinicManagementSystem.Services;

public class VisitService : IVisitService
{
    private readonly ApplicationDbContext _context;
    private readonly IVisitRepository _visits;
    private readonly IAppointmentService _appointments;
    private readonly IProcedureRepository _procedures;
    private readonly IVisitConsumptionService _visitConsumptionService;
    private readonly ISettingsService _settings;
    private readonly IAuditService _auditService;
    private readonly ILogger<VisitService> _logger;

    public VisitService(
        ApplicationDbContext context,
        IVisitRepository visits,
        IAppointmentService appointments,
        IProcedureRepository procedures,
        IVisitConsumptionService visitConsumptionService,
        ISettingsService settings,
        IAuditService auditService,
        ILogger<VisitService> logger)
    {
        _context = context;
        _visits = visits;
        _appointments = appointments;
        _procedures = procedures;
        _visitConsumptionService = visitConsumptionService;
        _settings = settings;
        _auditService = auditService;
        _logger = logger;
    }

    public Task<IReadOnlyList<Visit>> GetRecentAsync()
    {
        return _visits.GetRecentAsync();
    }

    public async Task<Visit?> GetDetailsAsync(int id)
    {
        return await _visits.GetDetailsAsync(id);
    }
    public async Task<IReadOnlyList<Visit>> GetPatientVisitsAsync(int patientId, int currentVisitId)
    {
        return await _visits.GetPatientVisitsAsync(patientId, currentVisitId);
    }
    public async Task<VisitCreateViewModel?> BuildFromAppointmentAsync(int appointmentId)
    {
        var appointment = await _appointments.GetDetailsAsync(appointmentId);
        if (appointment is null)
        {
            return null;
        }

        return new VisitCreateViewModel
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            Date = DateTime.Today,
            ExaminationPrice = await _settings.GetDecimal(SettingKeys.DefaultExamPrice)
        };
    }

    public async Task CreateAsync(VisitCreateViewModel model)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var allowDiscount = await _settings.GetBool(SettingKeys.AllowDiscount);
            var maxDiscount = await _settings.GetDecimal(SettingKeys.MaxDiscount);
            var defaultExamPrice = await _settings.GetDecimal(SettingKeys.DefaultExamPrice);

            ValidateVisitBasics(model);
            await ValidateAppointmentAsync(model.AppointmentId);

            var requestedProcedures = NormalizeRequestedProcedures(model.Procedures);
            var procedureById = await LoadProceduresAsync(requestedProcedures);
            var examinationPrice = ResolveExaminationPrice(model.ExaminationPrice, defaultExamPrice);
            var discount = ResolveDiscount(model.Discount, allowDiscount, maxDiscount);
            var proceduresPrice = CalculateProceduresPrice(requestedProcedures, procedureById);
            var totalPrice = CalculateTotalPrice(examinationPrice, proceduresPrice, discount);

            ValidatePayment(model.PaidAmount, totalPrice);
            var requestedProducts = NormalizeRequestedProducts(model.ConsumedProducts);

            var visit = new Visit
            {
                AppointmentId = model.AppointmentId,
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                Date = model.Date.Date,
                Notes = model.Notes,
                ExaminationPrice = examinationPrice,
                ProceduresPrice = proceduresPrice,
                Discount = discount,
                TotalPrice = totalPrice,
                PaidAmount = model.PaidAmount,
                Paid = totalPrice - model.PaidAmount <= 0m
            };

            if (model.PaidAmount > 0)
            {
                visit.Payments.Add(new Payment
                {
                    Amount = model.PaidAmount,
                    Notes = "Initial payment during visit creation",
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (var input in requestedProcedures)
            {
                var procedureId = input.ProcedureId.GetValueOrDefault();
                var quantity = input.Quantity.GetValueOrDefault();
                var procedure = procedureById[procedureId];
                visit.VisitProcedures.Add(new VisitProcedure
                {
                    ProcedureId = procedure.Id,
                    Quantity = quantity,
                    SubTotal = procedure.Price * quantity
                });
            }

            await _visits.AddAsync(visit);
            await _visits.SaveChangesAsync();

            foreach (var input in requestedProducts)
            {
                await _visitConsumptionService.ConsumeProductAsync(
                    visit.Id,
                    input.ProductId.GetValueOrDefault(),
                    input.Quantity.GetValueOrDefault());
            }

            if (model.AppointmentId.HasValue)
            {
                await _appointments.MarkDoneAsync(model.AppointmentId.Value);
            }

            await transaction.CommitAsync();

            await _auditService.LogAsync(
                AuditActionType.Create,
                nameof(Visit),
                $"Created visit #{visit.Id} for patient #{visit.PatientId} with total {visit.TotalPrice:N2}",
                visit.Id);

            if (visit.Paid || visit.PaidAmount > 0)
            {
                var paymentStatus = visit.Paid ? "paid in full" : "partially paid";
                await _auditService.LogAsync(
                    AuditActionType.Update,
                    nameof(Visit),
                    $"Visit #{visit.Id} payment status is {paymentStatus}; paid amount {visit.PaidAmount:N2}",
                    visit.Id);
            }
        }
        catch (ValidationException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(ex, "Validation error while creating visit for patient {PatientId}", model.PatientId);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Unexpected error while creating visit for patient {PatientId}", model.PatientId);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        await VoidAsync(id, "Voided through legacy delete workflow.");
    }

    public async Task VoidAsync(int id, string? reason = null)
    {
        var visit = await _visits.GetByIdAsync(id);
        if (visit == null) return;

        if (visit.Status == VisitStatus.Voided)
        {
            return;
        }

        visit.Status = VisitStatus.Voided;
        visit.VoidedAt = DateTime.UtcNow;
        visit.VoidReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        _visits.Update(visit);
        await _visits.SaveChangesAsync();

        var description = string.IsNullOrWhiteSpace(visit.VoidReason)
            ? $"Voided Visit #{visit.Id}"
            : $"Voided Visit #{visit.Id}. Reason: {visit.VoidReason}";

        await _auditService.LogAsync(
            AuditActionType.Delete,
            nameof(Visit),
            description,
            visit.Id);
    }

    private static List<VisitProcedureInput> NormalizeRequestedProcedures(IEnumerable<VisitProcedureInput> procedures)
    {
        return procedures
            .Where(x => x.ProcedureId > 0 && x.Quantity > 0)
            .GroupBy(x => x.ProcedureId)
            .Select(group => new VisitProcedureInput
            {
                ProcedureId = group.Key,
                Quantity = group.Sum(x => x.Quantity)
            })
            .ToList();
    }

    private static List<VisitProductConsumptionInput> NormalizeRequestedProducts(IEnumerable<VisitProductConsumptionInput> products)
    {
        return products
            .Where(x => x.ProductId > 0 && x.Quantity > 0)
            .GroupBy(x => x.ProductId)
            .Select(group => new VisitProductConsumptionInput
            {
                ProductId = group.Key,
                Quantity = group.Sum(x => x.Quantity)
            })
            .ToList();
    }

    private async Task<Dictionary<int, Procedure>> LoadProceduresAsync(IReadOnlyCollection<VisitProcedureInput> requestedProcedures)
    {
        var procedureIds = requestedProcedures
            .Where(x => x.ProcedureId.HasValue)
            .Select(x => x.ProcedureId.GetValueOrDefault())
            .ToArray();
        var procedures = await _procedures.FindAsync(x => procedureIds.Contains(x.Id));
        var procedureById = procedures.ToDictionary(x => x.Id);

        var missingProcedureIds = procedureIds.Except(procedureById.Keys).ToArray();
        if (missingProcedureIds.Length > 0)
        {
            throw new ValidationException("One or more selected procedures are invalid.");
        }

        return procedureById;
    }

    private static void ValidateVisitBasics(VisitCreateViewModel model)
    {
        if (model.PatientId <= 0)
        {
            throw new ValidationException("Patient is required.");
        }

        if (model.DoctorId <= 0)
        {
            throw new ValidationException("Doctor is required.");
        }
    }

    private async Task ValidateAppointmentAsync(int? appointmentId)
    {
        if (!appointmentId.HasValue)
        {
            return;
        }

        var appointment = await _appointments.GetDetailsAsync(appointmentId.Value);
        if (appointment is null)
        {
            throw new ValidationException("The selected appointment does not exist.");
        }

        if (appointment.Status != AppointmentStatus.Waiting)
        {
            throw new ValidationException("Only waiting appointments can be converted into visits.");
        }
    }

    private static decimal ResolveExaminationPrice(decimal submittedPrice, decimal defaultExamPrice)
    {
        if (submittedPrice < 0)
        {
            throw new ValidationException("Examination price cannot be negative.");
        }

        if (defaultExamPrice < 0)
        {
            throw new ValidationException("Default examination price setting is invalid.");
        }

        return submittedPrice == 0 ? defaultExamPrice : submittedPrice;
    }

    private static decimal ResolveDiscount(decimal submittedDiscount, bool allowDiscount, decimal maxDiscount)
    {
        if (!allowDiscount)
        {
            return 0m;
        }

        if (submittedDiscount < 0)
        {
            throw new ValidationException("Discount cannot be negative.");
        }

        if (maxDiscount < 0)
        {
            throw new ValidationException("Maximum discount setting is invalid.");
        }

        if (submittedDiscount > maxDiscount)
        {
            throw new ValidationException($"Discount cannot exceed {maxDiscount}.");
        }

        return submittedDiscount;
    }

    private static decimal CalculateProceduresPrice(
        IEnumerable<VisitProcedureInput> requestedProcedures,
        IReadOnlyDictionary<int, Procedure> procedureById)
    {
        return requestedProcedures.Sum(input =>
        {
            var procedureId = input.ProcedureId.GetValueOrDefault();
            var quantity = input.Quantity.GetValueOrDefault();
            return procedureById[procedureId].Price * quantity;
        });
    }

    private static decimal CalculateTotalPrice(decimal examinationPrice, decimal proceduresPrice, decimal discount)
    {
        return Math.Max(0m, examinationPrice + proceduresPrice - discount);
    }

    private static void ValidatePayment(decimal paidAmount, decimal totalPrice)
    {
        if (paidAmount < 0)
        {
            throw new ValidationException("Paid amount cannot be negative.");
        }

        if (paidAmount > totalPrice)
        {
            throw new ValidationException("Paid amount cannot exceed total price.");
        }
    }
}
