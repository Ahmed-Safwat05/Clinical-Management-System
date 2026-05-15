namespace ClinicManagementSystem.Services;

public class PatientMedicalHistoryService : IPatientMedicalHistoryService
{
    private const int SummaryEntriesCount = 4;
    private readonly IPatientRepository _patients;
    private readonly IPatientMedicalHistoryRepository _historyEntries;

    public PatientMedicalHistoryService(
        IPatientRepository patients,
        IPatientMedicalHistoryRepository historyEntries)
    {
        _patients = patients;
        _historyEntries = historyEntries;
    }

    public async Task<PatientMedicalHistoryViewModel?> GetPatientHistoryAsync(int patientId)
    {
        var patient = await _patients.GetByIdAsync(patientId);
        if (patient is null)
        {
            return null;
        }

        var entries = await _historyEntries.GetByPatientIdAsync(patientId);

        return new PatientMedicalHistoryViewModel
        {
            Patient = patient,
            Entries = entries,
            SummaryEntries = entries.Take(SummaryEntriesCount).ToList()
        };
    }

    public async Task<PatientMedicalHistoryEntryFormViewModel?> BuildCreateFormAsync(int patientId)
    {
        var patient = await _patients.GetByIdAsync(patientId);
        if (patient is null)
        {
            return null;
        }

        return new PatientMedicalHistoryEntryFormViewModel
        {
            PatientId = patient.Id,
            PatientName = patient.Name
        };
    }

    public async Task<PatientMedicalHistoryEntryFormViewModel?> BuildEditFormAsync(int entryId)
    {
        var entry = await _historyEntries.GetByIdWithPatientAsync(entryId);
        if (entry is null || entry.Patient is null)
        {
            return null;
        }

        return new PatientMedicalHistoryEntryFormViewModel
        {
            Id = entry.Id,
            PatientId = entry.PatientId,
            PatientName = entry.Patient.Name,
            Title = entry.Title,
            Description = entry.Description
        };
    }

    public async Task<int?> CreateAsync(PatientMedicalHistoryEntryFormViewModel model)
    {
        if (!await _patients.ExistsAsync(x => x.Id == model.PatientId))
        {
            return null;
        }

        var entry = new PatientMedicalHistoryEntry
        {
            PatientId = model.PatientId,
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _historyEntries.AddAsync(entry);
        await _historyEntries.SaveChangesAsync();

        return entry.PatientId;
    }

    public async Task<int?> UpdateAsync(PatientMedicalHistoryEntryFormViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return null;
        }

        var entry = await _historyEntries.GetByIdAsync(model.Id.Value);
        if (entry is null)
        {
            return null;
        }

        entry.Title = model.Title.Trim();
        entry.Description = model.Description.Trim();
        entry.UpdatedAt = DateTime.UtcNow;

        _historyEntries.Update(entry);
        await _historyEntries.SaveChangesAsync();

        return entry.PatientId;
    }

    public async Task<int?> DeleteAsync(int entryId)
    {
        var entry = await _historyEntries.GetByIdAsync(entryId);
        if (entry is null)
        {
            return null;
        }

        var patientId = entry.PatientId;
        _historyEntries.Delete(entry);
        await _historyEntries.SaveChangesAsync();

        return patientId;
    }
}
