using ClinicManagementSystem.Data;
using ClinicManagementSystem.Interfaces.Services;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Services;

public class PatientHistoryService : IPatientHistoryService
{
    private readonly ApplicationDbContext _context;

    public PatientHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets summary statistics excluding voided visits
    /// Single efficient query with projection
    /// </summary>
    public async Task<PatientHistorySummaryDto> GetPatientSummaryAsync(int patientId)
    {
        var summary = await _context.Visits
            .AsNoTracking()
            .Where(v => v.PatientId == patientId && v.Status != VisitStatus.Voided)
            .Select(v => new
            {
                VisitCount = 1,
                LastVisitDate = v.Date,
                LastDoctorName = v.Doctor!.Name,
                TotalPrice = v.TotalPrice,
                TotalPaid = v.TotalPaid
            })
            .GroupBy(_ => 1)
            .Select(g => new PatientHistorySummaryDto
            {
                PatientId = patientId,
                TotalVisits = g.Count(),
                LastVisitDate = g.Max(x => x.LastVisitDate),
                LastDoctorName = g.OrderByDescending(x => x.LastVisitDate).FirstOrDefault()!.LastDoctorName,
                TotalAmountSpent = g.Sum(x => x.TotalPrice),
                TotalAmountPaid = g.Sum(x => x.TotalPaid)
            })
            .FirstOrDefaultAsync();

        return summary ?? new PatientHistorySummaryDto { PatientId = patientId };
    }

    /// <summary>
    /// Gets complete visit timeline (newest first)
    /// Includes both active and voided visits for audit trail
    /// </summary>
    public async Task<PatientVisitTimelineDto> GetPatientVisitsTimelineAsync(int patientId)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .Where(p => p.Id == patientId)
            .Select(p => new { p.Id, p.Name })
            .FirstOrDefaultAsync();

        if (patient is null)
            return new PatientVisitTimelineDto { PatientId = patientId };

        var visits = await _context.Visits
            .AsNoTracking()
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.Date)
            .Select(v => new VisitTimelineItemDto
            {
                Id = v.Id,
                Date = v.Date,
                DoctorName = v.Doctor!.Name,
                TotalPrice = v.TotalPrice,
                TotalPaid = v.TotalPaid,
                Status = v.Status,
                Notes = v.Notes,
                VoidedAt = v.VoidedAt,
                VoidReason = v.VoidReason
            })
            .ToListAsync();

        return new PatientVisitTimelineDto
        {
            PatientId = patientId,
            PatientName = patient.Name,
            Visits = visits
        };
    }

    /// <summary>
    /// Gets previous visits (newest first) excluding current
    /// Used in Visit Details to show related visits
    /// </summary>
    public async Task<List<PreviousVisitDto>> GetPreviousVisitsAsync(int patientId, int excludeVisitId, int take = 5)
    {
        return await _context.Visits
            .AsNoTracking()
            .Where(v => v.PatientId == patientId && v.Id != excludeVisitId)
            .OrderByDescending(v => v.Date)
            .Take(take)
            .Select(v => new PreviousVisitDto
            {
                Id = v.Id,
                Date = v.Date,
                DoctorName = v.Doctor!.Name,
                TotalPrice = v.TotalPrice,
                Status = v.Status
            })
            .ToListAsync();
    }
}
