using ClinicManagementSystem.Interfaces.Repositories;
using ClinicManagementSystem.Interfaces.Services;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patients;
    private readonly IAuditService _auditService;

    public PatientService(IPatientRepository patients, IAuditService auditService)
    {
        _patients = patients;
        _auditService = auditService;
    }

    public Task<IReadOnlyList<Patient>> SearchAsync(string? searchTerm) => _patients.SearchAsync(searchTerm);

    public Task<Patient?> GetByIdAsync(int id) => _patients.GetByIdAsync(id);

    public Task<IReadOnlyList<Patient>> GetDeletedAsync() => _patients.GetDeletedAsync();

    public async Task CreateAsync(Patient patient)
    {
        await _patients.AddAsync(patient);
        await _patients.SaveChangesAsync();

        await _auditService.LogAsync(
            AuditActionType.Create,
            nameof(Patient),
            $"Created patient {patient.Name}",
            patient.Id);
    }

    public async Task UpdateAsync(Patient patient)
    {
        _patients.Update(patient);
        await _patients.SaveChangesAsync();

        await _auditService.LogAsync(
            AuditActionType.Update,
            nameof(Patient),
            $"Updated patient {patient.Name}",
            patient.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var patient = await _patients.GetByIdAsync(id);
        if (patient is null)
        {
            return;
        }

        patient.IsDeleted = true;
        _patients.Update(patient);
        await _patients.SaveChangesAsync();

        await _auditService.LogAsync(
            AuditActionType.Delete,
            nameof(Patient),
            $"Deleted patient {patient.Name}",
            patient.Id);
    }

    public async Task RestoreAsync(int id)
    {
        var patient = await _patients.GetDeletedByIdAsync(id);
        if (patient is null)
        {
            return;
        }

        patient.IsDeleted = false;
        _patients.Update(patient);
        await _patients.SaveChangesAsync();
    }
}
