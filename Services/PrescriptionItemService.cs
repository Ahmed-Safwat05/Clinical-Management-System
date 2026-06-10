using ClinicManagementSystem.Data; // 👈 تأكد أن اسم النيم-سبيس ده مطابق لملف الـ DbContext عندك
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Services;

public class PrescriptionItemService : IPrescriptionItemService
{
    private readonly IPrescriptionItemRepository _prescriptionRepository;
    private readonly IVisitRepository _visitRepository;
    private readonly IAuditService _auditService;
    private readonly ApplicationDbContext _context; // 👈 حقنا الكلمة المشئومة بس في مكانها الصح وعشان مصلحتنا!

    public PrescriptionItemService(
        IPrescriptionItemRepository prescriptionRepository,
        IVisitRepository visitRepository,
        IAuditService auditService,
        ApplicationDbContext context) 
    {
        _prescriptionRepository = prescriptionRepository;
        _visitRepository = visitRepository;
        _auditService = auditService;
        _context = context; 
    }

    public async Task<PrescriptionItem?> CreateAsync(PrescriptionItem item)
    {
        var visitExists = await _visitRepository.ExistsAsync(x => x.Id == item.VisitId);
        if (!visitExists) return null;

        await _prescriptionRepository.AddAsync(item);
        var saved = await _prescriptionRepository.SaveChangesAsync() > 0;

        if (saved)
        {
            await _auditService.LogAsync(
                AuditActionType.Create,
                nameof(PrescriptionItem),
                $"Added medication {item.MedicationName} to visit #{item.VisitId}",
                item.Id);

            return item;
        }

        return null;
    }

    public async Task<bool> UpdateAsync(PrescriptionItem item)
    {
        var existing = await _prescriptionRepository.GetByIdAsync(item.Id);
        if (existing is null) return false;

        existing.MedicationName = item.MedicationName;
        existing.Dosage = item.Dosage;
        existing.Frequency = item.Frequency;
        existing.Duration = item.Duration;
        existing.Notes = item.Notes;

        _prescriptionRepository.Update(existing);
        var saved = await _prescriptionRepository.SaveChangesAsync() > 0;

        if (saved)
        {
            await _auditService.LogAsync(
                AuditActionType.Update,
                nameof(PrescriptionItem),
                $"Updated medication {item.MedicationName} in visit #{item.VisitId}",
                item.Id);
        }

        return saved;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.PrescriptionItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return false;

        _context.PrescriptionItems.Remove(item);

        var saved = await _context.SaveChangesAsync() > 0;

        if (saved)
        {
            await _auditService.LogAsync(
                AuditActionType.Delete,
                nameof(PrescriptionItem),
                $"Deleted medication {item.MedicationName} from visit #{item.VisitId}",
                item.Id);
        }

        return saved;
    }
}