using System.ComponentModel.DataAnnotations;
using ClinicManagementSystem.Data;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Services;

public class VisitService : IVisitService
{
    private readonly ApplicationDbContext _context;
    private readonly IVisitRepository _visits;
    private readonly IAppointmentService _appointments;
    private readonly IProcedureRepository _procedures;
    private readonly ISettingsService _settings;
    private readonly ILogger<VisitService> _logger;

    public VisitService(
        ApplicationDbContext context,
        IVisitRepository visits,
        IAppointmentService appointments,
        IProcedureRepository procedures,
        ISettingsService settings,
        ILogger<VisitService> logger)
    {
        _context = context;
        _visits = visits;
        _appointments = appointments;
        _procedures = procedures;
        _settings = settings;
        _logger = logger;
    }

    public Task<IReadOnlyList<Visit>> GetRecentAsync()
    {
        return _visits.GetRecentAsync();
    }

    public Task<Visit?> GetDetailsAsync(int id) => _visits.GetDetailsAsync(id);

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
            var proceduresPrice = model.ProceduresPrice;
            var totalPrice = CalculateTotalPrice(examinationPrice, proceduresPrice, discount);

            ValidatePayment(model.PaidAmount, totalPrice);

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
                Paid = model.Paid || (model.PaidAmount == totalPrice && totalPrice > 0)
            };

            foreach (var input in requestedProcedures)
            {
                var procedure = procedureById[input.ProcedureId.Value];
                visit.VisitProcedures.Add(new VisitProcedure
                {
                    ProcedureId = procedure.Id,
                    Quantity = input.Quantity.Value,
                    SubTotal = procedure.Price * input.Quantity.Value
                });
            }

            await _visits.AddAsync(visit);
            await _visits.SaveChangesAsync();

            if (model.AppointmentId.HasValue)
            {
                await _appointments.MarkDoneAsync(model.AppointmentId.Value);
            }

            await transaction.CommitAsync();
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
        var visit = await _visits.GetByIdAsync(id);
        if (visit == null) return;

        _visits.Remove(visit);
        await _visits.SaveChangesAsync();
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

    private async Task<Dictionary<int, Procedure>> LoadProceduresAsync(IReadOnlyCollection<VisitProcedureInput> requestedProcedures)
    {
        var procedureIds = requestedProcedures
            .Where(x => x.ProcedureId.HasValue)
            .Select(x => x.ProcedureId.Value)
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

    //private static decimal CalculateProceduresPrice(
    //    IEnumerable<VisitProcedureInput> requestedProcedures,
    //    IReadOnlyDictionary<int, Procedure> procedureById)
    //{
    //    return requestedProcedures.Sum(input => procedureById[input.ProcedureId].Price * input.Quantity);
    //}

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
